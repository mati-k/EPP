using CommunityToolkit.Mvvm.ComponentModel;
using Pdoxcl2Sharp;
using System.Collections.Generic;

namespace EPP.Models
{
    public class EventFile : ObservableObject, IParadoxRead
    {
        public List<ModEvent> Events { get; set; } = new();
        public string Namespace { get; set; }

        public EventFile() { }

        public void TokenCallback(ParadoxParser parser, string token)
        {
            if (token.Equals("namespace"))
                Namespace = parser.ReadString();
            else if (token.Equals("country_event"))
                Events.Add(parser.Parse(new ModEvent(true)));
            else
                Events.Add(parser.Parse(new ModEvent(false)));
        }
    }
}
