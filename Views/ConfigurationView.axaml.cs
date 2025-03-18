using Avalonia.Controls;
using Avalonia.Platform.Storage;
using EPP.ViewModels;
using System.Threading.Tasks;

namespace EPP.Views;

public partial class ConfigurationView : UserControl
{
    private static FilePickerFileType LocalizationFiles { get; } = new("Localization files") { Patterns = new[] { "*.yml" } };

    public ConfigurationView()
    {
        InitializeComponent();
    }

    private ConfigurationViewModel ViewModel => (ConfigurationViewModel)DataContext;

    private async void EventFile_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        string filePath = await SelectFile("Select Event File", FilePickerFileTypes.TextPlain);

        if (!string.IsNullOrEmpty(filePath))
        {
            ViewModel.EventPath = filePath;
        }
    }

    private async void LocalizationFile_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        string filePath = await SelectFile("Select Localization File", LocalizationFiles);

        if (!string.IsNullOrEmpty(filePath))
        {
            ViewModel.LocalizationPath = filePath;
        }
    }

    private async void SourceDirectories_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select directory",
            AllowMultiple = false,

        });

        if (folders.Count >= 1)
        {
            ViewModel.SourceDirectories.Add(folders[0].TryGetLocalPath());
            ViewModel.StartCommand.NotifyCanExecuteChanged();
        }

    }

    private async ValueTask<string> SelectFile(string title, FilePickerFileType filter)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = new[] { filter }
        });

        if (files.Count >= 1)
        {
            return files[0].TryGetLocalPath();
        }

        return "";
    }
}