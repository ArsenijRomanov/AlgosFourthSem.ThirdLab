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
        if (curNode.Left == null && curNode.Right == null)
            return curSum;

        var sum = BigInteger.Zero;

        if (curNode.Left != null)
            sum += Dfs(curNode.Left, curSum * 10 + curNode.LeftValue!.Value);

        if (curNode.Right != null)
            sum += Dfs(curNode.Right, curSum * 10 + curNode.RightValue!.Value);

        return sum;
    }
}
