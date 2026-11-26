using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Payments;
using DeckZa.Application.Interfaces;
using DeckZa.Domain.Entities;
using DeckZa.Domain.Enums;
using DeckZa.Infrastructure.Data;

namespace DeckZa.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".pdf"
    };

    private const long MaxFileSizeBytes = 2 * 1024 * 1024;

    private readonly DeckZaDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailDispatchService _emailDispatch;
    private readonly INotificationService _notificationService;
    private readonly IHostEnvironment _environment;
    private readonly ICurrentUserService _currentUser;
    private readonly IInvoiceService _invoiceService;
    private readonly IUserSessionNotifier _sessionNotifier;
    private readonly IErrorLogService _errorLog;

    public PaymentService(
        DeckZaDbContext context,
        IUnitOfWork unitOfWork,
        IEmailDispatchService emailDispatch,
        INotificationService notificationService,
        IHostEnvironment environment,
        ICurrentUserService currentUser,
        IInvoiceService invoiceService,
        IUserSessionNotifier sessionNotifier,
        IErrorLogService errorLog)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _emailDispatch = emailDispatch;
        _notificationService = notificationService;
        _environment = environment;
        _currentUser = currentUser;
        _invoiceService = invoiceService;
        _sessionNotifier = sessionNotifier;
        _errorLog = errorLog;
    }

    public async Task<PaymentInstructionDto> GetInstructionsAsync(CancellationToken cancellationToken = default)
    {
        var codes = new[] { "PAY_BNK_NM", "PAY_BNK_NO", "PAY_BNK_AN", "PAY_INSTR" };
        var values = await _context.rf_default_value
            .AsNoTracking()
            .Where(e => codes.Contains(e.def_code) && e.isActive == true && e.isDeleted == false)
            .ToDictionaryAsync(e => e.def_code, e => e.def_value ?? string.Empty, cancellationToken);

        return new PaymentInstructionDto
        {
            BankName = values.GetValueOrDefault("PAY_BNK_NM", string.Empty),
            AccountNumber = values.GetValueOrDefault("PAY_BNK_NO", string.Empty),
            AccountHolder = values.GetValueOrDefault("PAY_BNK_AN", string.Empty),
            Instructions = values.GetValueOrDefault("PAY_INSTR", string.Empty),
        };
    }

    public async Task<List<PaymentRequestDto>> GetMyRequestsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await (from request in _context.tb_payment_request.AsNoTracking()
                      join plan in _context.rf_plan.AsNoTracking() on request.plan_id equals plan.id into planJoin
                      from plan in planJoin.DefaultIfEmpty()
                      where request.user_id == userId && request.isDeleted == false
                      orderby request.createdDate descending
                      select new PaymentRequestDto
                      {
                          Id = request.id,
                          RequestNo = request.request_no,
                          PlanId = request.plan_id,
                          PlanName = plan != null ? plan.plan_name : request.plan_id,
                          Amount = request.amount,
                          Currency = request.currency,
                          TransferReference = request.transfer_reference,
                          ProofFileName = request.proof_file_name,
                          ProofContentType = request.proof_content_type,
                          Status = request.status,
                          CreatedDate = request.createdDate,
                          ReviewNote = request.review_note,
                      }).ToListAsync(cancellationToken);
    }

    public async Task<BaseGeneralResponse> SubmitAsync(
        string userId,
        string planId,
        string? transferReference,
        string fileName,
        string contentType,
        long fileSizeBytes,
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Fail("User session is not valid.");
        }

        if (string.IsNullOrWhiteSpace(planId))
        {
            return Fail("Plan is required.");
        }

        var extension = Path.GetExtension(fileName ?? string.Empty);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            return Fail("File type is not allowed. Use JPG, PNG, or PDF.");
        }

        if (fileSizeBytes <= 0 || fileSizeBytes > MaxFileSizeBytes)
        {
            return Fail("File size must be between 1 byte and 2 MB.");
        }

        var plan = await _context.rf_plan
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.id == planId && e.isActive == true && e.isDeleted == false,
                cancellationToken);
        if (plan == null)
        {
            return Fail("Plan not found or inactive.");
        }

        var currentPlan = await GetCurrentPlanAsync(userId, cancellationToken);
        if (currentPlan != null)
        {
            if (string.Equals(currentPlan.id, plan.id, StringComparison.OrdinalIgnoreCase))
            {
                return FailKey("PaymentAlreadyOnPlan", "You are already on this plan.");
            }

            if (plan.order_seq < currentPlan.order_seq)
            {
                return FailKey("PaymentDowngradeNotAllowed", "Plan downgrade cannot be requested from this page. Contact an administrator if it is really required.");
            }

            if (plan.order_seq == currentPlan.order_seq)
            {
                return FailKey("PaymentChangeNotAllowed", "This plan change cannot be requested from the billing page.");
            }
        }

        if (plan.price_amount <= 0)
        {
            return FailKey("PaymentFreePlanNoProof", "A free plan does not require a transfer proof.");
        }

        var hasPending = await _context.tb_payment_request.AnyAsync(
            e => e.user_id == userId
                 && e.status == "PENDING"
                 && e.isDeleted == false,
            cancellationToken);
        if (hasPending)
        {
            return FailKey("PaymentPendingRequestExists", "You still have a payment request waiting for review.");
        }

        var requestNo = await GenerateRequestNoAsync(cancellationToken);
        var safeExtension = extension.ToLowerInvariant();
        var paymentsRoot = Path.Combine(_environment.ContentRootPath, "payments", userId);
        Directory.CreateDirectory(paymentsRoot);

        var storedFileName = $"{requestNo}{safeExtension}";
        var storedPath = Path.Combine(paymentsRoot, storedFileName);
        var relativePath = Path.Combine("payments", userId, storedFileName).Replace('\\', '/');

        await using (var output = new FileStream(storedPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await fileStream.CopyToAsync(output, cancellationToken);
        }

        var now = DateTime.Now;
        var entity = new tb_payment_request
        {
            request_no = requestNo,
            user_id = userId,
            plan_id = plan.id,
            amount = plan.price_amount,
            currency = string.IsNullOrWhiteSpace(plan.price_currency) ? "IDR" : plan.price_currency,
            payment_method = "TRANSFER",
            transfer_reference = string.IsNullOrWhiteSpace(transferReference) ? null : transferReference.Trim(),
            proof_file_name = Path.GetFileName(fileName) ?? storedFileName,
            proof_stored_path = relativePath,
            proof_content_type = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            proof_size_bytes = fileSizeBytes,
            status = "PENDING",
            isActive = true,
            isDeleted = false,
            createdBy = userId,
            createdDate = now,
        };

        await _context.tb_payment_request.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await NotifyAdministratorsAsync(entity, plan.plan_name, cancellationToken);
        await QueueApplicantEmailAsync(userId, entity, plan.plan_name, cancellationToken);

        return new BaseGeneralResponse
        {
            Success = true,
            Code = Const.SUCCESS_CODE,
            Message = $"Payment request {entity.request_no} submitted.",
        };
    }

    public async Task<GeneralListResponseDto<PaymentApprovalListItemDto>> GetAdminListAsync(
        DataTableRequestDto request,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var page = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is <= 0 or > 200 ? 10 : request.PageSize;

        var query =
            from r in _context.tb_payment_request.AsNoTracking()
            join u in _context.um_user_app.AsNoTracking() on r.user_id equals u.user_id into userJoin
            from u in userJoin.DefaultIfEmpty()
            join p in _context.rf_plan.AsNoTracking() on r.plan_id equals p.id into planJoin
            from p in planJoin.DefaultIfEmpty()
            join inv in _context.tb_payment_invoice.AsNoTracking()
                .Where(e => e.isDeleted == false)
                on r.id equals inv.payment_request_id into invoiceJoin
            from inv in invoiceJoin.DefaultIfEmpty()
            where r.isDeleted == false
            select new { Request = r, User = u, Plan = p, Invoice = inv };

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Request.status == status);
        }

        var search = request.SearchTerm?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Request.request_no.Contains(search)
                || x.Request.user_id.Contains(search)
                || (x.User != null && x.User.full_name != null && x.User.full_name.Contains(search))
                || (x.User != null && x.User.email != null && x.User.email.Contains(search)));
        }

        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortColumn ?? string.Empty).ToLowerInvariant() switch
        {
            "userid" => descending
                ? query.OrderByDescending(x => x.Request.user_id)
                : query.OrderBy(x => x.Request.user_id),
            "requestno" => descending
                ? query.OrderByDescending(x => x.Request.request_no)
                : query.OrderBy(x => x.Request.request_no),
            "planname" => descending
                ? query.OrderByDescending(x => x.Plan != null ? x.Plan.plan_name : x.Request.plan_id)
                : query.OrderBy(x => x.Plan != null ? x.Plan.plan_name : x.Request.plan_id),
            "amount" => descending
                ? query.OrderByDescending(x => x.Request.amount)
                : query.OrderBy(x => x.Request.amount),
            "status" => descending
                ? query.OrderByDescending(x => x.Request.status)
                : query.OrderBy(x => x.Request.status),
            "createddate" => descending
                ? query.OrderByDescending(x => x.Request.createdDate)
                : query.OrderBy(x => x.Request.createdDate),
            _ => descending
                ? query.OrderByDescending(x => x.Request.createdDate)
                : query.OrderBy(x => x.Request.createdDate),
        };

        var total = await query.CountAsync(cancellationToken);

        var rows = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PaymentApprovalListItemDto
            {
                Id = x.Request.id,
                RequestNo = x.Request.request_no,
                UserId = x.Request.user_id,
                UserName = x.User != null ? x.User.full_name : null,
                UserEmail = x.User != null ? x.User.email : null,
                PlanId = x.Request.plan_id,
                PlanName = x.Plan != null ? x.Plan.plan_name : x.Request.plan_id,
                Amount = x.Request.amount,
                Currency = x.Request.currency,
                TransferReference = x.Request.transfer_reference,
                ProofFileName = x.Request.proof_file_name,
                ProofContentType = x.Request.proof_content_type,
                Status = x.Request.status,
                CreatedDate = x.Request.createdDate,
                ReviewedBy = x.Request.reviewed_by,
                ReviewedDate = x.Request.reviewed_date,
                ReviewNote = x.Request.review_note,
                ActivatedFrom = x.Request.activated_from,
                ActivatedTo = x.Request.activated_to,
                InvoiceId = x.Invoice != null ? x.Invoice.id : null,
                InvoiceNo = x.Invoice != null ? x.Invoice.invoice_no : null,
            })
            .ToListAsync(cancellationToken);

        return new GeneralListResponseDto<PaymentApprovalListItemDto>
        {
            Data = rows,
            TotalCount = total,
        };
    }

    public async Task<BaseGeneralResponse> ApproveAsync(
        long id,
        string? note,
        CancellationToken cancellationToken = default)
    {
        var actor = _currentUser.UserId ?? "system";

        var request = await _context.tb_payment_request
            .FirstOrDefaultAsync(e => e.id == id && e.isDeleted == false, cancellationToken);

        if (request == null)
        {
            return Fail("Payment request not found.");
        }

        if (!string.Equals(request.status, Const.PaymentStatus.PENDING, StringComparison.OrdinalIgnoreCase))
        {
            return Fail("Payment request is not pending.");
        }

        var plan = await _context.rf_plan
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.id == request.plan_id && e.isActive == true && e.isDeleted == false,
                cancellationToken);

        if (plan == null)
        {
            return Fail("Plan not found or inactive.");
        }

        if (string.IsNullOrWhiteSpace(plan.upgrade_group_id))
        {
            return Fail("Plan does not define a target group.");
        }

        var targetGroup = await _context.um_group_app
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.group_id == plan.upgrade_group_id && e.isActive != false && e.isDeleted != true,
                cancellationToken);

        if (targetGroup == null)
        {
            return Fail("Target group is not active.");
        }

        var currentPlan = await GetCurrentPlanAsync(request.user_id, cancellationToken);
        if (currentPlan != null)
        {
            if (string.Equals(currentPlan.id, plan.id, StringComparison.OrdinalIgnoreCase))
            {
                return Fail("User is already on this plan.");
            }

            if (plan.order_seq < currentPlan.order_seq)
            {
                return Fail("Plan downgrade cannot be approved through this flow. Contact an administrator if it is truly required.");
            }

            if (plan.order_seq == currentPlan.order_seq)
            {
                return Fail("This plan cannot be approved as an upgrade.");
            }
        }

        if (plan.price_amount <= 0)
        {
            return Fail("Free plans do not require payment approval.");
        }

        var now = DateTime.Now;

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        request.status = Const.PaymentStatus.APPROVED;
        request.reviewed_by = actor;
        request.reviewed_date = now;
        request.review_note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        request.activated_from = now;
        request.activated_to = CalculateActivatedTo(plan, now);

        var role = await _unitOfWork.UmUserGroup.GetActiveByIdAsync(request.user_id, cancellationToken);
        if (role == null)
        {
            role = new um_user_group
            {
                user_id = request.user_id,
                createdBy = actor,
                createdDate = now,
            };
            await _unitOfWork.UmUserGroup.AddAsync(role, cancellationToken);
        }

        role.group_id = plan.upgrade_group_id;
        role.can_add_edit = true;
        role.can_delete = true;
        role.can_read_only = false;
        role.isActive = true;
        role.isDeleted = false;
        role.statusApproval = Const.StatusApproval.APPROVED;
        role.approvedBy = actor;
        role.approvedDate = now;
        await _unitOfWork.UmUserGroup.UpdateAsync(role);

        var user = await _unitOfWork.UmUserApp.GetByIdAsync(request.user_id);
        if (user != null)
        {
            user.plan_id = plan.id;
            user.modifiedBy = actor;
            user.modifiedDate = now;
            await _unitOfWork.UmUserApp.UpdateAsync(user);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        // Seluruh langkah berikut berada di luar transaksi: kegagalan di sini
        // tidak boleh membatalkan aktivasi plan yang sudah sah.
        await NotifyUserAsync(
            request.user_id,
            "Payment request approved",
            $"Your plan upgrade to {plan.plan_name} has been approved.",
            "/dashboard/billing",
            cancellationToken);

        var invoice = await IssueInvoiceSafelyAsync(request.id, actor, cancellationToken);
        await QueueOutcomeEmailAsync(request, plan, invoice, approved: true, note, cancellationToken);
        await RevokeUserSessionAsync(request.user_id, cancellationToken);

        return SuccessKey("PaymentApproved", new[] { request.request_no }, $"Payment request {request.request_no} approved.");
    }

    public async Task<BaseGeneralResponse> RejectAsync(
        long id,
        string? note,
        CancellationToken cancellationToken = default)
    {
        var actor = _currentUser.UserId ?? "system";

        var request = await _context.tb_payment_request
            .FirstOrDefaultAsync(e => e.id == id && e.isDeleted == false, cancellationToken);

        if (request == null)
        {
            return Fail("Payment request not found.");
        }

        if (!string.Equals(request.status, Const.PaymentStatus.PENDING, StringComparison.OrdinalIgnoreCase))
        {
            return Fail("Payment request is not pending.");
        }

        var plan = await _context.rf_plan
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.id == request.plan_id && e.isDeleted == false,
                cancellationToken);

        var now = DateTime.Now;
        request.status = Const.PaymentStatus.REJECTED;
        request.reviewed_by = actor;
        request.reviewed_date = now;
        request.review_note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await NotifyUserAsync(
            request.user_id,
            "Payment request rejected",
            $"Your plan upgrade request for {plan?.plan_name ?? request.plan_id} has been rejected.",
            "/dashboard/billing",
            cancellationToken);

        await QueueOutcomeEmailAsync(request, plan, null, approved: false, note, cancellationToken);

        return SuccessKey("PaymentRejected", new[] { request.request_no }, $"Payment request {request.request_no} rejected.");
    }

    public async Task<PaymentProofDto?> GetProofAsync(
        long id,
        string requesterUserId,
        CancellationToken cancellationToken = default)
    {
        var request = await _context.tb_payment_request
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.id == id && e.isDeleted == false, cancellationToken);

        if (request == null || string.IsNullOrWhiteSpace(request.proof_stored_path))
        {
            return null;
        }

        if (!await CanAccessProofAsync(request.user_id, requesterUserId, cancellationToken))
        {
            // Sembunyikan keberadaan file bila pemohon tidak berhak.
            return null;
        }

        var absolutePath = Path.IsPathRooted(request.proof_stored_path)
            ? request.proof_stored_path
            : Path.Combine(
                _environment.ContentRootPath,
                request.proof_stored_path.Replace('/', Path.DirectorySeparatorChar));

        if (!File.Exists(absolutePath))
        {
            return null;
        }

        return new PaymentProofDto
        {
            RequestId = request.id,
            FileName = request.proof_file_name,
            ContentType = string.IsNullOrWhiteSpace(request.proof_content_type)
                ? "application/octet-stream"
                : request.proof_content_type,
            SizeBytes = request.proof_size_bytes,
            AbsolutePath = absolutePath,
        };
    }

    /// <summary>Bukti bayar hanya boleh diakses pemilik pengajuan atau Administrator.</summary>
    private async Task<bool> CanAccessProofAsync(
        string ownerUserId,
        string requesterUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(requesterUserId))
        {
            return false;
        }

        if (string.Equals(ownerUserId, requesterUserId, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var role = await _unitOfWork.UmUserGroup.GetActiveByIdAsync(requesterUserId, cancellationToken);
        return role != null
               && string.Equals(role.group_id, Const.DEFAULT_GROUP_ID_ADMINISTRATOR_CODE, StringComparison.Ordinal);
    }

    /// <summary>
    /// Menentukan akhir masa aktif sesuai periode billing plan.
    /// ONE_TIME tidak berujung, MONTHLY/YEARLY dihitung dari jumlah interval.
    /// </summary>
    private static DateTime? CalculateActivatedTo(rf_plan plan, DateTime now)
    {
        var interval = plan.billing_interval_count <= 0 ? 1 : plan.billing_interval_count;

        if (string.Equals(plan.billing_period, Const.BillingPeriod.MONTHLY, StringComparison.OrdinalIgnoreCase))
        {
            return now.AddMonths(interval);
        }

        if (string.Equals(plan.billing_period, Const.BillingPeriod.YEARLY, StringComparison.OrdinalIgnoreCase))
        {
            return now.AddYears(interval);
        }

        return null;
    }

    private async Task NotifyUserAsync(
        string userId,
        string shortText,
        string longText,
        string href,
        CancellationToken cancellationToken)
    {
        try
        {
            await _notificationService.CreateAsync(
                userId,
                shortText,
                longText,
                href,
                NotificationTypeIds.PaymentRequest,
                cancellationToken);
        }
        catch (Exception ex)
        {
            await _errorLog.LogAsync("PaymentApproval", "ERROR", ex);
        }
    }

    /// <summary>
    /// Menerbitkan invoice di luar transaksi. Bila gagal, aktivasi plan tetap sah
    /// dan kegagalan dicatat agar admin dapat menerbitkan ulang.
    /// </summary>
    private async Task<InvoiceIssueResultDto?> IssueInvoiceSafelyAsync(
        long paymentRequestId,
        string actor,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _invoiceService.IssueAsync(paymentRequestId, actor, false, cancellationToken);
            if (!result.Success)
            {
                await _errorLog.LogAsync(
                    "Invoice",
                    "ERROR",
                    new InvalidOperationException(result.Message ?? "Invoice issuance failed."));
                return null;
            }

            return result;
        }
        catch (Exception ex)
        {
            await _errorLog.LogAsync("Invoice", "ERROR", ex);
            return null;
        }
    }

    private async Task QueueOutcomeEmailAsync(
        tb_payment_request request,
        rf_plan? plan,
        InvoiceIssueResultDto? invoice,
        bool approved,
        string? note,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UmUserApp.GetByIdAsync(request.user_id);
        if (user == null || string.IsNullOrWhiteSpace(user.email))
        {
            return;
        }

        try
        {
            var templateKey = approved
                ? EmailTemplateKeys.PaymentApproved
                : EmailTemplateKeys.PaymentRejected;

            var template = await _unitOfWork.EmailTemplates.GetActiveByKeyAsync(templateKey, cancellationToken);

            var planName = plan?.plan_name ?? request.plan_id;
            var subject = template?.subject
                          ?? (approved ? "Plan upgrade approved" : "Plan upgrade rejected");
            var bodyHtml = template?.body_html
                           ?? "<p>Hello {{full_name}},</p><p>Your plan upgrade request ({{request_no}}) for {{plan_name}} has been processed.</p>";
            var bodyText = template?.body_text
                           ?? "Hello {{full_name}}, your plan upgrade request ({{request_no}}) for {{plan_name}} has been processed.";

            var fullName = string.IsNullOrWhiteSpace(user.full_name) ? user.user_id : user.full_name;
            var longText = approved
                ? $"Your plan upgrade to {planName} has been approved."
                : (string.IsNullOrWhiteSpace(note)
                    ? "The request was rejected without additional notes."
                    : note.Trim());
            var href = "/dashboard/billing";

            var values = new PaymentTemplateValues
            {
                FullName = fullName,
                UserId = request.user_id,
                RequestNo = request.request_no,
                PlanName = planName,
                Amount = FormatAmount(request.amount, request.currency),
                Status = request.status,
                InvoiceNo = invoice?.InvoiceNo ?? string.Empty,
                LongText = longText,
                Link = href,
            };

            subject = ApplyTemplate(subject, values);
            bodyHtml = ApplyTemplate(bodyHtml, values);
            bodyText = ApplyTemplate(bodyText, values);

            EmailAttachmentDto? attachment = null;
            if (invoice != null && !invoice.AlreadyIssued)
            {
                attachment = new EmailAttachmentDto
                {
                    FileName = invoice.FileName,
                    ContentType = invoice.ContentType,
                    StoredPath = invoice.StoredPath,
                };
            }

            await _emailDispatch.QueueAsync(
                user.email,
                subject,
                bodyHtml,
                bodyText,
                "PAYMENT_REQUEST",
                request.request_no,
                attachment,
                cancellationToken);
        }
        catch (Exception ex)
        {
            await _errorLog.LogAsync("PaymentApproval", "ERROR", ex);
        }
    }

    /// <summary>
    /// Mencabut sesi aktif user supaya role baru (grup hasil upgrade) langsung berlaku.
    /// JWT yang sedang aktif masih memuat grup lama sampai kedaluwarsa.
    /// </summary>
    private async Task RevokeUserSessionAsync(string userId, CancellationToken cancellationToken)
    {
        try
        {
            var revokedSessionId = await _unitOfWork.UmUserAppFlag.RevokeSessionAsync(userId, cancellationToken);
            await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(userId);
            await _unitOfWork.UmUserAppSession.RemoveAsync(userId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(revokedSessionId))
            {
                await _sessionNotifier.NotifySessionRevokedAsync(userId, revokedSessionId);
            }
        }
        catch (Exception ex)
        {
            await _errorLog.LogAsync("PaymentApproval", "ERROR", ex);
        }
    }

    private static string FormatAmount(decimal amount, string currency)
        => $"{currency} {amount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("id-ID"))}";

    private async Task<string> GenerateRequestNoAsync(CancellationToken cancellationToken)
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"PAY-{datePart}-";

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var countToday = await _context.tb_payment_request.CountAsync(
                e => e.request_no.StartsWith(prefix),
                cancellationToken);
            var candidate = $"{prefix}{countToday + attempt + 1:D3}";
            var exists = await _context.tb_payment_request.AnyAsync(
                e => e.request_no == candidate,
                cancellationToken);
            if (!exists)
            {
                return candidate;
            }
        }

        return $"{prefix}{Guid.NewGuid():N}"[..24];
    }

    private async Task NotifyAdministratorsAsync(
        tb_payment_request request,
        string planName,
        CancellationToken cancellationToken)
    {
        var adminIds = await _unitOfWork.UmUserGroup.GetActiveAdministratorUserIdsAsync(cancellationToken);
        foreach (var adminId in adminIds.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            await _notificationService.CreateAsync(
                adminId,
                "Payment request submitted",
                $"Payment request {request.request_no} for plan {planName} is waiting for review.",
                "/dashboard/maintenance/payment-approval",
                NotificationTypeIds.PaymentRequest,
                cancellationToken);
        }
    }

    private async Task QueueApplicantEmailAsync(
        string userId,
        tb_payment_request request,
        string planName,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UmUserApp.GetByIdAsync(userId);
        if (user == null || string.IsNullOrWhiteSpace(user.email))
        {
            return;
        }

        var template = await _unitOfWork.EmailTemplates.GetActiveByKeyAsync(
            EmailTemplateKeys.PaymentReceived,
            cancellationToken);

        var subject = template?.subject ?? "Payment request received";
        var bodyHtml = template?.body_html
                       ?? "<p>Hi {{full_name}},</p><p>We received your payment request {{request_no}} for plan {{plan_name}} ({{amount}}).</p><p>Status: {{status}}.</p>";
        var bodyText = template?.body_text
                       ?? "Hi {{full_name}}, we received your payment request {{request_no}} for plan {{plan_name}} ({{amount}}). Status: {{status}}.";

        var values = new PaymentTemplateValues
        {
            FullName = user.full_name ?? user.user_id,
            UserId = request.user_id,
            RequestNo = request.request_no,
            PlanName = planName,
            Amount = FormatAmount(request.amount, request.currency),
            Status = request.status,
            InvoiceNo = string.Empty,
            LongText = string.Empty,
            Link = "/dashboard/billing",
        };

        bodyHtml = ApplyTemplate(bodyHtml, values);
        bodyText = ApplyTemplate(bodyText, values);
        subject = ApplyTemplate(subject, values);

        await _emailDispatch.QueueAsync(
            user.email,
            subject,
            bodyHtml,
            bodyText,
            "PAYMENT_REQUEST",
            request.request_no,
            null,
            cancellationToken);
    }

    private static string ApplyTemplate(string value, PaymentTemplateValues values)
        => value
            .Replace("{{full_name}}", values.FullName, StringComparison.OrdinalIgnoreCase)
            .Replace("{{user_id}}", values.UserId, StringComparison.OrdinalIgnoreCase)
            .Replace("{{request_no}}", values.RequestNo, StringComparison.OrdinalIgnoreCase)
            .Replace("{{plan_name}}", values.PlanName, StringComparison.OrdinalIgnoreCase)
            .Replace("{{amount}}", values.Amount, StringComparison.OrdinalIgnoreCase)
            .Replace("{{status}}", values.Status, StringComparison.OrdinalIgnoreCase)
            .Replace("{{invoice_no}}", values.InvoiceNo, StringComparison.OrdinalIgnoreCase)
            .Replace("{{short_text}}", values.LongText, StringComparison.OrdinalIgnoreCase)
            .Replace("{{long_text}}", values.LongText, StringComparison.OrdinalIgnoreCase)
            .Replace("{{link}}", values.Link, StringComparison.OrdinalIgnoreCase);

    /// <summary>Nilai placeholder untuk template email pengajuan pembayaran.</summary>
    private sealed class PaymentTemplateValues
    {
        public string FullName { get; init; } = string.Empty;
        public string UserId { get; init; } = string.Empty;
        public string RequestNo { get; init; } = string.Empty;
        public string PlanName { get; init; } = string.Empty;
        public string Amount { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string InvoiceNo { get; init; } = string.Empty;
        public string LongText { get; init; } = string.Empty;
        public string Link { get; init; } = string.Empty;
    }

    private async Task<rf_plan?> GetCurrentPlanAsync(string userId, CancellationToken cancellationToken)
    {
        var groupId = await _context.um_user_group
            .AsNoTracking()
            .Where(e => e.user_id == userId
                        && e.isActive == true
                        && e.isDeleted == false)
            .Select(e => e.group_id)
            .FirstOrDefaultAsync(cancellationToken);

        rf_plan? plan = null;
        if (!string.IsNullOrWhiteSpace(groupId))
        {
            plan = await _context.rf_plan
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    e => e.upgrade_group_id == groupId
                         && e.isActive == true
                         && e.isDeleted == false,
                    cancellationToken);
        }

        return plan ?? await _context.rf_plan
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.isDefault == true
                     && e.isActive == true
                     && e.isDeleted == false,
                cancellationToken);
    }

    private static BaseGeneralResponse Success(string message) => new()
    {
        Success = true,
        Code = Const.SUCCESS_CODE,
        Message = message,
    };

    private static BaseGeneralResponse SuccessKey(string messageKey, string[] args, string message) => new()
    {
        Success = true,
        Code = Const.SUCCESS_CODE,
        Message = message,
        MessageKey = messageKey,
        MessageArgs = args,
    };

    private static BaseGeneralResponse Fail(string message) => new()
    {
        Success = false,
        Code = Const.ERROR_CODE,
        Message = message,
    };

    private static BaseGeneralResponse FailKey(string messageKey, string message) => new()
    {
        Success = false,
        Code = Const.ERROR_CODE,
        Message = message,
        MessageKey = messageKey,
    };
}
