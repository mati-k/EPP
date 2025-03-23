using Pdoxcl2Sharp;
using System.Collections.Generic;

namespace EPP.Models.Gfx
{
    public class GfxFileModel : IParadoxRead
    {
        public IList<GfxModel> Gfx { get; set; }
        public IList<GFXOther> OtherGfx { get; set; }

        public GfxFileModel()
        {
            Gfx = new List<GfxModel>();
            OtherGfx = new List<GFXOther>();
        }

        public void TokenCallback(ParadoxParser parser, string token)
        {
            if (token == null)
                return;

            if (token.Equals("spriteType") || token.Equals("textSpriteType"))
                Gfx.Add(parser.Parse(new GfxModel()));
            else if (!token.Equals("spriteTypes"))
                OtherGfx.Add(parser.Parse(new GFXOther(token, null)));
        }
    }
}
