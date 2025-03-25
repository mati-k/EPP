using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Pfim;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace EPP.Services
{
    public class MockGfxService : IGfxService
    {
        private const string _gameFolder = "G:/Steam/steamapps/common/Europa Universalis IV";

        private readonly Dictionary<string, List<string>> _groupedPictures = new()
        {
            { "ADVISOR_eventPicture", ["ADVISOR_eventPicture"] },
            { "ANGRY_MOB_eventPicture", ["ANGRY_MOB_eventPicture", "asiangfx_ANGRY_MOB_eventPicture"] }
        };

        private readonly Dictionary<string, string> _basePictureCache = new()
        {
            { "ADVISOR_eventPicture", "ADVISOR_eventPicture" },
            { "ANGRY_MOB_eventPicture", "ANGRY_MOB_eventPicture" },
            { "asiangfx_ANGRY_MOB_eventPicture", "ANGRY_MOB_eventPicture" },
        };

        private readonly Dictionary<string, string> _picturePaths = new()
        {
            { "ADVISOR_eventPicture", "gfx/event_pictures/event_pictures_EUROPEAN/ADVISOR_eventPicture.dds" },
            { "ANGRY_MOB_eventPicture", "gfx/event_pictures/event_pictures_EUROPEAN/ANGRY_MOB_eventPicture.dds" },
            { "asiangfx_ANGRY_MOB_eventPicture", "gfx/event_pictures/event_pictures_CS/BUDDHISM_BAD_eventPicture.dds" },
            { "events_BG_top", "gfx/interface/events_BG_top.dds" },
            { "events_BG_middle", "gfx/interface/events_BG_middle.dds" },
            { "events_BG_bottom_M", "gfx/interface/events_BG_bottom_M.dds" },
            { "event_button_547", "gfx/interface/event_button_547.dds" },
        };

        public void GeneratePaths()
        {
        }

        public string GetBasePicture(string? picture)
        {
            if (picture == null || !_basePictureCache.ContainsKey(picture))
            {
                return String.Empty;
            }

            return _basePictureCache[picture];
        }

        public string? GetNameWithDlcText(string? name)
        {
            return name;
        }

        public Bitmap? GetPicture(string? name)
        {
            if (name == null)
            {
                return null;
            }

            if (!_picturePaths.TryGetValue(name, out string? filePath))
            {
                return null;
            }

            filePath = Path.Combine(_gameFolder, filePath);
            if (!File.Exists(filePath))
            {
                return null;
            }

            try
            {
                using var image = Pfimage.FromFile(filePath);
                try
                {
                    var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                    return new Bitmap(GfxService.PixelFormat(image), AlphaFormat.Unpremul, data, new Avalonia.PixelSize(image.Width, image.Height), new Avalonia.Vector(96, 96), image.Stride);
                }

                catch (Exception e)
                {
                    Log.Error(e, $"Error while loading picture {name}");
                    return null;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"Error while loading picture {name}");
                return null;
            }
        }

        public List<string> GetPictureNames()
        {
            return [.. _groupedPictures.Keys];
        }

        public List<string> GetVariants(string? picture)
        {
            if (string.IsNullOrEmpty(picture) || !_groupedPictures.ContainsKey(picture))
            {
                return [];
            }

            return _groupedPictures[picture]!;
        }

        public bool HasVariants(string? picture)
        {
            if (string.IsNullOrEmpty(picture) || !_groupedPictures.ContainsKey(picture))
            {
                return false;
            }

            return _groupedPictures[picture].Count > 1;
        }

        public async Task LoadSourceDirectory(string? path)
        {

        }
    }
}
