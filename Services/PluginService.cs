using licensePemoseServer.Data;
using licensePemoseServer.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace licensePemoseServer.Services
{
    public class PluginService : IPluginService
    {
        private readonly ApplicationDbContext _context;
        public PluginService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Plugin> CreatePluginAsync(string pluginName, string version, string description)
        {
            var plugin = new Plugin
            {
                PluginName = pluginName,
                Version = version,
                Description  = description
            };
            _context.Add(plugin);
            await _context.SaveChangesAsync();

            return plugin;

        }

        public async Task<Plugin> GetPluginByIdAsync(int pluginId)
        {
            return await _context.Set<Plugin>().FirstOrDefaultAsync(l => l.PluginId == pluginId);
        }
    }
}
