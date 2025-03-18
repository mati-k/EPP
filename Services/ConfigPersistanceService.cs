using EPP.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace EPP.Services
{
    public static class ConfigPersistanceService
    {
        private const string _fileName = "config.json";
        private static string _filePath = Path.Combine(Directory.GetCurrentDirectory(), _fileName);

        public static async Task SaveToFileAsync(ConfigData config)
        {
            // Ensure all directories exists
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

            using (var fs = File.Create(_filePath))
            {
                await JsonSerializer.SerializeAsync(fs, config);
            }
        }

        public static async Task<ConfigData?> LoadFromFileAsync()
        {
            try
            {
                using (var fs = File.OpenRead(_filePath))
                {
                    return await JsonSerializer.DeserializeAsync<ConfigData>(fs);
                }
            }
            catch (Exception e) when (e is FileNotFoundException || e is DirectoryNotFoundException)
            {
                return null;
            }
        }
    }
}
