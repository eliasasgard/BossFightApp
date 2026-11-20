
using HistoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace HistoryService.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<HistoryEntry> Entries => Set<HistoryEntry>();

    protected override void OnModelCreating(ModelBuilder b) =>
        b.Entity<HistoryEntry>().HasIndex(e => e.CharacterId);
}
