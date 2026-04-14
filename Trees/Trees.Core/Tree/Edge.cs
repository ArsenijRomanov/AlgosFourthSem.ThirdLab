using System.Numerics;

namespace Trees.Core.Tree;

public class Edge
{
    public BigInteger Value { get; }
    public Node To { get; set; }

    public Edge(BigInteger value, Node to)
    {
        if (value < 0 || value > 9)
            throw new ArgumentOutOfRangeException(nameof(value));
        ArgumentNullException.ThrowIfNull(to);

        Value = value;
        To = to;
    }
}
