using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Components.Forms;

using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Payments;

namespace DeckZa.Shared.Services;

public class PaymentApiService
{
    private readonly ApiService _api;

    public PaymentApiService(ApiService api)
    {
        _api = api;
    }

    public Task<PaymentInstructionDto?> GetInstructionsAsync(CancellationToken cancellationToken = default)
        => _api.GetFromJsonAsync<PaymentInstructionDto>("api/payment/instructions", unwrap: true, cancellationToken);

    public async Task<List<PaymentRequestDto>> GetMyRequestsAsync(CancellationToken cancellationToken = default)
        => await _api.GetFromJsonAsync<List<PaymentRequestDto>>("api/payment/my-requests", unwrap: true, cancellationToken)
           ?? new List<PaymentRequestDto>();

    public async Task<BaseGeneralResponse> SubmitAsync(
        string planId,
        string? transferReference,
        IBrowserFile file,
        CancellationToken cancellationToken = default)
    {
        var response = await _api.PostMultipartAsync(
            "api/payment/request",
            content =>
            {
                content.Add(new StringContent(planId), "planId");
                if (!string.IsNullOrWhiteSpace(transferReference))
                {
                    content.Add(new StringContent(transferReference), "transferReference");
                }

                var fileContent = new StreamContent(file.OpenReadStream(2 * 1024 * 1024, cancellationToken));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                    string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
                content.Add(fileContent, "file", file.Name);
            },
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<BaseGeneralResponse>(cancellationToken: cancellationToken)
               ?? new BaseGeneralResponse
               {
                   Success = false,
                   Code = "99",
                   Message = "Payment submission failed.",
               };
    }
}
