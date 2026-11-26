using MudBlazor;
using DeckZa.Shared.Services;

namespace DeckZa.Shared.Pages.Documentation;

/// <summary>
/// Katalog alur pemakaian aplikasi DeckZa sebagai task management.
/// Seluruh teks diambil dari <see cref="LanguageService"/> agar halaman
/// DeckZa Flows mengikuti bahasa aktif tanpa teks campuran.
/// </summary>
public static class DeckZaFlowCatalog
{
    public static IReadOnlyList<Flow> GetFlows(LanguageService L) => new[]
    {
        new Flow
        {
            Key = "getting-started",
            Title = L["DzGettingStartedTitle"],
            Tab = L["DzGettingStartedTitle"],
            Icon = Icons.Material.Filled.Dashboard,
            Summary = L["DzGettingStartedSummary"],
            Audience = L["DzGettingStartedAudience"],
            Approval = L["DzGettingStartedApproval"],
            Prerequisites = new[]
            {
                L["DzGettingStartedPre1"],
                L["DzGettingStartedPre2"],
            },
            Steps = new[]
            {
                new Step
                {
                    Title = L["DzGettingStartedStep1Title"],
                    Detail = L["DzGettingStartedStep1Detail"],
                },
                new Step
                {
                    Title = L["DzGettingStartedStep2Title"],
                    Detail = L["DzGettingStartedStep2Detail"],
                },
                new Step
                {
                    Title = L["DzGettingStartedStep3Title"],
                    Detail = L["DzGettingStartedStep3Detail"],
                },
                new Step
                {
                    Title = L["DzGettingStartedStep4Title"],
                    Detail = L["DzGettingStartedStep4Detail"],
                },
                new Step
                {
                    Title = L["DzGettingStartedStep5Title"],
                    Detail = L["DzGettingStartedStep5Detail"],
                },
            },
            Warnings = new[]
            {
                L["DzGettingStartedWarn1"],
                L["DzGettingStartedWarn2"],
            },
            Technical = new[]
            {
                "Groups: 00 = Administrator, 01 = Member, 99 = Guest",
                "Tables: um_user_app, um_user_group, rf_plan",
                "Master tables: rf_application, rf_task_type, rf_task_priority, rf_status",
            },
        },
        new Flow
        {
            Key = "task-parameter",
            Title = L["DzTaskParameterTitle"],
            Tab = L["DzTaskParameterTitle"],
            Icon = Icons.Material.Filled.Tune,
            Summary = L["DzTaskParameterSummary"],
            Audience = L["DzTaskParameterAudience"],
            Approval = L["DzTaskParameterApproval"],
            Prerequisites = new[]
            {
                L["DzTaskParameterPre1"],
                L["DzTaskParameterPre2"],
            },
            Steps = new[]
            {
                new Step
                {
                    Title = L["DzTaskParameterStep1Title"],
                    Detail = L["DzTaskParameterStep1Detail"],
                },
                new Step
                {
                    Title = L["DzTaskParameterStep2Title"],
                    Detail = L["DzTaskParameterStep2Detail"],
                },
                new Step
                {
                    Title = L["DzTaskParameterStep3Title"],
                    Detail = L["DzTaskParameterStep3Detail"],
                },
                new Step
                {
                    Title = L["DzTaskParameterStep4Title"],
                    Detail = L["DzTaskParameterStep4Detail"],
                },
                new Step
                {
                    Title = L["DzTaskParameterStep5Title"],
                    Detail = L["DzTaskParameterStep5Detail"],
                },
                new Step
                {
                    Title = L["DzTaskParameterStep6Title"],
                    Detail = L["DzTaskParameterStep6Detail"],
                },
                new Step
                {
                    Title = L["DzTaskParameterStep7Title"],
                    Detail = L["DzTaskParameterStep7Detail"],
                },
            },
            Warnings = new[]
            {
                L["DzTaskParameterWarn1"],
                L["DzTaskParameterWarn2"],
                L["DzTaskParameterWarn3"],
                L["DzTaskParameterWarn4"],
                L["DzTaskParameterWarn5"],
                L["DzTaskParameterWarn6"],
            },
            Technical = new[]
            {
                "Pages: /dashboard/task-parameter/application, task-type, priority, status",
                "Menus: TSKP (root), TSKP-APP, TSKP-TYP, TSKP-PRI, TSKP-STA",
                "Tables: rf_application, rf_task_type, rf_task_priority, rf_status",
                "Ownership column: owner_user_id (not null)",
                "Status flags: is_initial_status, is_completed_status",
                "Unique per owner: (owner_user_id, description)",
                "Quota column: rf_plan.max_master_rows_per_table",
            },
        },
        new Flow
        {
            Key = "task-management",
            Title = L["DzTaskManagementTitle"],
            Tab = L["DzTaskManagementTitle"],
            Icon = Icons.Material.Filled.ViewKanban,
            Summary = L["DzTaskManagementSummary"],
            Audience = L["DzTaskManagementAudience"],
            Approval = L["DzTaskManagementApproval"],
            Prerequisites = new[]
            {
                L["DzTaskManagementPre1"],
                L["DzTaskManagementPre2"],
            },
            Steps = new[]
            {
                new Step
                {
                    Title = L["DzTaskManagementStep1Title"],
                    Detail = L["DzTaskManagementStep1Detail"],
                },
                new Step
                {
                    Title = L["DzTaskManagementStep2Title"],
                    Detail = L["DzTaskManagementStep2Detail"],
                },
                new Step
                {
                    Title = L["DzTaskManagementStep3Title"],
                    Detail = L["DzTaskManagementStep3Detail"],
                },
                new Step
                {
                    Title = L["DzTaskManagementStep4Title"],
                    Detail = L["DzTaskManagementStep4Detail"],
                },
                new Step
                {
                    Title = L["DzTaskManagementStep5Title"],
                    Detail = L["DzTaskManagementStep5Detail"],
                },
                new Step
                {
                    Title = L["DzTaskManagementStep6Title"],
                    Detail = L["DzTaskManagementStep6Detail"],
                },
            },
            Warnings = new[]
            {
                L["DzTaskManagementWarn1"],
                L["DzTaskManagementWarn2"],
                L["DzTaskManagementWarn3"],
                L["DzTaskManagementWarn4"],
            },
            Technical = new[]
            {
                "Page: /dashboard/task-management",
                "APIs: /api/task, /api/task/{id}, /api/task/{id}/status",
                "Tables: tb_task, tb_task_history",
                "Quota column: rf_plan.max_active_tasks",
            },
        },
        new Flow
        {
            Key = "calendar",
            Title = L["DzCalendarTitle"],
            Tab = L["DzCalendarTitle"],
            Icon = Icons.Material.Filled.CalendarMonth,
            Summary = L["DzCalendarSummary"],
            Audience = L["DzCalendarAudience"],
            Approval = L["DzCalendarApproval"],
            Prerequisites = new[]
            {
                L["DzCalendarPre1"],
                L["DzCalendarPre2"],
            },
            Steps = new[]
            {
                new Step
                {
                    Title = L["DzCalendarStep1Title"],
                    Detail = L["DzCalendarStep1Detail"],
                },
                new Step
                {
                    Title = L["DzCalendarStep2Title"],
                    Detail = L["DzCalendarStep2Detail"],
                },
                new Step
                {
                    Title = L["DzCalendarStep3Title"],
                    Detail = L["DzCalendarStep3Detail"],
                },
                new Step
                {
                    Title = L["DzCalendarStep4Title"],
                    Detail = L["DzCalendarStep4Detail"],
                },
            },
            Warnings = new[]
            {
                L["DzCalendarWarn1"],
                L["DzCalendarWarn2"],
                L["DzCalendarWarn3"],
            },
            Technical = new[]
            {
                "Page: /dashboard/calendar",
                "API: /api/task/calendar",
                "Plan flag: rf_plan.allows_calendar",
            },
        },
        new Flow
        {
            Key = "report",
            Title = L["DzReportTitle"],
            Tab = L["DzReportTitle"],
            Icon = Icons.Material.Filled.BarChart,
            Summary = L["DzReportSummary"],
            Audience = L["DzReportAudience"],
            Approval = L["DzReportApproval"],
            Prerequisites = new[]
            {
                L["DzReportPre1"],
                L["DzReportPre2"],
            },
            Steps = new[]
            {
                new Step
                {
                    Title = L["DzReportStep1Title"],
                    Detail = L["DzReportStep1Detail"],
                },
                new Step
                {
                    Title = L["DzReportStep2Title"],
                    Detail = L["DzReportStep2Detail"],
                },
                new Step
                {
                    Title = L["DzReportStep3Title"],
                    Detail = L["DzReportStep3Detail"],
                },
                new Step
                {
                    Title = L["DzReportStep4Title"],
                    Detail = L["DzReportStep4Detail"],
                },
            },
            Warnings = new[]
            {
                L["DzReportWarn1"],
                L["DzReportWarn2"],
                L["DzReportWarn3"],
                L["DzReportWarn4"],
            },
            Technical = new[]
            {
                "Page: /dashboard/report",
                "APIs: /api/report/preview, /api/report/export",
                "Table: tb_export_job",
                "Plan column: rf_plan.report_period_months",
            },
        },
        new Flow
        {
            Key = "guest-guide",
            Title = L["DzGuestGuideTitle"],
            Tab = L["DzGuestGuideTitle"],
            Icon = Icons.Material.Filled.PersonOutline,
            Summary = L["DzGuestGuideSummary"],
            Audience = L["DzGuestGuideAudience"],
            Approval = L["DzGuestGuideApproval"],
            Prerequisites = new[]
            {
                L["DzGuestGuidePre1"],
            },
            Steps = new[]
            {
                new Step
                {
                    Title = L["DzGuestGuideStep1Title"],
                    Detail = L["DzGuestGuideStep1Detail"],
                },
                new Step
                {
                    Title = L["DzGuestGuideStep2Title"],
                    Detail = L["DzGuestGuideStep2Detail"],
                },
                new Step
                {
                    Title = L["DzGuestGuideStep3Title"],
                    Detail = L["DzGuestGuideStep3Detail"],
                },
                new Step
                {
                    Title = L["DzGuestGuideStep4Title"],
                    Detail = L["DzGuestGuideStep4Detail"],
                },
                new Step
                {
                    Title = L["DzGuestGuideStep5Title"],
                    Detail = L["DzGuestGuideStep5Detail"],
                },
                new Step
                {
                    Title = L["DzGuestGuideStep6Title"],
                    Detail = L["DzGuestGuideStep6Detail"],
                },
                new Step
                {
                    Title = L["DzGuestGuideStep7Title"],
                    Detail = L["DzGuestGuideStep7Detail"],
                },
            },
            Warnings = new[]
            {
                L["DzGuestGuideWarn1"],
                L["DzGuestGuideWarn2"],
                L["DzGuestGuideWarn3"],
                L["DzGuestGuideWarn4"],
                L["DzGuestGuideWarn5"],
            },
            Technical = new[]
            {
                "Guest group: 99",
                "Plan: PLAN-GUEST",
                "Limit columns: max_active_tasks = 50, report_period_months = 1, max_master_rows_per_table = 3",
                "Flags: allows_excel_import = 0, allows_calendar = 0",
                "Approval: tb_payment_request -> PENDING, APPROVED, REJECTED",
            },
        },
    };

    public sealed class Flow
    {
        public string Key { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Tab { get; init; } = string.Empty;
        public string Icon { get; init; } = Icons.Material.Filled.Circle;
        public string Summary { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
        public string Approval { get; init; } = string.Empty;
        public IReadOnlyList<string> Prerequisites { get; init; } = Array.Empty<string>();
        public IReadOnlyList<Step> Steps { get; init; } = Array.Empty<Step>();
        public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> Technical { get; init; } = Array.Empty<string>();
    }

    public sealed class Step
    {
        public string Title { get; init; } = string.Empty;
        public string Detail { get; init; } = string.Empty;
    }
}
