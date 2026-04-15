using System.Collections.Generic;
using Avalonia;
using Trees.Gui.Models;
using System;

namespace Trees.Gui.Services;

public sealed class TreeLayoutService
{
    public IReadOnlyList<TreeLayoutNode> Layout(EditableTreeNode? root)
    {
        if (root is null)
            return Array.Empty<TreeLayoutNode>();

        var result = new List<TreeLayoutNode>();
        var nextX = 0d;
        Build(root, 0, result, ref nextX);
        return result;
    }

    private static double Build(
        EditableTreeNode node,
        int depth,
        List<TreeLayoutNode> result,
        ref double nextX)
    {
        double x;

        if (node.Left is null && node.Right is null)
        {
            x = nextX;
            nextX += 1;
        }
        else if (node.Left is not null && node.Right is not null)
        {
            var leftX = Build(node.Left, depth + 1, result, ref nextX);
            var rightX = Build(node.Right, depth + 1, result, ref nextX);
            x = (leftX + rightX) / 2.0;
        }
        else if (node.Left is not null)
        {
            var leftX = Build(node.Left, depth + 1, result, ref nextX);
            x = leftX;
        }
        else
        {
            var rightX = Build(node.Right!, depth + 1, result, ref nextX);
            x = rightX;
        }

        result.Add(new TreeLayoutNode
        {
            Node = node,
            Depth = depth,
            Position = new Point(x, depth)
        });

        return x;
    }
}
