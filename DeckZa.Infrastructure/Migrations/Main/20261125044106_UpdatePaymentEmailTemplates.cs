using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeckZa.Infrastructure.Migrations.Main
{
    /// <inheritdoc />
    public partial class UpdatePaymentEmailTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE rf_email_template SET subject = v.subject, body_html = v.body_html, body_text = REPLACE(v.body_text, '\n', CHAR(10))
                FROM rf_email_template t
                INNER JOIN (VALUES
                    ('PAYMENT_RECEIVED', 'Payment request received',
                        '<p>Hello {{full_name}},</p><p>Your payment request ({{request_no}}) for plan {{plan_name}} has been received and is awaiting administrator approval.</p><p>Amount: {{amount}}</p><p>You will receive another email once your request is approved or rejected.</p>',
                        'Hello {{full_name}},\n\nYour payment request ({{request_no}}) for plan {{plan_name}} has been received and is awaiting administrator approval.\n\nAmount: {{amount}}\n\nYou will receive another email once your request is approved or rejected.'),
                    ('PAYMENT_APPROVED', 'Plan upgrade approved',
                        '<p>Hello {{full_name}},</p><p>Your plan upgrade to {{plan_name}} has been approved. Amount: {{amount}}.</p><p>Invoice No. {{invoice_no}}. The invoice PDF is attached to this email and is also available on your Billing page.</p><p><a href="{{link}}">Open DeckZa Billing</a></p>',
                        'Hello {{full_name}},\n\nYour plan upgrade to {{plan_name}} has been approved. Amount: {{amount}}.\n\nInvoice No. {{invoice_no}}. The invoice PDF is attached to this email and is also available on your Billing page.\n\nOpen DeckZa Billing: {{link}}'),
                    ('PAYMENT_REJECTED', 'Plan upgrade rejected',
                        '<p>Hello {{full_name}},</p><p>Your plan upgrade request ({{request_no}}) for plan {{plan_name}} has been rejected. Amount: {{amount}}.</p><p>{{long_text}}</p><p>Please review your payment details and submit the request again.</p><p><a href="{{link}}">Open DeckZa Billing</a></p>',
                        'Hello {{full_name}},\n\nYour plan upgrade request ({{request_no}}) for plan {{plan_name}} has been rejected. Amount: {{amount}}.\n\n{{long_text}}\n\nPlease review your payment details and submit the request again.\n\nOpen DeckZa Billing: {{link}}')
                ) AS v(template_key, subject, body_html, body_text)
                ON v.template_key = t.template_key;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
