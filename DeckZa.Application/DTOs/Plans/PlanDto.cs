namespace DeckZa.Application.DTOs.Plans;

public sealed class PlanDto
{
    public string Id { get; set; } = string.Empty;
    public string? PlanName { get; set; }
    public string? Description { get; set; }
    public decimal PriceAmount { get; set; }
    public string? PriceCurrency { get; set; }
    public string? BillingPeriod { get; set; }
    public int BillingIntervalCount { get; set; }
    public int? MaxActiveTasks { get; set; }
    public int? ReportPeriodMonths { get; set; }
    public bool AllowsCalendar { get; set; }
    public bool AllowsExcelImport { get; set; }
    public bool AllowsSupervisorMode { get; set; }
    public string? UpgradeGroupId { get; set; }
    public int OrderSeq { get; set; }
    public List<string> Features { get; set; } = new();
}
