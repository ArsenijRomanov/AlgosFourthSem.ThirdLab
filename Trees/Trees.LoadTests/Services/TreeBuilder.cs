using Trees.Core;
using Trees.Core.Tree;
using Trees.LoadTests.Models;

namespace Trees.LoadTests.Services;

public static class TreeBuilder
{
    public static Node CreateTree(int size, TreeShapeKind shape, int seed)
        => shape switch
        {
            TreeShapeKind.Complete => TreeFactory.CreateCompleteTree(size, seed),
            TreeShapeKind.DegenerateLeft => TreeFactory.CreateDegenerateTree(size, seed, right: false),
            TreeShapeKind.DegenerateRight => TreeFactory.CreateDegenerateTree(size, seed, right: true),
            TreeShapeKind.Random => TreeFactory.CreateRandomTree(size, seed),
            _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, null)
        };
}
