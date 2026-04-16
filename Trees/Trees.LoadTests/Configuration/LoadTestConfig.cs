namespace Trees.LoadTests.Configuration;

public sealed class LoadTestConfig
{
    public string OutputDirectory { get; set; } = "Results";
    public string RunFolderPrefix { get; set; } = "run";
    public int Seed { get; set; } = 1;
    public int IterationsPerCase { get; set; } = 15;
    public int WarmCacheIterationsPerCase { get; set; } = 15;
    public int WarmBaselineIterationsPerCase { get; set; } = 15;
    public List<int> Sizes { get; set; } = [10, 100, 1000];
    public List<string> TreeShapes { get; set; } = ["Complete", "DegenerateLeft", "DegenerateRight", "Random"];
    public List<string> Algorithms { get; set; } = ["RecursiveDfs", "Bfs", "CacheDfs", "Morris"];
    public bool ForceFullGcBeforeEachRun { get; set; } = true;
    public bool VerifyResultsConsistency { get; set; } = true;
    public bool SaveRunRecordsJson { get; set; } = true;
    public bool IncludeWarmCacheScenario { get; set; } = true;
    public bool IncludeWarmBaselineScenario { get; set; } = true;
}
