using DeckZa.Application.DTOs.General;

namespace DeckZa.Application.Interfaces;

/// <summary>
/// Dispatch email langsung tanpa bergantung pada baris <c>um_user_app</c>.
/// Diperlukan untuk email pendaftar: saat pendaftaran disubmit, user belum ada
/// di <c>um_user_app</c> (hanya di <c>temp_user_app_pending</c>), sehingga
/// <c>NotificationService.QueueEmailAsync</c> tidak dapat dipakai.
/// </summary>
public interface IEmailDispatchService
{
    Task<BaseGeneralResponse> QueueAsync(
        string recipient,
        string subject,
        string bodyHtml,
        string bodyText,
        string referenceType,
        string? referenceId = null,
        CancellationToken cancellationToken = default);

    Task<bool> IsEmailEnabledAsync(CancellationToken cancellationToken = default);
}
