using Trees.Core.Solvers;
using Trees.UnitTests.TestData;

namespace Trees.UnitTests;

public class MorrisSolverTests
{
    [Fact]
    public void Solve_SingleNode_ReturnsExpectedSum()
    {
        var solver = new MorrisSolver();
        var root = ManualTrees.CreateSingleNode();

        var actual = solver.Solve(root);

        Assert.Equal(ManualTrees.SingleNodeSum, actual);
    }

    [Fact]
    public void Solve_OneLeftEdge_ReturnsExpectedSum()
    {
        var solver = new MorrisSolver();
        var root = ManualTrees.CreateOneLeftEdge();

        var actual = solver.Solve(root);

        Assert.Equal(ManualTrees.OneLeftEdgeSum, actual);
    }

    [Fact]
    public void Solve_OneRightEdge_ReturnsExpectedSum()
    {
        var solver = new MorrisSolver();
        var root = ManualTrees.CreateOneRightEdge();

        var actual = solver.Solve(root);

        Assert.Equal(ManualTrees.OneRightEdgeSum, actual);
    }

    [Fact]
    public void Solve_TwoLeaves_ReturnsExpectedSum()
    {
        var solver = new MorrisSolver();
        var root = ManualTrees.CreateTwoLeaves();

        var actual = solver.Solve(root);

        Assert.Equal(ManualTrees.TwoLeavesSum, actual);
    }

    [Fact]
    public void Solve_MixedSmallTree_ReturnsExpectedSum()
    {
        var solver = new MorrisSolver();
        var root = ManualTrees.CreateMixedSmallTree();

        var actual = solver.Solve(root);

        Assert.Equal(ManualTrees.MixedSmallTreeSum, actual);
    }

    [Fact]
    public void Solve_LeftChain_ReturnsExpectedSum()
    {
        var solver = new MorrisSolver();
        var root = ManualTrees.CreateLeftChain();

        var actual = solver.Solve(root);

        Assert.Equal(ManualTrees.LeftChainSum, actual);
    }

    [Fact]
    public void Solve_RightChain_ReturnsExpectedSum()
    {
        var solver = new MorrisSolver();
        var root = ManualTrees.CreateRightChain();

        var actual = solver.Solve(root);

        Assert.Equal(ManualTrees.RightChainSum, actual);
    }

    [Fact]
    public void Solve_DoesNotCorruptTree()
    {
        var root = ManualTrees.CreateMixedSmallTree();
        var dfsSolver = new RecursiveDfsSolver();
        var morrisSolver = new MorrisSolver();

        var expectedBefore = dfsSolver.Solve(root);
        var morrisResult = morrisSolver.Solve(root);
        var expectedAfter = dfsSolver.Solve(root);

        Assert.Equal(expectedBefore, morrisResult);
        Assert.Equal(expectedBefore, expectedAfter);
    }
}
