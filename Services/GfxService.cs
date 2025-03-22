using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.DependencyInjection;
using EPP.Models;
using Pfim;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace EPP.Services
{
    // todo: read interface .gfx for extra name mappings
    // todo: handle picture variants (e.g. ANGRY_MOB_eventPicture, northamericagfx_ANGRY_MOB_eventPicture, east_slavic_ANGRY_MOB_eventPicture, etc.)
    public class GfxService : IGfxService
    {
        private Dictionary<string, string> _gfxFiles = new();
        private Dictionary<string, DlcPicture> _dlcGfxFiles = new();

        private string _eventPicturesPath = Path.Combine("gfx", "event_pictures");
        private string _interfacePath = Path.Combine("gfx", "interface");
        private string _dlcPath = "dlc";
        private string _builtIndlcPath = "dlc";

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
            await LoadDlcEventContent(Path.Combine(path, _builtIndlcPath), true);
            await LoadDlcEventContent(Path.Combine(path, _dlcPath), false);
        }

        private async Task LoadEventPictures(string path)
        {
            var fileService = Ioc.Default.GetService<IFileService>();

            var items = await fileService!.ListFolderAsync(path);
            if (items == null)
            {
                return;
            }

            await foreach (var item in items)
            {
                if (item is IStorageFolder)
                {
                    await LoadEventPictures(item.TryGetLocalPath()!);
                }

                else
                {
                    string nameWithoutExtension = Path.GetFileNameWithoutExtension(item.Name);

                    if (Path.GetExtension(item.Name) != ".dds" || _gfxFiles.ContainsKey(nameWithoutExtension))
                    {
                        continue;
                    }

                    _gfxFiles.Add(nameWithoutExtension, item.TryGetLocalPath()!);
                }
            }
        }

        private async Task LoadEventInterface(string path)
        {
            var fileService = Ioc.Default.GetService<IFileService>();
            path = Path.Combine(path, _interfacePath);

            var items = await fileService!.ListFolderAsync(path);
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
                        _gfxFiles.Add(interfaceIcon, item.TryGetLocalPath()!);
                    }
                }
            }
        }

        private async Task LoadDlcEventContent(string path, bool isBuiltIn)
        {
            var fileService = Ioc.Default.GetService<IFileService>();

            var items = await fileService!.ListFolderAsync(path);
            if (items == null)
            {
                return;
            }

            await foreach (var item in items)
            {
                var dlc_content = await fileService.ListFolderAsync(item.TryGetLocalPath()!);
                if (dlc_content == null)
                {
                    continue;
                }

                string dlcName = "";
                // No need to display dlc name for free dlcs
                if (!isBuiltIn)
                {
                    dlcName = await LoadDlcName(dlc_content);
                }

                await LoadDlcPictures(dlc_content, dlcName);
            }
        }

        private async Task<string> LoadDlcName(IAsyncEnumerable<IStorageItem> dlcFiles)
        {
            await foreach (var file in dlcFiles)
            {
                if (Path.GetExtension(file.Name) == ".dlc")
                {
                    var fileService = Ioc.Default.GetService<IFileService>();
                    var dlcFile = await fileService!.OpenFileAsync(file.TryGetLocalPath()!);

                    if (dlcFile == null)
                    {
                        return "";
                    }

                    await using (var stream = await dlcFile.OpenReadAsync())
                    {
                        using var streamReader = new StreamReader(stream);
                        while (!streamReader.EndOfStream)
                        {
                            var line = streamReader.ReadLine();
                            if (line != null && line.StartsWith("name"))
                            {
                                return line.Split('=')[1].Trim().Trim('\"');
                            }
                        }
                    }
                }
            }

            return "";
        }

        private async Task LoadDlcPictures(IAsyncEnumerable<IStorageItem> dlcFiles, string dlcName)
        {
            await foreach (var file in dlcFiles)
            {
                if (Path.GetExtension(file.Name) == ".zip")
                {
                    var fileService = Ioc.Default.GetService<IFileService>();
                    var dlcFile = await fileService!.OpenFileAsync(file.TryGetLocalPath()!);

                    if (dlcFile == null)
                    {
                        return;
                    }

                    using (ZipArchive archive = ZipFile.OpenRead(dlcFile.TryGetLocalPath()!))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            if (entry.FullName.EndsWith(".dds") && entry.FullName.Contains("event_pictures"))
                            {
                                using (var stream = entry.Open())
                                {
                                    var picture = LoadZipPicture(stream);
                                    if (picture != null)
                                    {
                                        var name = Path.GetFileNameWithoutExtension(entry.Name);

                                        if (!_dlcGfxFiles.ContainsKey(name) && !_gfxFiles.ContainsKey(name))
                                        {
                                            _dlcGfxFiles.Add(name, new DlcPicture(picture, dlcName));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private Bitmap? LoadZipPicture(Stream stream)
        {
            try
            {
                using (var image = Pfimage.FromStream(stream))
                {
                    try
                    {
                        var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                        return new Bitmap(PixelFormat(image), AlphaFormat.Unpremul, data, new Avalonia.PixelSize(image.Width, image.Height), new Avalonia.Vector(96, 96), image.Stride);
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

        public Bitmap? GetPicture(string? name)
        {
            if (name == null)
            {
                return null;
            }

            if (_dlcGfxFiles.ContainsKey(name))
            {
                return _dlcGfxFiles[name].Picture;
            }

            if (!_gfxFiles.ContainsKey(name))
            {
                return null;
            }

            string filePath = _gfxFiles[name];
            if (!File.Exists(filePath))
            {
                return null;
            }

            try
            {
                using (var image = Pfimage.FromFile(filePath))
                {
                    try
                    {
                        var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                        return new Bitmap(PixelFormat(image), AlphaFormat.Unpremul, data, new Avalonia.PixelSize(image.Width, image.Height), new Avalonia.Vector(96, 96), image.Stride);
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

        private static PixelFormat PixelFormat(IImage image)
        {
            switch (image.Format)
            {
                case ImageFormat.Rgb24:
                    return PixelFormats.Bgr24;
                case ImageFormat.Rgba32:
                    return PixelFormats.Bgra8888;
                case ImageFormat.Rgb8:
                    return PixelFormats.Gray8;
                case ImageFormat.R5g5b5a1:
                    return PixelFormats.Bgr555;
                case ImageFormat.R5g5b5:
                    return PixelFormats.Bgr555;
                case ImageFormat.R5g6b5:
                    return PixelFormats.Bgr565;
                default:
                    throw new Exception($"Unable to convert {image.Format} to WPF PixelFormat");
            }
        }

        public string? GetNameWithDlcText(string? name)
        {
            if (name == null)
            {
                return "";
            }

            if (_dlcGfxFiles.ContainsKey(name) && !string.IsNullOrEmpty(_dlcGfxFiles[name].DlcName))
            {
                return $"{name} ({_dlcGfxFiles[name].DlcName})";
            }

            return name;
        }

        public List<string> GetPictureNames()
        {
            return _gfxFiles.Keys.Where(key => key.Contains("eventPicture"))
                .Concat(_dlcGfxFiles.Keys.Where(key => key.Contains("eventPicture")))
                .ToList();
        }
    }
}
