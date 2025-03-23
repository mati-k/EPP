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

        public ConfigService()
        {
            try
            {
                using (var fs = File.OpenRead(_filePath))
                {
                    var config = JsonSerializer.Deserialize<ConfigData>(fs);

                    if (config != null)
                    {
                        ConfigData = config;
                    }
                }
            }
            catch (Exception e) when (e is FileNotFoundException || e is DirectoryNotFoundException)
            {
            }
        }

        public async Task SaveConfig(ConfigData config)
        {

            using (var fs = File.Create(_filePath))
            {
                await JsonSerializer.SerializeAsync(fs, config, new JsonSerializerOptions { WriteIndented = true });
            }

            ConfigData = config;
        }
    }
}
