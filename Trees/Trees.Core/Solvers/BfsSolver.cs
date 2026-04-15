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

            if (cur.node is { Left: null, Right: null })
                sum += cur.sum;

            if (cur.node.Left != null)
                queue.Enqueue((cur.node.Left, cur.sum * 10 + cur.node.LeftValue!.Value));

            if (cur.node.Right != null)
                queue.Enqueue((cur.node.Right, cur.sum * 10 + cur.node.RightValue!.Value));
        }

        return sum;
    }
}
