using System.ComponentModel.DataAnnotations;

namespace licensePemoseServer.Models
{
    public class License
    {
        [Key]
        public int Id { get; set; }
        public string LicenseKey { get; set; }
        public string MachineId { get; set; }
        public string MachineName { get; set; }
        public string Status { get; set; } = "Enable";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiryDate { get; set; }
    }
}
