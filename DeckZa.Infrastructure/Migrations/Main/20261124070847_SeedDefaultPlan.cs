using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeckZa.Infrastructure.Migrations.Main
{
    /// <summary>
    /// Menetapkan plan default untuk pengguna yang grupnya tidak punya plan eksplisit.
    ///
    /// Plan efektif di-resolve dari `rf_plan.upgrade_group_id` = grup user, lalu
    /// fallback ke `rf_plan.is_default = 1`. Tanpa baris default, grup seperti
    /// Administrator (00) tidak punya plan sehingga fitur berbasis plan
    /// (kuota task, import Excel) tidak dapat ditentukan dan UI menyembunyikan
    /// tombol-tombolnya secara keliru.
    ///
    /// Member dipilih sebagai default karena mewakili akses penuh (task tanpa batas,
    /// import Excel aktif) — sama dengan perilaku sebelum pembatasan plan ada.
    ///
    /// Idempoten: hanya jalan bila belum ada plan default, dan hanya menyentuh kolom
    /// `is_default`. Tidak mengubah skema maupun baris lain.
    /// </summary>
    public partial class SeedDefaultPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM rf_plan WHERE is_default = 1 AND is_deleted = 0)
                   AND EXISTS (SELECT 1 FROM rf_plan WHERE id = 'PLAN-MEMBER' AND is_deleted = 0)
                    UPDATE rf_plan SET is_default = 1 WHERE id = 'PLAN-MEMBER';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Sengaja no-op: nilai default plan adalah data referensi yang dipakai
            // aplikasi. Rollback tidak boleh menghapus konfigurasi yang mungkin
            // sudah diubah administrator.
        }
    }
}
