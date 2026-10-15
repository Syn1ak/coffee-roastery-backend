using Shop.Domain.Catalog;

namespace Shop.Application.Catalog;

public interface ICoffeeRepository
{
    Task<IReadOnlyList<Coffee>> ListPublishedAsync(CancellationToken ct);

    // Read only. No transaction needed, no lock taken.
    Task<Coffee?> GetByIdAsync(Guid id, CancellationToken ct);

    // Read and lock the row until the transaction ends. Requires IUnitOfWork.BeginAsync first.
    Task<Coffee?> GetByIdForUpdateAsync(Guid id, CancellationToken ct);

    Task UpdateAsync(Coffee coffee, CancellationToken ct);
}