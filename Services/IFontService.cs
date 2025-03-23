using Avalonia.Media;
using System.Collections.Generic;

namespace EPP.Services
{
    public interface IFontService
    {
        public void AddFontColor(char key, List<string> rgb);
        public Brush? GetColorForKey(char key);
        public bool IsLoaded();
    }
}
