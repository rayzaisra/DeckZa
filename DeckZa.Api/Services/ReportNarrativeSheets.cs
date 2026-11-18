using System.Globalization;

using ClosedXML.Excel;

using DeckZa.Application.DTOs.Reports;

namespace DeckZa.Api.Services;

/// <summary>
/// Builds the three narrative sheets (Dashboard, Aplikasi dan User, Kecepatan) using the
/// layout requested for the report: title in B2, period in C2, KPI blocks in fixed merged
/// ranges (B6:F7 style), a two row gap, then a native chart followed by its detail table.
/// </summary>
public static class ReportNarrativeSheets
{
    private const int TwoRowGap = 2;

    public static void BuildDashboard(
        XLWorkbook workbook,
        ReportResultDto report,
        ICollection<ExcelChartWriter.ChartSpec> charts)
    {
        var sheet = workbook.Worksheets.Add("Dashboard");
        ReportSheetLayout.ApplyBaseLayout(sheet);

        ReportSheetLayout.WriteTitle(sheet, $"Performa Task {report.MemberName}", report);

        // KPI blocks: B(2) H(8) N(14) T(20), each spanning 4 columns and 2 rows.
        ReportSheetLayout.WriteKpiBlock(sheet, 2, "Task Dalam Periode", report.Summary.TotalTasks);
        ReportSheetLayout.WriteKpiBlock(sheet, 8, "Task berstatus Done", report.Summary.DoneCount);

        var percentDone = ReportSheetLayout.Fraction(report.Summary.DoneCount, report.Summary.TotalTasks);
        ReportSheetLayout.WriteKpiBlock(sheet, 14, "Tingkat Penyelesaian", percentDone, percent: true);
        ReportSheetLayout.WriteKpiBlock(
            sheet,
            20,
            "Task Belum Selesai",
            report.Summary.TotalTasks - report.Summary.DoneCount);

        var chartRow = ReportSheetLayout.KpiValueEndRow + TwoRowGap + 1;
        // Chart height is ~3.5in ≈ 17 rows; add a margin so the tables never sit under it.
        var tableRow = chartRow + 19;

        // Two charts side by side on the same row: B and J (one column gap between them).
        const int volumeChartColumn = 2;   // B
        const int typeChartColumn = 10;    // J

        // Source tables sit below the charts, starting in column B as requested.
        var volumeHeaderRow = tableRow;
        var volumeTableRow = ReportSheetLayout.WriteTableHeader(
            sheet, volumeHeaderRow, 2, new[] { "Bulan", "Total task" });

        var volumeStartRow = volumeTableRow;
        foreach (var month in report.MonthlyRecap)
        {
            volumeTableRow = ReportSheetLayout.WriteTableRow(
                sheet, volumeTableRow, 2, new object?[] { month.MonthName, month.TotalTasks });
        }

        var volumeEndRow = volumeTableRow - 1;

        if (volumeEndRow >= volumeStartRow)
        {
            charts.Add(new ExcelChartWriter.ChartSpec
            {
                SheetName = sheet.Name,
                Title = "Volume task per bulan permintaan",
                SeriesName = $"'{sheet.Name}'!$C${volumeHeaderRow}",
                CategoriesFormula = $"'{sheet.Name}'!$B${volumeStartRow}:$B${volumeEndRow}",
                ValuesFormula = $"'{sheet.Name}'!$C${volumeStartRow}:$C${volumeEndRow}",
                AnchorColumn = volumeChartColumn,
                AnchorRow = chartRow,
            });
        }

        // Second table to the right of the first chart, aligned with the volume table row.
        var typeHeaderRow = volumeHeaderRow;
        var typeTableRow = ReportSheetLayout.WriteTableHeader(
            sheet, typeHeaderRow, 10, new[] { "Jenis Task Type", "Total task", "Done", "% Done" });

        var typeStartRow = typeTableRow;
        foreach (var taskType in report.TaskTypes)
        {
            var fraction = ReportSheetLayout.Fraction(taskType.DoneCount, taskType.TotalTasks);
            typeTableRow = ReportSheetLayout.WriteTableRow(
                sheet, typeTableRow, 10,
                new object?[] { taskType.TaskTypeName, taskType.TotalTasks, taskType.DoneCount, fraction });
        }

        var typeEndRow = typeTableRow - 1;

        if (typeEndRow >= typeStartRow)
        {
            charts.Add(new ExcelChartWriter.ChartSpec
            {
                SheetName = sheet.Name,
                Title = "Tingkat penyelesaian per jenis task type",
                SeriesName = $"'{sheet.Name}'!$M${typeHeaderRow}",
                CategoriesFormula = $"'{sheet.Name}'!$J${typeStartRow}:$J${typeEndRow}",
                ValuesFormula = $"'{sheet.Name}'!$M${typeStartRow}:$M${typeEndRow}",
                AnchorColumn = typeChartColumn,
                AnchorRow = chartRow,
            });
        }

        sheet.SheetView.FreezeRows(ReportSheetLayout.PeriodRow);
    }

    public static void BuildApplicationsAndUsers(
        XLWorkbook workbook,
        ReportResultDto report,
        ICollection<ExcelChartWriter.ChartSpec> charts)
    {
        var sheet = workbook.Worksheets.Add("Aplikasi dan User");
        ReportSheetLayout.ApplyBaseLayout(sheet, 14);

        ReportSheetLayout.WriteTitle(sheet, "Performa per aplikasi dan user peminta", report);

        // Charts start at B5 (left) and J5 (right); detail tables follow underneath.
        const int leftColumn = 2;    // B
        const int rightColumn = 10;  // J
        const int chartRow = 5;
        const int dataRow = chartRow + 19;

        var leftDataRow = ReportSheetLayout.WriteTableHeader(
            sheet,
            dataRow,
            leftColumn,
            new[] { "Aplikasi", "Total task", "Done", "Not Done", "% Done" });

        var applicationStartRow = leftDataRow;
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

            leftDataRow = ReportSheetLayout.WriteTableRow(
                sheet,
                leftDataRow,
                leftColumn,
                new object?[]
                {
                    label,
                    application.TotalTasks,
                    application.DoneCount,
                    application.TotalTasks - application.DoneCount,
                    ReportSheetLayout.Fraction(application.DoneCount, application.TotalTasks),
                });
        }

        var applicationEndRow = leftDataRow - 1;

        var rightDataRow = ReportSheetLayout.WriteTableHeader(
            sheet,
            dataRow,
            rightColumn,
            new[] { "User Peminta", "Total task", "Done", "Not Done", "% Done" });

        var userStartRow = rightDataRow;
        foreach (var requester in report.RequestedBy.Take(8))
        {
            rightDataRow = ReportSheetLayout.WriteTableRow(
                sheet,
                rightDataRow,
                rightColumn,
                new object?[]
                {
                    requester.RequestedBy,
                    requester.TotalTasks,
                    requester.DoneCount,
                    requester.TotalTasks - requester.DoneCount,
                    ReportSheetLayout.Fraction(requester.DoneCount, requester.TotalTasks),
                });
        }

        var userEndRow = rightDataRow - 1;

        if (applicationEndRow >= applicationStartRow)
        {
            charts.Add(new ExcelChartWriter.ChartSpec
            {
                SheetName = sheet.Name,
                Title = "Volume task per aplikasi",
                SeriesName = $"'{sheet.Name}'!$C${applicationStartRow - 1}",
                CategoriesFormula = $"'{sheet.Name}'!$B${applicationStartRow}:$B${applicationEndRow}",
                ValuesFormula = $"'{sheet.Name}'!$C${applicationStartRow}:$C${applicationEndRow}",
                AnchorColumn = leftColumn,
                AnchorRow = chartRow,
            });
        }

        if (userEndRow >= userStartRow)
        {
            charts.Add(new ExcelChartWriter.ChartSpec
            {
                SheetName = sheet.Name,
                Title = "Permintaan user",
                SeriesName = $"'{sheet.Name}'!$K${userStartRow - 1}",
                CategoriesFormula = $"'{sheet.Name}'!$J${userStartRow}:$J${userEndRow}",
                ValuesFormula = $"'{sheet.Name}'!$K${userStartRow}:$K${userEndRow}",
                AnchorColumn = rightColumn,
                AnchorRow = chartRow,
            });
        }

        sheet.SheetView.FreezeRows(ReportSheetLayout.PeriodRow);
    }

    public static void BuildSpeed(
        XLWorkbook workbook,
        ReportResultDto report,
        ICollection<ExcelChartWriter.ChartSpec> charts)
    {
        var sheet = workbook.Worksheets.Add("Kecepatan");
        ReportSheetLayout.ApplyBaseLayout(sheet);

        ReportSheetLayout.WriteTitle(sheet, "Kecepatan penyelesaian task", report);

        var validDurations = report.Speed.ValidTaskCount;

        ReportSheetLayout.WriteKpiBlock(sheet, 2, "Task Done", report.Speed.DoneCount);
        ReportSheetLayout.WriteKpiBlock(
            sheet,
            7,
            "Median Durasi (menit)",
            report.Speed.MedianMinutes,
            decimalValue: true);
        ReportSheetLayout.WriteKpiBlock(
            sheet,
            12,
            "Rata-rata Durasi (menit)",
            report.Speed.AverageMinutes,
            decimalValue: true);

        var withinHourCount = (int)Math.Round(
            report.Speed.ValidTaskCount * report.Speed.PercentWithinOneHour / 100m,
            MidpointRounding.AwayFromZero);

        ReportSheetLayout.WriteKpiBlock(sheet, 17, "Selesai dalam < 1 jam", withinHourCount);

        var row = ReportSheetLayout.KpiValueEndRow + TwoRowGap + 1;

        row = ReportSheetLayout.WriteSectionTitle(sheet, row, 2, "Jumlah task menurut durasi");
        row = ReportSheetLayout.WriteTableHeader(sheet, row, 2, new[] { "Durasi", "Task" });

        var bucketLabels = new[] { "< 1 jam", "1 - 4 jam", "4 - 24 jam", "1 - 7 hari", "> 7 hari" };
        var buckets = report.Speed.Buckets;

        for (var index = 0; index < bucketLabels.Length; index++)
        {
            var count = index < buckets.Count ? buckets[index].Count : 0;
            row = ReportSheetLayout.WriteTableRow(sheet, row, 2, new object?[] { bucketLabels[index], count });
        }

        row += TwoRowGap;
        sheet.Cell(row, 2).Value =
            $"Dihitung dari {validDurations} task Done yang memiliki durasi valid. "
            + "Durasi = waktu selesai aktual − request_date, dibulatkan ke menit.";
        sheet.Range(row, 2, row, 8).Merge();
        sheet.Cell(row, 2).Style.Font.Italic = true;

        sheet.SheetView.FreezeRows(ReportSheetLayout.PeriodRow);
    }
}
