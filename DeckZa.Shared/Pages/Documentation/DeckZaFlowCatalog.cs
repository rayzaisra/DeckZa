using MudBlazor;

namespace DeckZa.Shared.Pages.Documentation;

/// <summary>
/// Katalog alur pemakaian aplikasi DeckZa sebagai task management.
/// Berbeda dari <see cref="FrameworkFlowCatalog"/> yang menjelaskan framework
/// (user, grup, role, menu, parameter), katalog ini menjelaskan cara memakai
/// fitur DeckZa sehari-hari beserta batasan tiap plan.
/// </summary>
public static class DeckZaFlowCatalog
{
    public static IReadOnlyList<Flow> Flows { get; } = new[]
    {
        new Flow
        {
            Key = "getting-started",
            Title = "Getting Started",
            Tab = "Getting Started",
            Icon = Icons.Material.Filled.Dashboard,
            Summary = "Sign in, understand the workspace, and prepare the data every task needs.",
            Audience = "All users",
            Approval = "No approval",
            Prerequisites = new[]
            {
                "An active account that has already been approved.",
                "Your account has a group assigned (Guest 99, Member 01, or Administrator 00).",
            },
            Steps = new[]
            {
                new Step
                {
                    Title = "Sign in",
                    Detail = "Open the login page and sign in with your User ID and password. After an administrator resets your password, you must change it at the first login.",
                },
                new Step
                {
                    Title = "Read the dashboard",
                    Detail = "The dashboard summarises active tasks, due dates, and recent activity. Each block links to the page that holds the detail.",
                },
                new Step
                {
                    Title = "Open pages as tabs",
                    Detail = "Dashboard blocks and the sidebar open a page as a workspace tab instead of a full page reload. Use the tab bar to switch, and the sidebar to open another.",
                },
                new Step
                {
                    Title = "Create your master data first",
                    Detail = "A task needs an Application, a Task Type, a Priority, and a Status. Those lists belong to your own account and start empty, so set them up in My Master Data before creating a task.",
                },
                new Step
                {
                    Title = "Start using the task pages",
                    Detail = "Task Management holds the Kanban board, Calendar shows the due dates on a month grid, and Report turns finished work into totals and an Excel file.",
                },
            },
            Warnings = new[]
            {
                "Without master data you cannot save a task — the form has nothing to pick from.",
                "Each account sees only its own tasks and its own master data.",
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
            Key = "my-master-data",
            Title = "Set Up My Master Data",
            Tab = "My Master Data",
            Icon = Icons.Material.Filled.Tune,
            Summary = "Create the Application, Task Type, Priority, and Status lists that your tasks will use.",
            Audience = "Member and Guest",
            Approval = "No approval",
            Prerequisites = new[]
            {
                "You are signed in.",
                "You know how you want to group your work — otherwise start with the minimal setup below.",
            },
            Steps = new[]
            {
                new Step
                {
                    Title = "Open My Master Data",
                    Detail = "Go to /dashboard/my-master-data, or follow the prompt shown on the Task Management page when your lists are still empty.",
                },
                new Step
                {
                    Title = "Add Applications",
                    Detail = "Open the Application tab and add one row per application you work on, for example the name of a product or a client.",
                },
                new Step
                {
                    Title = "Add Task Types",
                    Detail = "Open the Task Type tab and add the kinds of work you do, for example Bug Fixing, Enhancement, or Daily Routine.",
                },
                new Step
                {
                    Title = "Add Priorities",
                    Detail = "Open the Priority tab and add the urgency levels you use. Order them from lowest to highest.",
                },
                new Step
                {
                    Title = "Add Statuses",
                    Detail = "Open the Status tab and add the stages a task moves through, for example Draft, In Progress, and Done.",
                },
                new Step
                {
                    Title = "Mark one Default and one Completed status",
                    Detail = "Switch on Default for the status a new task should start in, and switch on Completed for the status that means the work is finished. Marking a new one releases the mark from the previous status.",
                },
                new Step
                {
                    Title = "Go back to Task Management",
                    Detail = "The dropdowns are filled now, so you can create your first task.",
                },
            },
            Warnings = new[]
            {
                "Every account starts with an empty list. Nothing is shared between accounts.",
                "Guest plan allows at most 3 active rows per table — the page shows the counter, for example 2 / 3.",
                "Only one Status can be Default and only one can be Completed at a time.",
                "A master row that is already used by a task cannot be deleted; deactivate it instead so the task history stays intact.",
                "The Completed status drives the report: tasks in it fill the completed date, stop counting as overdue, and leave the Next up list.",
                "An administrator can see and edit your master rows through Parameter Engine. Changes there apply to your tasks immediately.",
            },
            Technical = new[]
            {
                "Page: /dashboard/my-master-data",
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
            Title = "Manage Tasks",
            Tab = "Task Management",
            Icon = Icons.Material.Filled.ViewKanban,
            Summary = "Create tasks on the Kanban board, move them between statuses, and keep the history correct.",
            Audience = "Member and Guest",
            Approval = "No approval",
            Prerequisites = new[]
            {
                "My Master Data already has at least one Application, Task Type, Priority, and Status.",
                "A Default status is marked, so a new task knows where to start.",
            },
            Steps = new[]
            {
                new Step
                {
                    Title = "Open the board",
                    Detail = "Go to Task Management. Each column is one Status, and each card is one task.",
                },
                new Step
                {
                    Title = "Create a task",
                    Detail = "Press New Task, then fill in the title, the Application, Task Type, Priority, request date, and due date.",
                },
                new Step
                {
                    Title = "Move a task",
                    Detail = "Drag the card to another column. The status changes immediately and the move is written to the task history.",
                },
                new Step
                {
                    Title = "Edit a task",
                    Detail = "Open the card to change the title, description, dates, priority, or status. Every change is recorded in the history with who changed it and when.",
                },
                new Step
                {
                    Title = "Complete a task",
                    Detail = "Move the card to the status marked Completed. The completed date is filled in automatically, the task stops counting as overdue, and it leaves the Next up list.",
                },
                new Step
                {
                    Title = "Delete a task",
                    Detail = "Deleting marks the row as inactive and deleted instead of removing it, so the history and the report totals stay correct.",
                },
            },
            Warnings = new[]
            {
                "Guest plan allows at most 50 active tasks. The counter appears next to the board title; deleting a task frees a slot.",
                "Excel import and the import template are not available on the Guest plan.",
                "A task can only use master data owned by the same account.",
                "Editing a completed task does not reopen it — move it back to another status first.",
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
            Title = "Read the Calendar",
            Tab = "Calendar",
            Icon = Icons.Material.Filled.CalendarMonth,
            Summary = "See tasks on a month grid by due date and open the task from the day cell.",
            Audience = "Member and Administrator",
            Approval = "No approval",
            Prerequisites = new[]
            {
                "You have at least one task with a due date.",
                "Your plan includes the Calendar — the Guest plan does not.",
            },
            Steps = new[]
            {
                new Step
                {
                    Title = "Open the Calendar",
                    Detail = "Go to Calendar from the sidebar. The current month is shown.",
                },
                new Step
                {
                    Title = "Move between months",
                    Detail = "Use the previous and next arrows to change the month.",
                },
                new Step
                {
                    Title = "Open a task",
                    Detail = "Click the entry in a day cell to open the task detail.",
                },
                new Step
                {
                    Title = "Read the colours",
                    Detail = "Each status has its own tone, so the board and the calendar stay consistent.",
                },
            },
            Warnings = new[]
            {
                "The Guest plan does not include the Calendar. Opening the page directly shows a notice instead of data.",
                "A task without a due date does not appear on the calendar.",
                "The calendar only shows tasks owned by the signed-in account.",
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
            Title = "Read the Report",
            Tab = "Report",
            Icon = Icons.Material.Filled.BarChart,
            Summary = "Filter by period, read the totals, and export the result to Excel.",
            Audience = "Member and Guest",
            Approval = "No approval",
            Prerequisites = new[]
            {
                "You have tasks in the period you want to look at.",
                "A Status is marked Completed, so finished work can be counted.",
            },
            Steps = new[]
            {
                new Step
                {
                    Title = "Pick a period",
                    Detail = "Set Date From and Date To, then choose the Application, Task Type, Priority, or Status filters you need.",
                },
                new Step
                {
                    Title = "Preview",
                    Detail = "Press Preview. The page shows task totals, completed work, duration, and a breakdown per task type and per month.",
                },
                new Step
                {
                    Title = "Read the sections",
                    Detail = "Section 6.13 reconciles the numbers: total tasks equals completed plus unfinished, and the per-month sheet follows the same period filter.",
                },
                new Step
                {
                    Title = "Export to Excel",
                    Detail = "Press Export Excel. The file is generated in the background and downloaded when ready, with the same filters you used for the preview.",
                },
            },
            Warnings = new[]
            {
                "Guest plan only allows the current calendar month. The date pickers are limited to it and the server clamps any other range.",
                "Durations are calculated in minutes and shown in hours with one decimal.",
                "Median and average over an empty set show a dash, not zero — zero would be misleading.",
                "The export download can only be opened by the account that created it.",
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
            Title = "Guest Guide",
            Tab = "Guest Guide",
            Icon = Icons.Material.Filled.PersonOutline,
            Summary = "What a Guest account can and cannot do, and how to move up to Member.",
            Audience = "Guest (group 99)",
            Approval = "Payment approval",
            Prerequisites = new[]
            {
                "You registered from the login page and an administrator approved your account.",
            },
            Steps = new[]
            {
                new Step
                {
                    Title = "Set up your master data",
                    Detail = "Add Application, Task Type, Priority, and Status. The Guest plan allows at most 3 active rows per table.",
                },
                new Step
                {
                    Title = "Create tasks",
                    Detail = "Use the Kanban board as usual. The Guest plan allows at most 50 active tasks, and deleting a task frees a slot.",
                },
                new Step
                {
                    Title = "Read the report",
                    Detail = "Only the current calendar month is available. Other ranges are clamped by the server.",
                },
                new Step
                {
                    Title = "Open Upgrade Plan",
                    Detail = "Go to the Upgrade Plan page from your profile menu to compare the Guest and Member plans.",
                },
                new Step
                {
                    Title = "Submit the payment",
                    Detail = "Open Billing, transfer the amount to the account shown, then upload the payment proof and submit the request.",
                },
                new Step
                {
                    Title = "Wait for approval",
                    Detail = "An administrator reviews the proof. While the request is pending the form is locked, so you cannot submit a second request.",
                },
                new Step
                {
                    Title = "Sign in again",
                    Detail = "After approval your session is closed and your account becomes Member. Sign in again to receive the Member menu and features.",
                },
            },
            Warnings = new[]
            {
                "Guest plan has no Excel import and no import template.",
                "Guest plan has no Calendar page.",
                "Guest plan report is limited to the current calendar month.",
                "Guest plan allows at most 3 active master rows per table.",
                "The upgrade price is a one-time payment. Your data and tasks carry over — nothing is lost when you become Member.",
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
