using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPP.Services
{
    public class FontService : IFontService
    {
        private Dictionary<char, Brush> _colors = new();

        public void AddFontColor(char key, List<string> rgb)
        {
            if (!_colors.ContainsKey(key))
            {
                _colors[key] = new SolidColorBrush(Color.FromRgb((byte)Int32.Parse(rgb[0]), (byte)Int32.Parse(rgb[1]), (byte)Int32.Parse(rgb[2])));
            }
        }

        public Brush? GetColorForKey(char key)
        {
            if (_colors.ContainsKey(key))
            {
                return _colors[key];
            }

            return null;
        }

        public bool IsLoaded()
        {
            return _colors.Any();
        }
    }
}
