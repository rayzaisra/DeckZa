using System.Globalization;

using ClosedXML.Excel;

using DeckZa.Application.DTOs.Reports;

namespace DeckZa.Api.Services;

/// <summary>
/// Layout helpers for the report workbook: fixed column widths, merged KPI blocks,
/// and the four requested sheets (Dashboard, Aplikasi dan User, Kecepatan).
/// </summary>
public static class ReportSheetLayout
{
    public const int HeaderRow = 2;   // title row (B2)
    public const int PeriodRow = 3;   // period row (C2 area / row 3 kept aligned)
    public const int KpiLabelRow = 5;
    public const int KpiValueRow = 6;
    public const int KpiValueEndRow = 7;

    private static readonly string[] LongMonthNames =
    {
        "Januari", "Februari", "Maret", "April", "Mei", "Juni",
        "Juli", "Agustus", "September", "Oktober", "November", "Desember",
    };

    public static readonly XLColor TitleColor = XLColor.FromHtml("#111C2D");
    public static readonly XLColor HeaderFill = XLColor.FromHtml("#E8F0FE");
    public static readonly XLColor KpiFill = XLColor.FromHtml("#F4F7FF");
    public static readonly XLColor BorderColor = XLColor.FromHtml("#C7D2E5");

    public static void ApplyBaseLayout(IXLWorksheet sheet, int lastColumnWidth = 18)
    {
        sheet.Column(1).Width = 3;   // spacer gutter (column A stays empty)
        sheet.Column(2).Width = 46;  // title column

        for (var column = 3; column <= 25; column++)
        {
            sheet.Column(column).Width = lastColumnWidth;
        }
    }

    public static void WriteTitle(IXLWorksheet sheet, string title, ReportResultDto report)
    {
        // B2 = report title, B3 = selected period.
        var titleCell = sheet.Cell(HeaderRow, 2);
        titleCell.Value = title;
        titleCell.Style.Font.Bold = true;
        titleCell.Style.Font.FontSize = 15;
        titleCell.Style.Font.FontColor = TitleColor;
        titleCell.Style.Alignment.WrapText = false;

        var periodCell = sheet.Cell(PeriodRow, 2);
        periodCell.Value = BuildPeriodLabel(report);
        periodCell.Style.Font.Italic = true;
        periodCell.Style.Font.FontSize = 10;
        periodCell.Style.Font.FontColor = XLColor.FromHtml("#5B6472");
    }

    /// <summary>
    /// Writes a KPI block: label at <paramref name="startColumn"/> in the label row,
    /// value merged across four columns and two rows.
    /// </summary>
    public static void WriteKpiBlock(
        IXLWorksheet sheet,
        int startColumn,
        string label,
        object? value,
        bool percent = false,
        bool decimalValue = false)
    {
        var endColumn = startColumn + 3;

        var labelRange = sheet.Range(KpiLabelRow, startColumn, KpiLabelRow, endColumn);
        labelRange.Merge();
        labelRange.FirstCell().Value = label;
        labelRange.Style.Font.Bold = true;
        labelRange.Style.Font.FontSize = 10;
        labelRange.Style.Font.FontColor = XLColor.FromHtml("#5B6472");
        labelRange.Style.Alignment.WrapText = true;
        labelRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        var valueRange = sheet.Range(KpiValueRow, startColumn, KpiValueEndRow, endColumn);
        valueRange.Merge();

        var cell = valueRange.FirstCell();
        if (value is null)
        {
            cell.Value = "–";
        }
        else if (value is decimal decimalNumber)
        {
            cell.Value = decimalNumber;
            cell.Style.NumberFormat.Format = percent ? "0.0%" : decimalValue ? "0.0" : "#,##0";
        }
        else if (value is int intNumber)
        {
            cell.Value = intNumber;
            cell.Style.NumberFormat.Format = "#,##0";
        }
        else if (value is double doubleNumber)
        {
            cell.Value = doubleNumber;
            cell.Style.NumberFormat.Format = percent ? "0.0%" : decimalValue ? "0.0" : "#,##0";
        }
        else
        {
            cell.Value = value.ToString();
        }

        cell.Style.Font.Bold = true;
        cell.Style.Font.FontSize = 22;
        cell.Style.Font.FontColor = TitleColor;
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        var outline = sheet.Range(KpiLabelRow, startColumn, KpiValueEndRow, endColumn);
        outline.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        outline.Style.Border.OutsideBorderColor = BorderColor;
        valueRange.Style.Fill.BackgroundColor = KpiFill;
    }

    public static int WriteSectionTitle(IXLWorksheet sheet, int row, int column, string text)
    {
        var cell = sheet.Cell(row, column);
        cell.Value = text;
        cell.Style.Font.Bold = true;
        cell.Style.Font.FontSize = 12;
        cell.Style.Font.FontColor = TitleColor;
        return row + 1;
    }

    public static int WriteTableHeader(IXLWorksheet sheet, int row, int column, IReadOnlyList<string> headers)
    {
        for (var index = 0; index < headers.Count; index++)
        {
            var cell = sheet.Cell(row, column + index);
            cell.Value = headers[index];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = HeaderFill;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.BottomBorderColor = BorderColor;
            cell.Style.Alignment.WrapText = true;
        }

        return row + 1;
    }

    public static int WriteTableRow(IXLWorksheet sheet, int row, int column, IReadOnlyList<object?> values)
    {
        for (var index = 0; index < values.Count; index++)
        {
            var cell = sheet.Cell(row, column + index);
            var value = values[index];

            switch (value)
            {
                case null:
                    cell.Value = string.Empty;
                    break;
                case int intValue:
                    cell.Value = intValue;
                    break;
                case decimal decimalValue:
                    cell.Value = decimalValue;
                    cell.Style.NumberFormat.Format = "#,##0.0";
                    break;
                case DateTime dateValue:
                    cell.Value = dateValue;
                    cell.Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
                    break;
                default:
                    cell.Value = value.ToString() ?? string.Empty;
                    break;
            }
        }

        return row + 1;
    }

    public static int WriteTotalRow(IXLWorksheet sheet, int row, int column, IReadOnlyList<object?> values)
    {
        var next = WriteTableRow(sheet, row, column, values);
        var lastColumn = column + values.Count - 1;
        var range = sheet.Range(row, column, row, lastColumn);
        range.Style.Font.Bold = true;
        range.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        range.Style.Border.TopBorderColor = BorderColor;
        return next;
    }

    public static string BuildPeriodLabel(ReportResultDto report)
    {
        var start = report.PeriodStart;
        var lastDay = report.PeriodEndExclusive.AddDays(-1);

        return start.Year == lastDay.Year && start.Month == lastDay.Month
            ? $"Periode: {start:dd MMM yyyy} – {lastDay:dd MMM yyyy}"
            : $"Periode: {start:dd MMM yyyy} – {lastDay:dd MMM yyyy}";
    }

    public static string LongMonth(int month) => LongMonthNames[month - 1];

    public static string FormatPercent(decimal fraction)
        => (fraction * 100m).ToString("0.0", CultureInfo.InvariantCulture) + "%";

    public static decimal Fraction(int part, int total)
        => total == 0 ? 0m : Math.Round(part / (decimal)total, 4, MidpointRounding.AwayFromZero);
}
