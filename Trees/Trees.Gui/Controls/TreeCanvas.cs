using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Trees.Gui.Models;
using Trees.Gui.Services;

namespace Trees.Gui.Controls;

public sealed class TreeCanvas : Control
{
    public static readonly StyledProperty<EditableTreeNode?> RootProperty =
        AvaloniaProperty.Register<TreeCanvas, EditableTreeNode?>(nameof(Root));

    public static readonly StyledProperty<int?> SelectedNodeIdProperty =
        AvaloniaProperty.Register<TreeCanvas, int?>(nameof(SelectedNodeId), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<int> RevisionProperty =
        AvaloniaProperty.Register<TreeCanvas, int>(nameof(Revision));

    private readonly TreeLayoutService _layoutService = new();
    private readonly List<RenderedNode> _renderedNodes = new();

    private bool _isPanning;
    private Point _lastPanPoint;
    private Vector _pan = default;
    private double _zoom = 1.0;

    public EditableTreeNode? Root
    {
        get => GetValue(RootProperty);
        set => SetValue(RootProperty, value);
    }

    public int? SelectedNodeId
    {
        get => GetValue(SelectedNodeIdProperty);
        set => SetValue(SelectedNodeIdProperty, value);
    }

    public int Revision
    {
        get => GetValue(RevisionProperty);
        set => SetValue(RevisionProperty, value);
    }

    static TreeCanvas()
    {
        AffectsRender<TreeCanvas>(RootProperty, SelectedNodeIdProperty, RevisionProperty);
        FocusableProperty.OverrideDefaultValue<TreeCanvas>(true);
    }

    public TreeCanvas()
    {
        ClipToBounds = true;
        PointerPressed += OnPointerPressed;
        PointerReleased += OnPointerReleased;
        PointerMoved += OnPointerMoved;
        PointerWheelChanged += OnPointerWheelChanged;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        DrawBackdrop(context);

        if (Root is null)
        {
            DrawEmptyState(context);
            return;
        }

        var layout = _layoutService.Layout(Root);
        if (layout.Count == 0)
            return;

        var renderData = BuildRenderData(layout);
        _renderedNodes.Clear();

        foreach (var item in layout)
        {
            if (item.Node.Left is not null)
                DrawEdge(context, item, item.Node.Left, item.Node.LeftValue, renderData);

            if (item.Node.Right is not null)
                DrawEdge(context, item, item.Node.Right, item.Node.RightValue, renderData);
        }

        foreach (var item in layout.OrderBy(x => x.Depth))
            DrawNode(context, item, renderData);
    }

    private void DrawBackdrop(DrawingContext context)
    {
        context.FillRectangle(new SolidColorBrush(Color.Parse("#0D1324")), Bounds);

        var gridPen = new Pen(new SolidColorBrush(Color.Parse("#15203A")), 1);
        const double step = 28;

        for (var x = 0d; x < Bounds.Width; x += step)
            context.DrawLine(gridPen, new Point(x, 0), new Point(x, Bounds.Height));

        for (var y = 0d; y < Bounds.Height; y += step)
            context.DrawLine(gridPen, new Point(0, y), new Point(Bounds.Width, y));
    }

    private void DrawEmptyState(DrawingContext context)
    {
        var text = new TextLayout(
            "",
            new Typeface(new FontFamily("Inter, Segoe UI, Arial"), FontStyle.Normal, FontWeight.Medium),
            18,
            new SolidColorBrush(Color.Parse("#AAB7D4")),
            TextAlignment.Center,
            TextWrapping.Wrap,
            maxWidth: Math.Max(200, Bounds.Width - 80));

        var origin = new Point((Bounds.Width - text.Width) / 2, (Bounds.Height - text.Height) / 2);
        text.Draw(context, origin);
    }

    private RenderData BuildRenderData(IReadOnlyList<TreeLayoutNode> layout)
    {
        const double logicalXSpacing = 160;
        const double logicalYSpacing = 130;
        const double margin = 42;

        var logicalPoints = layout.ToDictionary(
            x => x.Node.Id,
            x => new Point(x.Position.X * logicalXSpacing, x.Position.Y * logicalYSpacing));

        var minX = logicalPoints.Values.Min(x => x.X);
        var maxX = logicalPoints.Values.Max(x => x.X);
        var minY = logicalPoints.Values.Min(x => x.Y);
        var maxY = logicalPoints.Values.Max(x => x.Y);

        var center = new Point((minX + maxX) / 2, (minY + maxY) / 2);
        var logicalWidth = Math.Max(1, maxX - minX);
        var logicalHeight = Math.Max(1, maxY - minY);
        var fitScale = Math.Min((Bounds.Width - margin * 2) / logicalWidth, (Bounds.Height - margin * 2) / logicalHeight);
        fitScale = double.IsFinite(fitScale) ? Math.Clamp(fitScale, 0.2, 2.2) : 1.0;

        var totalScale = fitScale * _zoom;
        var nodeRadius = Math.Clamp(26 * totalScale, 14, 34);
        var edgeFontSize = Math.Clamp(10 * totalScale, 8, 12);
        var edgeBubbleWidth = Math.Clamp(20 * totalScale, 16, 24);
        var edgeBubbleHeight = Math.Clamp(16 * totalScale, 12, 18);
        var edgeOffset = Math.Clamp(12 * totalScale, 8, 16);

        Point ToScreen(Point logical)
        {
            var viewportCenter = new Point(Bounds.Width / 2, Bounds.Height / 2);
            var x = viewportCenter.X + (logical.X - center.X) * totalScale + _pan.X;
            var y = viewportCenter.Y + (logical.Y - center.Y) * totalScale + _pan.Y;
            return new Point(x, y);
        }

        var screenPoints = logicalPoints.ToDictionary(x => x.Key, x => ToScreen(x.Value));

        return new RenderData(screenPoints, nodeRadius, edgeFontSize, edgeBubbleWidth, edgeBubbleHeight, edgeOffset);
    }

    private void DrawEdge(DrawingContext context, TreeLayoutNode parent, EditableTreeNode child, byte? value, RenderData data)
    {
        var start = data.ScreenPoints[parent.Node.Id];
        var end = data.ScreenPoints[child.Id];

        var edgePen = new Pen(new SolidColorBrush(Color.Parse("#4E689D")), 1.8);
        context.DrawLine(edgePen, start, end);

        var mid = new Point((start.X + end.X) / 2, (start.Y + end.Y) / 2);
        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        var len = Math.Max(1.0, Math.Sqrt(dx * dx + dy * dy));
        var nx = -dy / len;
        var ny = dx / len;
        var labelCenter = new Point(mid.X + nx * data.EdgeOffset, mid.Y + ny * data.EdgeOffset);

        var bubble = new Rect(
            labelCenter.X - data.EdgeBubbleWidth / 2,
            labelCenter.Y - data.EdgeBubbleHeight / 2,
            data.EdgeBubbleWidth,
            data.EdgeBubbleHeight);

        context.FillRectangle(new SolidColorBrush(Color.Parse("#CC101A31")), bubble, 8);
        context.DrawRectangle(new Pen(new SolidColorBrush(Color.Parse("#5A6C93")), 1), bubble, 8);

        var text = new TextLayout(
            (value ?? 0).ToString(),
            new Typeface(new FontFamily("Inter, Segoe UI, Arial"), FontStyle.Normal, FontWeight.Medium),
            data.EdgeFontSize,
            new SolidColorBrush(Color.Parse("#C8D4F0")),
            TextAlignment.Center,
            TextWrapping.NoWrap);

        text.Draw(context, new Point(labelCenter.X - text.Width / 2, labelCenter.Y - text.Height / 2));
    }

    private void DrawNode(DrawingContext context, TreeLayoutNode item, RenderData data)
    {
        var center = data.ScreenPoints[item.Node.Id];
        var radius = data.NodeRadius;
        var isSelected = item.Node.Id == SelectedNodeId;

        context.DrawEllipse(
            new SolidColorBrush(Color.Parse(isSelected ? "#294AA6" : "#111A30")),
            null,
            new Point(center.X, center.Y + radius * 0.18),
            radius * 0.95,
            radius * 0.95);

        var fill = isSelected
            ? new SolidColorBrush(Color.Parse("#5B87FF"))
            : new SolidColorBrush(Color.Parse("#DCE7FF"));

        var border = isSelected
            ? new Pen(new SolidColorBrush(Color.Parse("#F7FAFF")), 3)
            : new Pen(new SolidColorBrush(Color.Parse("#6B7DA8")), 2);

        context.DrawEllipse(fill, border, center, radius, radius);
        _renderedNodes.Add(new RenderedNode(item.Node.Id, center, radius));
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Focus();
        var point = e.GetPosition(this);

        var hit = _renderedNodes.LastOrDefault(x => Distance(x.Center, point) <= x.Radius + 4);
        if (hit is not null && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            SelectedNodeId = hit.NodeId;
            InvalidateVisual();
            return;
        }

        _isPanning = true;
        _lastPanPoint = point;
        e.Pointer.Capture(this);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isPanning)
            return;

        var point = e.GetPosition(this);
        _pan += point - _lastPanPoint;
        _lastPanPoint = point;
        InvalidateVisual();
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isPanning = false;
        e.Pointer.Capture(null);
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        var delta = e.Delta.Y > 0 ? 1.1 : 0.9;
        _zoom = Math.Clamp(_zoom * delta, 0.25, 4.0);
        InvalidateVisual();
    }

    private static double Distance(Point a, Point b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private sealed record RenderedNode(int NodeId, Point Center, double Radius);

    private sealed record RenderData(
        IReadOnlyDictionary<int, Point> ScreenPoints,
        double NodeRadius,
        double EdgeFontSize,
        double EdgeBubbleWidth,
        double EdgeBubbleHeight,
        double EdgeOffset);
}
