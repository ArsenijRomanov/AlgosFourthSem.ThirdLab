namespace Trees.LoadTests.Models;

public sealed class CaseSummaryRecord
{
    public int Size { get; init; }
    public TreeShapeKind Shape { get; init; }
    public AlgorithmKind Algorithm { get; init; }
    public ExecutionScenarioKind Scenario { get; init; }
    public int SampleCount { get; init; }
    public double MeanMilliseconds { get; init; }
    public double MedianMilliseconds { get; init; }
    public double StdDevMilliseconds { get; init; }
    public double MinMilliseconds { get; init; }
    public double MaxMilliseconds { get; init; }
    public double MeanAllocatedBytes { get; init; }
    public long MinAllocatedBytes { get; init; }
    public long MaxAllocatedBytes { get; init; }
    public int Failures { get; init; }
}
