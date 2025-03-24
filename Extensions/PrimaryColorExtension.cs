using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Material.Colors;
using System;

namespace EPP.Extensions
{
    public class PrimaryColorExtension : MarkupExtension
    {
        public PrimaryColorExtension()
        {
        }

        public PrimaryColorExtension(PrimaryColor color)
        {
            Color = color;
        }

        [ConstructorArgument("color")]
        public PrimaryColor Color { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new SolidColorBrush(SwatchHelper.Lookup[(MaterialColor)Color]);
        }
    }
}
