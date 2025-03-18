using Pdoxcl2Sharp;

namespace EPP.Models
{
    public abstract class Node : IParadoxRead
    {
        public GroupNode Parent { get; protected set; }
        public string Name { get; set; }

        public abstract void TokenCallback(ParadoxParser parser, string token);
    }
}
