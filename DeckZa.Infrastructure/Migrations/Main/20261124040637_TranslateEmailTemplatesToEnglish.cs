using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeckZa.Infrastructure.Migrations.Main
{
    /// <summary>
    /// Terjemahkan seluruh template email (rf_email_template) ke Bahasa Inggris.
    /// Migration ini tidak mengubah skema — hanya UPDATE idempoten.
    ///
    /// Isi template mengikuti EmailTemplateSeeder (jalur seeder ≡ jalur migrasi).
    /// </summary>
    public partial class TranslateEmailTemplatesToEnglish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE rf_email_template SET subject = v.subject, body_html = v.body_html, body_text = REPLACE(v.body_text, '\n', CHAR(10))
                FROM rf_email_template t
                INNER JOIN (VALUES
                    ('REGISTRATION_RECEIVED', 'DeckZa registration received',
                        '<p>Hello {{full_name}},</p><p>Your DeckZa registration ({{user_id}}) has been received and is awaiting administrator approval.</p><p>You will receive another email once your registration is approved or rejected.</p>',
                        'Hello {{full_name}},\n\nYour DeckZa registration ({{user_id}}) has been received and is awaiting administrator approval.\n\nYou will receive another email once your registration is approved or rejected.'),
                    ('REGISTRATION_APPROVED', 'DeckZa registration approved',
                        '<p>Hello {{full_name}},</p><p>Your DeckZa registration ({{user_id}}) has been approved. You can now sign in to get started.</p><p><a href="{{link}}">Sign in to DeckZa</a></p>',
                        'Hello {{full_name}},\n\nYour DeckZa registration ({{user_id}}) has been approved. You can now sign in to get started.\n\nSign in: {{link}}'),
                    ('REGISTRATION_REJECTED', 'DeckZa registration rejected',
                        '<p>Hello {{full_name}},</p><p>Your DeckZa registration ({{user_id}}) has been rejected. {{long_text}}</p><p>Please contact an administrator if you would like to register again.</p>',
                        'Hello {{full_name}},\n\nYour DeckZa registration ({{user_id}}) has been rejected. {{long_text}}\n\nPlease contact an administrator if you would like to register again.'),
                    ('PAYMENT_RECEIVED', 'Payment request received',
                        '<p>Hello {{full_name}},</p><p>Your payment request has been received and is awaiting administrator approval.</p><p>Amount: {{long_text}}</p><p>You will receive another email once your request is approved or rejected.</p>',
                        'Hello {{full_name}},\n\nYour payment request has been received and is awaiting administrator approval.\n\nAmount: {{long_text}}\n\nYou will receive another email once your request is approved or rejected.'),
                    ('PAYMENT_APPROVED', 'Plan upgrade approved',
                        '<p>Hello {{full_name}},</p><p>Your plan upgrade has been approved. {{long_text}}</p><p>Invoice: {{link}}</p><p>Please sign in again for your new access to take effect.</p>',
                        'Hello {{full_name}},\n\nYour plan upgrade has been approved. {{long_text}}\n\nInvoice: {{link}}\n\nPlease sign in again for your new access to take effect.'),
                    ('PAYMENT_REJECTED', 'Plan upgrade rejected',
                        '<p>Hello {{full_name}},</p><p>Your plan upgrade has been rejected. {{long_text}}</p><p>Please review your payment details and submit the request again.</p>',
                        'Hello {{full_name}},\n\nYour plan upgrade has been rejected. {{long_text}}\n\nPlease review your payment details and submit the request again.'),
                    ('NOTIFICATION', 'DeckZa notification',
                        '<p>Hello {{full_name}},</p><p>{{short_text}}</p><p>{{long_text}}</p><p><a href="{{link}}">Open DeckZa</a></p>',
                        'Hello {{full_name}},\n\n{{short_text}}\n\n{{long_text}}\n\nOpen DeckZa: {{link}}'),
                    ('FORGOT_PASSWORD', 'Reset your DeckZa password',
                        '<p>Hello {{full_name}},</p><p>Use the link below to reset your password.</p><p><a href="{{link}}">Reset Password</a></p><p>This link expires in 30 minutes.</p>',
                        'Hello {{full_name}},\n\nUse the link below to reset your password:\n\n{{link}}\n\nThis link expires in 30 minutes.')
                ) AS v(template_key, subject, body_html, body_text)
                ON v.template_key = t.template_key;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Teks Indonesia sebelumnya tidak dipulihkan — isi lama sudah hilang.
            // Seeder (EmailTemplateSeeder) selalu menulis versi Inggris, jadi
            // menjalankan ulang seeder sudah mengembalikan kondisi yang diharapkan.
        }
    }
}
