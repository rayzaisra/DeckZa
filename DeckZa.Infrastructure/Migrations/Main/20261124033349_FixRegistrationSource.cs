using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeckZa.Infrastructure.Migrations.Main
{
    /// <summary>
    /// Backfill kolom registration_source untuk user & pending yang sudah terisi.
    /// Migration ini tidak mengubah skema — hanya UPDATE idempoten.
    ///
    /// Penyebab: ApplyUserFields di ApprovalService tidak menyalin registration_source,
    /// sehingga user hasil approve (khususnya pendaftaran mandiri) tersimpan NULL.
    ///
    /// Aturan penentuan sumber:
    ///   - Pending MODIFIED/DELETE  -> pertahankan sumber user yang sudah ada.
    ///   - created_by = user_id     -> pendaftaran mandiri (SELF): pemohon membuat dirinya sendiri.
    ///   - selebihnya               -> dibuat admin / seeded (ADMIN).
    /// </summary>
    public partial class FixRegistrationSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                -- ============================================================
                -- 1. Pending: pertahankan sumber user untuk request MODIFIED/DELETE
                -- ============================================================
                UPDATE p
                SET p.registration_source = u.registration_source
                FROM temp_user_app_pending p
                INNER JOIN um_user_app u ON p.user_id = u.user_id
                WHERE p.registration_source IS NULL
                  AND u.registration_source IS NOT NULL;

                -- ============================================================
                -- 2. Pending: pendaftaran mandiri (created_by = user_id)
                -- ============================================================
                UPDATE temp_user_app_pending
                SET registration_source = 'SELF'
                WHERE registration_source IS NULL
                  AND created_by = user_id;

                -- ============================================================
                -- 3. Pending: sisanya dibuat admin
                -- ============================================================
                UPDATE temp_user_app_pending
                SET registration_source = 'ADMIN'
                WHERE registration_source IS NULL;

                -- ============================================================
                -- 4. User: pendaftaran mandiri (created_by = user_id)
                -- ============================================================
                UPDATE um_user_app
                SET registration_source = 'SELF'
                WHERE registration_source IS NULL
                  AND created_by = user_id;

                -- ============================================================
                -- 5. User: sisanya dibuat admin / seeded
                -- ============================================================
                UPDATE um_user_app
                SET registration_source = 'ADMIN'
                WHERE registration_source IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Backfill tidak dapat dibalik secara bermakna: nilai NULL sebelum migrasi
            // sudah hilang. Kolom sengaja dibiarkan apa adanya.
        }
    }
}
