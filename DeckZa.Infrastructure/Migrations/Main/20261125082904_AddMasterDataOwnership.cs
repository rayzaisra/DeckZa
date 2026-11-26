using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeckZa.Infrastructure.Migrations.Main
{
    /// <inheritdoc />
    public partial class AddMasterDataOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "owner_user_id", table: "rf_task_type", type: "varchar(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<string>(name: "owner_user_id", table: "rf_task_priority", type: "varchar(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<string>(name: "owner_user_id", table: "rf_status", type: "varchar(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<string>(name: "owner_user_id", table: "rf_application", type: "varchar(100)", maxLength: 100, nullable: true);

            migrationBuilder.Sql("UPDATE rf_task_type SET owner_user_id = 'rayza' WHERE owner_user_id IS NULL OR LTRIM(RTRIM(owner_user_id)) = '';");
            migrationBuilder.Sql("UPDATE rf_task_priority SET owner_user_id = 'rayza' WHERE owner_user_id IS NULL OR LTRIM(RTRIM(owner_user_id)) = '';");
            migrationBuilder.Sql("UPDATE rf_status SET owner_user_id = 'rayza' WHERE owner_user_id IS NULL OR LTRIM(RTRIM(owner_user_id)) = '';");
            migrationBuilder.Sql("UPDATE rf_application SET owner_user_id = 'rayza' WHERE owner_user_id IS NULL OR LTRIM(RTRIM(owner_user_id)) = '';");

            migrationBuilder.AlterColumn<string>(name: "owner_user_id", table: "rf_task_type", type: "varchar(100)", maxLength: 100, nullable: false, defaultValue: "", oldClrType: typeof(string), oldType: "varchar(100)", oldMaxLength: 100, oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "owner_user_id", table: "rf_task_priority", type: "varchar(100)", maxLength: 100, nullable: false, defaultValue: "", oldClrType: typeof(string), oldType: "varchar(100)", oldMaxLength: 100, oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "owner_user_id", table: "rf_status", type: "varchar(100)", maxLength: 100, nullable: false, defaultValue: "", oldClrType: typeof(string), oldType: "varchar(100)", oldMaxLength: 100, oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "owner_user_id", table: "rf_application", type: "varchar(100)", maxLength: 100, nullable: false, defaultValue: "", oldClrType: typeof(string), oldType: "varchar(100)", oldMaxLength: 100, oldNullable: true);

            migrationBuilder.DropIndex(name: "IX_rf_task_type_description", table: "rf_task_type");
            migrationBuilder.DropIndex(name: "IX_rf_task_priority_description", table: "rf_task_priority");
            migrationBuilder.DropIndex(name: "IX_rf_status_description", table: "rf_status");
            migrationBuilder.DropIndex(name: "IX_rf_application_description", table: "rf_application");

            migrationBuilder.CreateIndex(name: "IX_rf_task_type_owner_user_id", table: "rf_task_type", column: "owner_user_id");
            migrationBuilder.CreateIndex(name: "IX_rf_task_type_owner_user_id_description", table: "rf_task_type", columns: new[] { "owner_user_id", "description" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_rf_task_priority_owner_user_id", table: "rf_task_priority", column: "owner_user_id");
            migrationBuilder.CreateIndex(name: "IX_rf_task_priority_owner_user_id_description", table: "rf_task_priority", columns: new[] { "owner_user_id", "description" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_rf_status_owner_user_id", table: "rf_status", column: "owner_user_id");
            migrationBuilder.CreateIndex(name: "IX_rf_status_owner_user_id_description", table: "rf_status", columns: new[] { "owner_user_id", "description" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_rf_application_owner_user_id", table: "rf_application", column: "owner_user_id");
            migrationBuilder.CreateIndex(name: "IX_rf_application_owner_user_id_description", table: "rf_application", columns: new[] { "owner_user_id", "description" }, unique: true);

            migrationBuilder.AddColumn<bool>(name: "is_completed_status", table: "rf_status", type: "bit", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "is_initial_status", table: "rf_status", type: "bit", nullable: false, defaultValue: false);
            migrationBuilder.Sql("UPDATE rf_status SET is_initial_status = 1 WHERE status_id = 'STS001';");
            migrationBuilder.Sql("UPDATE rf_status SET is_completed_status = 1 WHERE status_id = 'STS005';");

            migrationBuilder.AddColumn<int>(name: "max_master_rows_per_table", table: "rf_plan", type: "int", nullable: true);
            migrationBuilder.Sql("UPDATE rf_plan SET max_master_rows_per_table = 3 WHERE id = 'PLAN-GUEST';");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM tb_parameter_system_detail WHERE table_name = 'rf_application' AND field_name = 'owner_user_id')
    INSERT INTO tb_parameter_system_detail (table_name, field_name, field_desc, order_seq, field_key, field_type, is_mandatory, is_group_by, is_orderable)
    VALUES ('rf_application', 'owner_user_id', 'Owner User ID', 5, 0, 'text', 1, 0, 1);
IF NOT EXISTS (SELECT 1 FROM tb_parameter_system_detail WHERE table_name = 'rf_task_type' AND field_name = 'owner_user_id')
    INSERT INTO tb_parameter_system_detail (table_name, field_name, field_desc, order_seq, field_key, field_type, is_mandatory, is_group_by, is_orderable)
    VALUES ('rf_task_type', 'owner_user_id', 'Owner User ID', 5, 0, 'text', 1, 0, 1);
IF NOT EXISTS (SELECT 1 FROM tb_parameter_system_detail WHERE table_name = 'rf_task_priority' AND field_name = 'owner_user_id')
    INSERT INTO tb_parameter_system_detail (table_name, field_name, field_desc, order_seq, field_key, field_type, is_mandatory, is_group_by, is_orderable)
    VALUES ('rf_task_priority', 'owner_user_id', 'Owner User ID', 5, 0, 'text', 1, 0, 1);
IF NOT EXISTS (SELECT 1 FROM tb_parameter_system_detail WHERE table_name = 'rf_status' AND field_name = 'owner_user_id')
    INSERT INTO tb_parameter_system_detail (table_name, field_name, field_desc, order_seq, field_key, field_type, is_mandatory, is_group_by, is_orderable)
    VALUES ('rf_status', 'owner_user_id', 'Owner User ID', 5, 0, 'text', 1, 0, 1);
IF NOT EXISTS (SELECT 1 FROM tb_parameter_system_detail WHERE table_name = 'rf_status' AND field_name = 'is_initial_status')
    INSERT INTO tb_parameter_system_detail (table_name, field_name, field_desc, order_seq, field_key, field_type, is_mandatory, is_group_by, is_orderable)
    VALUES ('rf_status', 'is_initial_status', 'Default Status', 6, 0, 'bit', 0, 0, 1);
IF NOT EXISTS (SELECT 1 FROM tb_parameter_system_detail WHERE table_name = 'rf_status' AND field_name = 'is_completed_status')
    INSERT INTO tb_parameter_system_detail (table_name, field_name, field_desc, order_seq, field_key, field_type, is_mandatory, is_group_by, is_orderable)
    VALUES ('rf_status', 'is_completed_status', 'Completed Status', 7, 0, 'bit', 0, 0, 1);
IF NOT EXISTS (SELECT 1 FROM tb_parameter_system_detail WHERE table_name = 'rf_plan' AND field_name = 'max_master_rows_per_table')
    INSERT INTO tb_parameter_system_detail (table_name, field_name, field_desc, order_seq, field_key, field_type, is_mandatory, is_group_by, is_orderable)
    VALUES ('rf_plan', 'max_master_rows_per_table', 'Max Master Rows Per Table', 16, 0, 'number', 0, 0, 1);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM tb_parameter_system_detail WHERE table_name = 'rf_plan' AND field_name = 'max_master_rows_per_table';");
            migrationBuilder.Sql(@"DELETE FROM tb_parameter_system_detail WHERE table_name = 'rf_status' AND field_name IN ('owner_user_id','is_initial_status','is_completed_status');");
            migrationBuilder.Sql(@"DELETE FROM tb_parameter_system_detail WHERE table_name = 'rf_task_priority' AND field_name = 'owner_user_id';");
            migrationBuilder.Sql(@"DELETE FROM tb_parameter_system_detail WHERE table_name = 'rf_task_type' AND field_name = 'owner_user_id';");
            migrationBuilder.Sql(@"DELETE FROM tb_parameter_system_detail WHERE table_name = 'rf_application' AND field_name = 'owner_user_id';");

            migrationBuilder.DropColumn(name: "max_master_rows_per_table", table: "rf_plan");
            migrationBuilder.DropColumn(name: "is_initial_status", table: "rf_status");
            migrationBuilder.DropColumn(name: "is_completed_status", table: "rf_status");

            migrationBuilder.DropIndex(name: "IX_rf_task_type_owner_user_id", table: "rf_task_type");
            migrationBuilder.DropIndex(name: "IX_rf_task_type_owner_user_id_description", table: "rf_task_type");
            migrationBuilder.DropIndex(name: "IX_rf_task_priority_owner_user_id", table: "rf_task_priority");
            migrationBuilder.DropIndex(name: "IX_rf_task_priority_owner_user_id_description", table: "rf_task_priority");
            migrationBuilder.DropIndex(name: "IX_rf_status_owner_user_id", table: "rf_status");
            migrationBuilder.DropIndex(name: "IX_rf_status_owner_user_id_description", table: "rf_status");
            migrationBuilder.DropIndex(name: "IX_rf_application_owner_user_id", table: "rf_application");
            migrationBuilder.DropIndex(name: "IX_rf_application_owner_user_id_description", table: "rf_application");

            migrationBuilder.DropColumn(name: "owner_user_id", table: "rf_task_type");
            migrationBuilder.DropColumn(name: "owner_user_id", table: "rf_task_priority");
            migrationBuilder.DropColumn(name: "owner_user_id", table: "rf_status");
            migrationBuilder.DropColumn(name: "owner_user_id", table: "rf_application");

            migrationBuilder.CreateIndex(name: "IX_rf_task_type_description", table: "rf_task_type", column: "description", unique: true);
            migrationBuilder.CreateIndex(name: "IX_rf_task_priority_description", table: "rf_task_priority", column: "description", unique: true);
            migrationBuilder.CreateIndex(name: "IX_rf_status_description", table: "rf_status", column: "description", unique: true);
            migrationBuilder.CreateIndex(name: "IX_rf_application_description", table: "rf_application", column: "description", unique: true);
        }
    }
}
