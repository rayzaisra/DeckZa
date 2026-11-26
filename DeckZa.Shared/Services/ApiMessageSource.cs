namespace DeckZa.Shared.Services;

/// <summary>
/// Sumber pesan API yang bisa diterjemahkan: kunci opsional dari server,
/// argumennya, dan teks mentah sebagai cadangan.
/// </summary>
public sealed record ApiMessageSource(string? MessageKey, string[]? MessageArgs, string? Message)
{
    public static ApiMessageSource From(DeckZa.Application.DTOs.General.BaseGeneralResponse? response)
        => response is null
            ? new ApiMessageSource(null, null, null)
            : new ApiMessageSource(response.MessageKey, response.MessageArgs, response.Message);
}
