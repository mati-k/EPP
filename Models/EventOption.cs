using Pdoxcl2Sharp;

namespace EPP.Models
{
    public class EventOption : GroupNode
    {
        public override void TokenCallback(ParadoxParser parser, string token)
        {
            switch (token)
            {
                case "name": Name = parser.ReadString(); break;
                default: base.TokenCallback(parser, token); break;
            }
        }
    }
}
