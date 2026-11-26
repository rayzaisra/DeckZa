using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Components.Forms;

using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Payments;
using DeckZa.Shared.Services.Interfaces;

namespace DeckZa.Shared.Services;

public class PaymentApiService
{
    private readonly ApiService _api;
    private readonly ITokenStorage _tokenStorage;

    public PaymentApiService(ApiService api, ITokenStorage tokenStorage)
    {
        _api = api;
        _tokenStorage = tokenStorage;
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

    /// <summary>Daftar pengajuan pembayaran untuk halaman maintenance.</summary>
    public async Task<PaginatedResponse<PaymentApprovalListItemDto>> GetAdminListAsync(
        DataTableRequestDto request,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/payment/list?status={Uri.EscapeDataString(status ?? string.Empty)}";
        return await _api.PostRawFromJsonAsync<DataTableRequestDto, PaginatedResponse<PaymentApprovalListItemDto>>(
                   url,
                   request,
                   cancellationToken)
               ?? new PaginatedResponse<PaymentApprovalListItemDto>();
    }

    public async Task<BaseGeneralResponse> ApproveAsync(
        long id,
        string? note,
        CancellationToken cancellationToken = default)
    {
        var response = await _api.PostAsync($"api/payment/{id}/approve", new { note }, cancellationToken);
        return await ReadAsync(response, cancellationToken);
    }

    public async Task<BaseGeneralResponse> RejectAsync(
        long id,
        string? note,
        CancellationToken cancellationToken = default)
    {
        var response = await _api.PostAsync($"api/payment/{id}/reject", new { note }, cancellationToken);
        return await ReadAsync(response, cancellationToken);
    }

    public Task<List<PaymentInvoiceDto>?> GetMyInvoicesAsync(CancellationToken cancellationToken = default)
        => _api.GetFromJsonAsync<List<PaymentInvoiceDto>>("api/payment/invoices", unwrap: true, cancellationToken);

    /// <summary>
    /// URL bukti bayar untuk pratinjau (inline) atau unduhan (attachment).
    /// Memakai token pada query string agar bisa dipakai &lt;img&gt;/&lt;iframe&gt;.
    /// </summary>
    public async Task<string?> GetProofUrlAsync(long id, bool download = false, CancellationToken cancellationToken = default)
    {
        var token = await _tokenStorage.GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var baseAddress = _api.ApiBaseAddress;
        if (string.IsNullOrWhiteSpace(baseAddress))
        {
            return null;
        }

        var suffix = download ? "&download=true" : string.Empty;
        return $"{baseAddress}/api/payment/proof/{id}?access_token={Uri.EscapeDataString(token)}{suffix}";
    }

    /// <summary>
    /// URL unduhan invoice PDF. Memakai token pada query string agar bisa dibuka
    /// lewat navigasi browser, konsisten dengan pola unduhan export/proof.
    /// </summary>
    public async Task<string?> GetInvoiceDownloadUrlAsync(long invoiceId, CancellationToken cancellationToken = default)
    {
        var token = await _tokenStorage.GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var baseAddress = _api.ApiBaseAddress;
        if (string.IsNullOrWhiteSpace(baseAddress))
        {
            return null;
        }

        return $"{baseAddress}/api/payment/invoice/{invoiceId}?access_token={Uri.EscapeDataString(token)}";
    }

    private static async Task<BaseGeneralResponse> ReadAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<BaseGeneralResponse>>(cancellationToken: cancellationToken);
        if (payload == null)
        {
            return new BaseGeneralResponse
            {
                Success = false,
                Code = "99",
                Message = "Unknown error.",
            };
        }

        return new BaseGeneralResponse
        {
            Success = payload.Success,
            Code = payload.Code,
            Message = payload.Message,
        };
    }
}
