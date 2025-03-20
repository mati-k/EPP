using CommunityToolkit.Mvvm.DependencyInjection;
using EPP.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace EPP.Services
{
    public class ConfigService : IConfigService
    {
        private const string _fileName = "config.json";
        private static string _filePath = Path.Combine(Directory.GetCurrentDirectory(), _fileName);

        public ConfigData ConfigData { get; private set; } = new();

        public async Task<ConfigData?> LoadFromFileAsync()
        {
            var filesService = Ioc.Default.GetService<IFileService>();
            if (filesService == null)
            {
                return null;
            }

            var file = await filesService.OpenFileAsync(_fileName);

            if (file != null)
            {
                await using var stream = await file.OpenReadAsync();
                return await JsonSerializer.DeserializeAsync<ConfigData>(stream);
            }

            return null;
        }

        public static async Task<ConfigService> LoadAndInitializeConfig()
        {
            ConfigService service = new ConfigService();

            try
            {
                using (var fs = File.OpenRead(_filePath))
                {
                    var config = await JsonSerializer.DeserializeAsync<ConfigData>(fs);

                    if (config != null)
                    {
                        service.ConfigData = config;
                    }
                }
            }
            catch (Exception e) when (e is FileNotFoundException || e is DirectoryNotFoundException)
            {
                return service;
            }

            return service;
        }

        public async Task SaveConfig(ConfigData config)
        {

            using (var fs = File.Create(_filePath))
            {
                await JsonSerializer.SerializeAsync(fs, config);
            }

            ConfigData = config;
        }
    }
}
