using System.Numerics;
using Trees.Core.Tree;

namespace Trees.Core.Solvers;

public interface ISolver
{
    BigInteger Solve(Node root);
}
