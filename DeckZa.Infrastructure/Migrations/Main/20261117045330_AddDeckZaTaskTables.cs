using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeckZa.Infrastructure.Migrations.Main
{
    /// <inheritdoc />
    public partial class AddDeckZaTaskTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rf_application",
                columns: table => new
                {
                    application_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
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
                    table.PrimaryKey("PK_rf_application", x => x.application_id);
                });

            migrationBuilder.CreateTable(
                name: "rf_status",
                columns: table => new
                {
                    status_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
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
                    table.PrimaryKey("PK_rf_status", x => x.status_id);
                });

            migrationBuilder.CreateTable(
                name: "rf_task_priority",
                columns: table => new
                {
                    task_priority_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
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
                    table.PrimaryKey("PK_rf_task_priority", x => x.task_priority_id);
                });

            migrationBuilder.CreateTable(
                name: "rf_task_type",
                columns: table => new
                {
                    task_type_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
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
                    table.PrimaryKey("PK_rf_task_type", x => x.task_type_id);
                });

            migrationBuilder.CreateTable(
                name: "tb_import_batch",
                columns: table => new
                {
                    import_batch_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    file_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    file_hash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    import_by = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    import_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    total_rows = table.Column<int>(type: "int", nullable: false),
                    total_success = table.Column<int>(type: "int", nullable: false),
                    total_rejected = table.Column<int>(type: "int", nullable: false),
                    reject_summary = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_tb_import_batch", x => x.import_batch_id);
                });

            migrationBuilder.CreateTable(
                name: "tb_task",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    application_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    request_by = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    request_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    due_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    completed_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    status_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    task_type_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    task_priority_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    import_batch_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_tb_task", x => x.id);
                    table.ForeignKey(
                        name: "FK_tb_task_rf_application_application_id",
                        column: x => x.application_id,
                        principalTable: "rf_application",
                        principalColumn: "application_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_task_rf_status_status_id",
                        column: x => x.status_id,
                        principalTable: "rf_status",
                        principalColumn: "status_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_task_rf_task_priority_task_priority_id",
                        column: x => x.task_priority_id,
                        principalTable: "rf_task_priority",
                        principalColumn: "task_priority_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_task_rf_task_type_task_type_id",
                        column: x => x.task_type_id,
                        principalTable: "rf_task_type",
                        principalColumn: "task_type_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_task_tb_import_batch_import_batch_id",
                        column: x => x.import_batch_id,
                        principalTable: "tb_import_batch",
                        principalColumn: "import_batch_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_task_history",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    task_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    from_status_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    to_status_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    from_priority_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    to_priority_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    change_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    field_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    old_value = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    new_value = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_tb_task_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_tb_task_history_rf_status_from_status_id",
                        column: x => x.from_status_id,
                        principalTable: "rf_status",
                        principalColumn: "status_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_task_history_rf_status_to_status_id",
                        column: x => x.to_status_id,
                        principalTable: "rf_status",
                        principalColumn: "status_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_task_history_rf_task_priority_from_priority_id",
                        column: x => x.from_priority_id,
                        principalTable: "rf_task_priority",
                        principalColumn: "task_priority_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_task_history_rf_task_priority_to_priority_id",
                        column: x => x.to_priority_id,
                        principalTable: "rf_task_priority",
                        principalColumn: "task_priority_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_task_history_tb_task_task_id",
                        column: x => x.task_id,
                        principalTable: "tb_task",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_rf_application_description",
                table: "rf_application",
                column: "description",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rf_status_description",
                table: "rf_status",
                column: "description",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rf_task_priority_description",
                table: "rf_task_priority",
                column: "description",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rf_task_type_description",
                table: "rf_task_type",
                column: "description",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_import_batch_file_hash_import_by",
                table: "tb_import_batch",
                columns: new[] { "file_hash", "import_by" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_import_batch_import_by",
                table: "tb_import_batch",
                column: "import_by");

            migrationBuilder.CreateIndex(
                name: "IX_tb_task_application_id",
                table: "tb_task",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_task_created_by_is_active_is_deleted",
                table: "tb_task",
                columns: new[] { "created_by", "is_active", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_tb_task_import_batch_id",
                table: "tb_task",
                column: "import_batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_task_request_date",
                table: "tb_task",
                column: "request_date");

            migrationBuilder.CreateIndex(
                name: "IX_tb_task_status_id",
                table: "tb_task",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_task_task_priority_id",
                table: "tb_task",
                column: "task_priority_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_task_task_type_id",
                table: "tb_task",
                column: "task_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_task_history_from_priority_id",
                table: "tb_task_history",
                column: "from_priority_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_task_history_from_status_id",
                table: "tb_task_history",
                column: "from_status_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_task_history_task_id_created_date",
                table: "tb_task_history",
                columns: new[] { "task_id", "created_date" });

            migrationBuilder.CreateIndex(
                name: "IX_tb_task_history_to_priority_id",
                table: "tb_task_history",
                column: "to_priority_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_task_history_to_status_id",
                table: "tb_task_history",
                column: "to_status_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_task_history");

            migrationBuilder.DropTable(
                name: "tb_task");

            migrationBuilder.DropTable(
                name: "rf_application");

            migrationBuilder.DropTable(
                name: "rf_status");

            migrationBuilder.DropTable(
                name: "rf_task_priority");

            migrationBuilder.DropTable(
                name: "rf_task_type");

            migrationBuilder.DropTable(
                name: "tb_import_batch");
        }
    }
}
