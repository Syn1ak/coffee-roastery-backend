using Microsoft.EntityFrameworkCore;
using Shop.Domain.Catalog;

namespace Shop.Infrastructure.Persistence;

public class ShopDb(DbContextOptions<ShopDb> options) : DbContext(options)
{
    public DbSet<Coffee> Coffees => Set<Coffee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShopDb).Assembly);
    }
}