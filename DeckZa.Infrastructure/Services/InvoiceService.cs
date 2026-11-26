using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

using DeckZa.Application.DTOs.Payments;
using DeckZa.Application.Interfaces;
using DeckZa.Domain.Entities;
using DeckZa.Domain.Enums;
using DeckZa.Infrastructure.Data;

namespace DeckZa.Infrastructure.Services;

/// <summary>
/// Menerbitkan invoice PDF untuk pengajuan pembayaran yang sudah disetujui.
/// Dijalankan di luar transaksi approve — kegagalan render tidak membatalkan
/// aktivasi plan, dan admin dapat menerbitkan ulang lewat endpoint reissue.
/// </summary>
public class InvoiceService : IInvoiceService
{
    private const string InvoicesFolder = "invoices";

    private readonly DeckZaDbContext _context;
    private readonly IInvoiceRenderer _renderer;
    private readonly IHostEnvironment _environment;

    public InvoiceService(
        DeckZaDbContext context,
        IInvoiceRenderer renderer,
        IHostEnvironment environment)
    {
        _context = context;
        _renderer = renderer;
        _environment = environment;
    }

    public async Task<InvoiceIssueResultDto> IssueAsync(
        long paymentRequestId,
        string actor,
        bool reissue = false,
        CancellationToken cancellationToken = default)
    {
        var request = await _context.tb_payment_request
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.id == paymentRequestId && e.isDeleted == false,
                cancellationToken);

        if (request == null)
        {
            return Fail("Payment request not found.");
        }

        if (!string.Equals(request.status, Const.PaymentStatus.APPROVED, StringComparison.OrdinalIgnoreCase))
        {
            return Fail("Invoice can only be issued for an approved payment request.");
        }

        var existing = await _context.tb_payment_invoice
            .FirstOrDefaultAsync(
                e => e.payment_request_id == paymentRequestId && e.isDeleted == false,
                cancellationToken);

        if (existing != null && !reissue)
        {
            // Idempoten: approve kedua tidak menghasilkan invoice kedua.
            return new InvoiceIssueResultDto
            {
                Success = true,
                Message = "Invoice has already been issued.",
                InvoiceId = existing.id,
                InvoiceNo = existing.invoice_no,
                FileName = existing.file_name,
                StoredPath = existing.stored_path,
                ContentType = existing.content_type,
                SizeBytes = existing.size_bytes,
                AlreadyIssued = true,
            };
        }

        var user = await _context.um_user_app
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.user_id == request.user_id && e.isDeleted == false,
                cancellationToken);

        var plan = await _context.rf_plan
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.id == request.plan_id && e.isDeleted == false,
                cancellationToken);

        // Reissue atas invoice yang sudah ada mempertahankan nomor & nama berkas:
        // nomor invoice tetap stabil dan tidak meninggalkan berkas PDF yatim di server.
        var invoiceNo = existing != null
            ? existing.invoice_no
            : await GenerateInvoiceNoAsync(cancellationToken);
        var model = new InvoiceRenderModel
        {
            InvoiceNo = invoiceNo,
            InvoiceDate = DateTime.Now,
            UserId = request.user_id,
            FullName = user?.full_name ?? request.user_id,
            Email = user?.email ?? string.Empty,
            PlanName = plan?.plan_name ?? request.plan_id,
            Amount = request.amount,
            Currency = string.IsNullOrWhiteSpace(request.currency) ? "IDR" : request.currency,
            BillingPeriod = plan?.billing_period ?? Const.BillingPeriod.ONE_TIME,
            ActivatedFrom = request.activated_from,
            ActivatedTo = request.activated_to,
            RequestNo = request.request_no,
            PaymentMethod = string.IsNullOrWhiteSpace(request.payment_method) ? "TRANSFER" : request.payment_method,
            TransferReference = request.transfer_reference,
        };

        var pdf = _renderer.Render(model);

        var fileName = existing != null && !string.IsNullOrWhiteSpace(existing.file_name)
            ? existing.file_name
            : $"{invoiceNo.Replace('/', '-')}.pdf";
        var relativePath = Path.Combine(InvoicesFolder, request.user_id, fileName).Replace('\\', '/');
        var absolutePath = Path.Combine(_environment.ContentRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        await File.WriteAllBytesAsync(absolutePath, pdf, cancellationToken);

        var now = DateTime.Now;
        long invoiceId;

        if (existing != null)
        {
            existing.invoice_no = invoiceNo;
            existing.plan_name = model.PlanName;
            existing.amount = request.amount;
            existing.currency = model.Currency;
            existing.billing_period = model.BillingPeriod;
            existing.invoice_date = model.InvoiceDate;
            existing.activated_from = request.activated_from;
            existing.activated_to = request.activated_to;
            existing.status = Const.InvoiceStatus.ISSUED;
            existing.file_name = fileName;
            existing.stored_path = relativePath;
            existing.content_type = "application/pdf";
            existing.size_bytes = pdf.LongLength;
            existing.modifiedBy = actor;
            existing.modifiedDate = now;

            await _context.SaveChangesAsync(cancellationToken);
            invoiceId = existing.id;
        }
        else
        {
            var entity = new tb_payment_invoice
            {
                invoice_no = invoiceNo,
                payment_request_id = request.id,
                user_id = request.user_id,
                plan_id = request.plan_id,
                plan_name = model.PlanName,
                amount = request.amount,
                currency = model.Currency,
                billing_period = model.BillingPeriod,
                invoice_date = model.InvoiceDate,
                activated_from = request.activated_from,
                activated_to = request.activated_to,
                status = Const.InvoiceStatus.ISSUED,
                file_name = fileName,
                stored_path = relativePath,
                content_type = "application/pdf",
                size_bytes = pdf.LongLength,
                statusApproval = Const.StatusApproval.APPROVED,
                isActive = true,
                isDeleted = false,
                createdBy = actor,
                createdDate = now,
            };

            await _context.tb_payment_invoice.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            invoiceId = entity.id;
        }

        return new InvoiceIssueResultDto
        {
            Success = true,
            Message = "Invoice issued.",
            InvoiceId = invoiceId,
            InvoiceNo = invoiceNo,
            FileName = fileName,
            StoredPath = relativePath,
            ContentType = "application/pdf",
            SizeBytes = pdf.LongLength,
        };
    }

    public async Task<List<PaymentInvoiceDto>> GetMyInvoicesAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new List<PaymentInvoiceDto>();
        }

        return await _context.tb_payment_invoice
            .AsNoTracking()
            .Where(e => e.user_id == userId && e.isDeleted == false)
            .OrderByDescending(e => e.invoice_date)
            .ThenByDescending(e => e.id)
            .Select(e => new PaymentInvoiceDto
            {
                Id = e.id,
                InvoiceNo = e.invoice_no,
                InvoiceDate = e.invoice_date,
                PlanId = e.plan_id,
                PlanName = e.plan_name,
                Amount = e.amount,
                Currency = e.currency,
                Status = e.status,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentInvoiceFileResultDto> GetPdfFileAsync(
        long invoiceId,
        string requesterUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        var invoice = await _context.tb_payment_invoice
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.id == invoiceId && e.isDeleted == false, cancellationToken);

        if (invoice == null)
        {
            return PaymentInvoiceFileResultDto.NotFound();
        }

        if (!isAdministrator
            && !string.Equals(invoice.user_id, requesterUserId, StringComparison.OrdinalIgnoreCase))
        {
            return PaymentInvoiceFileResultDto.Denied();
        }

        var absolutePath = Path.IsPathRooted(invoice.stored_path)
            ? invoice.stored_path
            : Path.Combine(
                _environment.ContentRootPath,
                invoice.stored_path.Replace('/', Path.DirectorySeparatorChar));

        if (!File.Exists(absolutePath))
        {
            return PaymentInvoiceFileResultDto.NotFound("Invoice file not found.");
        }

        return PaymentInvoiceFileResultDto.Ok(new PaymentInvoiceFileDto
        {
            InvoiceId = invoice.id,
            FileName = string.IsNullOrWhiteSpace(invoice.file_name) ? $"invoice-{invoice.id}.pdf" : invoice.file_name,
            ContentType = string.IsNullOrWhiteSpace(invoice.content_type) ? "application/pdf" : invoice.content_type,
            AbsolutePath = absolutePath,
        });
    }

    /// <summary>
    /// Nomor invoice berformat <c>INV/{yyyyMM}/{seq:0000}</c>.
    /// Sequence dihitung per bulan; kandidat yang sudah dipakai dilewati.
    /// </summary>
    private async Task<string> GenerateInvoiceNoAsync(CancellationToken cancellationToken)
    {
        var monthPrefix = DateTime.Now.ToString("yyyyMM");

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var used = await _context.tb_payment_invoice.CountAsync(
                e => e.invoice_no.StartsWith($"INV/{monthPrefix}/"),
                cancellationToken);

            var candidate = $"INV/{monthPrefix}/{(used + attempt + 1):D4}";
            var exists = await _context.tb_payment_invoice.AnyAsync(
                e => e.invoice_no == candidate,
                cancellationToken);

            if (!exists)
            {
                return candidate;
            }
        }

        return $"INV/{monthPrefix}/{Guid.NewGuid():N}"[..20];
    }

    private static InvoiceIssueResultDto Fail(string message) => new()
    {
        Success = false,
        Message = message,
    };
}
