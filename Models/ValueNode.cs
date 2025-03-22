using Pdoxcl2Sharp;

namespace EPP.Models
{
    public class ValueNode : Node
    {
        public string Value { get; private set; } = "";

        public ValueNode(string name, string value, GroupNode parent)
        {
            Name = name;
            Value = value;
            Parent = parent;
        }

        public override void TokenCallback(ParadoxParser parser, string token) { }

        public override string ToString()
        {
            return Name + " = " + Value;
        }
    }
}
