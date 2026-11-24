using DeckZa.Application.DTOs.General;
using DeckZa.Application.Interfaces;
using DeckZa.Domain.Entities;
using DeckZa.Domain.Enums;

namespace DeckZa.Infrastructure.Services;

/// <summary>
/// Menulis email langsung ke <c>tb_email_outbox</c> tanpa bergantung pada baris
/// <c>um_user_app</c>. Dipakai untuk email pendaftar yang belum punya baris user.
/// </summary>
public class EmailDispatchService : IEmailDispatchService
{
    private const string NotificationEmailSettingCode = "NOTIF_MAIL";

    private readonly IUnitOfWork _unitOfWork;

    public EmailDispatchService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> IsEmailEnabledAsync(CancellationToken cancellationToken = default)
    {
        var setting = await _unitOfWork.DefaultValues.GetByCodeAsync(
            NotificationEmailSettingCode,
            cancellationToken);

        return setting != null
               && setting.isActive != false
               && string.Equals(setting.def_value, "true", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<BaseGeneralResponse> QueueAsync(
        string recipient,
        string subject,
        string bodyHtml,
        string bodyText,
        string referenceType,
        string? referenceId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recipient))
        {
            return Fail("Recipient is required.");
        }

        // Email dimatikan secara sengaja lewat parameter NOTIF_MAIL — bukan kegagalan.
        if (!await IsEmailEnabledAsync(cancellationToken))
        {
            return Success("Email notification is disabled.");
        }

        var provider = await _unitOfWork.EmailProviders.GetActiveProviderAsync(cancellationToken);
        var canEmail = provider != null;

        await _unitOfWork.EmailOutbox.AddAsync(new tb_email_outbox
        {
            id = Guid.NewGuid(),
            provider_type = canEmail ? provider!.provider_type : null,
            recipient = recipient,
            subject = subject,
            body_html = bodyHtml,
            body_text = bodyText,
            reference_type = referenceType,
            reference_id = referenceId,
            status = canEmail ? "Pending" : "AdminFallback",
            next_attempt_at = canEmail ? DateTime.UtcNow : null,
            created_date = DateTime.Now,
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return canEmail
            ? Success("Email queued.")
            : Success("Email provider is not configured. The message was queued for administrator fallback.");
    }

    private static BaseGeneralResponse Success(string message) => new()
    {
        Success = true,
        Code = Const.SUCCESS_CODE,
        Message = message,
    };

    private static BaseGeneralResponse Fail(string message) => new()
    {
        Success = false,
        Code = Const.ERROR_CODE,
        Message = message,
    };
}
