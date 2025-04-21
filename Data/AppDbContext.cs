using Microsoft.EntityFrameworkCore;
using WebCrawler.Models;

namespace WebCrawler.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<SearchHistory> SearchHistories { get; set; }
        public DbSet<FoundPdf> FoundPdfs => Set<FoundPdf>();
    }
}

