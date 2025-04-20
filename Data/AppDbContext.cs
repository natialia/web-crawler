using Microsoft.EntityFrameworkCore;

namespace WebCrawler.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Example DbSet (can be empty for now)
        public DbSet<Dummy> Dummies { get; set; }
    }

    public class Dummy
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }
}
