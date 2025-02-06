using licensePemoseServer.Models;

namespace licensePemoseServer.Services
{
    public interface IPluginService
    {
        Task<Plugin?> GetPluginByIdAsync(int pluginId);
        Task<Plugin> CreatePluginAsync(string pluginName, string version, string description);
    }
}
