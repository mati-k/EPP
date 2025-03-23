using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace EPP.Converters
{
    public class PreventNull : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value ?? BindingOperations.DoNothing;
        }
    }
}
