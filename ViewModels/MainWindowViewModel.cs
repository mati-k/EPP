﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using EPP.Helpers;
using EPP.Models;
using EPP.Services;
using Pdoxcl2Sharp;
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

        private EventFile _eventFile;

        public MainWindowViewModel()
        {
            CurrentPage = new ConfigurationViewModel(MoveToEditor);
        }

        private async void MoveToEditor(ConfigData config)
        {
            var gfxService = Ioc.Default.GetService<IGfxService>();

            // Load in reverse order, later directories override earlier ones
            for (int i = config.SourceDirectories.Count - 1; i >= 0; i--)
            {
                await gfxService.LoadSourceDirectory(config.SourceDirectories[i]);
            }

            try
            {
                string fileText = File.ReadAllText(config.EventPath);
                using (Stream fileStream = new MemoryStream(Encoding.UTF8.GetBytes(fileText ?? "")))
                {
                    EventFile eventFile = ParadoxParser.Parse(fileStream, new EventFile());
                    _eventFile = eventFile;
                    CurrentPage = new EditorViewModel(eventFile);
                }
            }
            catch (Exception e)
            {

            }
        }

        [RelayCommand]
        public async Task Save()
        {
            var fileService = Ioc.Default.GetService<IFileService>()!;
            var configService = Ioc.Default.GetService<IConfigService>()!;

            if (configService is not null && fileService is not null && configService.ConfigData.UseBackups)
            {
                fileService.CreateFileBackup(configService.ConfigData.EventPath);
            }

            await EventSavingHelper.SaveEvent(_eventFile);
        }

    }
}