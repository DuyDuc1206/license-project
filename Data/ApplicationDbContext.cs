using licensePemoseServer.Models;
using Microsoft.EntityFrameworkCore;


namespace licensePemoseServer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<License> Licenses { get; set; }
    }
}
