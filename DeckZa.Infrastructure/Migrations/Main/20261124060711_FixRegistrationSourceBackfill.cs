using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeckZa.Infrastructure.Migrations.Main
{
    /// <summary>
    /// Memperbaiki backfill registration_source yang terlewat.
    ///
    /// Masalah: migration AddGuestPlanAndBilling langsung menjalankan
    ///   UPDATE um_user_app SET registration_source = 'ADMIN' WHERE registration_source IS NULL
    /// tepat setelah kolom dibuat. Akibatnya tidak ada lagi baris bernilai NULL,
    /// sehingga backfill SELF di migration FixRegistrationSource tidak pernah
    /// mengisi apa pun.
    ///
    /// Migration ini tidak mengubah skema dan tidak bergantung pada NULL.
    /// Baris yang masih bernilai default 'ADMIN' tetapi jelas terdaftar sendiri
    /// (created_by = user_id) dikoreksi menjadi 'SELF'.
    ///
    /// Idempoten: dijalankan berulang tidak mengubah apa pun setelah baris benar.
    /// </summary>
    public partial class FixRegistrationSourceBackfill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                -- ============================================================
                -- User: pendaftaran mandiri yang terlanjur ditandai ADMIN
                -- ------------------------------------------------------------
                -- Syarat created_by = user_id hanya terpenuhi bila user membuat
                -- dirinya sendiri lewat pendaftaran mandiri. User yang dibuat
                -- admin selalu memiliki created_by = id admin.
                -- ============================================================
                UPDATE um_user_app
                SET registration_source = 'SELF'
                WHERE registration_source = 'ADMIN'
                  AND created_by = user_id;

                -- ============================================================
                -- Pending: permintaan pendaftaran mandiri yang belum di-approve
                -- ============================================================
                UPDATE temp_user_app_pending
                SET registration_source = 'SELF'
                WHERE registration_source = 'ADMIN'
                  AND created_by = user_id;

                -- ============================================================
                -- Sisa baris NULL (bila ada) tetap diisi ADMIN sebagai default
                -- ============================================================
                UPDATE um_user_app
                SET registration_source = 'ADMIN'
                WHERE registration_source IS NULL;

                UPDATE temp_user_app_pending
                SET registration_source = 'ADMIN'
                WHERE registration_source IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Koreksi data tidak dibalik: nilai sebelumnya tidak dapat
            // direkonstruksi tanpa menebak, dan mengembalikannya ke 'ADMIN'
            // justru menghidupkan kembali nilai yang salah.
        }
    }
}
