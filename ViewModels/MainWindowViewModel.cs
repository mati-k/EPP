using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using EPP.Helpers;
using EPP.Models;
using EPP.Services;
using Pdoxcl2Sharp;
using Serilog;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EPP.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase _currentPage;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        [NotifyCanExecuteChangedFor(nameof(ResetAllCommand))]
        private EventFile _eventFile = new();

        public MainWindowViewModel()
        {
            CurrentPage = new ConfigurationViewModel(MoveToEditor);
        }

        private async void MoveToEditor(ConfigData config)
        {
            var gfxService = Ioc.Default.GetService<IGfxService>()!;

            // Load in reverse order, later directories override earlier ones
            for (int i = config.SourceDirectories.Count - 1; i >= 0; i--)
            {
                await gfxService.LoadSourceDirectory(config.SourceDirectories[i]);
            }
            gfxService.GeneratePaths();

            try
            {
                string fileText = File.ReadAllText(config.EventPath);
                using (Stream fileStream = new MemoryStream(Encoding.UTF8.GetBytes(fileText ?? "")))
                {
                    EventFile = ParadoxParser.Parse(fileStream, new EventFile());
                }

                if (!string.IsNullOrEmpty(config.LocalizationPath))
                {
                    Localization localization = new();
                    await localization.LoadFromFileAsync(config.LocalizationPath);
                    EventFile.BindLocalization(localization);
                }

                CurrentPage = new EditorViewModel(EventFile);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error occured during reading event file");
                await DialogHost.Show(new InfoDialogData("Error occured during reading event file, see logs folder for more information"), "MainDialogHost");
            }
        }

        [RelayCommand(CanExecute = nameof(IsAnyEventChanged))]
        public async Task Save()
        {
            var fileService = Ioc.Default.GetService<IFileService>()!;
            var configService = Ioc.Default.GetService<IConfigService>()!;

            if (configService is not null && fileService is not null && configService.ConfigData.UseBackups)
            {
                fileService.CreateFileBackup(configService.ConfigData.EventPath);
            }

            await EventSavingHelper.SaveEvent(EventFile!);
            EventFile.UpdateIsAnyChanged();
        }

        [RelayCommand(CanExecute = nameof(IsAnyEventChanged))]
        public void ResetAll()
        {
            foreach (var modEvent in EventFile.Events)
            {
                modEvent.ResetIcon();
            }
        }

        public bool IsAnyEventChanged()
        {
            return EventFile.IsAnyEventChanged;
        }
    }
}