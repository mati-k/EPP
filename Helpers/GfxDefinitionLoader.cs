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
        public List<GfxModel> PicturePathDefinitions { get; private set; } = [];

        public async Task Load(string path)
        {
            await LoadDirectory(Path.Combine(path, _interfacePath));
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
                else if (Path.GetExtension(item.Name) == ".gfx")
                {
                    var gfxFile = await fileService!.OpenFileAsync(item.TryGetLocalPath()!);
                    if (gfxFile == null)
                    {
                        Log.Error($@"Failed to open .gfx file: {item.TryGetLocalPath()}\n");
                        continue;
                    }

                    await using var stream = await gfxFile.OpenReadAsync();
                    LoadFileContent(stream, item.Name);
                }
            }
        }

        public void LoadFileContent(Stream stream, string name)
        {
            var fontService = Ioc.Default.GetService<IFontService>()!;
            GfxFileModel gfxFileData = ParadoxParser.Parse(stream, new GfxFileModel());

            // Load only the first appearance of color definitions
            if (name.Contains("core.gfx") && !fontService.IsLoaded())
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
                if (gfx.TextureFile != null && !PicturePathDefinitions.Any(item => item.Name == gfx.Name)
                    && TransformPathToCommonFormat(gfx.TextureFile).StartsWith("gfx/") && gfx.Name.Contains("eventPicture")
                )
                {
                    gfx.TextureFile = TransformPathToCommonFormat(gfx.TextureFile);
                    PicturePathDefinitions.Add(gfx);
                }
            });
        }

        public static string TransformPathToCommonFormat(string path)
        {
            return path.Replace(@"\", @"/").Replace(@"//", @"/");
        }
    }
}
