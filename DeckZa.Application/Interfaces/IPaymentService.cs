using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Payments;

namespace DeckZa.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentInstructionDto> GetInstructionsAsync(CancellationToken cancellationToken = default);
    Task<List<PaymentRequestDto>> GetMyRequestsAsync(string userId, CancellationToken cancellationToken = default);
    Task<BaseGeneralResponse> SubmitAsync(
        string userId,
        string planId,
        string? transferReference,
        string fileName,
        string contentType,
        long fileSizeBytes,
        Stream fileStream,
        CancellationToken cancellationToken = default);
}
