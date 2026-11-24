using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeckZa.Infrastructure.Migrations.Main
{
    /// <summary>
    /// Melepas pemetaan menu Administrator (MNT*) dari grup Guest (99).
    /// Migration ini tidak mengubah skema — hanya DELETE.
    ///
    /// Menu "Setting" (MNT) tidak boleh tampil di sidebar Guest, sama seperti
    /// grup Member (01) yang memang tidak memetakan menu Administrator.
    /// </summary>
    public partial class RemoveGuestSettingMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE gm
                FROM um_group_menu gm
                INNER JOIN um_menu m ON gm.menu_id = m.id
                WHERE gm.group_id = '99'
                  AND m.menu_unique LIKE 'MNT%';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Menu Administrator tidak dipetakan ke Guest — tidak dikembalikan.
            // Grup Member (01) juga tidak memetakan MNT, jadi kondisi awal tidak pernah dipulihkan.
        }
    }
}
