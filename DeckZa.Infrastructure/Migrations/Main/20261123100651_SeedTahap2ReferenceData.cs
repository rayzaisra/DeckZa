using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeckZa.Infrastructure.Migrations.Main
{
    /// <summary>
    /// Data referensi tahap 2 (Guest tier &amp; billing) untuk database yang sudah terisi.
    /// Migration ini tidak mengubah skema — hanya INSERT idempoten.
    /// Seeder (DeckZaSeeder, PlanSeeder, EmailTemplateSeeder, NotificationSeeder, ParameterSeeder)
    /// tetap diperbaiki agar database kosong juga benar. Dua jalur saling melengkapi.
    /// </summary>
    public partial class SeedTahap2ReferenceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                -- ============================================================
                -- 1. Menu milik Guest (Calendar / TSK-CAL sengaja tidak termasuk)
                -- ============================================================
                IF NOT EXISTS (SELECT 1 FROM um_menu WHERE menu_unique = 'TSK')
                    INSERT INTO um_menu (menu_unique, menu_name, menu_url, parent_level_id, menu_unique_parent, menu_icon, order_seq, component_type, status_approval, is_active, is_deleted, is_locked, is_default, created_by, created_date)
                    VALUES ('TSK', 'Task Workspace', NULL, 'A', '', 'Work', 2, NULL, 4, 1, 0, 0, 0, 'system', GETDATE());

                IF NOT EXISTS (SELECT 1 FROM um_menu WHERE menu_unique = 'TSK-KANBAN')
                    INSERT INTO um_menu (menu_unique, menu_name, menu_url, parent_level_id, menu_unique_parent, menu_icon, order_seq, component_type, status_approval, is_active, is_deleted, is_locked, is_default, created_by, created_date)
                    VALUES ('TSK-KANBAN', 'Task Management', '/dashboard/task-management', 'D', 'TSK', 'Checklist', 2, 'DeckZa.Shared.Pages.AsMember.Kanban, DeckZa.Shared', 4, 1, 0, 0, 0, 'system', GETDATE());

                IF NOT EXISTS (SELECT 1 FROM um_menu WHERE menu_unique = 'TSK-RPT')
                    INSERT INTO um_menu (menu_unique, menu_name, menu_url, parent_level_id, menu_unique_parent, menu_icon, order_seq, component_type, status_approval, is_active, is_deleted, is_locked, is_default, created_by, created_date)
                    VALUES ('TSK-RPT', 'Report', '/dashboard/report', 'D', 'TSK', 'BarChart', 4, 'DeckZa.Shared.Pages.AsMember.Report, DeckZa.Shared', 4, 1, 0, 0, 0, 'system', GETDATE());

                -- ============================================================
                -- 2. Grup Guest + pemetaan menu
                -- ============================================================
                IF NOT EXISTS (SELECT 1 FROM um_group_app WHERE group_id = '99')
                    INSERT INTO um_group_app (group_id, group_name, status_approval, is_active, is_deleted, is_locked, is_default, created_by, created_date)
                    VALUES ('99', 'Guest', 4, 1, 0, 0, 0, 'system', GETDATE());

                INSERT INTO um_group_menu (group_id, menu_id, status_approval, is_active, is_deleted, is_locked, is_default, created_by, created_date)
                SELECT '99', m.id, 4, 1, 0, 0, 0, 'system', GETDATE()
                FROM um_menu m
                WHERE (m.menu_unique IN ('TSK', 'TSK-KANBAN', 'TSK-RPT', 'DOC', 'DOC-FLOW')
                       OR (m.parent_level_id = 'A' AND ISNULL(m.menu_unique_parent, '') = ''))
                  AND NOT EXISTS (SELECT 1 FROM um_group_menu gm WHERE gm.group_id = '99' AND gm.menu_id = m.id);

                -- ============================================================
                -- 3. Kartu Parameter Engine
                -- ============================================================
                IF NOT EXISTS (SELECT 1 FROM tb_parameter_system WHERE table_name = 'rf_plan')
                    INSERT INTO tb_parameter_system (table_name, description, param_maker_url, param_approval_url, is_param_maker, is_param_approval, is_param_upload, icon)
                    VALUES ('rf_plan', 'Plan', '/dashboard/maintenance/parameter-system/maker/rf_plan', '/dashboard/maintenance/parameter-system/approval/rf_plan', 1, 1, 0, 'WorkspacePremium');

                IF NOT EXISTS (SELECT 1 FROM tb_parameter_system WHERE table_name = 'rf_plan_feature')
                    INSERT INTO tb_parameter_system (table_name, description, param_maker_url, param_approval_url, is_param_maker, is_param_approval, is_param_upload, icon)
                    VALUES ('rf_plan_feature', 'Plan Feature', '/dashboard/maintenance/parameter-system/maker/rf_plan_feature', '/dashboard/maintenance/parameter-system/approval/rf_plan_feature', 1, 1, 0, 'List');

                IF NOT EXISTS (SELECT 1 FROM tb_parameter_system WHERE table_name = 'tb_payment_request')
                    INSERT INTO tb_parameter_system (table_name, description, param_maker_url, param_approval_url, is_param_maker, is_param_approval, is_param_upload, icon)
                    VALUES ('tb_payment_request', 'Payment Request', '/dashboard/maintenance/parameter-system/maker/tb_payment_request', '/dashboard/maintenance/parameter-system/approval/tb_payment_request', 1, 1, 0, 'Payment');

                IF NOT EXISTS (SELECT 1 FROM tb_parameter_system WHERE table_name = 'tb_payment_invoice')
                    INSERT INTO tb_parameter_system (table_name, description, param_maker_url, param_approval_url, is_param_maker, is_param_approval, is_param_upload, icon)
                    VALUES ('tb_payment_invoice', 'Payment Invoice', '/dashboard/maintenance/parameter-system/maker/tb_payment_invoice', '/dashboard/maintenance/parameter-system/approval/tb_payment_invoice', 1, 1, 0, 'Receipt');

                -- ============================================================
                -- 4. Metadata field Parameter Engine
                -- ============================================================
                INSERT INTO tb_parameter_system_detail (table_name, field_name, order_seq, field_desc, field_key, field_type, field_reff, is_mandatory, is_group_by, is_orderable, add_class)
                SELECT v.table_name, v.field_name, v.order_seq, v.field_desc, v.field_key, v.field_type, v.field_reff, v.is_mandatory, NULL, 1, NULL
                FROM (VALUES
                    ('rf_plan', 'id', 1, 'Plan ID', 1, 'text', CAST(NULL AS nvarchar(2000)), 1),
                    ('rf_plan', 'plan_name', 2, 'Plan Name', 0, 'text', CAST(NULL AS nvarchar(2000)), 1),
                    ('rf_plan', 'description', 3, 'Description', 0, 'text', CAST(NULL AS nvarchar(2000)), 0),
                    ('rf_plan', 'price_amount', 4, 'Price', 0, 'number', CAST(NULL AS nvarchar(2000)), 0),
                    ('rf_plan', 'price_currency', 5, 'Currency', 0, 'text', CAST(NULL AS nvarchar(2000)), 0),
                    ('rf_plan', 'billing_period', 6, 'Billing Period', 0, 'list', N'select N''ONE_TIME'' as value, N''ONE_TIME'' as text union all select N''MONTHLY'' as value, N''MONTHLY'' as text union all select N''YEARLY'' as value, N''YEARLY'' as text', 1),
                    ('rf_plan', 'billing_interval_count', 7, 'Billing Interval', 0, 'number', CAST(NULL AS nvarchar(2000)), 0),
                    ('rf_plan', 'max_active_tasks', 8, 'Max Active Tasks', 0, 'number', CAST(NULL AS nvarchar(2000)), 0),
                    ('rf_plan', 'report_period_months', 9, 'Report Period (Months)', 0, 'number', CAST(NULL AS nvarchar(2000)), 0),
                    ('rf_plan', 'allows_calendar', 10, 'Allow Calendar', 0, 'bit', CAST(NULL AS nvarchar(2000)), 0),
                    ('rf_plan', 'allows_excel_import', 11, 'Allow Excel Import', 0, 'bit', CAST(NULL AS nvarchar(2000)), 0),
                    ('rf_plan', 'allows_supervisor_mode', 12, 'Allow Supervisor Mode', 0, 'bit', CAST(NULL AS nvarchar(2000)), 0),
                    ('rf_plan', 'upgrade_group_id', 13, 'Upgrade Group', 0, 'list', N'select group_id as value, group_name as text from um_group_app where is_active=1 and is_deleted=0', 0),
                    ('rf_plan', 'order_seq', 14, 'Order', 0, 'number', CAST(NULL AS nvarchar(2000)), 0),
                    ('rf_plan', 'is_active', 15, 'Active', 0, 'bit', CAST(NULL AS nvarchar(2000)), 0),

                    ('rf_plan_feature', 'plan_id', 1, 'Plan', 1, 'list', N'select id as value, plan_name as text from rf_plan where is_active=1 and is_deleted=0', 1),
                    ('rf_plan_feature', 'feature_text', 2, 'Feature', 1, 'text', CAST(NULL AS nvarchar(2000)), 1),
                    ('rf_plan_feature', 'order_seq', 3, 'Order', 0, 'number', CAST(NULL AS nvarchar(2000)), 0),
                    ('rf_plan_feature', 'is_active', 4, 'Active', 0, 'bit', CAST(NULL AS nvarchar(2000)), 0),

                    ('tb_payment_request', 'request_no', 1, 'Request No', 1, 'text', CAST(NULL AS nvarchar(2000)), 1),
                    ('tb_payment_request', 'user_id', 2, 'User', 0, 'text', CAST(NULL AS nvarchar(2000)), 1),
                    ('tb_payment_request', 'plan_id', 3, 'Plan', 0, 'list', N'select id as value, plan_name as text from rf_plan where is_active=1 and is_deleted=0', 0),
                    ('tb_payment_request', 'amount', 4, 'Amount', 0, 'number', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_request', 'currency', 5, 'Currency', 0, 'text', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_request', 'payment_method', 6, 'Payment Method', 0, 'text', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_request', 'transfer_reference', 7, 'Transfer Reference', 0, 'text', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_request', 'proof_file_name', 8, 'Proof File Name', 0, 'text', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_request', 'proof_stored_path', 9, 'Proof Stored Path', 0, 'text', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_request', 'proof_content_type', 10, 'Proof Content Type', 0, 'text', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_request', 'proof_size_bytes', 11, 'Proof Size (Bytes)', 0, 'number', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_request', 'status', 12, 'Status', 0, 'list', N'select N''PENDING'' as value, N''PENDING'' as text union all select N''APPROVED'' as value, N''APPROVED'' as text union all select N''REJECTED'' as value, N''REJECTED'' as text', 0),
                    ('tb_payment_request', 'reviewed_by', 13, 'Reviewed By', 0, 'text', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_request', 'reviewed_date', 14, 'Reviewed Date', 0, 'date', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_request', 'review_note', 15, 'Review Note', 0, 'textarea', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_request', 'activated_from', 16, 'Activated From', 0, 'date', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_request', 'activated_to', 17, 'Activated To', 0, 'date', CAST(NULL AS nvarchar(2000)), 0),

                    ('tb_payment_invoice', 'invoice_no', 1, 'Invoice No', 1, 'text', CAST(NULL AS nvarchar(2000)), 1),
                    ('tb_payment_invoice', 'payment_request_id', 2, 'Payment Request ID', 0, 'number', CAST(NULL AS nvarchar(2000)), 1),
                    ('tb_payment_invoice', 'user_id', 3, 'User', 0, 'text', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_invoice', 'plan_id', 4, 'Plan', 0, 'text', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_invoice', 'plan_name', 5, 'Plan Name', 0, 'text', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_invoice', 'amount', 6, 'Amount', 0, 'number', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_invoice', 'currency', 7, 'Currency', 0, 'text', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_invoice', 'billing_period', 8, 'Billing Period', 0, 'text', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_invoice', 'invoice_date', 9, 'Invoice Date', 0, 'date', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_invoice', 'activated_from', 10, 'Activated From', 0, 'date', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_invoice', 'activated_to', 11, 'Activated To', 0, 'date', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_invoice', 'status', 12, 'Status', 0, 'text', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_invoice', 'file_name', 13, 'File Name', 0, 'text', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_invoice', 'stored_path', 14, 'Stored Path', 0, 'text', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_invoice', 'content_type', 15, 'Content Type', 0, 'text', CAST(NULL AS nvarchar(2000)), 0),
                    ('tb_payment_invoice', 'size_bytes', 16, 'Size (Bytes)', 0, 'number', CAST(NULL AS nvarchar(2000)), 0)
                ) AS v (table_name, field_name, order_seq, field_desc, field_key, field_type, field_reff, is_mandatory)
                WHERE NOT EXISTS (
                    SELECT 1 FROM tb_parameter_system_detail d
                    WHERE d.table_name = v.table_name AND d.field_name = v.field_name);

                -- ============================================================
                -- 5. Katalog plan + fitur
                -- ============================================================
                IF NOT EXISTS (SELECT 1 FROM rf_plan WHERE id = 'PLAN-GUEST')
                    INSERT INTO rf_plan (id, plan_name, description, price_amount, price_currency, billing_period, billing_interval_count, max_active_tasks, report_period_months, allows_calendar, allows_excel_import, allows_supervisor_mode, upgrade_group_id, order_seq, status_approval, is_active, is_deleted, is_locked, is_default, approved_by, approved_date, created_by, created_date)
                    VALUES ('PLAN-GUEST', 'Guest', 'Akses terbatas untuk pengguna yang baru mendaftar: maksimal 50 task aktif dan report hanya periode bulan berjalan.', 0, 'IDR', 'ONE_TIME', 1, 50, 1, 0, 0, 0, '99', 1, 4, 1, 0, 0, 0, 'system', GETDATE(), 'system', GETDATE());

                IF NOT EXISTS (SELECT 1 FROM rf_plan WHERE id = 'PLAN-MEMBER')
                    INSERT INTO rf_plan (id, plan_name, description, price_amount, price_currency, billing_period, billing_interval_count, max_active_tasks, report_period_months, allows_calendar, allows_excel_import, allows_supervisor_mode, upgrade_group_id, order_seq, status_approval, is_active, is_deleted, is_locked, is_default, approved_by, approved_date, created_by, created_date)
                    VALUES ('PLAN-MEMBER', 'Member', 'Akses penuh: task tanpa batas, report periode bebas, kalender, dan import Excel.', 49000, 'IDR', 'ONE_TIME', 1, NULL, NULL, 1, 1, 1, '01', 2, 4, 1, 0, 0, 0, 'system', GETDATE(), 'system', GETDATE());

                INSERT INTO rf_plan_feature (plan_id, feature_text, order_seq, status_approval, is_active, is_deleted, is_locked, is_default, approved_by, approved_date, created_by, created_date)
                SELECT v.plan_id, v.feature_text, v.order_seq, 4, 1, 0, 0, 0, 'system', GETDATE(), 'system', GETDATE()
                FROM (VALUES
                    ('PLAN-GUEST', 'Task management up to 50 task', 1),
                    ('PLAN-GUEST', 'Report performance per bulan today', 2),
                    ('PLAN-MEMBER', 'Task management unlimited', 1),
                    ('PLAN-MEMBER', 'Report Performance unlimited periodenya', 2),
                    ('PLAN-MEMBER', 'Calendar task', 3),
                    ('PLAN-MEMBER', 'Support supervisor mode', 4)
                ) AS v (plan_id, feature_text, order_seq)
                WHERE NOT EXISTS (
                    SELECT 1 FROM rf_plan_feature f
                    WHERE f.plan_id = v.plan_id AND f.feature_text = v.feature_text);

                -- ============================================================
                -- 6. Template email (isi \n diganti LF agar sama dengan seeder)
                -- ============================================================
                INSERT INTO rf_email_template (template_key, subject, body_html, body_text, is_active, is_deleted, created_by, created_date)
                SELECT v.template_key, v.subject, v.body_html, REPLACE(v.body_text, '\n', CHAR(10)), 1, 0, 'system', GETDATE()
                FROM (VALUES
                    ('REGISTRATION_RECEIVED', 'Pendaftaran DeckZa diterima',
                        '<p>Halo {{full_name}},</p><p>Pendaftaran DeckZa Anda ({{user_id}}) telah kami terima dan sedang menunggu persetujuan administrator.</p><p>Anda akan menerima email kembali setelah pendaftaran disetujui atau ditolak.</p>',
                        'Halo {{full_name}},\n\nPendaftaran DeckZa Anda ({{user_id}}) telah kami terima dan sedang menunggu persetujuan administrator.\n\nAnda akan menerima email kembali setelah pendaftaran disetujui atau ditolak.'),
                    ('REGISTRATION_APPROVED', 'Pendaftaran DeckZa disetujui',
                        '<p>Halo {{full_name}},</p><p>Pendaftaran DeckZa Anda ({{user_id}}) telah disetujui. Silakan masuk untuk mulai bekerja.</p><p><a href="{{link}}">Login ke DeckZa</a></p>',
                        'Halo {{full_name}},\n\nPendaftaran DeckZa Anda ({{user_id}}) telah disetujui. Silakan masuk untuk mulai bekerja.\n\nLogin: {{link}}'),
                    ('REGISTRATION_REJECTED', 'Pendaftaran DeckZa ditolak',
                        '<p>Halo {{full_name}},</p><p>Pendaftaran DeckZa Anda ({{user_id}}) telah ditolak. {{long_text}}</p><p>Silakan hubungi administrator bila Anda ingin mendaftar ulang.</p>',
                        'Halo {{full_name}},\n\nPendaftaran DeckZa Anda ({{user_id}}) telah ditolak. {{long_text}}\n\nSilakan hubungi administrator bila Anda ingin mendaftar ulang.'),
                    ('PAYMENT_RECEIVED', 'Pengajuan pembayaran diterima',
                        '<p>Halo {{full_name}},</p><p>Pengajuan pembayaran Anda telah kami terima dan sedang menunggu persetujuan administrator.</p><p>Nominal: {{long_text}}</p><p>Anda akan menerima email kembali setelah pengajuan disetujui atau ditolak.</p>',
                        'Halo {{full_name}},\n\nPengajuan pembayaran Anda telah kami terima dan sedang menunggu persetujuan administrator.\n\nNominal: {{long_text}}\n\nAnda akan menerima email kembali setelah pengajuan disetujui atau ditolak.'),
                    ('PAYMENT_APPROVED', 'Upgrade plan disetujui',
                        '<p>Halo {{full_name}},</p><p>Upgrade plan Anda telah disetujui. {{long_text}}</p><p>Invoice: {{link}}</p><p>Silakan login kembali agar akses baru langsung berlaku.</p>',
                        'Halo {{full_name}},\n\nUpgrade plan Anda telah disetujui. {{long_text}}\n\nInvoice: {{link}}\n\nSilakan login kembali agar akses baru langsung berlaku.'),
                    ('PAYMENT_REJECTED', 'Upgrade plan ditolak',
                        '<p>Halo {{full_name}},</p><p>Upgrade plan Anda telah ditolak. {{long_text}}</p><p>Silakan periksa kembali data pembayaran dan ajukan ulang.</p>',
                        'Halo {{full_name}},\n\nUpgrade plan Anda telah ditolak. {{long_text}}\n\nSilakan periksa kembali data pembayaran dan ajukan ulang.'),
                    ('NOTIFICATION', 'Notifikasi DeckZa',
                        '<p>Halo {{full_name}},</p><p>{{short_text}}</p><p>{{long_text}}</p><p><a href="{{link}}">Buka DeckZa</a></p>',
                        'Halo {{full_name}},\n\n{{short_text}}\n\n{{long_text}}\n\nBuka DeckZa: {{link}}'),
                    ('FORGOT_PASSWORD', 'Reset password DeckZa',
                        '<p>Halo {{full_name}},</p><p>Gunakan tautan berikut untuk mengatur ulang password Anda.</p><p><a href="{{link}}">Reset Password</a></p><p>Tautan ini berlaku 30 menit.</p>',
                        'Halo {{full_name}},\n\nAtur ulang password Anda melalui tautan berikut:\n\n{{link}}\n\nTautan ini berlaku 30 menit.')
                ) AS v (template_key, subject, body_html, body_text)
                WHERE NOT EXISTS (
                    SELECT 1 FROM rf_email_template t
                    WHERE t.template_key = v.template_key);

                -- ============================================================
                -- 7. Tipe notifikasi tahap 2
                -- ============================================================
                IF NOT EXISTS (SELECT 1 FROM rf_notification_type WHERE id = '44444444-4444-4444-4444-444444444444')
                    INSERT INTO rf_notification_type (id, short_text, long_text, href, is_send_mail, is_send_web, status_approval, approval_type_id, is_locked, is_default, is_active, is_deleted, approved_by, approved_date, created_by, created_date)
                    VALUES ('44444444-4444-4444-4444-444444444444', 'User Registration', 'Ada pendaftaran user baru yang menunggu persetujuan.', NULL, 0, 1, 4, 'A', 0, 0, 1, 0, 'system', GETDATE(), 'system', GETDATE());

                IF NOT EXISTS (SELECT 1 FROM rf_notification_type WHERE id = '55555555-5555-5555-5555-555555555555')
                    INSERT INTO rf_notification_type (id, short_text, long_text, href, is_send_mail, is_send_web, status_approval, approval_type_id, is_locked, is_default, is_active, is_deleted, approved_by, approved_date, created_by, created_date)
                    VALUES ('55555555-5555-5555-5555-555555555555', 'Payment Request', 'Ada pengajuan pembayaran yang menunggu persetujuan.', NULL, 0, 1, 4, 'A', 0, 0, 1, 0, 'system', GETDATE(), 'system', GETDATE());
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Seed data sengaja tidak dibalik.
            //
            // Up() memakai guard IF NOT EXISTS, sehingga tidak dapat membedakan
            // baris yang dibuat oleh migration ini dari baris yang sudah ada
            // sebelum migration dijalankan. Menghapusnya berisiko menghilangkan
            // data nyata:
            //   - um_group_app / um_group_menu grup '99': user Guest yang sudah
            //     di-approve akan kehilangan grup dan tidak bisa login.
            //   - rf_email_template: template bisa sudah ada dan sudah diedit admin.
            //   - tb_parameter_system(_detail): kartu bisa sudah ada sebelum migration.
            //
            // Rollback yang aman dilakukan manual setelah memastikan tidak ada
            // data yang bergantung.
        }
    }
}
