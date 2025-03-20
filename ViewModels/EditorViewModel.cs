using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using EPP.Models;
using EPP.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace EPP.ViewModels
{
    public partial class EditorViewModel : ViewModelBase
    {
        [ObservableProperty]
        private EventFile _eventFile;

        [ObservableProperty]
        private ModEvent _selectedEvent;

        [ObservableProperty]
        [NotifyPropertyChangedFor("ActivePictures")]
        private ObservableCollection<string> _pictures = new();

        [ObservableProperty]
        private string _pictureQuery = "";

        [ObservableProperty]
        private string _selectedPicture;

        public ObservableCollection<string> ActivePictures
        {
            get
            {
                return new ObservableCollection<string>(Pictures.Where(picture => picture.Contains(PictureQuery, System.StringComparison.OrdinalIgnoreCase)));
            }
        }

        partial void OnSelectedEventChanged(ModEvent value)
        {
            SelectedPicture = value.Picture;
        }

        partial void OnSelectedPictureChanged(string value)
        {
            if (value != null)
            {
                SelectedEvent.Picture = value;
            }
        }

        // Manualy update to restore selection if picture reappears
        partial void OnPictureQueryChanged(string value)
        {
            this.OnPropertyChanged(nameof(ActivePictures));
            if (SelectedPicture == null && ActivePictures.Contains(SelectedEvent.Picture))
            {
                SelectedPicture = SelectedEvent.Picture;
            }
        }

        public EditorViewModel() { }

        public EditorViewModel(EventFile eventFile)
        {
            EventFile = eventFile;
            SelectedEvent = eventFile.Events[0];

            var gfxService = Ioc.Default.GetService<IGfxService>();
            if (gfxService is not null)
            {
                Pictures = new ObservableCollection<string>(gfxService.GetPictureNames());
            }
        }
    }
}
