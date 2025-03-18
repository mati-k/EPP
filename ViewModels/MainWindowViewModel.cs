using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EPP.Models;
using EPP.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace EPP.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartCommand))]
        private string _eventPath = string.Empty;

        [ObservableProperty]
        private string _localizationPath = string.Empty;

        [ObservableProperty]
        private bool _useBackups = true;

        [ObservableProperty]
        private ObservableCollection<string> _sourceDirectories = new();

        [RelayCommand(CanExecute = nameof(CanStart))]
        public async void Start()
        {
            await ConfigPersistanceService.SaveToFileAsync(new ConfigData
            {
                EventPath = EventPath,
                LocalizationPath = LocalizationPath,
                SourceDirectories = SourceDirectories.ToList(),
                UseBackups = UseBackups
            });
        }

        public bool CanStart()
        {
            return !string.IsNullOrEmpty(EventPath) && SourceDirectories.Count > 0;
        }

        public void SetupInitialValues(ConfigData config)
        {
            EventPath = config.EventPath;
            LocalizationPath = config.LocalizationPath;
            UseBackups = config.UseBackups;
            SourceDirectories = new ObservableCollection<string>(config.SourceDirectories);

            StartCommand.NotifyCanExecuteChanged();
        }
    }
}
