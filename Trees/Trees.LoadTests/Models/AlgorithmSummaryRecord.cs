namespace Trees.LoadTests.Models;

public sealed class AlgorithmSummaryRecord
{
    public AlgorithmKind Algorithm { get; init; }
    public ExecutionScenarioKind Scenario { get; init; }
    public int SampleCount { get; init; }
    public double MeanMilliseconds { get; init; }
    public double MedianMilliseconds { get; init; }
    public double StdDevMilliseconds { get; init; }
    public double MeanAllocatedBytes { get; init; }
    public int Failures { get; init; }
}
