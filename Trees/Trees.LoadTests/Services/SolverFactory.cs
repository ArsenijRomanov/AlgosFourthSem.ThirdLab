using Trees.Core.Solvers;
using Trees.LoadTests.Models;

namespace Trees.LoadTests.Services;

public static class SolverFactory
{
    public static ISolver Create(AlgorithmKind algorithm)
        => algorithm switch
        {
            AlgorithmKind.RecursiveDfs => new RecursiveDfsSolver(),
            AlgorithmKind.Bfs => new BfsSolver(),
            AlgorithmKind.CacheDfs => new CacheDfsSolver(),
            AlgorithmKind.Morris => new MorrisSolver(),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };
}
