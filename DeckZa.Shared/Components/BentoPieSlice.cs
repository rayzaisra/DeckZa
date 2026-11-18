using System.Globalization;
using System.Text;

namespace DeckZa.Shared.Components;

public sealed class BentoPieSlice
{
    public string Label { get; init; } = string.Empty;
    public int Count { get; init; }
    public string Color { get; init; } = "#64748b";
    public string Path { get; init; } = string.Empty;

    public string Tooltip => $"{Label}: {Count} task";

    public static List<BentoPieSlice> Build(
        IEnumerable<(string Label, int Count, string Color)> items,
        double radius = 56d,
        double gapDegrees = 1.2d)
    {
        var source = items.Where(item => item.Count > 0).ToList();
        var total = source.Sum(item => item.Count);
        var slices = new List<BentoPieSlice>();

        if (total <= 0)
        {
            return slices;
        }

        var angle = -90d;

        foreach (var item in source)
        {
            var sweep = item.Count * 360d / total;
            var start = angle + (source.Count > 1 ? gapDegrees / 2 : 0);
            var end = angle + sweep - (source.Count > 1 ? gapDegrees / 2 : 0);

            if (end <= start)
            {
                end = start + 0.6d;
            }

            slices.Add(new BentoPieSlice
            {
                Label = item.Label,
                Count = item.Count,
                Color = item.Color,
                Path = BuildWedge(start, end, total == item.Count, radius),
            });

            angle += sweep;
        }

        return slices;
    }

    private static string BuildWedge(double startDegrees, double endDegrees, bool isFullCircle, double radius)
    {
        if (isFullCircle)
        {
            return $"M 0 -{Format(radius)} A {Format(radius)} {Format(radius)} 0 1 1 0 {Format(radius)} "
                   + $"A {Format(radius)} {Format(radius)} 0 1 1 0 -{Format(radius)} Z";
        }

        var start = ToPoint(startDegrees, radius);
        var end = ToPoint(endDegrees, radius);
        var largeArc = endDegrees - startDegrees > 180d ? 1 : 0;

        return $"M 0 0 L {start} A {Format(radius)} {Format(radius)} 0 {largeArc} 1 {end} Z";
    }

    private static string ToPoint(double degrees, double radius)
    {
        var radians = degrees * Math.PI / 180d;
        var x = Math.Cos(radians) * radius;
        var y = Math.Sin(radians) * radius;
        return $"{Format(x)} {Format(y)}";
    }

    private static string Format(double value)
        => value.ToString("0.###", CultureInfo.InvariantCulture);
}
