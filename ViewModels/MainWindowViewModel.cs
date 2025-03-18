using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace EPP.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartCommand))]
        private string _eventFile = string.Empty;

        [ObservableProperty]
        private string _localizationFile = string.Empty;

        [ObservableProperty]
        private bool _useBackups = true;

        [ObservableProperty]
        private ObservableCollection<string> _sourceDirectories = new();

        [RelayCommand(CanExecute = nameof(CanStart))]
        public void Start()
        {
        }

        public bool CanStart()
        {
            return !string.IsNullOrEmpty(EventFile) && SourceDirectories.Count > 0;
        }
    }
}
