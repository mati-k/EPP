using CommunityToolkit.Mvvm.ComponentModel;
using EPP.Models;

namespace EPP.ViewModels
{
    public partial class EditorViewModel : ViewModelBase
    {
        [ObservableProperty]
        private EventFile _eventFile;

        public EditorViewModel() { }

        public EditorViewModel(EventFile eventFile)
        {
            EventFile = eventFile;
        }
    }
}
