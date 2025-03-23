using Pdoxcl2Sharp;

namespace EPP.Models.Gfx
{
    public class GfxModel : IParadoxRead
    {
        public string Name { get; set; }
        public string TextureFile { get; set; }

        public void TokenCallback(ParadoxParser parser, string token)
        {
            if (token.Equals("name".ToLower()))
                Name = parser.ReadString();

            else if (token.Equals("texturefile".ToLower()))
                TextureFile = parser.ReadString();
            else
                parser.ReadString();
        }
    }
}
