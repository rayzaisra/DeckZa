using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeckZa.Infrastructure.Migrations.Main
{
    /// <summary>
    /// Data-only: menambah key rf_default_value untuk instruksi pembayaran
    /// (PAY_BNK_NM, PAY_BNK_NO, PAY_BNK_AN, PAY_INSTR). Diperlukan karena seeder
    /// hanya efektif saat DB kosong / guard AnyAsync, sedangkan DB existing sudah
    /// melewati seeding awal. Idempoten: IF NOT EXISTS per def_code.
    /// </summary>
    public partial class SeedPaymentDefaultValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM rf_default_value WHERE def_code = 'PAY_BNK_NM')
                    INSERT INTO rf_default_value (def_code, def_name, def_value, is_active, is_deleted, status_approval, is_locked, created_by, created_date)
                    VALUES ('PAY_BNK_NM', 'Payment bank name', 'BCA', 1, 0, 4, 0, 'system', SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM rf_default_value WHERE def_code = 'PAY_BNK_NO')
                    INSERT INTO rf_default_value (def_code, def_name, def_value, is_active, is_deleted, status_approval, is_locked, created_by, created_date)
                    VALUES ('PAY_BNK_NO', 'Payment bank account number', '1234567890', 1, 0, 4, 0, 'system', SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM rf_default_value WHERE def_code = 'PAY_BNK_AN')
                    INSERT INTO rf_default_value (def_code, def_name, def_value, is_active, is_deleted, status_approval, is_locked, created_by, created_date)
                    VALUES ('PAY_BNK_AN', 'Payment bank account holder', 'PT DeckZa Indonesia', 1, 0, 4, 0, 'system', SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM rf_default_value WHERE def_code = 'PAY_INSTR')
                    INSERT INTO rf_default_value (def_code, def_name, def_value, is_active, is_deleted, status_approval, is_locked, created_by, created_date)
                    VALUES ('PAY_INSTR', 'Payment transfer instructions', 'Transfer sesuai nominal, lalu unggah bukti pembayaran.', 1, 0, 4, 0, 'system', SYSUTCDATETIME());
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: data referensi, jangan hapus saat rollback.
        }
    }
}
