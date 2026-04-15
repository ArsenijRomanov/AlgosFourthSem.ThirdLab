using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Trees.Core;
using Trees.Core.Solvers;
using Trees.Core.Tree;

namespace Trees.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[SimpleJob(launchCount: 2, warmupCount: 5, iterationCount: 15)]
[RunOncePerIteration]
public class PathSumBenchmarks
{
    [Params(10, 100, 1000)]
    public int Size { get; set; }

    [Params(
        TreeShape.Complete,
        TreeShape.DegenerateLeft,
        TreeShape.DegenerateRight,
        TreeShape.Random)]
    public TreeShape Shape { get; set; }

    private Node _root = null!;

    private readonly RecursiveDfsSolver _recursiveDfsSolver = new();
    private readonly BfsSolver _bfsSolver = new();
    private readonly CacheDfsSolver _cacheDfsSolver = new();
    private readonly MorrisSolver _morrisSolver = new();

    [IterationSetup]
    public void IterationSetup()
    {
        _root = CreateTree(Size, Shape, seed: 1);
        _cacheDfsSolver.ClearCache();
    }

    [Benchmark(Baseline = true)]
    public BigInteger RecursiveDfs()
        => _recursiveDfsSolver.Solve(_root);

    [Benchmark]
    public BigInteger Bfs()
        => _bfsSolver.Solve(_root);

    [Benchmark]
    public BigInteger CacheDfs()
        => _cacheDfsSolver.Solve(_root);

    [Benchmark]
    public BigInteger Morris()
        => _morrisSolver.Solve(_root);

    private static Node CreateTree(int size, TreeShape shape, int seed)
        => shape switch
        {
            TreeShape.Complete => TreeFactory.CreateCompleteTree(size, seed),
            TreeShape.DegenerateLeft => TreeFactory.CreateDegenerateTree(size, seed, right: false),
            TreeShape.DegenerateRight => TreeFactory.CreateDegenerateTree(size, seed, right: true),
            TreeShape.Random => TreeFactory.CreateRandomTree(size, seed),
            _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, null)
        };
}
