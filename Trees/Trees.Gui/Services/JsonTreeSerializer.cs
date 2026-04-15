using System;
using System.Text.Json;
using Trees.Gui.Models;

namespace Trees.Gui.Services;

public sealed class JsonTreeSerializer
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string Serialize(TreeDocument document)
    {
        if (document.Root is null)
            return "null";

        var dto = ToDto(document.Root);
        return JsonSerializer.Serialize(dto, _options);
    }

    public TreeDocument Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json.Trim() == "null")
            return new TreeDocument();

        var dto = JsonSerializer.Deserialize<TreeJsonDto>(json, _options)
            ?? throw new InvalidOperationException("Не удалось разобрать JSON дерева.");

        var document = new TreeDocument();
        document.Root = FromDto(dto, null);
        document.ResetCounterFromTree(document.Root);
        return document;
    }

    private static TreeJsonDto ToDto(EditableTreeNode node)
        => new()
        {
            Id = node.Id,
            LeftValue = node.LeftValue,
            Left = node.Left is null ? null : ToDto(node.Left),
            RightValue = node.RightValue,
            Right = node.Right is null ? null : ToDto(node.Right)
        };

    private static EditableTreeNode FromDto(TreeJsonDto dto, EditableTreeNode? parent)
    {
        if (dto.Left is not null && (dto.LeftValue is null || dto.LeftValue < 0 || dto.LeftValue > 9))
            throw new InvalidOperationException($"У вершины {dto.Id} левое ребро должно иметь цифру от 0 до 9.");

        if (dto.Right is not null && (dto.RightValue is null || dto.RightValue < 0 || dto.RightValue > 9))
            throw new InvalidOperationException($"У вершины {dto.Id} правое ребро должно иметь цифру от 0 до 9.");

        var node = new EditableTreeNode
        {
            Id = dto.Id,
            Parent = parent,
            LeftValue = dto.Left is null ? null : (byte?)dto.LeftValue,
            RightValue = dto.Right is null ? null : (byte?)dto.RightValue
        };

        if (dto.Left is not null)
            node.Left = FromDto(dto.Left, node);

        if (dto.Right is not null)
            node.Right = FromDto(dto.Right, node);

        return node;
    }
}
