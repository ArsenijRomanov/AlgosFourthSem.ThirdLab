namespace Trees.LoadTests.Models;

public sealed class RunMetadata
{
    public string RunId { get; init; } = string.Empty;
    public DateTime StartedAtUtc { get; init; }
    public string ProjectRoot { get; init; } = string.Empty;
    public string ConfigPath { get; init; } = string.Empty;
    public string ResultsDirectory { get; init; } = string.Empty;
    public string FrameworkDescription { get; init; } = string.Empty;
    public string OsDescription { get; init; } = string.Empty;
    public string ProcessArchitecture { get; init; } = string.Empty;
    public int ProcessorCount { get; init; }
}
