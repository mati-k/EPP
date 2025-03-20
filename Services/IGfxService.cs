using Avalonia.Media.Imaging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPP.Services
{
    public interface IGfxService
    {
        public Task LoadSourceDirectory(string? path);
        public Bitmap? GetPicture(string? name);
        public string? GetNameWithDlcText(string? name);
        public List<string> GetPictureNames();
    }
}
