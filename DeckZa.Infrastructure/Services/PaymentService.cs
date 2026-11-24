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

    public PaymentService(
        DeckZaDbContext context,
        IUnitOfWork unitOfWork,
        IEmailDispatchService emailDispatch,
        INotificationService notificationService,
        IHostEnvironment environment)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _emailDispatch = emailDispatch;
        _notificationService = notificationService;
        _environment = environment;
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

        var hasPending = await _context.tb_payment_request.AnyAsync(
            e => e.user_id == userId
                 && e.plan_id == planId
                 && e.status == "PENDING"
                 && e.isDeleted == false,
            cancellationToken);
        if (hasPending)
        {
            return Fail("Anda masih memiliki pengajuan pembayaran yang sedang menunggu review untuk plan ini.");
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

        bodyHtml = ApplyTemplate(bodyHtml, user.full_name ?? user.user_id, request, planName);
        bodyText = ApplyTemplate(bodyText, user.full_name ?? user.user_id, request, planName);
        subject = ApplyTemplate(subject, user.full_name ?? user.user_id, request, planName);

        await _emailDispatch.QueueAsync(
            user.email,
            subject,
            bodyHtml,
            bodyText,
            "PAYMENT_REQUEST",
            request.request_no,
            cancellationToken);
    }

    private static string ApplyTemplate(string value, string fullName, tb_payment_request request, string planName)
    {
        var amount = $"{request.currency} {request.amount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("id-ID"))}";
        return value
            .Replace("{{full_name}}", fullName, StringComparison.OrdinalIgnoreCase)
            .Replace("{{request_no}}", request.request_no, StringComparison.OrdinalIgnoreCase)
            .Replace("{{plan_name}}", planName, StringComparison.OrdinalIgnoreCase)
            .Replace("{{amount}}", amount, StringComparison.OrdinalIgnoreCase)
            .Replace("{{status}}", request.status, StringComparison.OrdinalIgnoreCase);
    }

    private static BaseGeneralResponse Fail(string message) => new()
    {
        Success = false,
        Code = Const.ERROR_CODE,
        Message = message,
    };
}
