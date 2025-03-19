using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.DependencyInjection;
using Pfim;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace EPP.Services
{
    public class GfxService : IGfxService
    {
        private Dictionary<string, string> _gfxFiles = new();

        private string _eventPicturesPath = Path.Combine("gfx", "event_pictures");
        private string _interfacePath = Path.Combine("gfx", "interface");

        private List<string> _interfaceIcons = new List<string>() {
            "events_BG_top",
            "events_BG_middle",
            "events_BG_bottom_M",
            "event_button_547"
        };

        public async Task LoadSourceDirectory(string? path)
        {
            if (path == null)
            {
                return;
            }

            await LoadEventPictures(Path.Combine(path, _eventPicturesPath));
            await LoadEventInterface(path);
        }

        private async Task LoadEventPictures(string path)
        {
            var fileService = Ioc.Default.GetService<IFilesService>();

            var items = await fileService.ListFolderAsync(path);
            if (items == null)
            {
                return;
            }

            await foreach (var item in items)
            {
                if (item is IStorageFolder)
                {
                    await LoadEventPictures(item.TryGetLocalPath());
                }

                else
                {
                    string nameWithoutExtension = Path.GetFileNameWithoutExtension(item.Name);

                    if (Path.GetExtension(item.Name) != ".dds" || _gfxFiles.ContainsKey(nameWithoutExtension))
                    {
                        continue;
                    }

                    _gfxFiles.Add(nameWithoutExtension, item.TryGetLocalPath());
                }
            }
        }

        private async Task LoadEventInterface(string path)
        {
            var fileService = Ioc.Default.GetService<IFilesService>();
            path = Path.Combine(path, _interfacePath);

            var items = await fileService.ListFolderAsync(path);
            if (items == null)
            {
                return;
            }

            await foreach (var item in items)
            {
                foreach (string interfaceIcon in _interfaceIcons)
                {
                    if (Path.GetFileNameWithoutExtension(item.Name) == interfaceIcon && !_gfxFiles.ContainsKey(interfaceIcon))
                    {
                        _gfxFiles.Add(interfaceIcon, item.TryGetLocalPath());
                    }
                }
            }
        }

        public Bitmap? GetPicture(string? name)
        {
            if (name == null || !_gfxFiles.ContainsKey(name))
            {
                return null;
            }

            string filePath = _gfxFiles[name];
            if (!File.Exists(filePath))
                return null;

            try
            {
                using (var image = Pfimage.FromFile(filePath))
                {
                    try
                    {
                        var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                        return new Bitmap(PixelFormats.Bgra8888, AlphaFormat.Unpremul, data, new Avalonia.PixelSize(image.Width, image.Height), new Avalonia.Vector(96, 96), image.Stride);
                    }

                    catch (Exception e)
                    {
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
