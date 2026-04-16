using System.Numerics;
using Trees.Core.Solvers;
using Trees.LoadTests.Configuration;
using Trees.LoadTests.Models;

namespace Trees.LoadTests.Services;

public sealed class LoadTestRunner
{
    private readonly string _projectRoot;
    private readonly ResultPersistence _resultPersistence;
    private readonly MeasurementService _measurementService;

    public LoadTestRunner(string projectRoot)
    {
        _projectRoot = projectRoot;
        _resultPersistence = new ResultPersistence(projectRoot);
        _measurementService = new MeasurementService();
    }

    public string Run(LoadTestConfig config, string configPath)
    {
        var startedAtUtc = DateTime.UtcNow;
        var runDirectory = _resultPersistence.CreateRunDirectory(config);
        var runId = Path.GetFileName(runDirectory);

        _resultPersistence.SaveConfigSnapshot(runDirectory, config);
        _resultPersistence.SaveMetadata(runDirectory, CreateMetadata(runId, startedAtUtc, runDirectory, configPath));

        var shapes = config.TreeShapes.Select(ParseTreeShape).ToArray();
        var algorithms = config.Algorithms.Select(ParseAlgorithm).ToArray();

        var records = new List<RunRecord>();

        foreach (var shape in shapes)
        {
            foreach (var size in config.Sizes)
            {
                var baselineTree = TreeBuilder.CreateTree(size, shape, config.Seed);
                var baselineResult = config.VerifyResultsConsistency
                    ? new RecursiveDfsSolver().Solve(baselineTree)
                    : BigInteger.Zero;

                for (var iteration = 1; iteration <= config.IterationsPerCase; iteration++)
                {
                    foreach (var algorithm in algorithms)
                    {
                        var tree = TreeBuilder.CreateTree(size, shape, config.Seed);
                        var record = _measurementService.Measure(
                            runId,
                            tree,
                            algorithm,
                            ExecutionScenarioKind.Cold,
                            iteration,
                            size,
                            shape,
                            config.Seed,
                            config.ForceFullGcBeforeEachRun,
                            baselineResult,
                            startedAtUtc);

                        records.Add(record);
                        Console.WriteLine($"[{record.Scenario}, {record.Shape}, size={record.Size}, iter={record.Iteration}] {record.Algorithm}: {record.ElapsedMilliseconds:F4} ms, {record.AllocatedBytes} B");
                    }
                }

                if (config.IncludeWarmBaselineScenario && algorithms.Contains(AlgorithmKind.RecursiveDfs))
                {
                    var warmBaselineTree = TreeBuilder.CreateTree(size, shape, config.Seed);
                    var warmBaselineSolver = new RecursiveDfsSolver();
                    warmBaselineSolver.Solve(warmBaselineTree);

                    for (var iteration = 1; iteration <= config.WarmBaselineIterationsPerCase; iteration++)
                    {
                        var warmBaselineRecord = _measurementService.MeasureWithSolver(
                            runId,
                            warmBaselineTree,
                            warmBaselineSolver,
                            AlgorithmKind.RecursiveDfs,
                            ExecutionScenarioKind.WarmBaseline,
                            iteration,
                            size,
                            shape,
                            config.Seed,
                            config.ForceFullGcBeforeEachRun,
                            baselineResult,
                            startedAtUtc);

                        records.Add(warmBaselineRecord);
                        Console.WriteLine($"[{warmBaselineRecord.Scenario}, {warmBaselineRecord.Shape}, size={warmBaselineRecord.Size}, iter={warmBaselineRecord.Iteration}] {warmBaselineRecord.Algorithm}: {warmBaselineRecord.ElapsedMilliseconds:F4} ms, {warmBaselineRecord.AllocatedBytes} B");
                    }
                }

                if (config.IncludeWarmCacheScenario && algorithms.Contains(AlgorithmKind.CacheDfs))
                {
                    var warmTree = TreeBuilder.CreateTree(size, shape, config.Seed);
                    var warmSolver = new CacheDfsSolver();
                    warmSolver.ClearCache();
                    warmSolver.Solve(warmTree);

                    for (var iteration = 1; iteration <= config.WarmCacheIterationsPerCase; iteration++)
                    {
                        var warmRecord = _measurementService.MeasureWithSolver(
                            runId,
                            warmTree,
                            warmSolver,
                            AlgorithmKind.CacheDfs,
                            ExecutionScenarioKind.WarmCache,
                            iteration,
                            size,
                            shape,
                            config.Seed,
                            config.ForceFullGcBeforeEachRun,
                            baselineResult,
                            startedAtUtc);

                        records.Add(warmRecord);
                        Console.WriteLine($"[{warmRecord.Scenario}, {warmRecord.Shape}, size={warmRecord.Size}, iter={warmRecord.Iteration}] {warmRecord.Algorithm}: {warmRecord.ElapsedMilliseconds:F4} ms, {warmRecord.AllocatedBytes} B");
                    }
                }
            }
        }

        var caseSummaries = SummaryBuilder.BuildCaseSummaries(records);
        var algorithmSummaries = SummaryBuilder.BuildAlgorithmSummaries(records);

        _resultPersistence.SaveRunRecords(runDirectory, records, config.SaveRunRecordsJson);
        _resultPersistence.SaveSummaries(runDirectory, caseSummaries, algorithmSummaries);

        return runDirectory;
    }

    private RunMetadata CreateMetadata(string runId, DateTime startedAtUtc, string runDirectory, string configPath)
        => new()
        {
            RunId = runId,
            StartedAtUtc = startedAtUtc,
            ProjectRoot = _projectRoot,
            ConfigPath = configPath,
            ResultsDirectory = runDirectory,
            FrameworkDescription = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            OsDescription = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            ProcessArchitecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
            ProcessorCount = Environment.ProcessorCount
        };

    private static TreeShapeKind ParseTreeShape(string value)
    {
        if (Enum.TryParse<TreeShapeKind>(value, ignoreCase: true, out var parsed))
            return parsed;

        throw new InvalidOperationException($"Unknown tree shape: {value}");
    }

    private static AlgorithmKind ParseAlgorithm(string value)
    {
        if (Enum.TryParse<AlgorithmKind>(value, ignoreCase: true, out var parsed))
            return parsed;

        throw new InvalidOperationException($"Unknown algorithm: {value}");
    }
}
