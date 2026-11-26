using Microsoft.EntityFrameworkCore;

using DeckZa.Application.DTOs.Plans;
using DeckZa.Application.DTOs.Reports;
using DeckZa.Application.Interfaces;
using DeckZa.Application.Interfaces.Queries;
using DeckZa.Domain.Entities;
using DeckZa.Infrastructure.Data;

namespace DeckZa.Infrastructure.Queries;

public class ReportQueryService : IReportQueryService
{

    private const string DurationValid = "Valid";
    private const string DurationNotFinished = "Belum selesai";
    private const string DurationIncomplete = "Waktu tidak lengkap";
    private const string DurationBeforeRequest = "Selesai sebelum permintaan";

    private static readonly string[] LongMonthNames =
    {
        "Januari", "Februari", "Maret", "April", "Mei", "Juni",
        "Juli", "Agustus", "September", "Oktober", "November", "Desember",
    };

    private static readonly string[] ShortMonthNames =
    {
        "Jan", "Feb", "Mar", "Apr", "Mei", "Jun",
        "Jul", "Agu", "Sep", "Okt", "Nov", "Des",
    };

    private static readonly (string Label, Func<int, bool> Match)[] DurationBuckets =
    {
        ("≤ 1 jam", minutes => minutes <= 60),
        ("> 1–4 jam", minutes => minutes is >= 61 and <= 240),
        ("> 4–24 jam", minutes => minutes is >= 241 and <= 1440),
        ("> 1–7 hari", minutes => minutes is >= 1441 and <= 10080),
        ("> 7 hari", minutes => minutes >= 10081),
    };

    private readonly DeckZaDbContext _context;
    private readonly IPlanService _planService;

    public ReportQueryService(DeckZaDbContext context, IPlanService planService)
    {
        _context = context;
        _planService = planService;
    }

    public async Task<ReportResultDto> BuildAsync(
        string userId,
        ReportFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var plan = await _planService.GetEffectivePlanAsync(userId, cancellationToken);
        var (periodStart, periodEndExclusive) = ResolvePeriod(filter, plan);

        var memberName = await _context.um_user_app
            .AsNoTracking()
            .Where(e => e.user_id == userId)
            .Select(e => e.full_name)
            .FirstOrDefaultAsync(cancellationToken);

        var query = BuildBaseQuery(userId);

        if (!string.IsNullOrWhiteSpace(filter.TaskTypeId))
        {
            var taskTypeId = filter.TaskTypeId.Trim();
            query = query.Where(row => row.TaskTypeId == taskTypeId);
        }

        if (!string.IsNullOrWhiteSpace(filter.ApplicationId))
        {
            var applicationId = filter.ApplicationId.Trim();
            query = query.Where(row => row.ApplicationId == applicationId);
        }

        if (!string.IsNullOrWhiteSpace(filter.TaskPriorityId))
        {
            var taskPriorityId = filter.TaskPriorityId.Trim();
            query = query.Where(row => row.TaskPriorityId == taskPriorityId);
        }

        if (!string.IsNullOrWhiteSpace(filter.StatusId))
        {
            var statusId = filter.StatusId.Trim();
            query = query.Where(row => row.StatusId == statusId);
        }

        var recordedRows = await query.ToListAsync(cancellationToken);
        var periodRows = recordedRows
            .Where(row => row.RequestDate >= periodStart && row.RequestDate < periodEndExclusive)
            .ToList();
        var enrichedRows = periodRows.Select(Enrich).ToList();

        var masterTaskTypes = await _context.rf_task_type
            .AsNoTracking()
            .Where(e => e.owner_user_id == userId)
            .ToListAsync(cancellationToken);

        var taskTypeColumns = BuildTaskTypeColumns(masterTaskTypes, enrichedRows);
        var summary = BuildSummary(enrichedRows);
        var taskTypeRows = BuildTaskTypeRows(taskTypeColumns, enrichedRows, summary.TotalTasks);
        var applicationRows = BuildApplicationRows(enrichedRows);
        var requestedByRows = BuildRequestedByRows(enrichedRows);
        var monthlyRecap = BuildMonthlyRecap(periodStart, periodEndExclusive, enrichedRows, taskTypeColumns);
        var speed = BuildSpeed(enrichedRows, periodStart, periodEndExclusive);
        var statusBreakdown = BuildStatusBreakdown(enrichedRows);
        var details = BuildDetails(enrichedRows);

        var result = new ReportResultDto
        {
            MemberName = memberName ?? userId,
            PeriodStart = periodStart,
            PeriodEndExclusive = periodEndExclusive,
            Summary = summary,
            Speed = speed,
            TaskTypeColumns = taskTypeColumns,
            TaskTypes = taskTypeRows,
            Applications = applicationRows,
            RequestedBy = requestedByRows,
            MonthlyRecap = monthlyRecap,
            StatusBreakdown = statusBreakdown,
            Details = details,
            TaskTypeComposition = taskTypeColumns
                .Select(column => new ReportCompositionDto
                {
                    Key = column.TaskTypeId,
                    Name = column.DisplayName,
                    Count = enrichedRows.Count(row => row.Row.TaskTypeId == column.TaskTypeId),
                })
                .ToList(),
            ApplicationComposition = applicationRows
                .Select(row => new ReportCompositionDto
                {
                    Key = row.ApplicationId,
                    Name = row.ApplicationName,
                    Count = row.TotalTasks,
                })
                .ToList(),
            Coverage = new ReportCoverageDto
            {
                PeriodStart = periodStart,
                PeriodEndExclusive = periodEndExclusive,
                TotalRecorded = recordedRows.Count,
                TotalInPeriod = enrichedRows.Count,
                TotalOutsidePeriod = recordedRows.Count - enrichedRows.Count,
                CreatedAfterPeriod = enrichedRows.Count(row =>
                    row.Row.CreatedDate.HasValue && row.Row.CreatedDate.Value > periodEndExclusive),
                FinishedBeforeRequest = speed.FinishedBeforeRequestCount,
                TaskTypeSumCheck = taskTypeRows.Sum(row => row.TotalTasks) - summary.TotalTasks,
                DetailCountCheck = summary.TotalTasks - details.Count,
            },
        };

        result.Insights = BuildInsights(result);
        return result;
    }

    private IQueryable<RawRow> BuildBaseQuery(string userId)
        => from task in _context.tb_task.AsNoTracking()
           join application in _context.rf_application.AsNoTracking()
               on task.application_id equals application.application_id into applicationJoin
           from application in applicationJoin.DefaultIfEmpty()
           join taskType in _context.rf_task_type.AsNoTracking()
               on task.task_type_id equals taskType.task_type_id into taskTypeJoin
           from taskType in taskTypeJoin.DefaultIfEmpty()
           join status in _context.rf_status.AsNoTracking()
               on task.status_id equals status.status_id into statusJoin
           from status in statusJoin.DefaultIfEmpty()
           where task.createdBy == userId
                 && task.isActive == true
                 && task.isDeleted == false
           select new RawRow
           {
               Id = task.id,
               Title = task.title,
               RequestBy = task.request_by,
               RequestDate = task.request_date,
               DueDate = task.due_date,
               CompletedDate = task.completed_date,
               CreatedDate = task.createdDate,
               StatusId = task.status_id,
               StatusName = status != null ? status.description : task.status_id,
               StatusOrder = status != null ? status.order_seq : 0,
               IsInitialStatus = status != null && status.is_initial_status,
               IsCompletedStatus = status != null && status.is_completed_status,
               ApplicationId = task.application_id,
               ApplicationName = application != null ? application.description : task.application_id,
               ApplicationOrder = application != null ? application.order_seq : 0,
               ApplicationIsActive = application != null && application.isActive == true,
               ApplicationIsDeleted = application != null && application.isDeleted == true,
               TaskTypeId = task.task_type_id,
               TaskTypeName = taskType != null ? taskType.description : task.task_type_id,
               TaskTypeOrder = taskType != null ? taskType.order_seq : 0,
               TaskTypeIsActive = taskType != null && taskType.isActive == true,
               TaskTypeIsDeleted = taskType != null && taskType.isDeleted == true,
               TaskPriorityId = task.task_priority_id,
           };

    private static (DateTime Start, DateTime EndExclusive) ResolvePeriod(
        ReportFilterDto filter,
        EffectivePlanDto? plan)
    {
        var today = DateTime.Today;
        DateTime start;
        DateTime endExclusive;

        if (plan?.ReportPeriodMonths is int months && months > 0)
        {
            start = new DateTime(today.Year, today.Month, 1);
            endExclusive = start.AddMonths(months);
        }
        else
        {
            start = filter.DateFrom?.Date
                    ?? new DateTime(today.Year, today.Month, 1);

            endExclusive = filter.DateTo.HasValue
                ? filter.DateTo.Value.Date.AddDays(1)
                : start.AddMonths(1);
        }

        if (endExclusive <= start)
        {
            endExclusive = start.AddDays(1);
        }

        return (start, endExclusive);
    }

    private static EnrichedRow Enrich(RawRow row)
    {
        var isDraft = row.IsInitialStatus;
        var isDone = row.IsCompletedStatus;
        var isInProgress = !isDraft && !isDone;

        var completion = isDone ? row.CompletedDate ?? row.DueDate : null;
        var durationStatus = DurationNotFinished;
        var durationMinutes = (int?)null;

        if (isDone)
        {
            if (row.RequestDate == default || completion == null)
            {
                durationStatus = DurationIncomplete;
            }
            else if (completion.Value < row.RequestDate)
            {
                durationStatus = DurationBeforeRequest;
            }
            else
            {
                durationStatus = DurationValid;
                durationMinutes = (int)Math.Round(
                    (completion.Value - row.RequestDate).TotalMinutes,
                    MidpointRounding.AwayFromZero);
            }
        }

        return new EnrichedRow
        {
            Row = row,
            IsDraft = isDraft,
            IsDone = isDone,
            IsInProgress = isInProgress,
            ActualCompletion = completion,
            DurationMinutes = durationMinutes,
            DurationStatus = durationStatus,
        };
    }

    private static ReportSummaryDto BuildSummary(List<EnrichedRow> rows)
    {
        var total = rows.Count;
        var done = rows.Count(row => row.IsDone);

        return new ReportSummaryDto
        {
            TotalTasks = total,
            DoneCount = done,
            InProgressCount = rows.Count(row => row.IsInProgress),
            DraftCount = rows.Count(row => row.IsDraft),
            PercentDone = Percent(done, total),
        };
    }

    private static List<ReportTaskTypeColumnDto> BuildTaskTypeColumns(
        List<rf_task_type> masterTaskTypes,
        List<EnrichedRow> rows)
        => masterTaskTypes
            .Where(type => (type.isActive == true && type.isDeleted == false)
                           || rows.Any(row => row.Row.TaskTypeId == type.task_type_id))
            .OrderBy(type => type.order_seq)
            .ThenBy(type => type.description)
            .Select(type => new ReportTaskTypeColumnDto
            {
                TaskTypeId = type.task_type_id,
                Name = type.description,
                DisplayName = type.isDeleted == true
                    ? $"{type.description} (dihapus)"
                    : type.isActive == false
                        ? $"{type.description} (nonaktif)"
                        : type.description,
                IsInactive = type.isActive == false,
                IsDeleted = type.isDeleted == true,
                OrderSeq = type.order_seq,
            })
            .ToList();

    private static List<ReportTaskTypeRowDto> BuildTaskTypeRows(
        List<ReportTaskTypeColumnDto> columns,
        List<EnrichedRow> rows,
        int totalPeriod)
        => columns
            .Select(column =>
            {
                var group = rows.Where(row => row.Row.TaskTypeId == column.TaskTypeId).ToList();
                var done = group.Count(row => row.IsDone);

                return new ReportTaskTypeRowDto
                {
                    TaskTypeId = column.TaskTypeId,
                    TaskTypeName = column.Name,
                    IsInactive = column.IsInactive,
                    IsDeleted = column.IsDeleted,
                    OrderSeq = column.OrderSeq,
                    TotalTasks = group.Count,
                    DoneCount = done,
                    InProgressCount = group.Count(row => row.IsInProgress),
                    PercentDone = Percent(done, group.Count),
                    PercentOfTotal = Percent(group.Count, totalPeriod),
                };
            })
            .ToList();

    private static List<ReportApplicationRowDto> BuildApplicationRows(List<EnrichedRow> rows)
        => rows
            .GroupBy(row => row.Row.ApplicationId)
            .Select(group =>
            {
                var first = group.First().Row;
                var total = group.Count();
                var done = group.Count(row => row.IsDone);
                var validDurations = group
                    .Where(row => row.DurationStatus == DurationValid && row.DurationMinutes.HasValue)
                    .Select(row => row.DurationMinutes!.Value)
                    .ToList();

                return new ReportApplicationRowDto
                {
                    ApplicationId = first.ApplicationId,
                    ApplicationName = first.ApplicationName,
                    IsInactive = !first.ApplicationIsActive,
                    IsDeleted = first.ApplicationIsDeleted,
                    OrderSeq = first.ApplicationOrder,
                    TotalTasks = total,
                    DoneCount = done,
                    InProgressCount = group.Count(row => row.IsInProgress),
                    PercentDone = Percent(done, total),
                    ValidDurationCount = validDurations.Count,
                    PercentWithinOneHour = Percent(validDurations.Count(minutes => minutes <= 60), validDurations.Count),
                };
            })
            .OrderBy(row => row.OrderSeq)
            .ThenBy(row => row.ApplicationName)
            .ToList();

    private static List<ReportRequestedByRowDto> BuildRequestedByRows(List<EnrichedRow> rows)
        => rows
            .GroupBy(row => row.Row.RequestBy.Trim().ToLowerInvariant())
            .Select(group =>
            {
                var display = group.First().Row.RequestBy.Trim();
                var total = group.Count();
                var done = group.Count(row => row.IsDone);
                var validDurations = group
                    .Where(row => row.DurationStatus == DurationValid && row.DurationMinutes.HasValue)
                    .Select(row => row.DurationMinutes!.Value)
                    .ToList();

                return new ReportRequestedByRowDto
                {
                    RequestedBy = display,
                    TotalTasks = total,
                    DoneCount = done,
                    InProgressCount = group.Count(row => row.IsInProgress),
                    PercentDone = Percent(done, total),
                    ValidDurationCount = validDurations.Count,
                    PercentWithinOneHour = Percent(validDurations.Count(minutes => minutes <= 60), validDurations.Count),
                };
            })
            .OrderByDescending(row => row.TotalTasks)
            .ThenBy(row => row.RequestedBy)
            .ToList();

    private static List<ReportMonthlyRecapRowDto> BuildMonthlyRecap(
        DateTime periodStart,
        DateTime periodEndExclusive,
        List<EnrichedRow> rows,
        List<ReportTaskTypeColumnDto> taskTypeColumns)
    {
        var result = new List<ReportMonthlyRecapRowDto>();

        foreach (var month in EnumerateMonths(periodStart, periodEndExclusive))
        {
            var monthRows = rows
                .Where(row => row.Row.RequestDate.Year == month.Year
                              && row.Row.RequestDate.Month == month.Month)
                .ToList();
            var done = monthRows.Count(row => row.IsDone);

            result.Add(new ReportMonthlyRecapRowDto
            {
                Year = month.Year,
                Month = month.Month,
                MonthName = LongMonthNames[month.Month - 1],
                MonthShort = ShortMonthNames[month.Month - 1],
                MonthStart = month,
                TotalTasks = monthRows.Count,
                DoneCount = done,
                InProgressCount = monthRows.Count(row => row.IsInProgress),
                PercentDone = Percent(done, monthRows.Count),
                TaskTypeCounts = taskTypeColumns.ToDictionary(
                    column => column.TaskTypeId,
                    column => monthRows.Count(row => row.Row.TaskTypeId == column.TaskTypeId)),
            });
        }

        return result;
    }

    private static ReportSpeedDto BuildSpeed(
        List<EnrichedRow> rows,
        DateTime periodStart,
        DateTime periodEndExclusive)
    {
        var doneRows = rows.Where(row => row.IsDone).ToList();
        var validDurations = rows
            .Where(row => row.DurationStatus == DurationValid && row.DurationMinutes.HasValue)
            .Select(row => row.DurationMinutes!.Value)
            .ToList();

        var result = new ReportSpeedDto
        {
            DoneCount = doneRows.Count,
            ValidTaskCount = validDurations.Count,
            FinishedBeforeRequestCount = doneRows.Count(row => row.DurationStatus == DurationBeforeRequest),
            IncompleteCount = doneRows.Count(row => row.DurationStatus == DurationIncomplete),
            MedianMinutes = Median(validDurations),
            AverageMinutes = Average(validDurations),
            PercentWithinOneHour = Percent(validDurations.Count(minutes => minutes <= 60), validDurations.Count),
            Buckets = DurationBuckets
                .Select(bucket => new ReportDurationBucketDto
                {
                    Label = bucket.Label,
                    Count = validDurations.Count(bucket.Match),
                })
                .ToList(),
        };

        foreach (var month in EnumerateMonths(periodStart, periodEndExclusive))
        {
            var monthDurations = rows
                .Where(row => row.DurationStatus == DurationValid
                              && row.DurationMinutes.HasValue
                              && row.Row.RequestDate.Year == month.Year
                              && row.Row.RequestDate.Month == month.Month)
                .Select(row => row.DurationMinutes!.Value)
                .ToList();

            result.ByMonth.Add(new ReportSpeedByMonthDto
            {
                Year = month.Year,
                Month = month.Month,
                MonthName = LongMonthNames[month.Month - 1],
                MonthShort = ShortMonthNames[month.Month - 1],
                ValidTaskCount = monthDurations.Count,
                MedianMinutes = Median(monthDurations),
                AverageMinutes = Average(monthDurations),
                PercentWithinOneHour = Percent(monthDurations.Count(minutes => minutes <= 60), monthDurations.Count),
            });
        }

        return result;
    }

    private static List<ReportStatusBreakdownDto> BuildStatusBreakdown(List<EnrichedRow> rows)
        => rows
            .GroupBy(row => row.Row.StatusId)
            .Select(group => new ReportStatusBreakdownDto
            {
                StatusId = group.Key,
                StatusName = group.First().Row.StatusName,
                OrderSeq = group.First().Row.StatusOrder,
                Count = group.Count(),
            })
            .OrderBy(item => item.OrderSeq)
            .ThenBy(item => item.StatusName)
            .ToList();

    private static List<ReportTaskDetailRowDto> BuildDetails(List<EnrichedRow> rows)
        => rows
            .OrderBy(row => row.Row.RequestDate)
            .ThenBy(row => row.Row.Title)
            .Select(row => new ReportTaskDetailRowDto
            {
                Id = row.Row.Id,
                Title = row.Row.Title,
                RequestedBy = row.Row.RequestBy,
                TaskTypeName = row.Row.TaskTypeName,
                RequestDate = row.Row.RequestDate,
                DueDate = row.Row.DueDate,
                StatusName = row.Row.StatusName,
                IsCompleted = row.IsDone,
                CreatedDate = row.Row.CreatedDate,
                ApplicationName = row.Row.ApplicationName,
                ActualCompletion = row.ActualCompletion,
                DurationMinutes = row.DurationMinutes,
                DurationStatus = row.DurationStatus,
            })
            .ToList();

    private static List<ReportInsightDto> BuildInsights(ReportResultDto report)
    {
        var insights = new List<ReportInsightDto>();
        var monthsWithTasks = report.MonthlyRecap
            .Where(month => month.TotalTasks > 0)
            .OrderByDescending(month => month.TotalTasks)
            .ThenBy(month => month.MonthStart)
            .ToList();

        if (monthsWithTasks.Count == 0)
        {
            insights.Add(new ReportInsightDto
            {
                Text = "Tidak ada task yang ditugaskan pada periode yang dipilih.",
            });
        }
        else
        {
            var topMonth = monthsWithTasks[0];
            var monthTypeCounts = report.TaskTypeColumns
                .Select(column => new
                {
                    column.Name,
                    column.OrderSeq,
                    Count = topMonth.TaskTypeCounts.GetValueOrDefault(column.TaskTypeId),
                })
                .OrderByDescending(item => item.Count)
                .ThenBy(item => item.OrderSeq)
                .ThenBy(item => item.Name)
                .ToList();
            var monthType = monthTypeCounts[0];

            insights.Add(new ReportInsightDto
            {
                Text = monthType.Count > 0
                    ? $"{topMonth.MonthName} mencatat volume tertinggi dengan {topMonth.TotalTasks} task, termasuk {monthType.Count} {monthType.Name}."
                    : $"{topMonth.MonthName} mencatat volume tertinggi dengan {topMonth.TotalTasks} task.",
            });
        }

        if (report.Summary.TotalTasks == 0)
        {
            insights.Add(new ReportInsightDto
            {
                Text = "Tidak ada task pada periode yang dipilih.",
            });
        }
        else
        {
            var dominantType = report.TaskTypes
                .OrderByDescending(row => row.TotalTasks)
                .ThenBy(row => row.OrderSeq)
                .ThenBy(row => row.TaskTypeName)
                .First();

            insights.Add(new ReportInsightDto
            {
                Text = $"{dominantType.TotalTasks} dari {report.Summary.TotalTasks} task masuk kategori {dominantType.TaskTypeName}.",
            });
        }

        insights.Add(new ReportInsightDto
        {
            Text = report.Speed.DoneCount == 0
                ? "Tidak ada task Done pada periode yang dipilih."
                : $"{report.Speed.ValidTaskCount} dari {report.Speed.DoneCount} task Done memiliki durasi valid. "
                  + $"{report.Speed.FinishedBeforeRequestCount} task Done mencatat waktu selesai sebelum waktu permintaan.",
        });

        return insights;
    }

    private static IEnumerable<DateTime> EnumerateMonths(DateTime start, DateTime endExclusive)
    {
        var current = new DateTime(start.Year, start.Month, 1);
        var lastDate = endExclusive.AddDays(-1);

        while (current <= lastDate)
        {
            yield return current;
            current = current.AddMonths(1);
        }
    }

    private static decimal Percent(int part, int total)
        => total == 0
            ? 0
            : Math.Round(part * 100m / total, 2, MidpointRounding.AwayFromZero);

    private static decimal? Average(List<int> values)
        => values.Count == 0
            ? null
            : Math.Round(values.Average(value => (decimal)value), 2, MidpointRounding.AwayFromZero);

    private static decimal? Median(List<int> values)
    {
        if (values.Count == 0)
        {
            return null;
        }

        var sorted = values.OrderBy(value => value).ToList();
        var middle = sorted.Count / 2;

        return sorted.Count % 2 == 1
            ? sorted[middle]
            : Math.Round((sorted[middle - 1] + sorted[middle]) / 2m, 2, MidpointRounding.AwayFromZero);
    }

    private sealed class RawRow
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string RequestBy { get; init; } = string.Empty;
        public DateTime RequestDate { get; init; }
        public DateTime? DueDate { get; init; }
        public DateTime? CompletedDate { get; init; }
        public DateTime? CreatedDate { get; init; }
        public string StatusId { get; init; } = string.Empty;
        public string StatusName { get; init; } = string.Empty;
        public int StatusOrder { get; init; }
        public bool IsInitialStatus { get; init; }
        public bool IsCompletedStatus { get; init; }
        public string ApplicationId { get; init; } = string.Empty;
        public string ApplicationName { get; init; } = string.Empty;
        public int ApplicationOrder { get; init; }
        public bool ApplicationIsActive { get; init; }
        public bool ApplicationIsDeleted { get; init; }
        public string TaskTypeId { get; init; } = string.Empty;
        public string TaskTypeName { get; init; } = string.Empty;
        public int TaskTypeOrder { get; init; }
        public bool TaskTypeIsActive { get; init; }
        public bool TaskTypeIsDeleted { get; init; }
        public string TaskPriorityId { get; init; } = string.Empty;
    }

    private sealed class EnrichedRow
    {
        public RawRow Row { get; init; } = new();
        public bool IsDraft { get; init; }
        public bool IsDone { get; init; }
        public bool IsInProgress { get; init; }
        public DateTime? ActualCompletion { get; init; }
        public int? DurationMinutes { get; init; }
        public string DurationStatus { get; init; } = string.Empty;
    }
}
