using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.DependencyInjection;
using EPP.Services;

namespace EPP.Converters
{
    internal class DDSConverter
    {
        public static readonly DDSConverter Instance = new();

        public static FuncValueConverter<string, Bitmap?> LoadDDS { get; } = new FuncValueConverter<string, Bitmap?>((filePath) => Ioc.Default?.GetService<IGfxService>()?.GetPicture(filePath));
    }
}
