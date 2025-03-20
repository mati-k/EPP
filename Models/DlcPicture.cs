using Avalonia.Media.Imaging;

namespace EPP.Models
{
    public class DlcPicture
    {
        public Bitmap Picture { get; private set; }
        public string DlcName { get; private set; }

        public DlcPicture(Bitmap picture, string dlcName)
        {
            Picture = picture;
            DlcName = dlcName;
        }
    }
}
