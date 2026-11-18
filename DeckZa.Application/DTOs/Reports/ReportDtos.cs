namespace DeckZa.Application.DTOs.Reports;

public class ReportFilterDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? ApplicationId { get; set; }
    public string? TaskTypeId { get; set; }
    public string? TaskPriorityId { get; set; }
    public string? StatusId { get; set; }
}

public class ReportResultDto
{
    public string MemberName { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEndExclusive { get; set; }
    public ReportSummaryDto Summary { get; set; } = new();
    public ReportSpeedDto Speed { get; set; } = new();
    public List<ReportCompositionDto> TaskTypeComposition { get; set; } = new();
    public List<ReportCompositionDto> ApplicationComposition { get; set; } = new();
    public List<ReportStatusBreakdownDto> StatusBreakdown { get; set; } = new();
    public List<ReportApplicationRowDto> Applications { get; set; } = new();
    public List<ReportRequestedByRowDto> RequestedBy { get; set; } = new();
    public List<ReportTaskTypeRowDto> TaskTypes { get; set; } = new();
    public List<ReportTaskTypeColumnDto> TaskTypeColumns { get; set; } = new();
    public List<ReportMonthlyRecapRowDto> MonthlyRecap { get; set; } = new();
    public ReportCoverageDto Coverage { get; set; } = new();
    public List<ReportInsightDto> Insights { get; set; } = new();
    public List<ReportTaskDetailRowDto> Details { get; set; } = new();
}

public class ReportSummaryDto
{
    public int TotalTasks { get; set; }
    public int DoneCount { get; set; }
    public int InProgressCount { get; set; }
    public int DraftCount { get; set; }
    public decimal PercentDone { get; set; }
}

public class ReportSpeedDto
{
    public int DoneCount { get; set; }
    public int ValidTaskCount { get; set; }
    public int FinishedBeforeRequestCount { get; set; }
    public int IncompleteCount { get; set; }
    public decimal? MedianMinutes { get; set; }
    public decimal? AverageMinutes { get; set; }
    public decimal PercentWithinOneHour { get; set; }
    public List<ReportSpeedByMonthDto> ByMonth { get; set; } = new();
    public List<ReportDurationBucketDto> Buckets { get; set; } = new();
}

public class ReportSpeedByMonthDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public string MonthShort { get; set; } = string.Empty;
    public int ValidTaskCount { get; set; }
    public decimal? MedianMinutes { get; set; }
    public decimal? AverageMinutes { get; set; }
    public decimal PercentWithinOneHour { get; set; }
}

public class ReportDurationBucketDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ReportCompositionDto
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ReportStatusBreakdownDto
{
    public string StatusId { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public int Count { get; set; }
    public int OrderSeq { get; set; }
}

public class ReportApplicationRowDto
{
    public string ApplicationId { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public bool IsInactive { get; set; }
    public bool IsDeleted { get; set; }
    public int OrderSeq { get; set; }
    public int TotalTasks { get; set; }
    public int DoneCount { get; set; }
    public int InProgressCount { get; set; }
    public decimal PercentDone { get; set; }
    public int ValidDurationCount { get; set; }
    public decimal PercentWithinOneHour { get; set; }
}

public class ReportRequestedByRowDto
{
    public string RequestedBy { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int DoneCount { get; set; }
    public int InProgressCount { get; set; }
    public decimal PercentDone { get; set; }
    public int ValidDurationCount { get; set; }
    public decimal PercentWithinOneHour { get; set; }
}

public class ReportTaskTypeRowDto
{
    public string TaskTypeId { get; set; } = string.Empty;
    public string TaskTypeName { get; set; } = string.Empty;
    public bool IsInactive { get; set; }
    public bool IsDeleted { get; set; }
    public int OrderSeq { get; set; }
    public int TotalTasks { get; set; }
    public int DoneCount { get; set; }
    public int InProgressCount { get; set; }
    public decimal PercentDone { get; set; }
    public decimal PercentOfTotal { get; set; }
}

public class ReportTaskTypeColumnDto
{
    public string TaskTypeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsInactive { get; set; }
    public bool IsDeleted { get; set; }
    public int OrderSeq { get; set; }
}

public class ReportMonthlyRecapRowDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public string MonthShort { get; set; } = string.Empty;
    public DateTime MonthStart { get; set; }
    public int TotalTasks { get; set; }
    public int DoneCount { get; set; }
    public int InProgressCount { get; set; }
    public decimal PercentDone { get; set; }
    public Dictionary<string, int> TaskTypeCounts { get; set; } = new();
}

public class ReportCoverageDto
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEndExclusive { get; set; }
    public int TotalRecorded { get; set; }
    public int TotalInPeriod { get; set; }
    public int TotalOutsidePeriod { get; set; }
    public int CreatedAfterPeriod { get; set; }
    public int FinishedBeforeRequest { get; set; }
    public int TaskTypeSumCheck { get; set; }
    public int DetailCountCheck { get; set; }
}

public class ReportInsightDto
{
    public string Text { get; set; } = string.Empty;
}

public class ReportTaskDetailRowDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public string TaskTypeName { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public DateTime? ActualCompletion { get; set; }
    public int? DurationMinutes { get; set; }
    public string DurationStatus { get; set; } = string.Empty;
}
