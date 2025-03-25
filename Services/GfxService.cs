using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.DependencyInjection;
using EPP.Helpers;
using EPP.Models;
using Pfim;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace EPP.Services
{
    public class GfxService : IGfxService
    {
        private readonly Dictionary<string, string> _gfxFiles = [];
        private readonly Dictionary<string, DlcPicture> _dlcGfxFiles = [];

        private readonly Dictionary<string, string> _picturePaths = [];
        private readonly Dictionary<string, DlcPicture> _dlcPictures = [];

        private readonly string _eventPicturesPath = Path.Combine("gfx", "event_pictures");
        private readonly string _interfacePath = Path.Combine("gfx", "interface");
        private readonly string _dlcPath = "dlc";
        private readonly string _builtIndlcPath = "builtin_dlc";

        private readonly Dictionary<string, List<string>> _groupedPictures = [];
        private readonly Dictionary<string, string> _basePictureCache = [];

        private readonly List<string> _interfaceIcons = [
            "events_BG_top",
            "events_BG_middle",
            "events_BG_bottom_M",
            "event_button_547"
        ];

        private readonly GfxDefinitionLoader _gfxDefinitionLoader = new();
        private readonly TagLoader _tagLoader = new();

        public async Task LoadSourceDirectory(string? path)
        {
            if (path == null)
            {
                return;
            }

            await _gfxDefinitionLoader.Load(path);
            await _tagLoader.Load(path);
            await LoadEventPictures(Path.Combine(path, _eventPicturesPath), _eventPicturesPath);
            await LoadEventInterface(path);
            await LoadDlcEventContent(Path.Combine(path, _builtIndlcPath), true);
            await LoadDlcEventContent(Path.Combine(path, _dlcPath), false);
        }

        private async Task LoadEventPictures(string path, string localPath)
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
                    await LoadEventPictures(item.TryGetLocalPath()!, Path.Combine(localPath, item.Name));
                }

                else
                {
                    string relativeFilePath = GfxDefinitionLoader.TransformPathToCommonFormat(Path.Combine(localPath, item.Name));

                    if (Path.GetExtension(item.Name) != ".dds" || _gfxFiles.ContainsKey(relativeFilePath))
                    {
                        continue;
                    }

                    _gfxFiles.Add(relativeFilePath, item.TryGetLocalPath()!);
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
                    if (Path.GetFileNameWithoutExtension(item.Name) == interfaceIcon && !_picturePaths.ContainsKey(interfaceIcon))
                    {
                        _picturePaths.Add(interfaceIcon, item.TryGetLocalPath()!);
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

                await LoadDlcZipData(dlc_content, dlcName);
            }
        }

        private static async Task<string> LoadDlcName(IAsyncEnumerable<IStorageItem> dlcFiles)
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

                    await using var stream = await dlcFile.OpenReadAsync();
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

            return "";
        }

        private async Task LoadDlcZipData(IAsyncEnumerable<IStorageItem> dlcFiles, string dlcName)
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

                    using ZipArchive archive = ZipFile.OpenRead(dlcFile.TryGetLocalPath()!);
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith(".dds") && entry.FullName.Contains("event_pictures"))
                        {
                            LoadDlcPictures(entry, dlcName);
                        }

                        else if (entry.FullName.EndsWith(".gfx"))
                        {
                            LoadDlcGfxData(entry);
                        }
                    }
                }
            }
        }

        private void LoadDlcGfxData(ZipArchiveEntry entry)
        {
            using var stream = entry.Open();
            _gfxDefinitionLoader.LoadFileContent(stream, entry.Name);
        }

        private void LoadDlcPictures(ZipArchiveEntry entry, string dlcName)
        {
            using var stream = entry.Open();
            var picture = LoadZipPicture(stream);
            if (picture != null)
            {
                string formattedPath = GfxDefinitionLoader.TransformPathToCommonFormat(entry.FullName);
                if (!_dlcGfxFiles.ContainsKey(formattedPath) && !_gfxFiles.ContainsKey(entry.FullName))
                {
                    _dlcGfxFiles.Add(formattedPath, new DlcPicture(picture, dlcName));
                }
            }
        }

        private static Bitmap? LoadZipPicture(Stream stream)
        {
            try
            {
                using var image = Pfimage.FromStream(stream);
                try
                {
                    var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                    return new Bitmap(PixelFormat(image), AlphaFormat.Unpremul, data, new Avalonia.PixelSize(image.Width, image.Height), new Avalonia.Vector(96, 96), image.Stride);
                }

                catch (Exception e)
                {
                    Log.Error(e, "Error while loading picture from dlc zip");
                    return null;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Error while loading picture from dlc zip");
                return null;
            }
        }

        public void GeneratePaths()
        {
            foreach (var pathDefinition in _gfxDefinitionLoader.PicturePathDefinitions)
            {
                if (_gfxFiles.TryGetValue(pathDefinition.TextureFile, out string? path))
                {
                    _picturePaths[pathDefinition.Name] = path;
                }
                else if (_dlcGfxFiles.TryGetValue(pathDefinition.TextureFile, out DlcPicture? picture))
                {
                    _dlcPictures[pathDefinition.Name] = picture;
                }
                else
                {
                    Log.Error($"Event picture defintion {pathDefinition.Name} did not have corresponding image for defined path {pathDefinition.TextureFile}");
                }
            }

            _gfxFiles.Clear();
            _dlcGfxFiles.Clear();

            _groupedPictures.Clear();
            _basePictureCache.Clear();

            HashSet<string> pictures = [.. _picturePaths.Keys, .. _dlcPictures.Keys];
            foreach (var picture in pictures)
            {
                if (!picture.Contains("eventPicture"))
                {
                    continue;
                }

                string basePicture = CalculateBasePicture(picture);
                if (pictures.Contains(basePicture))
                {
                    if (!_groupedPictures.ContainsKey(basePicture))
                    {
                        // Make sure base picture will be at top
                        _groupedPictures[basePicture] = [basePicture];
                        _basePictureCache[basePicture] = basePicture;
                    }

                    if (!picture.Equals(basePicture))
                    {
                        _groupedPictures[basePicture].Add(picture);
                        _basePictureCache[picture] = basePicture;
                    }
                }
                else
                {
                    // If the base picture doesn't exist, there is no grouping
                    _groupedPictures[picture] = [picture];
                    _basePictureCache[picture] = picture;
                }
            }
        }

        private string CalculateBasePicture(string picture)
        {
            if (!picture.Contains('_'))
            {
                return picture;
            }

            int separationPosition = picture.IndexOf('_');
            string prefix = picture.Substring(0, separationPosition);
            string suffix = picture.Substring(separationPosition + 1);
            if (_tagLoader.IsCountryTag(prefix))
            {
                return suffix;
            }
            else if (prefix == prefix.ToLower())
            {
                while (prefix == prefix.ToLower())
                {
                    suffix = picture.Substring(separationPosition + 1);
                    separationPosition = picture.IndexOf('_', separationPosition + 1);
                    prefix = picture.Substring(0, separationPosition);
                }

                return suffix;
            }

            return picture;
        }


        public Bitmap? GetPicture(string? name)
        {
            if (name == null)
            {
                return null;
            }

            if (_dlcPictures.TryGetValue(name, out DlcPicture? value))
            {
                return value.Picture;
            }

            if (!_picturePaths.TryGetValue(name, out string? filePath))
            {
                return null;
            }

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
                    return new Bitmap(PixelFormat(image), AlphaFormat.Unpremul, data, new Avalonia.PixelSize(image.Width, image.Height), new Avalonia.Vector(96, 96), image.Stride);
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

        public static PixelFormat PixelFormat(IImage image)
        {
            return image.Format switch
            {
                ImageFormat.Rgb24 => PixelFormats.Bgr24,
                ImageFormat.Rgba32 => PixelFormats.Bgra8888,
                ImageFormat.Rgb8 => PixelFormats.Gray8,
                ImageFormat.R5g5b5a1 => PixelFormats.Bgr555,
                ImageFormat.R5g5b5 => PixelFormats.Bgr555,
                ImageFormat.R5g6b5 => PixelFormats.Bgr565,
                _ => throw new Exception($"Unable to convert {image.Format} to Avalonia PixelFormat"),
            };
        }

        public string? GetNameWithDlcText(string? name)
        {
            if (name == null)
            {
                return "";
            }

            if (_dlcPictures.TryGetValue(name, out DlcPicture? picture) && !string.IsNullOrEmpty(picture.DlcName))
            {
                return $"{name} ({picture.DlcName})";
            }

            return name;
        }

        public List<string> GetPictureNames()
        {
            return [.. _groupedPictures.Keys];
        }

        public bool HasVariants(string? picture)
        {
            if (string.IsNullOrEmpty(picture) || !_groupedPictures.ContainsKey(picture))
            {
                return false;
            }

            return _groupedPictures[picture].Count > 1;
        }

        public List<string> GetVariants(string? picture)
        {
            if (string.IsNullOrEmpty(picture) || !_groupedPictures.ContainsKey(picture))
            {
                return [];
            }

            return _groupedPictures[picture]!;
        }

        public string GetBasePicture(string? picture)
        {
            if (picture == null || !_basePictureCache.ContainsKey(picture))
            {
                return String.Empty;
            }

            return _basePictureCache[picture];
        }
    }
}
