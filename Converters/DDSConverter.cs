using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.DependencyInjection;
using EPP.Services;

namespace EPP.Converters
{
    public class DDSConverter
    {
        public static readonly DDSConverter Instance = new();

        public static FuncValueConverter<string, Bitmap?> LoadDDS { get; } = new FuncValueConverter<string, Bitmap?>((pictureName) => Ioc.Default?.GetService<IGfxService>()?.GetPicture(pictureName));
        public static FuncValueConverter<string, string?> AddDlcText { get; } = new FuncValueConverter<string, string?>((pictureName) => Ioc.Default?.GetService<IGfxService>()?.GetNameWithDlcText(pictureName));
    }
}
