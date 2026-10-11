using Shop.Domain.Catalog;

namespace Shop.Application.Catalog;

public interface ICoffeeRepository
{
    Task<IReadOnlyList<Coffee>> ListPublishedAsync(CancellationToken ct);
}