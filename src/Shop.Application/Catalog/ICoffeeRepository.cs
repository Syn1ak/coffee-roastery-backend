using Shop.Domain.Catalog;

namespace Shop.Application.Catalog;

public interface ICoffeeRepository
{
    Task<IReadOnlyList<Coffee>> ListPublishedAsync(CancellationToken ct);

    Task<Coffee?> GetByIdAsync(Guid id, CancellationToken ct);

    Task UpdateAsync(Coffee coffee, CancellationToken ct);
}