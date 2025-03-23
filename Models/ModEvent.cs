using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EPP.Models
{
    public partial class ModEvent : ObservableObject, IParadoxRead
    {
        public string Id { get; set; }
        public bool IsCountryEvent { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ResetIconCommand))]
        private EventPicture _selectedPicture;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasMultiplePictures))]
        private ObservableCollection<EventPicture> _pictures = [];
        public bool FireOnlyOnce { get; set; } = false;
        public bool IsTriggeredOnly { get; set; } = false;
        public bool Hidden { get; set; } = false;
        public bool Major { get; set; } = false;
        public string GoTo { get; set; }
        public string IsMtthScaledToSize { get; set; }

        public GroupNode MeanTimeToHappen { get; set; }
        public GroupNode Trigger { get; set; }
        public GroupNode MajorTrigger { get; set; }
        public GroupNode Immediate { get; set; }
        public GroupNode After { get; set; }
        public List<EventOption> Options { get; set; } = [];
        public bool IsChanged { get => Pictures.Any(picture => picture.IsChanged); }
        public bool HasMultiplePictures { get => Pictures.Count > 1; }

        public ModEvent() { }

        public ModEvent(bool isCountryEvent)
        {
            this.IsCountryEvent = isCountryEvent;
        }

        public void TokenCallback(ParadoxParser parser, string token)
        {
            try
            {
                token = token.ToLower();
                switch (token)
                {
                    case "id": Id = parser.ReadString(); break;
                    case "title": Title = parser.ReadString(); break;
                    case "desc":
                        string desc;
                        if (parser.NextIsBracketed())
                            desc = (parser.Parse(new GroupNode()).Nodes.Where(n => n.Name.Equals("desc")).First() as ValueNode)!.Value;
                        else
                            desc = parser.ReadString();

                        if (String.IsNullOrWhiteSpace(Description))
                            Description = desc;
                        break;

                    case "picture":
                        var picture = parser.NextIsBracketed() ? parser.Parse(new EventPicture()) : new EventPicture(parser.ReadString());
                        Pictures.Add(picture);
                        if (SelectedPicture == null)
                        {
                            SelectedPicture = picture;
                        }
                        break;
                    case "is_triggered_only": IsTriggeredOnly = parser.ReadBool(); break;
                    case "fire_only_once": FireOnlyOnce = parser.ReadBool(); break;
                    case "hidden": Hidden = parser.ReadBool(); break;
                    case "major": Hidden = parser.ReadBool(); break;
                    case "goto": GoTo = parser.ReadString(); break;
                    case "is_mtth_scaled_to_size": IsMtthScaledToSize = parser.ReadString(); break;

                    case "mean_time_to_happen": MeanTimeToHappen = parser.Parse(new GroupNode() { Name = "mean_time_to_happen" }); break;
                    case "trigger": Trigger = parser.Parse(new GroupNode() { Name = "trigger" }); break;
                    case "major_trigger": Trigger = parser.Parse(new GroupNode() { Name = "trigger" }); break;
                    case "immediate": Immediate = parser.Parse(new GroupNode() { Name = "immediate" }); break;
                    case "after": Immediate = parser.Parse(new GroupNode() { Name = "after" }); break;
                    case "option": Options.Add(parser.Parse(new EventOption())); break;
                    default: parser.Parse(new GroupNode()); break;
                }
            }
            catch (Exception e)
            {
                string eventTypeText = IsCountryEvent ? "country_event" : "province_event";
                throw new Exception($"Event exception, event type: {eventTypeText}, id: {Id}, issue at token: {token}\n{e}");
            }
        }

        [RelayCommand(CanExecute = nameof(IsChanged))]
        public void ResetIcon()
        {
            foreach (var picture in Pictures)
            {
                picture.Reset();
            }
        }

        partial void OnSelectedPictureChanged(EventPicture? oldValue, EventPicture newValue)
        {
            if (oldValue != null)
            {
                oldValue.PropertyChanged -= OnSelectedPicturePropertyChanged;
            }

            if (newValue != null)
            {
                newValue.PropertyChanged += OnSelectedPicturePropertyChanged;
            }
        }

        private void OnSelectedPicturePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(SelectedPicture));
            ResetIconCommand.NotifyCanExecuteChanged();
        }

        public void RefreshIsChanged()
        {
            ResetIconCommand.NotifyCanExecuteChanged();
        }
    }
}