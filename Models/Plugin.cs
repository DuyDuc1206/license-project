using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace licensePemoseServer.Models
{
    public class Plugin
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PluginId { get; set; }

        public string PluginName { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }

        // Thêm danh sách License liên kết với Plugin này
        public List<License>? Licenses { get; set; } = new List<License>();
    }
}
