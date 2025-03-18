﻿using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPP.Models
{
    public class ModEvent : IParadoxRead
    {
        public string Id { get; set; }
        public bool IsCountryEvent { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Picture { get; set; } = "";
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

        public string PicturePath
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(Picture) && GfxStorage.Instance.GfxFiles.ContainsKey(Picture))
                    return GfxStorage.Instance.GfxFiles[Picture];
                return "";
            }
        }

        public ModEvent(bool isCountryEvent)
        {
            this.IsCountryEvent = isCountryEvent;
        }

        public void TokenCallback(ParadoxParser parser, string token)
        {
            token = token.ToLower();
            switch (token)
            {
                case "id": Id = parser.ReadString(); break;
                case "title": Title = parser.ReadString(); break;
                case "desc":
                    string desc;
                    if (parser.NextIsBracketed())
                        desc = (parser.Parse(new GroupNode()).Nodes.Where(n => n.Name.Equals("desc")).First() as ValueNode).Value;
                    else
                        desc = parser.ReadString();

                    if (String.IsNullOrWhiteSpace(Description))
                        Description = desc;
                    break;

                case "picture":
                    string picture;
                    if (parser.NextIsBracketed())
                        picture = (parser.Parse(new GroupNode()).Nodes.Where(n => n.Name.Equals("picture")).First() as ValueNode).Value;
                    else
                        picture = parser.ReadString();

                    if (String.IsNullOrWhiteSpace(Picture))
                        Picture = picture;

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
    }
}
