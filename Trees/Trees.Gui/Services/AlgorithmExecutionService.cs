using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Trees.Core.Solvers;
using Trees.Gui.Models;

namespace Trees.Gui.Services;

public sealed class AlgorithmExecutionService
{
    public RunMeasurement Execute(AlgorithmOption algorithm, EditableTreeNode sourceRoot, string scopeName)
    {
        var root = TreeMapper.ToCoreNode(sourceRoot);
        var solver = CreateSolver(algorithm);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var before = GC.GetAllocatedBytesForCurrentThread();
        var stopwatch = Stopwatch.StartNew();
        var result = solver.Solve(root);
        stopwatch.Stop();
        var after = GC.GetAllocatedBytesForCurrentThread();

        return new RunMeasurement
        {
            ScopeName = scopeName,
            AlgorithmName = GetAlgorithmName(algorithm),
            ResultValue = result.ToString(),
            DurationText = FormatDuration(stopwatch.Elapsed),
            AllocatedText = FormatBytes(after - before)
        };
    }

    public IReadOnlyList<RunMeasurement> ExecuteAll(EditableTreeNode sourceRoot, string scopeName)
        => Enum.GetValues<AlgorithmOption>()
            .Select(x => Execute(x, sourceRoot, scopeName))
            .ToList();

    public static string GetAlgorithmName(AlgorithmOption option)
        => option switch
        {
            AlgorithmOption.RecursiveDfs => "Recursive DFS",
            AlgorithmOption.Bfs => "BFS",
            AlgorithmOption.Morris => "Morris Traversal",
            AlgorithmOption.CacheDfs => "Cache DFS",
            _ => option.ToString()
        };

    private static ISolver CreateSolver(AlgorithmOption option)
        => option switch
        {
            AlgorithmOption.RecursiveDfs => new RecursiveDfsSolver(),
            AlgorithmOption.Bfs => new BfsSolver(),
            AlgorithmOption.Morris => new MorrisSolver(),
            AlgorithmOption.CacheDfs => new CacheDfsSolver(),
            _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
        };

    private static string FormatDuration(TimeSpan elapsed)
    {
        if (elapsed.TotalMilliseconds >= 1)
            return $"{elapsed.TotalMilliseconds:F3} ms";

        return $"{elapsed.TotalMicroseconds:F1} μs";
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";

        if (bytes < 1024 * 1024)
            return $"{bytes / 1024d:F2} KB";

        return $"{bytes / 1024d / 1024d:F2} MB";
    }
}
