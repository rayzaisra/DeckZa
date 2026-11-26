using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeckZa.Infrastructure.Migrations.Main
{
    /// <inheritdoc />
    public partial class MasterDataSoftDeleteIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_rf_task_type_owner_user_id_description",
                table: "rf_task_type");

            migrationBuilder.DropIndex(
                name: "IX_rf_task_priority_owner_user_id_description",
                table: "rf_task_priority");

            migrationBuilder.DropIndex(
                name: "IX_rf_status_owner_user_id_description",
                table: "rf_status");

            migrationBuilder.DropIndex(
                name: "IX_rf_application_owner_user_id_description",
                table: "rf_application");

            migrationBuilder.CreateIndex(
                name: "IX_rf_task_type_owner_user_id_description",
                table: "rf_task_type",
                columns: new[] { "owner_user_id", "description" },
                unique: true,
                filter: "[is_deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_rf_task_priority_owner_user_id_description",
                table: "rf_task_priority",
                columns: new[] { "owner_user_id", "description" },
                unique: true,
                filter: "[is_deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_rf_status_owner_user_id_description",
                table: "rf_status",
                columns: new[] { "owner_user_id", "description" },
                unique: true,
                filter: "[is_deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_rf_application_owner_user_id_description",
                table: "rf_application",
                columns: new[] { "owner_user_id", "description" },
                unique: true,
                filter: "[is_deleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_rf_task_type_owner_user_id_description",
                table: "rf_task_type");

            migrationBuilder.DropIndex(
                name: "IX_rf_task_priority_owner_user_id_description",
                table: "rf_task_priority");

            migrationBuilder.DropIndex(
                name: "IX_rf_status_owner_user_id_description",
                table: "rf_status");

            migrationBuilder.DropIndex(
                name: "IX_rf_application_owner_user_id_description",
                table: "rf_application");

            migrationBuilder.CreateIndex(
                name: "IX_rf_task_type_owner_user_id_description",
                table: "rf_task_type",
                columns: new[] { "owner_user_id", "description" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rf_task_priority_owner_user_id_description",
                table: "rf_task_priority",
                columns: new[] { "owner_user_id", "description" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rf_status_owner_user_id_description",
                table: "rf_status",
                columns: new[] { "owner_user_id", "description" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rf_application_owner_user_id_description",
                table: "rf_application",
                columns: new[] { "owner_user_id", "description" },
                unique: true);
        }
    }
}
