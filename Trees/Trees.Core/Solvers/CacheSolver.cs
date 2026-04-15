using System.Numerics;
using Trees.Core.Tree;

namespace Trees.Core.Solvers;

public class CacheDfsSolver : ISolver
{
    private readonly Dictionary<Node, CacheInfo> _cache = new();

    public BigInteger Solve(Node root)
    {
        ArgumentNullException.ThrowIfNull(root);
        if (_cache.TryGetValue(root, out var info))
            return info.Sum;

        Dfs(root);
        var sum = _cache[root].Sum;

        return sum;
    }

    public void ClearCache()
        => _cache.Clear();

    private void Dfs(Node curNode)
    {
        if (_cache.ContainsKey(curNode))
            return;

        if (curNode.Left == null && curNode.Right == null)
        {
            _cache[curNode] = new CacheInfo(BigInteger.Zero, BigInteger.One);
            return;
        }

        var curSum = BigInteger.Zero;
        var curShift = BigInteger.Zero;

        if (curNode.Left != null)
        {
            Dfs(curNode.Left);

            var leftInfo = _cache[curNode.Left];
            curSum += curNode.LeftValue!.Value * leftInfo.Shift + leftInfo.Sum;
            curShift += leftInfo.Shift * 10;
        }

        if (curNode.Right != null)
        {
            Dfs(curNode.Right);

            var rightInfo = _cache[curNode.Right];
            curSum += curNode.RightValue!.Value * rightInfo.Shift + rightInfo.Sum;
            curShift += rightInfo.Shift * 10;
        }

        _cache[curNode] = new CacheInfo(curSum, curShift);
    }

    private record CacheInfo(BigInteger Sum, BigInteger Shift);
}
