using Avalonia.Media.Imaging;
using System.Threading.Tasks;

namespace EPP.Services
{
    public interface IGfxService
    {
        public Task LoadSourceDirectory(string? path);
        public Bitmap? GetPicture(string? name);
    }
}
