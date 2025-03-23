using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.DependencyInjection;
using EPP.Models.Gfx;
using EPP.Services;
using Pdoxcl2Sharp;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EPP.Helpers
{
    public class GfxDefinitionLoader
    {
        private const string _interfacePath = "interface";
        private List<GfxModel> _unassignedFilePaths = [];
        Dictionary<string, string> _gfxFiles = new();

        public async Task Load(string path)
        {
            await LoadDirectory(Path.Combine(path, _interfacePath));
        }

        private async Task LoadDirectory(string path)
        {
            var fileService = Ioc.Default.GetService<IFileService>();
            var fontService = Ioc.Default.GetService<IFontService>()!;

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
                else if (Path.GetExtension(item.Name) == ".gfx")
                {
                    var gfxFile = await fileService!.OpenFileAsync(item.TryGetLocalPath()!);
                    if (gfxFile == null)
                    {
                        Log.Error($@"Failed to open .gfx file: {item.TryGetLocalPath()}\n");
                        continue;
                    }

                    await using var stream = await gfxFile.OpenReadAsync();
                    GfxFileModel gfxFileData = ParadoxParser.Parse(stream, new GfxFileModel());

                    // Load only the first appearance of color definitions
                    if (item.Name.Contains("core.gfx") && !fontService.IsLoaded())
                    {
                        var colors = gfxFileData.OtherGfx.Where(topNode => topNode.Name.Equals("bitmapfonts")).First()
                            .Nodes.Where(node => node.Name.Equals("textcolors")).First().Nodes;

                        foreach (var color in colors)
                        {
                            fontService.AddFontColor(color.Name[0], color.Colors);
                        }
                    }

                    gfxFileData.Gfx.ToList().ForEach(gfx =>
                    {
                        if (gfx.TextureFile != null && !_gfxFiles.ContainsKey(gfx.Name)
                            && gfx.TextureFile.Replace(@"//", @"/").StartsWith("gfx/") && gfx.TextureFile.Contains("eventPicture")
                        )
                        {
                            _unassignedFilePaths.Add(gfx);
                        }
                    });
                }
            }
        }
    }
}
