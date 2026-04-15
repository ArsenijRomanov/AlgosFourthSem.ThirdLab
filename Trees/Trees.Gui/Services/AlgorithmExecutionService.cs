using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Trees.Core.Solvers;
using Trees.Core.Tree;
using Trees.Gui.Models;

namespace Trees.Gui.Services;

public sealed class AlgorithmExecutionService
{
    private readonly CacheDfsSolver _persistentCacheSolver = new();

    private EditableTreeNode? _persistentEditableRoot;
    private Node? _persistentCoreRoot;
    private Dictionary<int, Node>? _persistentNodeMap;

    public RunMeasurement Execute(AlgorithmOption algorithm, EditableTreeNode sourceRoot, string scopeName)
    {
        if (algorithm == AlgorithmOption.CacheDfs)
            return ExecuteCache(sourceRoot, scopeName);

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

    public void InvalidatePersistentCache()
    {
        _persistentEditableRoot = null;
        _persistentCoreRoot = null;
        _persistentNodeMap = null;
        _persistentCacheSolver.ClearCache();
    }

    public void ClearPersistentCache()
        => _persistentCacheSolver.ClearCache();

    public static string GetAlgorithmName(AlgorithmOption option) => option switch
    {
        AlgorithmOption.RecursiveDfs => "Recursive DFS",
        AlgorithmOption.Bfs => "BFS",
        AlgorithmOption.Morris => "Morris Traversal",
        AlgorithmOption.CacheDfs => "Cache DFS",
        _ => option.ToString()
    };

    private RunMeasurement ExecuteCache(EditableTreeNode sourceRoot, string scopeName)
    {
        EnsurePersistentTree(sourceRoot);

        var coreNode = _persistentNodeMap![sourceRoot.Id];

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var before = GC.GetAllocatedBytesForCurrentThread();
        var stopwatch = Stopwatch.StartNew();
        var result = _persistentCacheSolver.Solve(coreNode);
        stopwatch.Stop();
        var after = GC.GetAllocatedBytesForCurrentThread();

        return new RunMeasurement
        {
            ScopeName = scopeName,
            AlgorithmName = GetAlgorithmName(AlgorithmOption.CacheDfs),
            ResultValue = result.ToString(),
            DurationText = FormatDuration(stopwatch.Elapsed),
            AllocatedText = FormatBytes(after - before)
        };
    }

    private void EnsurePersistentTree(EditableTreeNode sourceRoot)
    {
        var editableRoot = GetDocumentRoot(sourceRoot);

        if (ReferenceEquals(_persistentEditableRoot, editableRoot) &&
            _persistentCoreRoot is not null &&
            _persistentNodeMap is not null)
        {
            return;
        }

        var nodeMap = new Dictionary<int, Node>();
        var coreRoot = BuildCoreTree(editableRoot, nodeMap);

        _persistentEditableRoot = editableRoot;
        _persistentCoreRoot = coreRoot;
        _persistentNodeMap = nodeMap;
        _persistentCacheSolver.ClearCache();
    }

    private static EditableTreeNode GetDocumentRoot(EditableTreeNode node)
    {
        var current = node;

        while (current.Parent is not null)
            current = current.Parent;

        return current;
    }

    private static Node BuildCoreTree(EditableTreeNode source, Dictionary<int, Node> nodeMap)
    {
        var node = new Node();
        nodeMap[source.Id] = node;

        if (source.Left is not null)
        {
            node.Left = BuildCoreTree(source.Left, nodeMap);
            node.LeftValue = source.LeftValue;
        }

        if (source.Right is not null)
        {
            node.Right = BuildCoreTree(source.Right, nodeMap);
            node.RightValue = source.RightValue;
        }

        return node;
    }

    private static ISolver CreateSolver(AlgorithmOption option) => option switch
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