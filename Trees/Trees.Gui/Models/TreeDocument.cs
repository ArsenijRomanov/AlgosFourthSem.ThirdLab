using System;

namespace Trees.Gui.Models;

public sealed class TreeDocument
{
    private int _nextId = 1;

    public EditableTreeNode? Root { get; set; }

    public EditableTreeNode CreateNode()
        => new() { Id = _nextId++ };

    public void ResetCounterFromTree(EditableTreeNode? root)
        => _nextId = root is null ? 1 : MaxId(root) + 1;

    private static int MaxId(EditableTreeNode node)
    {
        var max = node.Id;

        if (node.Left is not null)
            max = Math.Max(max, MaxId(node.Left));

        if (node.Right is not null)
            max = Math.Max(max, MaxId(node.Right));

        return max;
    }
}
