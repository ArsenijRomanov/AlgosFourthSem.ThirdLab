namespace Trees.Gui.Models;

public sealed class TreeJsonDto
{
    public int Id { get; set; }
    public int? LeftValue { get; set; }
    public TreeJsonDto? Left { get; set; }
    public int? RightValue { get; set; }
    public TreeJsonDto? Right { get; set; }
}
