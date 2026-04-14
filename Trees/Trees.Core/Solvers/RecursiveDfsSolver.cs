using System.Numerics;
using Trees.Core.Tree;

namespace Trees.Core.Solvers;

public class RecursiveDfsSolver : ISolver
{
    public BigInteger Solve(Node root)
    {
        ArgumentNullException.ThrowIfNull(root);
        return Dfs(root, 0);
    }

    private static BigInteger Dfs(Node curNode, BigInteger curSum)
    {
        var nextSum = curSum * 10;
        
        if (curNode.Left == null && curNode.Right == null)
            return nextSum + curNode.Value;
        
        BigInteger sum = 0;
        
        if (curNode.Left != null)
            sum += Dfs(curNode.Left, nextSum + curNode.Value);
        
        if (curNode.Right != null)
            sum += Dfs(curNode.Right, nextSum + curNode.Value);

        return sum;
    }
}
