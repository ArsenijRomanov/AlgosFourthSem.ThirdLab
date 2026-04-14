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
        if (curNode.LeftEdge == null && curNode.RightEdge == null)
        {
            _cache[curNode] = new CacheInfo(BigInteger.Zero, BigInteger.One);
            return;
        }

        BigInteger curSum = BigInteger.Zero;
        BigInteger curShift = BigInteger.Zero;

        if (curNode.LeftEdge != null)
        {
            Dfs(curNode.LeftEdge.To);

            var leftInfo = _cache[curNode.LeftEdge.To];
            curSum += curNode.LeftEdge.Value * leftInfo.Shift + leftInfo.Sum;
            curShift += leftInfo.Shift * 10;
        }

        if (curNode.RightEdge != null)
        {
            Dfs(curNode.RightEdge.To);

            var rightInfo = _cache[curNode.RightEdge.To];
            curSum += curNode.RightEdge.Value * rightInfo.Shift + rightInfo.Sum;
            curShift += rightInfo.Shift * 10;
        }

        _cache[curNode] = new CacheInfo(curSum, curShift);
    }

    private record CacheInfo(BigInteger Sum, BigInteger Shift);
}
