using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.DependencyInjection;
using EPP.Services;
using System.Linq;

namespace EPP.Converters
{
    public class DDSConverter
    {
        public static FuncValueConverter<string, Bitmap?> LoadDDS { get; } = new FuncValueConverter<string, Bitmap?>((pictureName) => Ioc.Default?.GetService<IGfxService>()?.GetPicture(pictureName));
        public static FuncMultiValueConverter<string, Bitmap?> LoadWithVariantDDS { get; } = new FuncMultiValueConverter<string, Bitmap?>((pictures) =>
        {
            if (pictures == null || pictures.Count() != 2 || pictures.Any(picture => picture == null))
            {
                return null;
            }

            var gfxService = Ioc.Default?.GetService<IGfxService>();
            if (gfxService != null)
            {
                string listPicture = pictures.ElementAt(0)!;
                string selectedPicture = pictures.ElementAt(1)!;

                string picture = listPicture;
                if (gfxService.GetBasePicture(selectedPicture) == listPicture)
                {
                    picture = selectedPicture;
                }

                return gfxService.GetPicture(picture);
            }

            return null;
        });
        public static FuncValueConverter<string, string?> AddDlcText { get; } = new FuncValueConverter<string, string?>((pictureName) => Ioc.Default?.GetService<IGfxService>()?.GetNameWithDlcText(pictureName));
        public static FuncMultiValueConverter<string, string?> AddDlcAndPrefixText { get; } = new FuncMultiValueConverter<string, string?>((pictures) =>
        {
            if (pictures == null || pictures.Count() != 2 || pictures.Any(picture => picture == null))
            {
                return null;
            }

            var gfxService = Ioc.Default?.GetService<IGfxService>();
            if (gfxService != null)
            {
                string listPicture = pictures.ElementAt(0)!;
                string selectedPicture = pictures.ElementAt(1)!;

                string picture = listPicture;
                if (gfxService.GetBasePicture(selectedPicture) == listPicture)
                {
                    picture = selectedPicture;
                }

                return gfxService.GetNameWithDlcText(picture);
            }

            return null;
        });

        public static FuncValueConverter<string, bool> HasVariant { get; } = new FuncValueConverter<string, bool>((pictureName) =>
        {
            var gfxService = Ioc.Default?.GetService<IGfxService>();
            if (gfxService != null)
            {
                return gfxService.HasVariants(pictureName);
            }

            return false;
        });
        public static FuncValueConverter<string, string?> GetBasePicture { get; } = new FuncValueConverter<string, string?>((pictureName) => Ioc.Default?.GetService<IGfxService>()?.GetBasePicture(pictureName));

    }
}
