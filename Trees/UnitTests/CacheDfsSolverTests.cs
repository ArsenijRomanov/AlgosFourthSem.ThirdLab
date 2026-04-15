using Trees.Core.Solvers;
using UnitTests.TestData;
using Xunit;

namespace UnitTests;

public class CacheDfsSolverTests
{
    [Fact]
    public void Solve_SingleNode_ReturnsExpectedSum()
    {
        var solver = new CacheDfsSolver();
        var root = ManualTrees.CreateSingleNode();

        var actual = solver.Solve(root);

        Assert.Equal(ManualTrees.SingleNodeSum, actual);
    }

    [Fact]
    public void Solve_OneLeftEdge_ReturnsExpectedSum()
    {
        var solver = new CacheDfsSolver();
        var root = ManualTrees.CreateOneLeftEdge();

        var actual = solver.Solve(root);

        Assert.Equal(ManualTrees.OneLeftEdgeSum, actual);
    }

    [Fact]
    public void Solve_OneRightEdge_ReturnsExpectedSum()
    {
        var solver = new CacheDfsSolver();
        var root = ManualTrees.CreateOneRightEdge();

        var actual = solver.Solve(root);

        Assert.Equal(ManualTrees.OneRightEdgeSum, actual);
    }

    [Fact]
    public void Solve_TwoLeaves_ReturnsExpectedSum()
    {
        var solver = new CacheDfsSolver();
        var root = ManualTrees.CreateTwoLeaves();

        var actual = solver.Solve(root);

        Assert.Equal(ManualTrees.TwoLeavesSum, actual);
    }

    [Fact]
    public void Solve_MixedSmallTree_ReturnsExpectedSum()
    {
        var solver = new CacheDfsSolver();
        var root = ManualTrees.CreateMixedSmallTree();

        var actual = solver.Solve(root);

        Assert.Equal(ManualTrees.MixedSmallTreeSum, actual);
    }

    [Fact]
    public void Solve_LeftChain_ReturnsExpectedSum()
    {
        var solver = new CacheDfsSolver();
        var root = ManualTrees.CreateLeftChain();

        var actual = solver.Solve(root);

        Assert.Equal(ManualTrees.LeftChainSum, actual);
    }

    [Fact]
    public void Solve_RightChain_ReturnsExpectedSum()
    {
        var solver = new CacheDfsSolver();
        var root = ManualTrees.CreateRightChain();

        var actual = solver.Solve(root);

        Assert.Equal(ManualTrees.RightChainSum, actual);
    }

    [Fact]
    public void Solve_RepeatedCall_ReturnsSameResult()
    {
        var solver = new CacheDfsSolver();
        var root = ManualTrees.CreateMixedSmallTree();

        var first = solver.Solve(root);
        var second = solver.Solve(root);

        Assert.Equal(first, second);
    }

    [Fact]
    public void ClearCache_DoesNotChangeResult()
    {
        var solver = new CacheDfsSolver();
        var root = ManualTrees.CreateMixedSmallTree();

        var first = solver.Solve(root);
        solver.ClearCache();
        var second = solver.Solve(root);

        Assert.Equal(first, second);
    }
}