using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using DeckZa.Api.Authorization;
using DeckZa.Api.Helpers;
using DeckZa.Application.Common;
using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Payments;
using DeckZa.Application.Interfaces;
using DeckZa.Domain.Enums;

namespace DeckZa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ICurrentUserService _currentUser;
    private readonly IInvoiceService _invoiceService;

    public PaymentController(
        IPaymentService paymentService,
        ICurrentUserService currentUser,
        IInvoiceService invoiceService)
    {
        _paymentService = paymentService;
        _currentUser = currentUser;
        _invoiceService = invoiceService;
    }

    [HttpGet("instructions")]
    public async Task<IActionResult> GetInstructions(CancellationToken cancellationToken)
    {
        var instructions = await _paymentService.GetInstructionsAsync(cancellationToken);
        return Ok(ApiResponseFactory.Success(instructions, "Payment instructions retrieved successfully"));
    }

    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyRequests(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponseFactory.Error<List<PaymentRequestDto>>("User session is not valid."));
        }

        var requests = await _paymentService.GetMyRequestsAsync(userId, cancellationToken);
        return Ok(ApiResponseFactory.Success(requests, "Payment requests retrieved successfully"));
    }

    [HttpPost("request")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    public async Task<IActionResult> Submit([FromForm] PaymentSubmitForm form, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponseFactory.Error<BaseGeneralResponse>("User session is not valid."));
        }

        if (form.File == null || form.File.Length == 0)
        {
            return BadRequest(ApiResponseFactory.Error<BaseGeneralResponse>("Proof file is required."));
        }

        await using var stream = form.File.OpenReadStream();
        var result = await _paymentService.SubmitAsync(
            userId,
            form.PlanId ?? string.Empty,
            form.TransferReference,
            form.File.FileName,
            form.File.ContentType,
            form.File.Length,
            stream,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(ApiResponseFactory.Error<BaseGeneralResponse>(result.Message ?? "Payment submission failed."));
        }

        return Ok(ApiResponseFactory.Success(result, result.Message ?? "Payment request submitted."));
    }

    public sealed class PaymentSubmitForm
    {
        public string? PlanId { get; set; }
        public string? TransferReference { get; set; }
        public IFormFile? File { get; set; }
    }

    /// <summary>
    /// Daftar pengajuan pembayaran untuk halaman maintenance.
    /// <paramref name="status"/> kosong berarti semua status.
    /// </summary>
    [HttpPost("list")]
    [RequireMenuAccess(PermissionAction.Load, "MNT-PAY-LST")]
    public async Task<IActionResult> List(
        [FromBody] DataTableRequestDto request,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetAdminListAsync(request, status, cancellationToken);
        return Ok(ApiResponseFactory.Page(result.Data, request, result.TotalCount, "Payment requests retrieved successfully."));
    }

    [HttpPost("{id:long}/approve")]
    [RequireMenuAccess(PermissionAction.Approve, "MNT-PAY-APR")]
    public async Task<IActionResult> Approve(
        long id,
        [FromBody] PaymentReviewForm? form,
        CancellationToken cancellationToken)
    {
        var result = await _paymentService.ApproveAsync(id, form?.Note, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(ApiResponseFactory.Error<BaseGeneralResponse>(result.Message ?? "Payment approval failed."));
        }

        return Ok(ApiResponseFactory.Success(result, result.Message ?? "Payment request approved."));
    }

    [HttpPost("{id:long}/reject")]
    [RequireMenuAccess(PermissionAction.Reject, "MNT-PAY-APR")]
    public async Task<IActionResult> Reject(
        long id,
        [FromBody] PaymentReviewForm? form,
        CancellationToken cancellationToken)
    {
        var result = await _paymentService.RejectAsync(id, form?.Note, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(ApiResponseFactory.Error<BaseGeneralResponse>(result.Message ?? "Payment rejection failed."));
        }

        return Ok(ApiResponseFactory.Success(result, result.Message ?? "Payment request rejected."));
    }

    /// <summary>
    /// Bukti bayar. Hanya pemilik pengajuan atau Administrator yang boleh.
    /// <c>?download=true</c> memakai Content-Disposition attachment.
    /// </summary>
    [HttpGet("proof/{id:long}")]
    public async Task<IActionResult> Proof(long id, [FromQuery] bool download = false, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponseFactory.Error<BaseGeneralResponse>("User session is not valid."));
        }

        var proof = await _paymentService.GetProofAsync(id, userId, cancellationToken);
        if (proof == null)
        {
            return NotFound(ApiResponseFactory.Error<BaseGeneralResponse>("Proof file not found."));
        }

        var fileName = string.IsNullOrWhiteSpace(proof.FileName) ? $"proof-{id}" : proof.FileName;
        var disposition = download ? "attachment" : "inline";
        Response.Headers.ContentDisposition = $"{disposition}; filename=\"{fileName}\"";

        return PhysicalFile(proof.AbsolutePath, proof.ContentType, enableRangeProcessing: true);
    }

    /// <summary>Terbitkan ulang invoice bila render PDF sempat gagal.</summary>
    [HttpPost("{id:long}/reissue-invoice")]
    [RequireMenuAccess(PermissionAction.Edit, "MNT-PAY-APR")]
    public async Task<IActionResult> ReissueInvoice(long id, CancellationToken cancellationToken)
    {
        var actor = _currentUser.UserId ?? "system";
        var result = await _invoiceService.IssueAsync(id, actor, true, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(ApiResponseFactory.Error<BaseGeneralResponse>(result.Message ?? "Invoice reissue failed."));
        }

        return Ok(ApiResponseFactory.Success(result, result.Message ?? "Invoice reissued."));
    }

    /// <summary>Daftar invoice milik user yang sedang login.</summary>
    [HttpGet("invoices")]
    public async Task<IActionResult> GetMyInvoices(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponseFactory.Error<List<PaymentInvoiceDto>>("User session is not valid."));
        }

        var invoices = await _invoiceService.GetMyInvoicesAsync(userId, cancellationToken);
        return Ok(ApiResponseFactory.Success(invoices, "Invoices retrieved successfully"));
    }

    /// <summary>
    /// Unduh invoice PDF. Pemilik atau Administrator saja; user lain mendapat 403.
    /// </summary>
    [HttpGet("invoice/{id:long}")]
    public async Task<IActionResult> GetInvoice(long id, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponseFactory.Error<BaseGeneralResponse>("User session is not valid."));
        }

        var isAdministrator = User.IsInRole(Const.DEFAULT_GROUP_ID_ADMINISTRATOR_CODE);
        var result = await _invoiceService.GetPdfFileAsync(id, userId, isAdministrator, cancellationToken);

        if (result.Forbidden)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponseFactory.Error<BaseGeneralResponse>(result.Message ?? "Access denied."));
        }

        if (!result.Success || result.File == null)
        {
            return NotFound(ApiResponseFactory.Error<BaseGeneralResponse>(result.Message ?? "Invoice not found."));
        }

        return PhysicalFile(result.File.AbsolutePath, result.File.ContentType, result.File.FileName);
    }

    public sealed class PaymentReviewForm
    {
        public string? Note { get; set; }
    }
}
