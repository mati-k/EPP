using System.Collections.Generic;

namespace EPP.Models
{
    public class GfxStorage
    {
        private static GfxStorage _instance;
        public static GfxStorage Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GfxStorage();
                return _instance;
            }
        }

        public Dictionary<string, string> GfxFiles { get; set; }
    }
}
