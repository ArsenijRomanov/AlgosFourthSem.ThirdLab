namespace Trees.Core.Tree;

public class Node
{
    public byte Value { get; }
    public Node? Left { get; set; }
    public Node? Right { get; set; }

    public Node(byte value, Node? left = null, Node? right = null)
    {
        if (value > 9)
            throw new ArgumentOutOfRangeException(nameof(value));
        Value = value;
        Left = left;
        Right = right;
    }
}
