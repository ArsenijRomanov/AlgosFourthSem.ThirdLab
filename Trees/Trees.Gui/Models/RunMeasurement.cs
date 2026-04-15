namespace Trees.Gui.Models;

public sealed class RunMeasurement
{
    public required string ScopeName { get; init; }
    public required string AlgorithmName { get; init; }
    public required string ResultValue { get; init; }
    public required string DurationText { get; init; }
    public required string AllocatedText { get; init; }
}
