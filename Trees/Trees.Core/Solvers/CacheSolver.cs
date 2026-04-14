using System.Numerics;
using Trees.Core.Tree;

namespace Trees.Core.Solvers;

public class CacheDfsSolver : ISolver
{
    private readonly Dictionary<Node, CacheInfo> _cache = new();

    public BigInteger Solve(Node root)
    {
        ArgumentNullException.ThrowIfNull(root);
        Dfs(root);

        var sum = _cache[root].Sum;

        _cache.Clear();
        return sum;
    }
    
    private void Dfs(Node curNode)
    {
        
        if (curNode.Left == null && curNode.Right == null)
        {
            _cache[curNode] = new CacheInfo(curNode.Value, 10);
            return;
        }
        
        BigInteger curSum = 0;
        BigInteger curShift = 0;
        
        if (curNode.Left != null)
        {
            Dfs(curNode.Left);
            curSum += _cache[curNode.Left].Sum;
            curShift += _cache[curNode.Left].Shift;
        }
        
        if (curNode.Right != null)
        {
            Dfs(curNode.Right);
            curSum += _cache[curNode.Right].Sum;
            curShift += _cache[curNode.Right].Shift;
        }

        curSum = curNode.Value * curShift + curSum;
        curShift *= 10;

        _cache[curNode] = new CacheInfo(curSum, curShift);
    }

    private record CacheInfo(BigInteger Sum, BigInteger Shift);
}
