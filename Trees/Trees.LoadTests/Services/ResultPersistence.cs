using System.Text.Json;
using Trees.LoadTests.Configuration;
using Trees.LoadTests.Models;
using Trees.LoadTests.Utilities;

namespace Trees.LoadTests.Services;

public sealed class ResultPersistence
{
    private readonly string _projectRoot;

    public ResultPersistence(string projectRoot)
    {
        _projectRoot = projectRoot;
    }

    public string CreateRunDirectory(LoadTestConfig config)
    {
        var baseDirectory = ResolveOutputDirectory(config.OutputDirectory);
        Directory.CreateDirectory(baseDirectory);

        var runId = $"{config.RunFolderPrefix}-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
        var runDirectory = Path.Combine(baseDirectory, runId);
        Directory.CreateDirectory(runDirectory);

        return runDirectory;
    }

    public void SaveConfigSnapshot(string runDirectory, LoadTestConfig config)
    {
        var configPath = Path.Combine(runDirectory, "config.snapshot.json");
        var json = JsonSerializer.Serialize(config, ConfigLoader.JsonOptions());
        File.WriteAllText(configPath, json);
    }

    public void SaveMetadata(string runDirectory, RunMetadata metadata)
    {
        var metadataPath = Path.Combine(runDirectory, "metadata.json");
        var json = JsonSerializer.Serialize(metadata, ConfigLoader.JsonOptions());
        File.WriteAllText(metadataPath, json);
    }

    public void SaveRunRecords(string runDirectory, IReadOnlyList<RunRecord> records, bool saveRunRecordsJson)
    {
        CsvFileWriter.WriteRunRecords(Path.Combine(runDirectory, "run_records.csv"), records);

        if (!saveRunRecordsJson)
            return;

        var jsonPath = Path.Combine(runDirectory, "run_records.json");
        var json = JsonSerializer.Serialize(records, ConfigLoader.JsonOptions());
        File.WriteAllText(jsonPath, json);
    }

    public void SaveSummaries(
        string runDirectory,
        IReadOnlyList<CaseSummaryRecord> caseSummaries,
        IReadOnlyList<AlgorithmSummaryRecord> algorithmSummaries)
    {
        CsvFileWriter.WriteCaseSummaries(Path.Combine(runDirectory, "case_summary.csv"), caseSummaries);
        CsvFileWriter.WriteAlgorithmSummaries(Path.Combine(runDirectory, "algorithm_summary.csv"), algorithmSummaries);
    }

    private string ResolveOutputDirectory(string configuredOutputDirectory)
        => Path.IsPathRooted(configuredOutputDirectory)
            ? configuredOutputDirectory
            : Path.GetFullPath(Path.Combine(_projectRoot, configuredOutputDirectory));
}
