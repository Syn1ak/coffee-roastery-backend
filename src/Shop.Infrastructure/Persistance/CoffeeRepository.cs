using Microsoft.EntityFrameworkCore;
using Shop.Application.Catalog;
using Shop.Domain.Catalog;

namespace Shop.Infrastructure.Persistence;

public class CoffeeRepository : ICoffeeRepository
{
    private readonly ShopDb _db;

    public CoffeeRepository(ShopDb shopDb)
    {
        _db = shopDb;
    }

    public async Task<IReadOnlyList<Coffee>> ListPublishedAsync(CancellationToken ct)
    {
        return await _db.Coffees.AsNoTracking().Where(c => c.Status == CoffeeStatus.Published).ToListAsync(ct);
    }

    public async Task<Coffee?> GetByIdAsync(Guid id, CancellationToken ct) 
    {
        return await _db.Coffees.FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task UpdateAsync(Coffee coffee, CancellationToken ct)
    {
        try
        {
            var entry = _db.Entry(coffee).Property<int>("Version");
            entry.CurrentValue = entry.OriginalValue + 1; 

            await _db.SaveChangesAsync(ct);
        } catch (DbUpdateConcurrencyException)
        {
            throw new CoffeeConflictException(coffee.Id);
        }
    }
}