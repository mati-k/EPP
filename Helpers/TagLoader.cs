using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.DependencyInjection;
using EPP.Services;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EPP.Helpers
{
    public class TagLoader
    {
        private static readonly string _tagPath = Path.Combine("common", "country_tags");
        private readonly HashSet<string> _readFiles = [];
        private readonly HashSet<string> _tags = [];

        public async Task Load(string path)
        {
            await LoadDirectory(Path.Combine(path, _tagPath));
        }

        private async Task LoadDirectory(string path)
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
                    await LoadDirectory(item.TryGetLocalPath()!);
                }
                else if (Path.GetExtension(item.Name) == ".txt" && !_readFiles.Contains(item.Name))
                {
                    var gfxFile = await fileService!.OpenFileAsync(item.TryGetLocalPath()!);
                    if (gfxFile == null)
                    {
                        Log.Error($@"Failed to open country tag file : {item.TryGetLocalPath()}\n");
                        continue;
                    }

                    _readFiles.Add(item.Name);

                    await using var stream = await gfxFile.OpenReadAsync();
                    LoadFileContent(stream);
                }
            }
        }

        private void LoadFileContent(Stream stream)
        {
            var fontService = Ioc.Default.GetService<IFontService>()!;

            using StreamReader reader = new(stream);
            while (!reader.EndOfStream)
            {
                string? line = reader.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                line = line.Trim();
                // Ignore comment line
                if (line.StartsWith("#"))
                {
                    continue;
                }

                if (line.Contains("#"))
                {
                    line = line.Substring(0, line.IndexOf("#")).Trim();
                }

                if (!line.Contains("="))
                {
                    continue;
                }

                string tag = line.Split('=')[0].Trim();
                _tags.Add(tag);
            }
        }

        public bool IsCountryTag(string tag)
        {
            return _tags.Contains(tag);
        }
    }
}
