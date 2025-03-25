using Avalonia.Controls;
using Avalonia.Platform.Storage;
using EPP.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EPP.Views;

public partial class ConfigurationView : UserControl
{
    private static FilePickerFileType LocalizationFiles { get; } = new("Localization files") { Patterns = ["*.yml"] };
    private ConfigurationViewModel viewModel { get => (ConfigurationViewModel)DataContext!; }

    public ConfigurationView()
    {
        InitializeComponent();
    }

    private ConfigurationViewModel ViewModel => (ConfigurationViewModel)DataContext!;

    private async void EventFile_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var hints = GetStartHints(viewModel.EventPath, viewModel.LocalizationPath, "events");
        string filePath = await SelectFile("Select Event File", FilePickerFileTypes.TextPlain, hints);

        if (!string.IsNullOrEmpty(filePath))
        {
            ViewModel.EventPath = filePath;
        }
    }

    private async void LocalizationFile_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var hints = GetStartHints(viewModel.LocalizationPath, viewModel.EventPath, "localisation");
        string filePath = await SelectFile("Select Localization File", LocalizationFiles, hints);

        if (!string.IsNullOrEmpty(filePath))
        {
            ViewModel.LocalizationPath = filePath;
        }
    }

    private async void SourceDirectories_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var folders = await topLevel!.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select directory",
            AllowMultiple = false,

        });

        if (folders.Count >= 1)
        {
            ViewModel.SourceDirectories.Add(folders[0].TryGetLocalPath()!);
            ViewModel.StartCommand.NotifyCanExecuteChanged();
        }

    }

    private List<string?> GetStartHints(string currentFile, string complementaryFile, string endFolder)
    {
        List<string?> hints = [];
        if (currentFile != null)
        {
            try
            {
                hints.Add(Path.GetDirectoryName(currentFile));
            }
            catch { }
        }

        if (complementaryFile != null)
        {
            try
            {
                string? parentDirectory = Directory.GetParent(Path.GetDirectoryName(complementaryFile) ?? "")?.FullName;
                if (parentDirectory != null)
                {
                    hints.Add(Path.Combine(parentDirectory, endFolder));
                }
            }
            catch { }
        }

        if (ViewModel.SourceDirectories.Count > 0)
        {
            try
            {
                hints.Add(Path.Combine(viewModel.SourceDirectories.Last(), endFolder));
            }
            catch { };
        }

        return hints;
    }

    private async ValueTask<string> SelectFile(string title, FilePickerFileType filter, List<string?> hints)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        FilePickerOpenOptions options = new()
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = [filter]
        };

        foreach (var hint in hints)
        {
            if (hint != null)
            {
                if (await topLevel!.StorageProvider.TryGetFolderFromPathAsync(hint) is IStorageFolder folder)
                {
                    options.SuggestedStartLocation = folder;
                    break;
                }
            }
        }

        var files = await topLevel!.StorageProvider.OpenFilePickerAsync(options);

        if (files.Count >= 1)
        {
            return files[0].TryGetLocalPath()!;
        }

        return "";
    }
}