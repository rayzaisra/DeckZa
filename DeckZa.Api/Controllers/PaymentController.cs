using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using DeckZa.Api.Helpers;
using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Payments;
using DeckZa.Application.Interfaces;

namespace DeckZa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ICurrentUserService _currentUser;

    public PaymentController(IPaymentService paymentService, ICurrentUserService currentUser)
    {
        _paymentService = paymentService;
        _currentUser = currentUser;
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
}
