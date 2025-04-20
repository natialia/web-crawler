using Microsoft.EntityFrameworkCore;
using WebCrawler.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<SearchHistory> SearchHistories => Set<SearchHistory>();
    public DbSet<FoundPdf> FoundPdfs => Set<FoundPdf>();
}

