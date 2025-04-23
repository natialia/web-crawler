using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebCrawler.Models;

namespace WebCrawler.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<SearchHistory> SearchHistories { get; set; }
        public DbSet<PdfDocument> Pdfs { get; set; }
        public DbSet<WordStat> WordStats { get; set; }
    }
}
