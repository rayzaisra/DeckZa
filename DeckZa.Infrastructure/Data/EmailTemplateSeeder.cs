using Microsoft.EntityFrameworkCore;

using DeckZa.Domain.Entities;
using DeckZa.Domain.Enums;

namespace DeckZa.Infrastructure.Data;

public static class EmailTemplateSeeder
{
    private static readonly (string Key, string Subject, string BodyHtml, string BodyText)[] Templates =
    {
        (
            EmailTemplateKeys.RegistrationReceived,
            "DeckZa registration received",
            "<p>Hello {{full_name}},</p>"
            + "<p>Your DeckZa registration ({{user_id}}) has been received and is awaiting administrator approval.</p>"
            + "<p>You will receive another email once your registration is approved or rejected.</p>",
            "Hello {{full_name}},\n\nYour DeckZa registration ({{user_id}}) has been received and is awaiting administrator approval.\n\nYou will receive another email once your registration is approved or rejected."
        ),
        (
            EmailTemplateKeys.RegistrationApproved,
            "DeckZa registration approved",
            "<p>Hello {{full_name}},</p>"
            + "<p>Your DeckZa registration ({{user_id}}) has been approved. You can now sign in to get started.</p>"
            + "<p><a href=\"{{link}}\">Sign in to DeckZa</a></p>",
            "Hello {{full_name}},\n\nYour DeckZa registration ({{user_id}}) has been approved. You can now sign in to get started.\n\nSign in: {{link}}"
        ),
        (
            EmailTemplateKeys.RegistrationRejected,
            "DeckZa registration rejected",
            "<p>Hello {{full_name}},</p>"
            + "<p>Your DeckZa registration ({{user_id}}) has been rejected. {{long_text}}</p>"
            + "<p>Please contact an administrator if you would like to register again.</p>",
            "Hello {{full_name}},\n\nYour DeckZa registration ({{user_id}}) has been rejected. {{long_text}}\n\nPlease contact an administrator if you would like to register again."
        ),
        (
            EmailTemplateKeys.PaymentReceived,
            "Payment request received",
            "<p>Hello {{full_name}},</p>"
            + "<p>Your payment request ({{request_no}}) for plan {{plan_name}} has been received and is awaiting administrator approval.</p>"
            + "<p>Amount: {{amount}}</p>"
            + "<p>You will receive another email once your request is approved or rejected.</p>",
            "Hello {{full_name}},\n\nYour payment request ({{request_no}}) for plan {{plan_name}} has been received and is awaiting administrator approval.\n\nAmount: {{amount}}\n\nYou will receive another email once your request is approved or rejected."
        ),
        (
            EmailTemplateKeys.PaymentApproved,
            "Plan upgrade approved",
            "<p>Hello {{full_name}},</p>"
            + "<p>Your plan upgrade to {{plan_name}} has been approved. Amount: {{amount}}.</p>"
            + "<p>Invoice No. {{invoice_no}}. The invoice PDF is attached to this email and is also available on your Billing page.</p>"
            + "<p><a href=\"{{link}}\">Open DeckZa Billing</a></p>",
            "Hello {{full_name}},\n\nYour plan upgrade to {{plan_name}} has been approved. Amount: {{amount}}.\n\nInvoice No. {{invoice_no}}. The invoice PDF is attached to this email and is also available on your Billing page.\n\nOpen DeckZa Billing: {{link}}"
        ),
        (
            EmailTemplateKeys.PaymentRejected,
            "Plan upgrade rejected",
            "<p>Hello {{full_name}},</p>"
            + "<p>Your plan upgrade request ({{request_no}}) for plan {{plan_name}} has been rejected. Amount: {{amount}}.</p>"
            + "<p>{{long_text}}</p>"
            + "<p>Please review your payment details and submit the request again.</p>"
            + "<p><a href=\"{{link}}\">Open DeckZa Billing</a></p>",
            "Hello {{full_name}},\n\nYour plan upgrade request ({{request_no}}) for plan {{plan_name}} has been rejected. Amount: {{amount}}.\n\n{{long_text}}\n\nPlease review your payment details and submit the request again.\n\nOpen DeckZa Billing: {{link}}"
        ),
        (
            EmailTemplateKeys.Notification,
            "DeckZa notification",
            "<p>Hello {{full_name}},</p>"
            + "<p>{{short_text}}</p>"
            + "<p>{{long_text}}</p>"
            + "<p><a href=\"{{link}}\">Open DeckZa</a></p>",
            "Hello {{full_name}},\n\n{{short_text}}\n\n{{long_text}}\n\nOpen DeckZa: {{link}}"
        ),
        (
            EmailTemplateKeys.ForgotPassword,
            "Reset your DeckZa password",
            "<p>Hello {{full_name}},</p>"
            + "<p>Use the link below to reset your password.</p>"
            + "<p><a href=\"{{link}}\">Reset Password</a></p>"
            + "<p>This link expires in 30 minutes.</p>",
            "Hello {{full_name}},\n\nUse the link below to reset your password:\n\n{{link}}\n\nThis link expires in 30 minutes."
        ),
    };

    /// <summary>
    /// Upsert template email. Selain insert, isi template yang sudah ada diselaraskan
    /// ke versi terbaru — <c>rf_email_template</c> tidak bisa diedit lewat Parameter Engine,
    /// jadi aman di-update otomatis.
    /// </summary>
    public static async Task SeedAsync(DeckZaDbContext context, CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;

        foreach (var item in Templates)
        {
            var existing = await context.rf_email_template
                .FirstOrDefaultAsync(e => e.template_key == item.Key, cancellationToken);

            if (existing != null)
            {
                existing.subject = item.Subject;
                existing.body_html = item.BodyHtml;
                existing.body_text = item.BodyText;
                existing.modifiedDate = now;
                existing.modifiedBy = "system";
                continue;
            }

            await context.rf_email_template.AddAsync(new rf_email_template
            {
                template_key = item.Key,
                subject = item.Subject,
                body_html = item.BodyHtml,
                body_text = item.BodyText,
                isActive = true,
                isDeleted = false,
                createdBy = "system",
                createdDate = now,
            }, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
