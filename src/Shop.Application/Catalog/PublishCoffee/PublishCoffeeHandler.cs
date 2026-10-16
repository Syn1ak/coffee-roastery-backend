using Shop.Domain.Catalog;

namespace Shop.Application.Catalog.PublishCoffee;

public class PublishCoffeeHandler(ICoffeeRepository repository, IUnitOfWork unitOfWork)
{
    public async Task<bool> HandleAsync(Guid id, CancellationToken ct)
    {
        Coffee? coffee = await repository.GetByIdAsync(id, ct);
        if (coffee is null) return false;

        coffee.Publish();

        await unitOfWork.BeginAsync(ct);
        await repository.UpdateAsync(coffee, ct);
        await unitOfWork.CommitAsync(ct);
        return true;
    }
}
