using Microsoft.EntityFrameworkCore;
using PlayniteAnywhere.Backend.Models;

namespace PlayniteAnywhere.Backend.Data;

public class PlayniteAnywhereDbContext : DbContext
{
    public PlayniteAnywhereDbContext(
        DbContextOptions<PlayniteAnywhereDbContext> options)
        : base(options)
    {
    }

    public DbSet<Game> Games => Set<Game>();
    public DbSet<Preferences> Preferences => Set<Preferences>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Preferences>()
            .Property(p => p.CollapsedGroups)
            .HasColumnType("jsonb");
    }
}