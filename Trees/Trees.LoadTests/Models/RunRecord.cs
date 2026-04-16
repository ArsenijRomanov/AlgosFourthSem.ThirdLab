namespace Trees.LoadTests.Models;

public sealed class RunRecord
{
    public string RunId { get; init; } = string.Empty;
    public DateTime StartedAtUtc { get; init; }
    public int Size { get; init; }
    public TreeShapeKind Shape { get; init; }
    public AlgorithmKind Algorithm { get; init; }
    public ExecutionScenarioKind Scenario { get; init; }
    public int Iteration { get; init; }
    public int Seed { get; init; }
    public long ElapsedTicks { get; init; }
    public double ElapsedMilliseconds { get; init; }
    public long AllocatedBytes { get; init; }
    public int Gen0Collections { get; init; }
    public int Gen1Collections { get; init; }
    public int Gen2Collections { get; init; }
    public bool MatchesBaseline { get; init; }
    public int ResultDigits { get; init; }
    public string ResultValue { get; init; } = string.Empty;
    public string MachineName { get; init; } = string.Empty;
    public string FrameworkDescription { get; init; } = string.Empty;
    public string OsDescription { get; init; } = string.Empty;
}
