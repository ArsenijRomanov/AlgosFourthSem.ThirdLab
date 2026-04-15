using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Trees.Gui.Infrastructure;
using Trees.Gui.Models;
using Trees.Gui.Services;

namespace Trees.Gui.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly TreeDocumentFactory _documentFactory = new();
    private readonly TreeEditorService _editorService = new();
    private readonly JsonTreeSerializer _jsonSerializer = new();
    private readonly AlgorithmExecutionService _algorithmExecutionService = new();

    private TreeDocument _document;
    private int? _selectedNodeId;
    private AlgorithmOption _selectedAlgorithm = AlgorithmOption.RecursiveDfs;
    private TreeShapeOption _selectedShape = TreeShapeOption.Random;
    private int _generatedSize = 100;
    private int _seed = 1;
    private bool _runFromSelectedNode = true;
    private int? _selectedLeftDigit;
    private int? _selectedRightDigit;
    private int _treeRevision;
    private string _statusMessage = "Готово к работе.";

    public MainWindowViewModel()
    {
        _document = _documentFactory.CreateEmpty();

        AvailableAlgorithms = Enum.GetValues<AlgorithmOption>();
        AvailableShapes = Enum.GetValues<TreeShapeOption>();
        Digits = Enumerable.Range(0, 10).ToArray();
        Results = new ObservableCollection<RunMeasurement>();

        CreateRootCommand = new RelayCommand(CreateRoot);
        GenerateCommand = new RelayCommand(GenerateTree);
        AddLeftCommand = new RelayCommand(AddLeftChild, () => SelectedNode is not null);
        AddRightCommand = new RelayCommand(AddRightChild, () => SelectedNode is not null);
        DeleteNodeCommand = new RelayCommand(DeleteSelectedNode, () => SelectedNode is not null);
        RunSelectedAlgorithmCommand = new RelayCommand(RunSelectedAlgorithm, () => ActiveRunRoot is not null);
        RunAllAlgorithmsCommand = new RelayCommand(RunAllAlgorithms, () => ActiveRunRoot is not null);
        ClearResultsCommand = new RelayCommand(ClearResults, () => Results.Count > 0);
    }

    public Array AvailableAlgorithms { get; }
    public Array AvailableShapes { get; }
    public IReadOnlyList<int> Digits { get; }

    public ObservableCollection<RunMeasurement> Results { get; }

    public RelayCommand CreateRootCommand { get; }
    public RelayCommand GenerateCommand { get; }
    public RelayCommand AddLeftCommand { get; }
    public RelayCommand AddRightCommand { get; }
    public RelayCommand DeleteNodeCommand { get; }
    public RelayCommand RunSelectedAlgorithmCommand { get; }
    public RelayCommand RunAllAlgorithmsCommand { get; }
    public RelayCommand ClearResultsCommand { get; }

    public EditableTreeNode? Root => _document.Root;

    public int TreeRevision
    {
        get => _treeRevision;
        private set => SetProperty(ref _treeRevision, value);
    }

    public int? SelectedNodeId
    {
        get => _selectedNodeId;
        set
        {
            if (!SetProperty(ref _selectedNodeId, value))
                return;

            RefreshSelectionState();
        }
    }

    public EditableTreeNode? SelectedNode => Root is null || SelectedNodeId is null
        ? null
        : _editorService.FindById(Root, SelectedNodeId.Value);

    public EditableTreeNode? ActiveRunRoot => RunFromSelectedNode ? SelectedNode ?? Root : Root;

    public AlgorithmOption SelectedAlgorithm
    {
        get => _selectedAlgorithm;
        set => SetProperty(ref _selectedAlgorithm, value);
    }

    public TreeShapeOption SelectedShape
    {
        get => _selectedShape;
        set => SetProperty(ref _selectedShape, value);
    }

    public int GeneratedSize
    {
        get => _generatedSize;
        set => SetProperty(ref _generatedSize, value);
    }

    public int Seed
    {
        get => _seed;
        set => SetProperty(ref _seed, value);
    }

    public bool RunFromSelectedNode
    {
        get => _runFromSelectedNode;
        set
        {
            if (!SetProperty(ref _runFromSelectedNode, value))
                return;

            OnPropertyChanged(nameof(ActiveRunRoot));
            OnPropertyChanged(nameof(ActiveScopeTitle));
            RaiseCommandStates();
        }
    }

    public int? SelectedLeftDigit
    {
        get => _selectedLeftDigit;
        set
        {
            if (!SetProperty(ref _selectedLeftDigit, value))
                return;

            if (SelectedNode?.Left is not null && value is not null)
            {
                SelectedNode.LeftValue = (byte)value.Value;
                TouchTree("Изменено значение левого ребра.");
            }
        }
    }

    public int? SelectedRightDigit
    {
        get => _selectedRightDigit;
        set
        {
            if (!SetProperty(ref _selectedRightDigit, value))
                return;

            if (SelectedNode?.Right is not null && value is not null)
            {
                SelectedNode.RightValue = (byte)value.Value;
                TouchTree("Изменено значение правого ребра.");
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string SelectedNodeTitle => SelectedNode is null ? "Вершина не выбрана" : $"Вершина #{SelectedNode.Id}";

    public bool HasTree => Root is not null;

    public bool HasSelection => SelectedNode is not null;

    public bool HasLeftChild => SelectedNode?.Left is not null;

    public bool HasRightChild => SelectedNode?.Right is not null;

    public string ActiveScopeTitle
    {
        get
        {
            if (ActiveRunRoot is null)
                return "—";

            if (RunFromSelectedNode && SelectedNode is not null)
                return $"Поддерево от вершины #{SelectedNode.Id}";

            return "Все дерево";
        }
    }

    public string TreeSummary
    {
        get
        {
            if (Root is null)
                return "Дерево пустое";

            var nodes = _editorService.Traverse(Root).ToList();
            var leafCount = nodes.Count(x => x.IsLeaf);
            var height = GetHeight(Root);
            return $"Вершин: {nodes.Count} · Листьев: {leafCount} · Высота: {height}";
        }
    }

    public void ImportJson(string json)
    {
        _document = _jsonSerializer.Deserialize(json);
        SelectedNodeId = _document.Root?.Id;
        Results.Clear();
        RefreshWholeState();
        StatusMessage = "JSON успешно загружен.";
        RaiseCommandStates();
    }

    public string ExportJson() => _jsonSerializer.Serialize(_document);

    private void CreateRoot()
    {
        var root = _editorService.EnsureRoot(_document);
        SelectedNodeId = root.Id;
        Results.Clear();
        TouchTree("Создан корень дерева.");
    }

    private void GenerateTree()
    {
        GeneratedSize = Math.Clamp(GeneratedSize, 1, 1000);
        _document = _documentFactory.CreateGenerated(SelectedShape, GeneratedSize, Seed);
        SelectedNodeId = _document.Root?.Id;
        Results.Clear();
        RefreshWholeState();
        StatusMessage = $"Сгенерировано дерево: {SelectedShape}, размер {GeneratedSize}, seed {Seed}.";
        RaiseCommandStates();
    }

    private void AddLeftChild()
    {
        if (SelectedNode is null)
            return;

        var child = _editorService.AddLeftChild(_document, SelectedNode, SelectedNode.LeftValue ?? 0);
        SelectedNodeId = child.Id;
        TouchTree($"Добавлена левая вершина к #{child.Parent!.Id}.");
    }

    private void AddRightChild()
    {
        if (SelectedNode is null)
            return;

        var child = _editorService.AddRightChild(_document, SelectedNode, SelectedNode.RightValue ?? 0);
        SelectedNodeId = child.Id;
        TouchTree($"Добавлена правая вершина к #{child.Parent!.Id}.");
    }

    private void DeleteSelectedNode()
    {
        var node = SelectedNode;
        if (node is null)
            return;

        var nextSelection = node.Parent?.Id;
        _editorService.DeleteSubtree(_document, node);
        SelectedNodeId = nextSelection ?? _document.Root?.Id;
        Results.Clear();
        TouchTree(node.Parent is null ? "Дерево очищено." : $"Удалено поддерево вершины #{node.Id}.");
    }

    private void RunSelectedAlgorithm()
    {
        var root = ActiveRunRoot;
        if (root is null)
            return;

        var measurement = _algorithmExecutionService.Execute(SelectedAlgorithm, root, ActiveScopeTitle);
        Results.Insert(0, measurement);
        StatusMessage = $"Выполнен {measurement.AlgorithmName}.";
        RaiseCommandStates();
    }

    private void RunAllAlgorithms()
    {
        var root = ActiveRunRoot;
        if (root is null)
            return;

        var batch = _algorithmExecutionService.ExecuteAll(root, ActiveScopeTitle);
        Results.Clear();
        foreach (var item in batch)
            Results.Add(item);

        StatusMessage = $"Сравнение завершено: {batch.Count} алгоритма(ов).";
        RaiseCommandStates();
    }

    private void ClearResults()
    {
        Results.Clear();
        StatusMessage = "История запусков очищена.";
        RaiseCommandStates();
    }

    private void RefreshSelectionState()
    {
        OnPropertyChanged(nameof(SelectedNode));
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(SelectedNodeTitle));
        OnPropertyChanged(nameof(HasLeftChild));
        OnPropertyChanged(nameof(HasRightChild));
        OnPropertyChanged(nameof(ActiveRunRoot));
        OnPropertyChanged(nameof(ActiveScopeTitle));

        var selected = SelectedNode;
        _selectedLeftDigit = selected?.LeftValue;
        _selectedRightDigit = selected?.RightValue;
        OnPropertyChanged(nameof(SelectedLeftDigit));
        OnPropertyChanged(nameof(SelectedRightDigit));

        RaiseCommandStates();
    }

    private void RefreshWholeState()
    {
        OnPropertyChanged(nameof(Root));
        OnPropertyChanged(nameof(HasTree));
        OnPropertyChanged(nameof(TreeSummary));
        RefreshSelectionState();
        TreeRevision++;
    }

    private void TouchTree(string message)
    {
        OnPropertyChanged(nameof(Root));
        OnPropertyChanged(nameof(TreeSummary));
        RefreshSelectionState();
        TreeRevision++;
        StatusMessage = message;
    }

    private void RaiseCommandStates()
    {
        AddLeftCommand.RaiseCanExecuteChanged();
        AddRightCommand.RaiseCanExecuteChanged();
        DeleteNodeCommand.RaiseCanExecuteChanged();
        RunSelectedAlgorithmCommand.RaiseCanExecuteChanged();
        RunAllAlgorithmsCommand.RaiseCanExecuteChanged();
        ClearResultsCommand.RaiseCanExecuteChanged();
    }

    private static int GetHeight(EditableTreeNode? node)
    {
        if (node is null)
            return 0;

        return 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));
    }
}
