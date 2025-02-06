
using LicenseManagerCloud.Models;
using LicenseManagerCloud.Services;
using licensePemoseServer.Models;
using Microsoft.AspNetCore.Mvc;

namespace LicenseManagerCloud.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LicenseController : ControllerBase
    {
        private readonly ILicenseService _licenseService;

        public LicenseController(ILicenseService license)
        {
            _licenseService = license;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateLicense([FromBody] License request)
        {
            if (string.IsNullOrEmpty(request.LicenseKey) || string.IsNullOrEmpty(request.MachineId))
            {
                return BadRequest("License Key and Device ID are required.");
            }

            var existLicenseKey = await _licenseService.GetLicenseByMachineIdAsync(request.MachineId);
            if (existLicenseKey != null)
            {
                return Conflict("A license for this machine already exists.");
            }

            var license = await _licenseService.CreateLicenseAsync(
            request.LicenseKey,
            request.MachineId,
            request.MachineName,
            request.Status ?? "Enable",
            request.ExpiryDate);

            return CreatedAtAction(nameof(GetLicense), new { id = license.Id }, license);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLicense(int id)
        {
            var license = await _licenseService.GetLicenseByMachineIdAsync(id.ToString());
            if (license == null)
            {
                return NotFound();
            }

            return Ok(license);
        }

        [HttpPost("GetToken")]
        public async Task<IActionResult> GetToken([FromBody] GetTokenRequest request)
        {

            if (string.IsNullOrEmpty(request.LicenseKey))
            {
                return BadRequest("License Key is required.");
            }

            //var token = await _licenseService.GetLicenseByMachineIdAsync(request.LicenseKey);
            var token = await _licenseService.GetTokenByLicenseAsync(request.LicenseKey);

            if (token == null)
            {
                return BadRequest("Your license is invalid.");
            }

            return Ok(token);
        }

        //[HttpPut("UpdateStatus")]
        //public async Task<IActionResult> UpdateLicenseStatus([FromBody] UpdateLicenseStatusRequest request)
        //{
        //    var result = await _licenseService.UpdateLicenseStatusAsync(request.LicenseKey, request.Status);

        //    if (!result)
        //    {
        //        return NotFound(new { Message = "License not found or invalid status" });
        //    }

        //    return Ok(new { Message = "License status updated successfully" });
        //}

        [HttpGet("GetAllLicense")]
        public async Task<IActionResult> GetAllLicense()
        {
            var licenses = await _licenseService.GetAllLicensesAsync();

            if (licenses == null || !licenses.Any())
            {
                return NotFound(new { Message = "No licenses found" });
            }

            return Ok(licenses);
        }

        [HttpGet("Validate")]
        public IActionResult ValidateLicense()
        {
            return Ok(new { Message = "Token is valid" });
        }
    }

}