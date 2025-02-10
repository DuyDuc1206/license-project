
using licensePemoseServer.Models;

namespace LicenseManagerCloud.Services
{
    public interface ILicenseService
    {
        Task<License?> GetLicenseByMachineIdAsync(string machineId);
        Task<License> CreateLicenseAsync(string licenseKey, string machineId, string machineName, string status, DateTime expiryDate, int pluginId);
        Task<String> GetTokenByLicenseAsync(string licenseKey);
        Task<bool> UpdateLicenseStatusAsync(string licenseKey, string status);
        Task<IEnumerable<License>> GetAllLicensesAsync();
    }
}
