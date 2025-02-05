
using licensePemoseServer.Data;
using licensePemoseServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace LicenseManagerCloud.Services
{
    public class LicenseService : ILicenseService
    {
        private readonly ApplicationDbContext _context;

        ////private readonly string privatePem = System.IO.File.ReadAllText("private_key.pem");
        //private readonly string _issuer = "licensemanagerapiapp";
        //private readonly string _audience = "licensemanagerapi";

        public LicenseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<License> CreateLicenseAsync(string licensekey, string machineId, string machineName, string status, DateTime expicyDate)
        {
            DateTime ExpicydateTime = DateTime.SpecifyKind(expicyDate, DateTimeKind.Utc);

            var license = new License
            {
                Id = 0,
                LicenseKey = licensekey,
                MachineId = Base64Encode(machineId),
                MachineName = Base64Encode(machineName),
                Status = status,
                //CreatedAt = CreateAtDateTime,
                ExpiryDate = ExpicydateTime
            };
            _context.Add(license);
            await _context.SaveChangesAsync();

            return license;
        }

        public async Task<License> GetLicenseByMachineIdAsync(string machineId)
        {
            //var decodeMachineId = Base64Decode(machineId);
            return await _context.Set<License>().FirstOrDefaultAsync(l => l.MachineId == machineId);
        }

        public async Task<string> GetTokenByLicenseAsync(string licensekey)
        {
            var license = await _context.Set<License>().FirstOrDefaultAsync(l => l.LicenseKey == licensekey);
            if (license == null || license.Status != "Enable" || license.ExpiryDate <= DateTime.UtcNow)
            {
                return null;
            }
            var token = GenerateToken(license.MachineId, license.MachineName, license.ExpiryDate);
            if (token != null)
            {
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
            var rsa = GetRsaPrivateKey();
            var key = new RsaSecurityKey(rsa);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("DeviceID", machineId),
                    new Claim("DeviceName", machineName),
                    new Claim("ExpiryTime", expiryDate.ToString("yyyy-MM-ddTHH:mm:ss"))
                }),
                //Expires = DateTime.UtcNow.AddDays(30), // Đặt thời gian hết hạn
                //NotBefore = DateTime.UtcNow,           // Token có hiệu lực ngay lập tức
                //IssuedAt = DateTime.UtcNow,            // Thời điểm phát hành token
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private RSA GetRsaPrivateKey()
        {
            //RSA rSA = RSA.Create();
            //rSA.ImportFromEncryptedPem(privatePem.ToString(), "");
            RSA rsa = new RSACryptoServiceProvider();
            string privateKeyXml = File.ReadAllText("privateKey.xml");
            rsa.FromXmlString(privateKeyXml);
            //RSA pemPrivateKey = ExportPrivateKeyToPem(rsa);
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
