using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EPP.ViewModels;
using EPP.Views;
using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
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
        [NotifyPropertyChangedFor(nameof(IsChanged))]
        [NotifyCanExecuteChangedFor(nameof(ResetIconCommand))]
        private string _picture;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsChanged))]
        [NotifyCanExecuteChangedFor(nameof(ResetIconCommand))]
        private string _originalPicture;
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
        public List<EventOption> Options { get; set; } = new();
        public bool IsChanged { get => Picture != OriginalPicture; }

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
                        string picture;
                        if (parser.NextIsBracketed())
                            picture = (parser.Parse(new GroupNode()).Nodes.Where(n => n.Name.Equals("picture")).First() as ValueNode)!.Value;
                        else
                            picture = parser.ReadString();

                        if (String.IsNullOrWhiteSpace(Picture))
                            Picture = picture;

                        OriginalPicture = picture;
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
                throw new Exception($"Event exception, event type: {eventTypeText}, id: {Id}, issue at token: {token.ToString()}\n{e.ToString()}");
            }
        }

        [RelayCommand(CanExecute = nameof(IsChanged))]
        public void ResetIcon(EditorView editorView)
        {
            Picture = OriginalPicture;

            EditorViewModel viewModel = (EditorViewModel)editorView.DataContext!;
            if (viewModel != null && viewModel.SelectedEvent == this)
            {
                viewModel.SelectedPicture = OriginalPicture;
            }
        }
    }
}