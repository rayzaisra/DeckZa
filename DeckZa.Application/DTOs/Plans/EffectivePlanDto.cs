namespace DeckZa.Application.DTOs.Plans;

public sealed class EffectivePlanDto
{
    public string Id { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public int OrderSeq { get; set; }
    public int? MaxActiveTasks { get; set; }
    public int? ReportPeriodMonths { get; set; }
    public bool AllowsCalendar { get; set; }
    public bool AllowsExcelImport { get; set; }
    public bool AllowsSupervisorMode { get; set; }
    public int? ActiveTaskCount { get; set; }
}
