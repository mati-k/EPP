using CommunityToolkit.Mvvm.ComponentModel;
using EPP.Models;
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

        private void MoveToEditor(ConfigData config)
        {
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
