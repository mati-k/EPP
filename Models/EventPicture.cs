﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using EPP.Services;
using Pdoxcl2Sharp;

namespace EPP.Models
{
    public partial class EventPicture : ObservableObject, IParadoxRead
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentBasePicture))]
        private string _current = "";
        [ObservableProperty]
        private string _original = "";
        [ObservableProperty]
        private bool _isSimplePicture = true;
        private GroupNode? _trigger;

        public string CurrentBasePicture
        {
            get
            {
                return Ioc.Default?.GetService<IGfxService>()?.GetBasePicture(Current) ?? "";
            }
            set
            {
                Current = value;
            }
        }


        public string TriggerText
        {
            get
            {
                if (_trigger == null)
                {
                    return string.Empty;
                }

                return _trigger.GetText(-1);
            }
        }

        public bool IsChanged { get => Current != Original; }

        public EventPicture()
        {
            _isSimplePicture = false;
        }

        public EventPicture(string name)
        {
            Original = name;
            Current = name;
        }

        public void TokenCallback(ParadoxParser parser, string token)
        {
            if (token.Equals("picture"))
            {
                Original = parser.ReadString();
                Current = Original;
            }
            else if (token.Equals("trigger"))
            {
                _trigger = parser.Parse(new GroupNode() { Name = "trigger" });
            }
        }

        public void Reset()
        {
            Current = Original;
        }
    }
}
