using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EPP.Converters
{
    public class SizeLimiter : IValueConverter
    {
        private static readonly DDSConverter defaultInstace = new DDSConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (Int32.TryParse((string)parameter!, out int count))
            {
                return ((IEnumerable<object>)value!).Take(count);
            }

            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
