using System.Numerics;
using Trees.Core.Tree;

namespace Trees.Core.Solvers;

public class BfsSolver : ISolver
{
    public BigInteger Solve(Node root)
    {
        ArgumentNullException.ThrowIfNull(root);

        var queue = new Queue<(Node node, BigInteger sum)>();
        queue.Enqueue((root, BigInteger.Zero));

        var sum = BigInteger.Zero;

        while (queue.Count != 0)
        {
            var cur = queue.Dequeue();

            if (cur.node is { LeftEdge: null, RightEdge: null })
                sum += cur.sum;

            if (cur.node.LeftEdge != null)
                queue.Enqueue((cur.node.LeftEdge.To, cur.sum * 10 + cur.node.LeftEdge.Value));

            if (cur.node.RightEdge != null)
                queue.Enqueue((cur.node.RightEdge.To, cur.sum * 10 + cur.node.RightEdge.Value));
        }

        return sum;
    }
}
