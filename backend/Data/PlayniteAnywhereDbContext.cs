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
}