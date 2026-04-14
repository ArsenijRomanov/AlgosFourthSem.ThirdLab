namespace Trees.Core.Tree;

public class Node(Edge? left = null, Edge? right = null)
{
    public Edge? LeftEdge { get; set; } = left;
    public Edge? RightEdge { get; set; } = right;
}
