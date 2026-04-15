using Trees.Core.Tree;
using Trees.Gui.Models;

namespace Trees.Gui.Services;

public static class TreeMapper
{
    public static TreeDocument FromCore(Node root)
    {
        var document = new TreeDocument();
        document.Root = FromCoreNode(root, null, document);
        document.ResetCounterFromTree(document.Root);
        return document;
    }

    public static Node ToCoreNode(EditableTreeNode source)
    {
        var node = new Node();

        if (source.Left is not null)
        {
            node.Left = ToCoreNode(source.Left);
            node.LeftValue = source.LeftValue;
        }

        if (source.Right is not null)
        {
            node.Right = ToCoreNode(source.Right);
            node.RightValue = source.RightValue;
        }

        return node;
    }

    private static EditableTreeNode FromCoreNode(Node source, EditableTreeNode? parent, TreeDocument document)
    {
        var node = document.CreateNode();
        node.Parent = parent;

        if (source.Left is not null)
        {
            node.LeftValue = source.LeftValue;
            node.Left = FromCoreNode(source.Left, node, document);
        }

        if (source.Right is not null)
        {
            node.RightValue = source.RightValue;
            node.Right = FromCoreNode(source.Right, node, document);
        }

        return node;
    }
}
