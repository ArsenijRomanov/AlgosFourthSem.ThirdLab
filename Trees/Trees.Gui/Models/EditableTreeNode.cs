namespace Trees.Gui.Models;

public sealed class EditableTreeNode
{
    public int Id { get; init; }

    public EditableTreeNode? Parent { get; set; }

    public EditableTreeNode? Left { get; set; }
    public byte? LeftValue { get; set; }

    public EditableTreeNode? Right { get; set; }
    public byte? RightValue { get; set; }

    public bool IsLeaf => Left is null && Right is null;
}
