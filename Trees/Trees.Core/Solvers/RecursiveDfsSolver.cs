using System.Numerics;
using Trees.Core.Tree;

namespace Trees.Core.Solvers;

public class RecursiveDfsSolver : ISolver
{
    public BigInteger Solve(Node root)
    {
        ArgumentNullException.ThrowIfNull(root);
        return Dfs(root, BigInteger.Zero);
    }

    private static BigInteger Dfs(Node curNode, BigInteger curSum)
    {
        if (curNode.LeftEdge == null && curNode.RightEdge == null)
            return curSum;

        var sum = BigInteger.Zero;

        if (curNode.LeftEdge != null)
            sum += Dfs(curNode.LeftEdge.To, curSum * 10 + curNode.LeftEdge.Value);

        if (curNode.RightEdge != null)
            sum += Dfs(curNode.RightEdge.To, curSum * 10 + curNode.RightEdge.Value);

        return sum;
    }
}
