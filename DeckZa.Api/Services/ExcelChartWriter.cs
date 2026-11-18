using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using A = DocumentFormat.OpenXml.Drawing;
using C = DocumentFormat.OpenXml.Drawing.Charts;
using Xdr = DocumentFormat.OpenXml.Drawing.Spreadsheet;

namespace DeckZa.Api.Services;

/// <summary>
/// Writes native Excel charts into an existing workbook. ClosedXML cannot create charts,
/// so the workbook is saved first and the chart parts are injected afterwards with the
/// Open XML SDK. Charts stay interactive and follow the referenced cell ranges.
/// </summary>
public static class ExcelChartWriter
{
    private const long ChartWidthEmu = 3_900_000;   // ~4.3 inch (fits two charts with a column gap)
    private const long ChartHeightEmu = 2_900_000;  // ~3.2 inch

    public sealed record ChartSpec
    {
        public string SheetName { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string SeriesName { get; init; } = string.Empty;
        public string CategoriesFormula { get; init; } = string.Empty;
        public string ValuesFormula { get; init; } = string.Empty;
        public int AnchorColumn { get; init; }
        public int AnchorRow { get; init; }
    }

    public static void Apply(string filePath, IReadOnlyList<ChartSpec> specs)
    {
        if (specs.Count == 0)
        {
            return;
        }

        using var document = SpreadsheetDocument.Open(filePath, true);
        var workbookPart = document.WorkbookPart;
        if (workbookPart is null)
        {
            return;
        }

        var drawingId = 1u;
        foreach (var group in specs.GroupBy(spec => spec.SheetName, StringComparer.Ordinal))
        {
            var sheet = workbookPart.Workbook.Descendants<Sheet>()
                .FirstOrDefault(item => item.Name is not null && item.Name.Value == group.Key);
            if (sheet?.Id?.Value is null)
            {
                continue;
            }

            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id.Value);
            var drawingsPart = worksheetPart.DrawingsPart ?? worksheetPart.AddNewPart<DrawingsPart>();
            drawingsPart.WorksheetDrawing ??= new Xdr.WorksheetDrawing();

            foreach (var spec in group)
            {
                var chartPart = drawingsPart.AddNewPart<ChartPart>();
                chartPart.ChartSpace = BuildChartSpace(spec);

                drawingsPart.WorksheetDrawing.Append(CreateAnchor(
                    spec,
                    drawingsPart.GetIdOfPart(chartPart),
                    drawingId++));
            }

            drawingsPart.WorksheetDrawing.Save();

            if (!worksheetPart.Worksheet.ChildElements.Any(element => element is Drawing))
            {
                InsertDrawing(worksheetPart);
            }

            worksheetPart.Worksheet.Save();
        }
    }

    private static Xdr.OneCellAnchor CreateAnchor(ChartSpec spec, string chartRelationId, uint shapeId)
    {
        var columnIndex = (uint)Math.Max(0, spec.AnchorColumn - 1);
        var rowIndex = (uint)Math.Max(0, spec.AnchorRow - 1);

        return new Xdr.OneCellAnchor(
            new Xdr.FromMarker(
                new Xdr.ColumnId(columnIndex.ToString()),
                new Xdr.ColumnOffset("0"),
                new Xdr.RowId(rowIndex.ToString()),
                new Xdr.RowOffset("0")),
            new Xdr.Extent { Cx = ChartWidthEmu, Cy = ChartHeightEmu },
            new Xdr.GraphicFrame(
                new Xdr.NonVisualGraphicFrameProperties(
                    new Xdr.NonVisualDrawingProperties { Id = shapeId, Name = $"Chart {shapeId}" },
                    new Xdr.NonVisualGraphicFrameDrawingProperties()),
                new Xdr.Transform(
                    new A.Offset { X = 0, Y = 0 },
                    new A.Extents { Cx = ChartWidthEmu, Cy = ChartHeightEmu }),
                new A.Graphic(
                    new A.GraphicData(new C.ChartReference { Id = chartRelationId })
                    { Uri = "http://schemas.openxmlformats.org/drawingml/2006/chart" })),
            new Xdr.ClientData());
    }

    private static void InsertDrawing(WorksheetPart worksheetPart)
    {
        var worksheet = worksheetPart.Worksheet;
        var drawing = new Drawing { Id = worksheetPart.GetIdOfPart(worksheetPart.DrawingsPart!) };

        var successor = worksheet.ChildElements.FirstOrDefault(element =>
            element is LegacyDrawing or LegacyDrawingHeaderFooter or Picture or OleObjects
                or Controls or WebPublishItems or TableParts or WorksheetExtensionList);

        if (successor is not null)
        {
            worksheet.InsertBefore(drawing, successor);
        }
        else
        {
            worksheet.Append(drawing);
        }
    }

    private static C.ChartSpace BuildChartSpace(ChartSpec spec)
    {
        var barChart = new C.BarChart(
            new C.BarDirection { Val = C.BarDirectionValues.Column },
            new C.BarGrouping { Val = C.BarGroupingValues.Clustered },
            new C.VaryColors { Val = false },
            new C.BarChartSeries(
                new C.Index { Val = 0 },
                new C.Order { Val = 0 },
                new C.SeriesText(new C.StringReference(new C.Formula(spec.SeriesName))),
                new C.CategoryAxisData(new C.StringReference(new C.Formula(spec.CategoriesFormula))),
                new C.Values(new C.NumberReference(new C.Formula(spec.ValuesFormula)))),
            new C.GapWidth { Val = 60 },
            new C.AxisId { Val = 1 },
            new C.AxisId { Val = 2 });

        var chart = new C.Chart();

        // Schema order inside c:chart: title, autoTitleDeleted, plotArea, ...
        chart.Append(new C.Title(
            new C.ChartText(new C.RichText(
                new A.BodyProperties(),
                new A.ListStyle(),
                new A.Paragraph(new A.Run(new A.Text(spec.Title))))),
            new C.Overlay { Val = false }));

        chart.Append(new C.AutoTitleDeleted { Val = false });

        chart.Append(new C.PlotArea(
            new C.Layout(),
            barChart,
            new C.CategoryAxis(
                new C.AxisId { Val = 1 },
                new C.Scaling(new C.Orientation { Val = C.OrientationValues.MinMax }),
                new C.Delete { Val = false },
                new C.AxisPosition { Val = C.AxisPositionValues.Bottom },
                new C.TickLabelPosition { Val = C.TickLabelPositionValues.NextTo },
                new C.CrossingAxis { Val = 2 },
                new C.Crosses { Val = C.CrossesValues.AutoZero },
                new C.AutoLabeled { Val = true },
                new C.LabelAlignment { Val = C.LabelAlignmentValues.Center },
                new C.LabelOffset { Val = 100 }),
            new C.ValueAxis(
                new C.AxisId { Val = 2 },
                new C.Scaling(new C.Orientation { Val = C.OrientationValues.MinMax }),
                new C.Delete { Val = false },
                new C.AxisPosition { Val = C.AxisPositionValues.Left },
                new C.MajorGridlines(),
                new C.CrossingAxis { Val = 1 },
                new C.Crosses { Val = C.CrossesValues.AutoZero },
                new C.CrossBetween { Val = C.CrossBetweenValues.Between })));

        chart.Append(new C.PlotVisibleOnly { Val = true });
        chart.Append(new C.DisplayBlanksAs { Val = C.DisplayBlanksAsValues.Gap });

        var chartSpace = new C.ChartSpace();
        chartSpace.Append(chart);
        return chartSpace;
    }
}
