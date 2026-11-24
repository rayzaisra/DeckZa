using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeckZa.Infrastructure.Migrations.Main
{
    /// <summary>
    /// Menambahkan menu DeckZa Flows (DOC-DECKZA) di bawah Docs (DOC), dan
    /// menegakkan aturan akses dokumentasi:
    ///
    ///   Docs (root)      -> semua grup aktif
    ///   DeckZa Flows     -> semua grup aktif (Administrator, Member, Guest)
    ///   Framework Flows  -> HANYA Administrator (grup 00)
    ///
    /// Diperlukan karena MenuSeeder.SeedAsync berhenti lebih awal bila menu MNT
    /// sudah ada, sehingga perubahan seeder tidak pernah sampai ke database yang
    /// sudah berisi. Migration ini idempoten: aman dijalankan berulang dan aman
    /// bila menu sudah dibuat manual.
    ///
    /// Tidak mengubah skema.
    /// </summary>
    public partial class SeedDeckZaFlowsMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                -- ============================================================
                -- 1. Pastikan ikon AutoStories tersedia untuk dipilih di Menu Maintenance
                -- ============================================================
                IF NOT EXISTS (SELECT 1 FROM enum_menu_icon WHERE menu_icon_code = 'AutoStories')
                    INSERT INTO enum_menu_icon (menu_icon_code, menu_icon_name, menu_icon_name_alternate)
                    VALUES ('AutoStories', 'Auto Stories', NULL);

                -- ============================================================
                -- 2. Root Docs (DOC) — hanya bila belum ada
                -- ============================================================
                IF NOT EXISTS (SELECT 1 FROM um_menu WHERE menu_unique = 'DOC')
                    INSERT INTO um_menu
                        (menu_unique, menu_name, menu_url, parent_level_id, menu_unique_parent,
                         menu_icon, order_seq, component_type, status_approval,
                         is_active, is_deleted, is_locked, is_default, created_by, created_date)
                    VALUES
                        ('DOC', 'Docs', NULL, 'A', '',
                         'MenuBook', 90, NULL, 4,
                         1, 0, 0, 0, 'system', GETDATE());

                -- ============================================================
                -- 3. Framework Flows (DOC-FLOW) — hanya bila belum ada
                -- ============================================================
                IF NOT EXISTS (SELECT 1 FROM um_menu WHERE menu_unique = 'DOC-FLOW')
                    INSERT INTO um_menu
                        (menu_unique, menu_name, menu_url, parent_level_id, menu_unique_parent,
                         menu_icon, order_seq, component_type, status_approval,
                         is_active, is_deleted, is_locked, is_default, created_by, created_date)
                    VALUES
                        ('DOC-FLOW', 'Framework Flows', '/dashboard/documentation/framework-flows', 'D', 'DOC',
                         'Description', 1, 'DeckZa.Shared.Pages.Documentation.FrameworkFlows, DeckZa.Shared', 4,
                         1, 0, 0, 0, 'system', GETDATE());

                -- ============================================================
                -- 4. DeckZa Flows (DOC-DECKZA) — hanya bila belum ada
                -- ============================================================
                IF NOT EXISTS (SELECT 1 FROM um_menu WHERE menu_unique = 'DOC-DECKZA')
                    INSERT INTO um_menu
                        (menu_unique, menu_name, menu_url, parent_level_id, menu_unique_parent,
                         menu_icon, order_seq, component_type, status_approval,
                         is_active, is_deleted, is_locked, is_default, created_by, created_date)
                    VALUES
                        ('DOC-DECKZA', 'DeckZa Flows', '/dashboard/documentation/deckza-flows', 'D', 'DOC',
                         'AutoStories', 2, 'DeckZa.Shared.Pages.Documentation.DeckZaFlows, DeckZa.Shared', 4,
                         1, 0, 0, 0, 'system', GETDATE());

                -- ============================================================
                -- 5. Pemetaan: Docs (root) -> semua grup aktif
                -- ============================================================
                INSERT INTO um_group_menu
                    (group_id, menu_id, status_approval,
                     is_active, is_deleted, is_locked, is_default, created_by, created_date)
                SELECT g.group_id, m.id, 4, 1, 0, 0, 0, 'system', GETDATE()
                FROM um_group_app g
                CROSS JOIN um_menu m
                WHERE m.menu_unique = 'DOC'
                  AND g.is_active <> 0
                  AND g.is_deleted <> 1
                  AND g.group_id IS NOT NULL
                  AND g.group_id <> ''
                  AND NOT EXISTS (
                      SELECT 1 FROM um_group_menu x
                      WHERE x.menu_id = m.id AND x.group_id = g.group_id);

                -- ============================================================
                -- 6. Pemetaan: DeckZa Flows -> semua grup aktif
                -- ============================================================
                INSERT INTO um_group_menu
                    (group_id, menu_id, status_approval,
                     is_active, is_deleted, is_locked, is_default, created_by, created_date)
                SELECT g.group_id, m.id, 4, 1, 0, 0, 0, 'system', GETDATE()
                FROM um_group_app g
                CROSS JOIN um_menu m
                WHERE m.menu_unique = 'DOC-DECKZA'
                  AND g.is_active <> 0
                  AND g.is_deleted <> 1
                  AND g.group_id IS NOT NULL
                  AND g.group_id <> ''
                  AND NOT EXISTS (
                      SELECT 1 FROM um_group_menu x
                      WHERE x.menu_id = m.id AND x.group_id = g.group_id);

                -- ============================================================
                -- 7. Pemetaan: Framework Flows -> HANYA Administrator (grup 00)
                -- ============================================================
                INSERT INTO um_group_menu
                    (group_id, menu_id, status_approval,
                     is_active, is_deleted, is_locked, is_default, created_by, created_date)
                SELECT '00', m.id, 4, 1, 0, 0, 0, 'system', GETDATE()
                FROM um_menu m
                WHERE m.menu_unique = 'DOC-FLOW'
                  AND NOT EXISTS (
                      SELECT 1 FROM um_group_menu x
                      WHERE x.menu_id = m.id AND x.group_id = '00');

                -- ============================================================
                -- 8. Cabut Framework Flows dari grup selain Administrator
                --    (DB existing terlanjur memetakannya ke semua grup)
                -- ============================================================
                DELETE x
                FROM um_group_menu x
                INNER JOIN um_menu m ON x.menu_id = m.id
                WHERE m.menu_unique = 'DOC-FLOW'
                  AND x.group_id <> '00';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Seed data sengaja tidak dibalik: penghapusan berisiko mencabut
            // menu yang sudah dipakai grup atau dibuat manual oleh admin.
        }
    }
}
