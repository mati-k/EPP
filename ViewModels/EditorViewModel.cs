using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using DialogHostAvalonia;
using EPP.Models;
using EPP.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace EPP.ViewModels
{
    public partial class EditorViewModel : ViewModelBase
    {
        [ObservableProperty]
        private EventFile _eventFile;

        [ObservableProperty]
        private ModEvent _selectedEvent;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ActivePictures))]
        private ObservableCollection<string> _pictures = [];

        [ObservableProperty]
        private string _pictureQuery = "";

        public ObservableCollection<string> ActivePictures
        {
            get
            {
                return new ObservableCollection<string>(Pictures.Where(picture => picture.Contains(PictureQuery, System.StringComparison.OrdinalIgnoreCase)));
            }
        }

        partial void OnSelectedEventChanged(ModEvent? oldValue, ModEvent newValue)
        {

            if (oldValue == newValue)
            {
                return;
            }

            if (oldValue != null)
            {
                oldValue.PropertyChanged -= OnSelectedEventPropertyChanged;
            }

            if (newValue != null)
            {
                newValue.PropertyChanged += OnSelectedEventPropertyChanged;

                // Force refresh on combobox to start with initial value
                if (newValue.HasMultiplePictures)
                {
                    ForceRefreshSelectedEventPicture(newValue);
                }
            }
        }

        private void OnSelectedEventPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(SelectedEvent));
        }

        // Manualy update to restore selection if picture reappears
        partial void OnPictureQueryChanged(string value)
        {
            this.OnPropertyChanged(nameof(ActivePictures));
            if (SelectedEvent != null && ActivePictures.Contains(SelectedEvent.SelectedPicture.Current))
            {
                ForceRefreshSelectedEventPicture(SelectedEvent);
            }
        }

        private static void ForceRefreshSelectedEventPicture(ModEvent modEvent)
        {
            var tmp = modEvent.SelectedPicture;
            modEvent.SelectedPicture = null!;
            modEvent.SelectedPicture = tmp;
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

        public async Task ShowVariantDialog(string picture)
        {
            IGfxService gfxService = Ioc.Default.GetService<IGfxService>()!;
            List<string> variants = gfxService.GetVariants(picture);
            await DialogHost.Show(new PictureVariantDialogData(variants, SelectedEvent.SelectedPicture.Current, SelectVariant), "MainDialogHost");
        }

        private void SelectVariant(string picture)
        {
            SelectedEvent.SelectedPicture.Current = picture;
        }
    }
}
