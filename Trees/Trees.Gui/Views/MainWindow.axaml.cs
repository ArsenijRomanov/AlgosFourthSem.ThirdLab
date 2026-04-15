using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Trees.Gui.ViewModels;

namespace Trees.Gui.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;

    private async void ImportJsonClicked(object? sender, RoutedEventArgs e)
    {
        if (StorageProvider is null)
            return;

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите JSON-файл дерева",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("JSON")
                {
                    Patterns = ["*.json"]
                },
                FilePickerFileTypes.All
            ]
        });

        var file = files.FirstOrDefault();
        if (file is null)
            return;

        try
        {
            await using var stream = await file.OpenReadAsync();
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            ViewModel.ImportJson(json);
        }
        catch (Exception ex)
        {
            ViewModel.StatusMessage = $"Ошибка импорта: {ex.Message}";
        }
    }

    private async void ExportJsonClicked(object? sender, RoutedEventArgs e)
    {
        if (StorageProvider is null)
            return;

        try
        {
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Сохранить дерево в JSON",
                SuggestedFileName = "tree.json",
                FileTypeChoices =
                [
                    new FilePickerFileType("JSON")
                    {
                        Patterns = ["*.json"]
                    }
                ]
            });

            if (file is null)
                return;

            var json = ViewModel.ExportJson();
            await using var stream = await file.OpenWriteAsync();
            stream.SetLength(0);
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(json);
            await writer.FlushAsync();
            ViewModel.StatusMessage = "JSON успешно экспортирован.";
        }
        catch (Exception ex)
        {
            ViewModel.StatusMessage = $"Ошибка экспорта: {ex.Message}";
        }
    }
}
