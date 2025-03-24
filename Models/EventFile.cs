using CommunityToolkit.Mvvm.ComponentModel;
using EPP.Helpers;
using Pdoxcl2Sharp;
using System.Collections.ObjectModel;
using System.Linq;

namespace EPP.Models
{
    public partial class EventFile : ObservableObject, IParadoxRead
    {
        public ObservableCollection<ModEvent> Events { get; set; } = [];
        public string Namespace { get; set; } = "";
        [ObservableProperty]
        private bool _isAnyEventChanged = false;

        public void TokenCallback(ParadoxParser parser, string token)
        {
            if (token.Equals("namespace"))
            {
                Namespace = parser.ReadString();
            }
            else
            {
                var parsedEvent = parser.Parse(new ModEvent(token.Equals("country_event")));

                Events.Add(parsedEvent);
                if (parsedEvent.Pictures.Count == 0)
                {
                    var picture = new EventPicture("");
                    parsedEvent.Pictures.Add(picture);
                    parsedEvent.SelectedPicture = picture;
                }
                parsedEvent.PropertyChanged += OnModEventPropertyChanged;
            }
        }

        public void BindLocalization(LocalizationLoader localization)
        {
            foreach (ModEvent modEvent in Events)
            {
                if (modEvent.Title != null)
                {
                    modEvent.Title = localization.GetValueForKey(modEvent.Title);
                }

                if (modEvent.Description != null)
                {
                    modEvent.Description = localization.GetValueForKey(modEvent.Description);
                }

                foreach (EventOption option in modEvent.Options)
                {
                    option.Name = localization.GetValueForKey(option.Name);
                }
            }
        }

        private void OnModEventPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var modEvent = (ModEvent)sender!;
            bool isEventChanged = modEvent.IsChanged;

            if (isEventChanged != IsAnyEventChanged)
            {
                UpdateIsAnyChanged();
            }
        }

        public void UpdateIsAnyChanged()
        {
            IsAnyEventChanged = Events.Any(modEvent => modEvent.IsChanged);
        }
    }
}
