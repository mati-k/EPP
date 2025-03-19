using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using EPP.Models;
using EPP.Services;
using Pdoxcl2Sharp;
using System;
using System.IO;
using System.Text;

namespace EPP.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase _currentPage;

        public MainWindowViewModel()
        {
            CurrentPage = new ConfigurationViewModel();
        }

        public MainWindowViewModel(ConfigData config)
        {
            CurrentPage = new ConfigurationViewModel(config, MoveToEditor);
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
                    CurrentPage = new EditorViewModel(eventFile);
                }
            }
            catch (Exception e)
            {

            }
        }
    }
}