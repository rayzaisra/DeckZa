using Microsoft.EntityFrameworkCore;

using DeckZa.Domain.Entities;
using DeckZa.Domain.Enums;
using DeckZa.Infrastructure.Data;

namespace DeckZa.Infrastructure.Data;

public static class NotificationSeeder
{
    public static async Task SeedAsync(DeckZaDbContext context, CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        await EnsureTypeAsync(
            context,
            NotificationTypeIds.Approval,
            "Approval",
            "There is an approval update.",
            isSendMail: false,
            now,
            cancellationToken);

        await EnsureTypeAsync(
            context,
            NotificationTypeIds.Export,
            "Export ready",
            "Your export is ready to download.",
            isSendMail: false,
            now,
            cancellationToken);

        await EnsureTypeAsync(
            context,
            NotificationTypeIds.PasswordReset,
            "Password reset request",
            "A password reset was requested. Link: @[tboId]",
            isSendMail: false,
            now,
            cancellationToken);

        await EnsureTypeAsync(
            context,
            NotificationTypeIds.UserRegistration,
            "User Registration",
            "There is a user registration update. Reference: @[tboId]",
            isSendMail: false,
            now,
            cancellationToken);

        await EnsureTypeAsync(
            context,
            NotificationTypeIds.PaymentRequest,
            "Payment Request",
            "There is a payment request update. Reference: @[tboId]",
            isSendMail: false,
            now,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Upsert tipe notifikasi. Selain insert, teks tipe yang sudah ada diselaraskan
    /// ke versi terbaru. Teks notifikasi disimpan di level tipe, bukan per-instance —
    /// teks spesifik per kejadian hanya dikirim lewat push SignalR dan email.
    /// </summary>
    private static async Task EnsureTypeAsync(
        DeckZaDbContext context,
        string id,
        string shortText,
        string longText,
        bool isSendMail,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var existing = await context.rf_notification_type
            .FirstOrDefaultAsync(e => e.id == id, cancellationToken);

        if (existing != null)
        {
            existing.short_text = shortText;
            existing.long_text = longText;
            existing.modifiedDate = now;
            existing.modifiedBy = "system";
            return;
        }

        await context.rf_notification_type.AddAsync(new rf_notification_type
        {
            id = id,
            short_text = shortText,
            long_text = longText,
            href = null,
            is_send_mail = isSendMail,
            is_send_web = true,
            statusApproval = Const.StatusApproval.APPROVED,
            approvalTypeId = Const.ApprovalType.ADD,
            isLocked = false,
            isDefault = false,
            isActive = true,
            isDeleted = false,
            approvedBy = "system",
            approvedDate = now,
            createdBy = "system",
            createdDate = now,
        }, cancellationToken);
    }
}
