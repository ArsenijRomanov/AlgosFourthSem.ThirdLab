using System.Diagnostics;
using System.Numerics;
using Trees.Core.Solvers;
using Trees.Core.Tree;
using Trees.LoadTests.Models;

namespace Trees.LoadTests.Services;

public sealed class MeasurementService
{
    public RunRecord Measure(
        string runId,
        Node root,
        AlgorithmKind algorithm,
        ExecutionScenarioKind scenario,
        int iteration,
        int size,
        TreeShapeKind shape,
        int seed,
        bool forceFullGcBeforeEachRun,
        BigInteger baselineResult,
        DateTime startedAtUtc)
    {
        var solver = SolverFactory.Create(algorithm);
        return MeasureWithSolver(
            runId,
            root,
            solver,
            algorithm,
            scenario,
            iteration,
            size,
            shape,
            seed,
            forceFullGcBeforeEachRun,
            baselineResult,
            startedAtUtc);
    }

    public RunRecord MeasureWithSolver(
        string runId,
        Node root,
        ISolver solver,
        AlgorithmKind algorithm,
        ExecutionScenarioKind scenario,
        int iteration,
        int size,
        TreeShapeKind shape,
        int seed,
        bool forceFullGcBeforeEachRun,
        BigInteger baselineResult,
        DateTime startedAtUtc)
    {
        if (forceFullGcBeforeEachRun)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var gen0Before = GC.CollectionCount(0);
        var gen1Before = GC.CollectionCount(1);
        var gen2Before = GC.CollectionCount(2);

        var stopwatch = Stopwatch.StartNew();
        var result = solver.Solve(root);
        stopwatch.Stop();

        var allocatedAfter = GC.GetAllocatedBytesForCurrentThread();
        var gen0After = GC.CollectionCount(0);
        var gen1After = GC.CollectionCount(1);
        var gen2After = GC.CollectionCount(2);

        return new RunRecord
        {
            RunId = runId,
            StartedAtUtc = startedAtUtc,
            Size = size,
            Shape = shape,
            Algorithm = algorithm,
            Scenario = scenario,
            Iteration = iteration,
            Seed = seed,
            ElapsedTicks = stopwatch.ElapsedTicks,
            ElapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds,
            AllocatedBytes = allocatedAfter - allocatedBefore,
            Gen0Collections = gen0After - gen0Before,
            Gen1Collections = gen1After - gen1Before,
            Gen2Collections = gen2After - gen2Before,
            MatchesBaseline = result == baselineResult,
            ResultDigits = result.ToString().Length,
            ResultValue = result.ToString(),
            MachineName = Environment.MachineName,
            FrameworkDescription = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            OsDescription = System.Runtime.InteropServices.RuntimeInformation.OSDescription
        };
    }
}
