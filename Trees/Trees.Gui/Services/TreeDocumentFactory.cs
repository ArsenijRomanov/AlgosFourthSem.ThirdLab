using System;
using Trees.Core;
using Trees.Core.Tree;
using Trees.Gui.Models;

namespace Trees.Gui.Services;

public sealed class TreeDocumentFactory
{
    public TreeDocument CreateEmpty()
        => new();

    public TreeDocument CreateWithSingleRoot()
    {
        var document = new TreeDocument();
        document.Root = document.CreateNode();
        return document;
    }

    public TreeDocument CreateGenerated(TreeShapeOption shape, int size, int seed)
    {
        var root = shape switch
        {
            TreeShapeOption.Complete => TreeFactory.CreateCompleteTree(size, seed),
            TreeShapeOption.DegenerateLeft => TreeFactory.CreateDegenerateTree(size, seed, right: false),
            TreeShapeOption.DegenerateRight => TreeFactory.CreateDegenerateTree(size, seed, right: true),
            TreeShapeOption.Random => TreeFactory.CreateRandomTree(size, seed),
            _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, null)
        };

        return TreeMapper.FromCore(root);
    }
}
