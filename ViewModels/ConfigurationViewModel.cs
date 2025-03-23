using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using EPP.Models;
using EPP.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace EPP.ViewModels
{
    public partial class ConfigurationViewModel : ViewModelBase
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

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(MoveUpCommand))]
        [NotifyCanExecuteChangedFor(nameof(MoveDownCommand))]
        [NotifyCanExecuteChangedFor(nameof(RemoveDirectoryCommand))]
        private int _selectedDirectoryIndex = 0;

        private Action<ConfigData>? _onContinue;

        public ConfigurationViewModel()
        {
            if (Design.IsDesignMode)
            {
                return;
            }

            var configService = Ioc.Default.GetService<IConfigService>();
            if (configService is not null && configService.ConfigData is not null)
            {
                SetupInitialValues(configService.ConfigData);
            }
        }

        public ConfigurationViewModel(Action<ConfigData> onContinue) : this()
        {
            _onContinue = onContinue;
        }

        [RelayCommand(CanExecute = nameof(CanStart))]
        public async Task Start()
        {
            var newConfig = new ConfigData
            {
                EventPath = EventPath,
                LocalizationPath = LocalizationPath,
                SourceDirectories = SourceDirectories.ToList(),
                UseBackups = UseBackups
            };

            var configService = Ioc.Default.GetService<IConfigService>()!;
            await configService.SaveConfig(newConfig);

            if (_onContinue is not null)
            {
                _onContinue(newConfig);
            }
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

        [RelayCommand(CanExecute = nameof(CanMoveUp))]
        public void MoveUp()
        {
            if (SelectedDirectoryIndex > 0)
            {
                // SelectedDirectoryIndex might get automaticaly changed to -1 during list changing
                int index = SelectedDirectoryIndex;

                string temp = SourceDirectories[index - 1];
                SourceDirectories[index - 1] = SourceDirectories[index];
                SourceDirectories[index] = temp;

                SelectedDirectoryIndex = index - 1;
            }
        }

        public bool CanMoveUp()
        {
            return SelectedDirectoryIndex > 0;
        }

        [RelayCommand(CanExecute = nameof(CanMoveDown))]
        public void MoveDown()
        {
            if (CanMoveDown())
            {
                // SelectedDirectoryIndex might get automaticaly changed to -1 during list changing
                int index = SelectedDirectoryIndex;

                string temp = SourceDirectories[index + 1];
                SourceDirectories[index + 1] = SourceDirectories[index];
                SourceDirectories[index] = temp;

                SelectedDirectoryIndex = index + 1;
            }
        }

        public bool CanMoveDown()
        {
            return SelectedDirectoryIndex > -1 && SelectedDirectoryIndex < SourceDirectories.Count - 1;
        }

        [RelayCommand]
        public void RemoveDirectory()
        {
            if (SelectedDirectoryIndex > -1)
            {
                SourceDirectories.RemoveAt(SelectedDirectoryIndex);
            }
        }

        public bool CanRemoveDirectory()
        {
            return SelectedDirectoryIndex > -1;
        }
    }
}
