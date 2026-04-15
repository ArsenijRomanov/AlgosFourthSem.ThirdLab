using System.Collections.Generic;
using System.Linq;
using Trees.Gui.Models;

namespace Trees.Gui.Services;

public sealed class TreeEditorService
{
    public EditableTreeNode EnsureRoot(TreeDocument document)
    {
        document.Root ??= document.CreateNode();
        return document.Root;
    }

    public EditableTreeNode AddLeftChild(TreeDocument document, EditableTreeNode parent, byte edgeValue = 0)
    {
        if (parent.Left is not null)
            return parent.Left;

        var child = document.CreateNode();
        child.Parent = parent;
        parent.Left = child;
        parent.LeftValue = edgeValue;
        return child;
    }

    public EditableTreeNode AddRightChild(TreeDocument document, EditableTreeNode parent, byte edgeValue = 0)
    {
        if (parent.Right is not null)
            return parent.Right;

        var child = document.CreateNode();
        child.Parent = parent;
        parent.Right = child;
        parent.RightValue = edgeValue;
        return child;
    }

    public void DeleteSubtree(TreeDocument document, EditableTreeNode node)
    {
        if (document.Root == node)
        {
            document.Root = null;
            return;
        }

        var parent = node.Parent;
        if (parent is null)
            return;

        if (parent.Left == node)
        {
            parent.Left = null;
            parent.LeftValue = null;
        }
        else if (parent.Right == node)
        {
            parent.Right = null;
            parent.RightValue = null;
        }
    }

    public IEnumerable<EditableTreeNode> Traverse(EditableTreeNode? root)
    {
        if (root is null)
            yield break;

        var stack = new Stack<EditableTreeNode>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            yield return node;

            if (node.Right is not null)
                stack.Push(node.Right);

            if (node.Left is not null)
                stack.Push(node.Left);
        }
    }

    public EditableTreeNode? FindById(EditableTreeNode? root, int id)
        => Traverse(root).FirstOrDefault(x => x.Id == id);
}
