using Trees.Core.Tree;

namespace Trees.Core;

public static class TreeFactory
{
    public static Node CreateDegenerateTree(int size, int seed = 1, bool right = true)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);

        var randomSource = new Random(seed);
        var root = new Node();
        var curNode = root;
        
        for (var i = 0; i < size - 1; ++i)
        {
            var newNode = new Node();
            var curValue = GetRandomWeight(randomSource);
            if (right)
            {
                curNode.RightValue = curValue;
                curNode.Right = newNode;
            }
            else
            {
                curNode.LeftValue = curValue;
                curNode.Left = newNode;
            }

            curNode = newNode;
        }

        return root;
    }

    public static Node CreateCompleteTree(int h, int seed = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(h, 1);
        
        var randomSource = new Random(seed);
        var queue = new Queue<Node>();
        
        var root = new Node();
        queue.Enqueue(root);

        for (var level = 1; level < h; ++level)
        {
            var nodesOnCurrentLevel = queue.Count;
            
            for (var i = 0; i < nodesOnCurrentLevel; ++i)
            {
                var curNode = queue.Dequeue();
                var leftNode = new Node();
                var rightNode = new Node();
                
                curNode.Left = leftNode;
                curNode.LeftValue = GetRandomWeight(randomSource);
                curNode.Right = rightNode;
                curNode.RightValue = GetRandomWeight(randomSource);
                
                queue.Enqueue(leftNode);
                queue.Enqueue(rightNode);
            }
        }

        return root;
    }
    
    public static Node CreateRandomTree(int size, int seed = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);

        var randomSource = new Random(seed);
        var root = new Node();

        if (size == 1)
            return root;

        var freeSlots = new List<(Node parent, bool right)>
        {
            (root, false),
            (root, true)
        };

        for (var i = 1; i < size; ++i)
        {
            var slotIndex = randomSource.Next(freeSlots.Count);
            var (parent, right) = freeSlots[slotIndex];
            freeSlots.RemoveAt(slotIndex);

            var child = new Node();
            var value = GetRandomWeight(randomSource);

            if (right)
            {
                parent.Right = child;
                parent.RightValue = value;
            }
            else
            {
                parent.Left = child;
                parent.LeftValue = value;
            }

            freeSlots.Add((child, false));
            freeSlots.Add((child, true));
        }

        return root;
    }

    private static byte GetRandomWeight(Random randomSource)
        => (byte)randomSource.Next(0, 10);
}
