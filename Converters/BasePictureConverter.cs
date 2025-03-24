using Avalonia.Data;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.DependencyInjection;
using EPP.Services;
using System;
using System.Globalization;

namespace EPP.Converters
{
    public class BasePictureConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var gfxService = Ioc.Default?.GetService<IGfxService>();
            if (gfxService != null)
            {
                return gfxService.GetBasePicture(value as string);
            }

            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Prevent listbox setting picture to null when picture dissapears from list due to filtering
            return value ?? BindingOperations.DoNothing;
        }
    }
}
