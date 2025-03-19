using CommunityToolkit.Mvvm.ComponentModel;
using EPP.Models;

namespace EPP.ViewModels
{
    public partial class EditorViewModel : ViewModelBase
    {
        [ObservableProperty]
        private EventFile _eventFile;

        [ObservableProperty]
        private ModEvent _selectedEvent;

        public EditorViewModel() { }

        public EditorViewModel(EventFile eventFile)
        {
            EventFile = eventFile;
            SelectedEvent = eventFile.Events[0];
        }
    }
}
