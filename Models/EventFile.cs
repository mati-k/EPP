using CommunityToolkit.Mvvm.ComponentModel;
using Pdoxcl2Sharp;
using System.Collections.ObjectModel;
using System.Linq;

namespace EPP.Models
{
    public partial class EventFile : ObservableObject, IParadoxRead
    {
        public ObservableCollection<ModEvent> Events { get; set; } = new();
        public string Namespace { get; set; } = "";
        [ObservableProperty]
        private bool _isAnyEventChanged = false;

        public void TokenCallback(ParadoxParser parser, string token)
        {
            if (token.Equals("namespace"))
                Namespace = parser.ReadString();
            else if (token.Equals("country_event"))
                Events.Add(parser.Parse(new ModEvent(true)));
            else
                Events.Add(parser.Parse(new ModEvent(false)));
        }

        public void BindLocalization(Localization localization)
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

        public void TrackEventChange()
        {
            foreach (ModEvent modEvent in Events)
            {
                modEvent.PropertyChanged += OnModEventPropertyChanged;
            }
        }

        private void OnModEventPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var modEvent = (ModEvent)sender!;
            bool isEventChanged = modEvent.Picture != modEvent.OriginalPicture;

            if (isEventChanged != IsAnyEventChanged)
            {
                IsAnyEventChanged = Events.Any(modEvent => modEvent.Picture != modEvent.OriginalPicture);
            }
        }
    }
}
