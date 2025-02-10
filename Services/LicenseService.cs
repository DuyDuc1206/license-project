using licensePemoseServer.Data;
using licensePemoseServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LicenseManagerCloud.Services
{
    public class LicenseService : ILicenseService
    {
        private readonly ApplicationDbContext _context;

        public LicenseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<License> CreateLicenseAsync(string licensekey, string machineId, string machineName, string status, DateTime expicyDate, int pluginId)
        {
            DateTime ExpicydateTime = DateTime.SpecifyKind(expicyDate, DateTimeKind.Utc);
            var plugin = await _context.Set<Plugin>().FirstOrDefaultAsync(p => p.PluginId == pluginId);
            if (plugin == null)
            {
                throw new Exception("Plugin not found.");
            }

            var license = new License
            {
                LicenseKey = licensekey,
                MachineId = Base64Encode(machineId),
                MachineName = Base64Encode(machineName),
                Status = status,
                ExpiryDate = ExpicydateTime,
                PluginId = pluginId
            };
            _context.Add(license);
            await _context.SaveChangesAsync();

            return license;
        }

        public async Task<License> GetLicenseByMachineIdAsync(string licenseKey)
        {
            //var decodeMachineId = Base64Decode(machineId);
            return await _context.Set<License>().FirstOrDefaultAsync(l => l.LicenseKey == licenseKey);
        }

        public async Task<string> GetTokenByLicenseAsync(string licensekey)
        {
            var licenseData = Encoding.UTF8.GetString(Convert.FromBase64String(licensekey));
            var licenseParts = licenseData.Split('|');
            if (licenseParts.Length != 3)
            {
                return null;  // Dữ liệu không hợp lệ
            }

            var machineId = licenseParts[0];
            var machineName = licenseParts[1];
            var plugin = licenseParts[2];

            var dbContext = await _context.Set<License>().FirstOrDefaultAsync(l => l.LicenseKey == licensekey);
            if (dbContext == null || dbContext.Status != "Enable")
            {
                return null;
            }
            var token = GenerateToken(dbContext.MachineId, dbContext.MachineName, dbContext.ExpiryDate);
            if (token != null)
            {
                Console.WriteLine("Generated JWT Token: " + token);
                return token;
            }
            return null;
        }

        public async Task<bool> UpdateLicenseStatusAsync(string licenseKey, string status)
        {
            if (status != "Enable" && status != "Disable")
            {
                return false;
            }
            var license = await _context.Set<License>().FirstOrDefaultAsync(l => l.LicenseKey == licenseKey);
            if (license == null)
            {
                return false;
            }
            license.Status = status;
            _context.Update(license);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<License>> GetAllLicensesAsync()
        {
            return await _context.Set<License>().ToListAsync();
        }

        private string GenerateToken(string machineId,string machineName, DateTime expiryDate)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var rsa = GetRsaPrivateKey();  // Bạn cần sử dụng private key để ký token
            var key = new RsaSecurityKey(rsa);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("DeviceID", machineId),
                    new Claim("DeviceName", machineName),
                    new Claim("ExpiryTime", expiryDate.ToUniversalTime().ToString("o"))
                }),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            Console.WriteLine("JWT Token: " + token);
            return tokenHandler.WriteToken(token);
        }
        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private RSA GetRsaPrivateKey()
        {
            RSA rsa = new RSACryptoServiceProvider();
            string privateKeyXml = File.ReadAllText("privateKey.xml");
            rsa.FromXmlString(privateKeyXml);
            return rsa;
        }
        static string ExportPrivateKeyToPem(RSACryptoServiceProvider rsa)
        {
            var privateKeyBytes = rsa.ExportRSAPrivateKey();
            return "-----BEGIN PRIVATE KEY-----\n" +
                   Convert.ToBase64String(privateKeyBytes, Base64FormattingOptions.InsertLineBreaks) +
                   "\n-----END PRIVATE KEY-----";
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
