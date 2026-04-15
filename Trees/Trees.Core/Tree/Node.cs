using System.Numerics;

namespace Trees.Core.Tree;

public class Node
{
    public Node? Left { get; set; }
    public byte? LeftValue { get; set; }

    public Node? Right { get; set; }
    public byte? RightValue { get; set; }

    public Node(Node? left = null, byte? leftValue = null, Node? right = null, byte? rightValue = null)
    {
        if (leftValue is < 0 or > 9)
            throw new ArgumentOutOfRangeException(nameof(leftValue));

        if (rightValue is < 0 or > 9)
            throw new ArgumentOutOfRangeException(nameof(rightValue));

        Left = left;
        LeftValue = leftValue;
        Right = right;
        RightValue = rightValue;
    }
}
