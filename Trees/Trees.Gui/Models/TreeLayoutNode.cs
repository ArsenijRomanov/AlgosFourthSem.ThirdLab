using Avalonia;

namespace Trees.Gui.Models;

public sealed class TreeLayoutNode
{
    public required EditableTreeNode Node { get; init; }
    public required Point Position { get; init; }
    public int Depth { get; init; }
}
