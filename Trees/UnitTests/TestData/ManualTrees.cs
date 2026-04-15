using System.Numerics;
using Trees.Core.Tree;

namespace UnitTests.TestData;

public static class ManualTrees
{
    public const string SingleNodeName = "Single node";
    public static readonly BigInteger SingleNodeSum = 0;

    public static Node CreateSingleNode()
        => new Node();

    public const string OneLeftEdgeName = "One left edge";
    public static readonly BigInteger OneLeftEdgeSum = 5;

    public static Node CreateOneLeftEdge()
    {
        var leaf = new Node();
        var root = new Node(left: leaf, leftValue: 5);
        return root;
    }

    public const string OneRightEdgeName = "One right edge";
    public static readonly BigInteger OneRightEdgeSum = 7;

    public static Node CreateOneRightEdge()
    {
        var leaf = new Node();
        var root = new Node(right: leaf, rightValue: 7);
        return root;
    }

    public const string TwoLeavesName = "Two leaves";
    public static readonly BigInteger TwoLeavesSum = 3;

    public static Node CreateTwoLeaves()
    {
        var leftLeaf = new Node();
        var rightLeaf = new Node();

        var root = new Node(
            left: leftLeaf, leftValue: 1,
            right: rightLeaf, rightValue: 2);

        return root;
    }

    public const string MixedSmallTreeName = "Mixed small tree";
    public static readonly BigInteger MixedSmallTreeSum = 48; // 1 + 23 + 24

    //      *
    //    /   \ 
    //   1     2
    //  /     / \
    // *     3   4
    //      /     \
    //     *       *
    public static Node CreateMixedSmallTree()
    {
        var leftLeaf = new Node();
        var rightLeftLeaf = new Node();
        var rightRightLeaf = new Node();

        var rightSubtree = new Node(
            left: rightLeftLeaf, leftValue: 3,
            right: rightRightLeaf, rightValue: 4);

        var root = new Node(
            left: leftLeaf, leftValue: 1,
            right: rightSubtree, rightValue: 2);

        return root;
    }

    public const string LeftChainName = "Left chain";
    public static readonly BigInteger LeftChainSum = 123;

    public static Node CreateLeftChain()
    {
        var leaf = new Node();
        var second = new Node(left: leaf, leftValue: 3);
        var first = new Node(left: second, leftValue: 2);
        var root = new Node(left: first, leftValue: 1);

        return root;
    }

    public const string RightChainName = "Right chain";
    public static readonly BigInteger RightChainSum = 987;

    public static Node CreateRightChain()
    {
        var leaf = new Node();
        var second = new Node(right: leaf, rightValue: 7);
        var first = new Node(right: second, rightValue: 8);
        var root = new Node(right: first, rightValue: 9);

        return root;
    }

    public const string AsymmetricTreeName = "Asymmetric tree";
    public static readonly BigInteger AsymmetricTreeSum = new(381);

    //               *
    //             /   \
    //            1     2
    //           / \   /
    //          5   2 1
    //         /       \
    //        6         3
    //       /           \
    //      *             *

    public static Node CreateAsymmetricTree()
    {
        var path156Leaf = new Node();
        var path12Leaf = new Node();
        var path213Leaf = new Node();

        var leftDeep = new Node(
            left: path156Leaf, leftValue: 6);

        var leftSubtree = new Node(
            left: leftDeep, leftValue: 5,
            right: path12Leaf, rightValue: 2);

        var rightSubtree = new Node(
            left: new Node(
                right: path213Leaf, rightValue: 3),
            leftValue: 1);

        var root = new Node(
            left: leftSubtree, leftValue: 1,
            right: rightSubtree, rightValue: 2);

        return root;
    }
}
