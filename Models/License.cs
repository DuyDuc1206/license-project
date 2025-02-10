using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace licensePemoseServer.Models
{
    public class License
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string LicenseKey { get; set; }
        public string MachineId { get; set; }
        public string MachineName { get; set; }
        public string Status { get; set; } = "Enable";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiryDate { get; set; }


        // Khóa ngoại tham chiếu đến plugin
        [Required]
        public int PluginId { get; set; }
        public Plugin? Plugin { get; set; }
    }
}
