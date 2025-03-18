using System.Collections.Generic;

namespace EPP.Models
{
    public class ConfigData
    {
        public string EventPath { get; set; } = string.Empty;
        public string LocalizationPath { get; set; } = string.Empty;
        public List<string> SourceDirectories { get; set; } = new();
        public bool UseBackups { get; set; } = true;
    }
}
