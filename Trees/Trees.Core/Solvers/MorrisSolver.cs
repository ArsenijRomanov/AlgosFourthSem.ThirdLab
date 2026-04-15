using System.Numerics;
using Trees.Core.Tree;

namespace Trees.Core.Solvers;

public class MorrisSolver : ISolver
{
    public BigInteger Solve(Node root)
    {
        ArgumentNullException.ThrowIfNull(root);

        var sum = BigInteger.Zero;
        var currentNumber = BigInteger.Zero;
        var current = root;

        while (current is not null)
        {
            if (!HasLeftChild(current))
            {
                if (IsRealLeaf(current))
                {
                    sum += currentNumber;
                    current = null;
                    continue;
                }

                if (HasRealRightChild(current))
                    currentNumber = AppendDigit(currentNumber, GetRightValue(current));

                current = current.Right;
                continue;
            }

            var predecessor = FindPredecessor(current, out var steps);

            if (HasNoRightLink(predecessor))
            {
                SetRightThread(predecessor, current);
                currentNumber = AppendDigit(currentNumber, GetLeftValue(current));
                current = current.Left;
                continue;
            }

            if (IsLeafIgnoringThread(predecessor))
                sum += currentNumber;

            currentNumber = RemoveLastDigits(currentNumber, steps);
            ClearRightThread(predecessor, current);

            if (HasRealRightChild(current))
                currentNumber = AppendDigit(currentNumber, GetRightValue(current));

            current = current.Right;
        }

        return sum;
    }

    private static Node FindPredecessor(Node current, out int steps)
    {
        var predecessor = current.Left
            ?? throw new InvalidOperationException("Current node does not have a left child.");

        steps = 1;

        while (HasRealRightChild(predecessor))
        {
            predecessor = predecessor.Right
                ?? throw new InvalidOperationException("Right child is missing.");

            steps++;
        }

        return predecessor;
    }

    private static BigInteger AppendDigit(BigInteger number, byte digit)
        => number * 10 + digit;

    private static BigInteger RemoveLastDigits(BigInteger number, int count)
    {
        for (var i = 0; i < count; i++)
            number /= 10;

        return number;
    }

    private static bool HasLeftChild(Node node)
        => node.Left is not null;

    private static bool HasNoRightLink(Node node)
        => node.Right is null;

    private static bool HasRealRightChild(Node node)
        => node.Right is not null && node.RightValue is not null;

    private static bool HasRightThread(Node node)
        => node.Right is not null && node.RightValue is null;

    private static bool HasRightThreadTo(Node node, Node target)
        => node.Right == target && node.RightValue is null;

    private static bool IsRealLeaf(Node node)
        => node.Left is null && node.Right is null;

    private static bool IsLeafIgnoringThread(Node node)
        => node.Left is null && !HasRealRightChild(node);

    private static byte GetLeftValue(Node node)
        => node.LeftValue ?? throw new InvalidOperationException("Left edge value is missing.");

    private static byte GetRightValue(Node node)
        => node.RightValue ?? throw new InvalidOperationException("Right edge value is missing.");

    private static void SetRightThread(Node node, Node target)
    {
        if (!HasNoRightLink(node))
            throw new InvalidOperationException("Cannot create thread because right link is already occupied.");

        node.Right = target;
    }

    private static void ClearRightThread(Node node, Node target)
    {
        if (!HasRightThreadTo(node, target))
            throw new InvalidOperationException("Cannot clear thread because the expected thread was not found.");

        node.Right = null;
    }
}
