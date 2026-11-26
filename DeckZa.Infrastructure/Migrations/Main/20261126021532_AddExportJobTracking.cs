using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeckZa.Infrastructure.Migrations.Main
{
    /// <inheritdoc />
    public partial class AddExportJobTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_export_job",
                columns: table => new
                {
                    job_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    file_name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    export_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true),
                    created_by = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    modified_by = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    modified_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    deleted_by = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    deleted_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    status_approval = table.Column<int>(type: "int", nullable: true),
                    is_locked = table.Column<bool>(type: "bit", nullable: true),
                    is_default = table.Column<bool>(type: "bit", nullable: true),
                    approved_by = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    approved_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    approval_type_id = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_export_job", x => x.job_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_export_job_expires_at",
                table: "tb_export_job",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_tb_export_job_file_name",
                table: "tb_export_job",
                column: "file_name");

            migrationBuilder.CreateIndex(
                name: "IX_tb_export_job_user_id",
                table: "tb_export_job",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_export_job");
        }
    }
}
