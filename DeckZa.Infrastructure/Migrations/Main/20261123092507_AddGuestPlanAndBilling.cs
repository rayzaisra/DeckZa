using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeckZa.Infrastructure.Migrations.Main
{
    /// <summary>
    /// Menambah tabel plan, billing, invoice, export job, serta kolom
    /// registrasi eksternal dan lampiran email.
    ///
    /// CATATAN: UPDATE registration_source = 'ADMIN' di migration ini keliru —
    /// nilai itu menutup semua baris sehingga backfill SELF pada migration
    /// FixRegistrationSource tidak pernah berjalan. Koreksi dilakukan oleh
    /// migration FixRegistrationSourceBackfill. Blok tersebut sengaja
    /// dipertahankan agar riwayat migration tetap konsisten dengan database
    /// yang sudah menerapkannya.
    /// </summary>
    public partial class AddGuestPlanAndBilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "avatar_url",
                table: "um_user_app",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_provider",
                table: "um_user_app",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_provider_id",
                table: "um_user_app",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "plan_id",
                table: "um_user_app",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "profile_synced_date",
                table: "um_user_app",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "registration_source",
                table: "um_user_app",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE um_user_app SET registration_source = 'ADMIN' WHERE registration_source IS NULL;");

            migrationBuilder.AddColumn<string>(
                name: "external_provider",
                table: "temp_user_app_pending",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_provider_id",
                table: "temp_user_app_pending",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "registration_source",
                table: "temp_user_app_pending",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "attachment_content_type",
                table: "tb_email_outbox",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "attachment_file_name",
                table: "tb_email_outbox",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "attachment_storage_path",
                table: "tb_email_outbox",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "rf_plan",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    plan_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    price_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    price_currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    billing_period = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    billing_interval_count = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    max_active_tasks = table.Column<int>(type: "int", nullable: true),
                    report_period_months = table.Column<int>(type: "int", nullable: true),
                    allows_calendar = table.Column<bool>(type: "bit", nullable: false),
                    allows_excel_import = table.Column<bool>(type: "bit", nullable: false),
                    allows_supervisor_mode = table.Column<bool>(type: "bit", nullable: false),
                    upgrade_group_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    order_seq = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_rf_plan", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_payment_request",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    request_no = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    user_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    plan_id = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    payment_method = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    transfer_reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    proof_file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    proof_stored_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    proof_content_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    proof_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    reviewed_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    reviewed_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    review_note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    activated_from = table.Column<DateTime>(type: "datetime2", nullable: true),
                    activated_to = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_tb_payment_request", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rf_plan_feature",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    plan_id = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    feature_text = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    order_seq = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_rf_plan_feature", x => x.id);
                    table.ForeignKey(
                        name: "FK_rf_plan_feature_rf_plan_plan_id",
                        column: x => x.plan_id,
                        principalTable: "rf_plan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_payment_invoice",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    invoice_no = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    payment_request_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    plan_id = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    plan_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    billing_period = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    invoice_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    activated_from = table.Column<DateTime>(type: "datetime2", nullable: true),
                    activated_to = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    stored_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    content_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_tb_payment_invoice", x => x.id);
                    table.ForeignKey(
                        name: "FK_tb_payment_invoice_tb_payment_request_payment_request_id",
                        column: x => x.payment_request_id,
                        principalTable: "tb_payment_request",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_um_user_app_external_provider_external_provider_id",
                table: "um_user_app",
                columns: new[] { "external_provider", "external_provider_id" },
                unique: true,
                filter: "[external_provider_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_rf_plan_feature_plan_id",
                table: "rf_plan_feature",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_payment_invoice_invoice_no",
                table: "tb_payment_invoice",
                column: "invoice_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_payment_invoice_payment_request_id",
                table: "tb_payment_invoice",
                column: "payment_request_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_payment_invoice_user_id",
                table: "tb_payment_invoice",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_payment_request_request_no",
                table: "tb_payment_request",
                column: "request_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_payment_request_user_id",
                table: "tb_payment_request",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rf_plan_feature");

            migrationBuilder.DropTable(
                name: "tb_payment_invoice");

            migrationBuilder.DropTable(
                name: "rf_plan");

            migrationBuilder.DropTable(
                name: "tb_payment_request");

            migrationBuilder.DropIndex(
                name: "IX_um_user_app_external_provider_external_provider_id",
                table: "um_user_app");

            migrationBuilder.DropColumn(
                name: "avatar_url",
                table: "um_user_app");

            migrationBuilder.DropColumn(
                name: "external_provider",
                table: "um_user_app");

            migrationBuilder.DropColumn(
                name: "external_provider_id",
                table: "um_user_app");

            migrationBuilder.DropColumn(
                name: "plan_id",
                table: "um_user_app");

            migrationBuilder.DropColumn(
                name: "profile_synced_date",
                table: "um_user_app");

            migrationBuilder.DropColumn(
                name: "registration_source",
                table: "um_user_app");

            migrationBuilder.DropColumn(
                name: "external_provider",
                table: "temp_user_app_pending");

            migrationBuilder.DropColumn(
                name: "external_provider_id",
                table: "temp_user_app_pending");

            migrationBuilder.DropColumn(
                name: "registration_source",
                table: "temp_user_app_pending");

            migrationBuilder.DropColumn(
                name: "attachment_content_type",
                table: "tb_email_outbox");

            migrationBuilder.DropColumn(
                name: "attachment_file_name",
                table: "tb_email_outbox");

            migrationBuilder.DropColumn(
                name: "attachment_storage_path",
                table: "tb_email_outbox");
        }
    }
}
