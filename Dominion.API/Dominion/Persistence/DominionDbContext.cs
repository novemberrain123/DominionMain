using Microsoft.EntityFrameworkCore;

namespace Dominion.API.Dominion.Persistance;

public class DominionDbContext : DbContext
{
    public DominionDbContext(DbContextOptions<DominionDbContext> options)
        : base(options)
    {
    }

    public DbSet<GameEntity> Games => Set<GameEntity>();
}