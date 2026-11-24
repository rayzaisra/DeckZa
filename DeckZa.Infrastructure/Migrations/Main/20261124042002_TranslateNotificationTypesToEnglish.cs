using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeckZa.Infrastructure.Migrations.Main
{
    /// <summary>
    /// Terjemahkan teks tipe notifikasi (rf_notification_type) ke Bahasa Inggris.
    /// Migration ini tidak mengubah skema — hanya UPDATE idempoten.
    ///
    /// Teks notifikasi disimpan di level TIPE, bukan per-instance. Teks spesifik per
    /// kejadian (mis. "Registration approved") hanya dikirim lewat push SignalR
    /// dan email — bukan disimpan per baris notifikasi.
    ///
    /// Isi mengikuti NotificationSeeder (jalur seeder ≡ jalur migrasi).
    /// </summary>
    public partial class TranslateNotificationTypesToEnglish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE rf_notification_type
                SET short_text = v.short_text, long_text = v.long_text
                FROM rf_notification_type t
                INNER JOIN (VALUES
                    ('44444444-4444-4444-4444-444444444444', 'User Registration',
                        'There is a user registration update. Reference: @[tboId]'),
                    ('55555555-5555-5555-5555-555555555555', 'Payment Request',
                        'There is a payment request update. Reference: @[tboId]')
                ) AS v(id, short_text, long_text)
                ON v.id = t.id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Teks Indonesia sebelumnya tidak dipulihkan — isi lama sudah hilang.
            // Seeder (NotificationSeeder) selalu menulis versi Inggris, jadi
            // menjalankan ulang seeder sudah mengembalikan kondisi yang diharapkan.
        }
    }
}
