namespace DeckZa.Shared.Components;

public sealed class BentoBar
{
    public string Label { get; init; } = string.Empty;
    public int Count { get; init; }
    public double HeightPixels { get; init; }
    public bool ShowLabel { get; init; } = true;

    public string Tooltip => $"{Label}: {Count} task";
}
