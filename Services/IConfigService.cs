using EPP.Models;
using System.Threading.Tasks;

namespace EPP.Services
{
    public interface IConfigService
    {
        public ConfigData ConfigData { get; }
        public Task SaveConfig(ConfigData config);
    }
}
