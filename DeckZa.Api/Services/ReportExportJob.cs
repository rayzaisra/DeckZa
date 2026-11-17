using System.Globalization;

using ClosedXML.Excel;

using DeckZa.Application.DTOs.Reports;
using DeckZa.Application.Interfaces;
using DeckZa.Application.Interfaces.Queries;

namespace DeckZa.Api.Services;

public class ReportExportJob
{
    private static readonly string[] LongMonthNames =
    {
        "Januari", "Februari", "Maret", "April", "Mei", "Juni",
        "Juli", "Agustus", "September", "Oktober", "November", "Desember",
    };

    private readonly IReportQueryService _reportQueryService;
    private readonly IErrorLogService _errorLog;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ReportExportJob> _logger;

    public ReportExportJob(
        IReportQueryService reportQueryService,
        IErrorLogService errorLog,
        IWebHostEnvironment environment,
        ILogger<ReportExportJob> logger)
    {
        _reportQueryService = reportQueryService;
        _errorLog = errorLog;
        _environment = environment;
        _logger = logger;
    }

    public async Task ExportAsync(
        ReportFilterDto filter,
        string userId,
        string jobId,
        string fileName)
    {
        try
        {
            var report = await _reportQueryService.BuildAsync(userId, filter);

            using var workbook = new XLWorkbook();
            BuildDashboardSheet(workbook, report);
            BuildApplicationsSheet(workbook, report);
            BuildSpeedSheet(workbook, report);
            BuildRecapSheet(workbook, report);
            BuildDetailsSheet(workbook, report);

            SaveWorkbook(workbook, fileName);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Report export failed for job {JobId}", jobId);
            await _errorLog.LogAsync("ReportExport", "ERROR", exception, userId);
        }
    }

    private static void BuildDashboardSheet(XLWorkbook workbook, ReportResultDto report)
    {
        var sheet = workbook.Worksheets.Add("Dashboard");
        sheet.Column(1).Width = 34;
        sheet.Column(2).Width = 80;

        sheet.Cell(1, 1).Value = $"Performa Task {report.MemberName}";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;

        sheet.Cell(2, 1).Value = BuildSubtitle(report);
        sheet.Cell(2, 1).Style.Font.Italic = true;

        var row = 4;
        WriteLabelValue(sheet, row++, "Task dalam periode", report.Summary.TotalTasks);
        WriteLabelValue(sheet, row++, "Task berstatus Done", report.Summary.DoneCount);
        WriteLabelValue(sheet, row++, "Tingkat penyelesaian (% Done)", FormatPercent(report.Summary.PercentDone));
        WriteLabelValue(sheet, row++, "Task masih In Progress", report.Summary.InProgressCount);

        row++;
        sheet.Cell(row, 1).Value = "Temuan utama";
        sheet.Cell(row, 1).Style.Font.Bold = true;
        row++;

        foreach (var insight in report.Insights)
        {
            sheet.Cell(row, 1).Value = insight.Text;
            sheet.Range(row, 1, row, 2).Merge();
            row++;
        }

        row++;
        sheet.Cell(row, 1).Value = "Analisis lanjutan";
        sheet.Cell(row, 1).Style.Font.Bold = true;
        row++;
        sheet.Cell(row, 1).Value = "Lihat sheet Aplikasi dan User serta Kecepatan untuk rincian.";
        sheet.Range(row, 1, row, 2).Merge();
        row += 2;
        sheet.Cell(row, 1).Value = "Sumber data: semua angka dari sheet Rekap.";
        sheet.Range(row, 1, row, 2).Merge();
    }

    private static void BuildApplicationsSheet(XLWorkbook workbook, ReportResultDto report)
    {
        var sheet = workbook.Worksheets.Add("Aplikasi dan User");
        var row = 1;

        sheet.Cell(row, 1).Value = "Rincian Aplikasi";
        sheet.Cell(row, 1).Style.Font.Bold = true;
        row++;

        var headers = new[]
        {
            "Aplikasi", "Total task", "Done", "In Progress", "% Done", "Durasi valid", "% ≤ 1 jam",
        };

        row = WriteTableHeader(sheet, row, headers);

        var totalTasks = 0;
        var totalDone = 0;
        var totalInProgress = 0;
        var totalValid = 0;
        var totalWithinHour = 0;

        foreach (var application in report.Applications)
        {
            var label = application.ApplicationName;
            if (application.IsDeleted)
            {
                label += " (dihapus)";
            }
            else if (application.IsInactive)
            {
                label += " (nonaktif)";
            }

            WriteTableRow(sheet, row++, new object?[]
            {
                label,
                application.TotalTasks,
                application.DoneCount,
                application.InProgressCount,
                FormatPercent(application.PercentDone),
                application.ValidDurationCount,
                FormatPercent(application.PercentWithinOneHour),
            });

            totalTasks += application.TotalTasks;
            totalDone += application.DoneCount;
            totalInProgress += application.InProgressCount;
            totalValid += application.ValidDurationCount;
            totalWithinHour += (int)Math.Round(
                application.ValidDurationCount * application.PercentWithinOneHour / 100m,
                MidpointRounding.AwayFromZero);
        }

        WriteTotalRow(sheet, row++, new object?[]
        {
            "Total periode",
            totalTasks,
            totalDone,
            totalInProgress,
            FormatPercent(Percent(totalDone, totalTasks)),
            totalValid,
            FormatPercent(Percent(totalWithinHour, totalValid)),
        });

        row++;
        sheet.Cell(row, 1).Value = "Rincian User Peminta";
        sheet.Cell(row, 1).Style.Font.Bold = true;
        row++;
        row = WriteTableHeader(sheet, row, headers);

        totalTasks = 0;
        totalDone = 0;
        totalInProgress = 0;
        totalValid = 0;
        totalWithinHour = 0;

        foreach (var requester in report.RequestedBy)
        {
            WriteTableRow(sheet, row++, new object?[]
            {
                requester.RequestedBy,
                requester.TotalTasks,
                requester.DoneCount,
                requester.InProgressCount,
                FormatPercent(requester.PercentDone),
                requester.ValidDurationCount,
                FormatPercent(requester.PercentWithinOneHour),
            });

            totalTasks += requester.TotalTasks;
            totalDone += requester.DoneCount;
            totalInProgress += requester.InProgressCount;
            totalValid += requester.ValidDurationCount;
            totalWithinHour += (int)Math.Round(
                requester.ValidDurationCount * requester.PercentWithinOneHour / 100m,
                MidpointRounding.AwayFromZero);
        }

        WriteTotalRow(sheet, row, new object?[]
        {
            "Total periode",
            totalTasks,
            totalDone,
            totalInProgress,
            FormatPercent(Percent(totalDone, totalTasks)),
            totalValid,
            FormatPercent(Percent(totalWithinHour, totalValid)),
        });

        ClampColumnWidths(sheet);
    }

    private static void BuildSpeedSheet(XLWorkbook workbook, ReportResultDto report)
    {
        var sheet = workbook.Worksheets.Add("Kecepatan");
        var row = 1;

        sheet.Cell(row, 1).Value = "Ringkasan";
        sheet.Cell(row, 1).Style.Font.Bold = true;
        row++;
        WriteLabelValue(sheet, row++, "Median durasi", FormatHours(report.Speed.MedianMinutes));
        WriteLabelValue(sheet, row++, "Rata-rata durasi", FormatHours(report.Speed.AverageMinutes));
        WriteLabelValue(sheet, row++, "Selesai dalam ≤ 1 jam", FormatPercent(report.Speed.PercentWithinOneHour));

        row++;
        sheet.Cell(row, 1).Value = "Kecepatan menurut bulan permintaan";
        sheet.Cell(row, 1).Style.Font.Bold = true;
        row++;
        row = WriteTableHeader(sheet, row, new[]
        {
            "Bulan", "Durasi valid", "Rata-rata (jam)", "% ≤ 1 jam",
        });

        foreach (var month in report.Speed.ByMonth)
        {
            WriteTableRow(sheet, row++, new object?[]
            {
                month.MonthName,
                month.ValidTaskCount,
                FormatHours(month.AverageMinutes).Replace(" jam", string.Empty),
                FormatPercent(month.PercentWithinOneHour),
            });
        }

        row++;
        sheet.Cell(row, 1).Value = "Cakupan pengukuran";
        sheet.Cell(row, 1).Style.Font.Bold = true;
        row++;
        sheet.Cell(row, 1).Value =
            $"{report.Speed.ValidTaskCount} dari {report.Speed.DoneCount} task Done memiliki durasi valid. "
            + $"{report.Speed.FinishedBeforeRequestCount} task Done mencatat waktu selesai sebelum waktu permintaan.";
        sheet.Range(row, 1, row, 4).Merge();
        row += 2;

        sheet.Cell(row, 1).Value = "Jumlah task menurut durasi";
        sheet.Cell(row, 1).Style.Font.Bold = true;
        row++;
        row = WriteTableHeader(sheet, row, new[] { "Durasi", "Jumlah" });

        foreach (var bucket in report.Speed.Buckets)
        {
            WriteTableRow(sheet, row++, new object?[] { bucket.Label, bucket.Count });
        }

        row++;
        sheet.Cell(row, 1).Value =
            "Untuk task Done, waktu selesai aktual berasal dari completed_date jika terisi, atau due_date. "
            + "Durasi dihitung dalam menit kalender.";
        sheet.Range(row, 1, row, 4).Merge();

        ClampColumnWidths(sheet);
    }

    private static void BuildRecapSheet(XLWorkbook workbook, ReportResultDto report)
    {
        var sheet = workbook.Worksheets.Add("Rekap");
        var row = 1;

        sheet.Cell(row, 1).Value = "Rekap bulanan";
        sheet.Cell(row, 1).Style.Font.Bold = true;
        row++;
        sheet.Cell(row, 1).Value =
            "Baris bulan mengikuti filter periode; bulan pertama dan terakhir dapat bersifat parsial.";
        sheet.Range(row, 1, row, 6).Merge();
        row++;

        var headers = new List<string>
        {
            "Bulan permintaan", "Bulan (grafik)", "Total task", "Done", "In Progress", "% Done",
        };
        headers.AddRange(report.TaskTypeColumns.Select(column => column.DisplayName));
        headers.Add("Awal bulan");

        row = WriteTableHeader(sheet, row, headers.ToArray());

        foreach (var month in report.MonthlyRecap)
        {
            var values = new List<object?>
            {
                month.MonthName,
                month.MonthShort,
                month.TotalTasks,
                month.DoneCount,
                month.InProgressCount,
                FormatPercent(month.PercentDone),
            };
            values.AddRange(report.TaskTypeColumns
                .Select(column => (object?)month.TaskTypeCounts.GetValueOrDefault(column.TaskTypeId)));
            values.Add(month.MonthStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

            WriteTableRow(sheet, row++, values.ToArray());
        }

        var totalValues = new List<object?>
        {
            "Total periode",
            string.Empty,
            report.Summary.TotalTasks,
            report.Summary.DoneCount,
            report.Summary.InProgressCount,
            FormatPercent(report.Summary.PercentDone),
        };
        totalValues.AddRange(report.TaskTypeColumns.Select(column =>
            (object?)report.MonthlyRecap.Sum(month => month.TaskTypeCounts.GetValueOrDefault(column.TaskTypeId))));
        totalValues.Add(string.Empty);
        WriteTotalRow(sheet, row, totalValues.ToArray());

        row += 3;
        sheet.Cell(row, 1).Value = "Status menurut jenis task";
        sheet.Cell(row, 1).Style.Font.Bold = true;
        row++;
        row = WriteTableHeader(sheet, row, new[]
        {
            "Jenis task", "Total task", "Done", "In Progress", "% Done", "% dari total",
        });

        foreach (var taskType in report.TaskTypes)
        {
            WriteTableRow(sheet, row++, new object?[]
            {
                taskType.TaskTypeName,
                taskType.TotalTasks,
                taskType.DoneCount,
                taskType.InProgressCount,
                FormatPercent(taskType.PercentDone),
                FormatPercent(taskType.PercentOfTotal),
            });
        }

        WriteTotalRow(sheet, row, new object?[]
        {
            "Total periode",
            report.Summary.TotalTasks,
            report.Summary.DoneCount,
            report.Summary.InProgressCount,
            FormatPercent(report.Summary.PercentDone),
            FormatPercent(report.Summary.TotalTasks == 0 ? 0 : 100),
        });

        row += 3;
        sheet.Cell(row, 1).Value = "Cakupan dan pemeriksaan data";
        sheet.Cell(row, 1).Style.Font.Bold = true;
        row++;

        var coverage = report.Coverage;
        var coverageRows = new (string Label, object? Value)[]
        {
            ("Awal periode", coverage.PeriodStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            ("Akhir eksklusif", coverage.PeriodEndExclusive.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            ("Task tercatat", coverage.TotalRecorded),
            ("Task pada periode", coverage.TotalInPeriod),
            ("Task di luar periode", coverage.TotalOutsidePeriod),
            ("Created setelah periode", coverage.CreatedAfterPeriod),
            ("Selesai sebelum permintaan", coverage.FinishedBeforeRequest),
            ("Selisih rekap jenis dan bulanan", coverage.TaskTypeSumCheck),
            ("Selisih rekap dan detail", coverage.DetailCountCheck),
        };

        foreach (var item in coverageRows)
        {
            WriteLabelValue(sheet, row++, item.Label, item.Value);
        }

        ClampColumnWidths(sheet);
    }

    private static void BuildDetailsSheet(XLWorkbook workbook, ReportResultDto report)
    {
        var sheet = workbook.Worksheets.Add("Detail Task");
        var headers = new[]
        {
            "ID", "Judul task", "RequestedBy", "TaskType", "RequestDate", "DueDate", "Status",
            "IsCompleted", "created_date", "Aplikasi", "Waktu selesai aktual", "Durasi (menit)",
            "Status durasi",
        };

        sheet.Cell(1, 1).Value = "Sumber: tabel task DeckZa. ID adalah primary key task.";
        sheet.Range(1, 1, 1, headers.Length).Merge();

        var row = WriteTableHeader(sheet, 2, headers);

        foreach (var detail in report.Details)
        {
            WriteTableRow(sheet, row++, new object?[]
            {
                detail.Id.ToString(),
                detail.Title,
                detail.RequestedBy,
                detail.TaskTypeName,
                detail.RequestDate,
                detail.DueDate,
                detail.StatusName,
                detail.IsCompleted ? 1 : 0,
                detail.CreatedDate,
                detail.ApplicationName,
                detail.ActualCompletion,
                detail.DurationMinutes,
                detail.DurationStatus,
            });
        }

        sheet.Cell(row + 1, 1).Value =
            "Untuk task Done, waktu selesai aktual berasal dari completed_date jika terisi, atau due_date. "
            + "Filter Status durasi untuk melihat baris yang dikecualikan.";
        sheet.Range(row + 1, 1, row + 1, headers.Length).Merge();

        sheet.SheetView.FreezeRows(2);
        ClampColumnWidths(sheet);
    }

    private void SaveWorkbook(XLWorkbook workbook, string fileName)
    {
        var exportsPath = Path.Combine(_environment.ContentRootPath, "exports");
        Directory.CreateDirectory(exportsPath);
        var finalPath = Path.Combine(exportsPath, fileName);
        var temporaryPath = finalPath + ".tmp.xlsx";
        workbook.SaveAs(temporaryPath);
        System.IO.File.Move(temporaryPath, finalPath, overwrite: true);
    }

    private static int WriteTableHeader(IXLWorksheet sheet, int row, IReadOnlyList<string> headers)
    {
        for (var column = 0; column < headers.Count; column++)
        {
            var cell = sheet.Cell(row, column + 1);
            cell.Value = headers[column];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F0FE");
        }

        return row + 1;
    }

    private static void WriteTableRow(IXLWorksheet sheet, int row, IReadOnlyList<object?> values)
    {
        for (var column = 0; column < values.Count; column++)
        {
            SetCellValue(sheet.Cell(row, column + 1), values[column]);
        }
    }

    private static void WriteTotalRow(IXLWorksheet sheet, int row, IReadOnlyList<object?> values)
    {
        WriteTableRow(sheet, row, values);
        sheet.Row(row).Style.Font.Bold = true;
    }

    private static void WriteLabelValue(IXLWorksheet sheet, int row, string label, object? value)
    {
        sheet.Cell(row, 1).Value = label;
        SetCellValue(sheet.Cell(row, 2), value);
    }

    private static void SetCellValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                cell.Value = string.Empty;
                break;
            case DateTime dateTime:
                cell.Value = dateTime;
                cell.Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
                break;
            case bool boolean:
                cell.Value = boolean ? "Yes" : "No";
                break;
            case IFormattable formattable:
                cell.Value = formattable.ToString(null, CultureInfo.InvariantCulture);
                break;
            default:
                cell.Value = value.ToString() ?? string.Empty;
                break;
        }
    }

    private static void ClampColumnWidths(IXLWorksheet sheet)
    {
        sheet.Columns().AdjustToContents();

        foreach (var column in sheet.ColumnsUsed())
        {
            if (column.Width > 45)
            {
                column.Width = 45;
            }
        }
    }

    private static string BuildSubtitle(ReportResultDto report)
    {
        var start = report.PeriodStart;
        var lastDay = report.PeriodEndExclusive.AddDays(-1);
        var startLabel = $"{LongMonthNames[start.Month - 1]} {start.Year}";
        var endLabel = $"{LongMonthNames[lastDay.Month - 1]} {lastDay.Year}";

        return start.Year == lastDay.Year
            ? $"{LongMonthNames[start.Month - 1]}–{LongMonthNames[lastDay.Month - 1]} {start.Year} · Bulan mengikuti tanggal permintaan"
            : $"{startLabel}–{endLabel} · Bulan mengikuti tanggal permintaan";
    }

    private static string FormatPercent(decimal value)
        => value.ToString("0.0", CultureInfo.InvariantCulture) + "%";

    private static string FormatHours(decimal? minutes)
        => minutes.HasValue
            ? (minutes.Value / 60m).ToString("0.0", CultureInfo.InvariantCulture) + " jam"
            : "–";

    private static decimal Percent(int part, int total)
        => total == 0
            ? 0
            : Math.Round(part * 100m / total, 2, MidpointRounding.AwayFromZero);
}
