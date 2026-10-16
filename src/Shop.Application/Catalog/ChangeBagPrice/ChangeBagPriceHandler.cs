using Shop.Domain.Catalog;

namespace Shop.Application.Catalog.ChangeBagPrice;

public class ChangeBagPriceHandler(ICoffeeRepository coffeeRepository, IUnitOfWork unitOfWork)
{
    public async Task<bool> HandleAsync(Guid id, BagSize size, decimal amount, CancellationToken ct)
    {
        Coffee? coffee = await coffeeRepository.GetByIdAsync(id, ct);
        if (coffee is null) return false;

        Money money = new Money(amount, "EUR");
        coffee.SetPrice(size, money);

        await unitOfWork.BeginAsync(ct);
        await coffeeRepository.UpdateAsync(coffee, ct);
        await unitOfWork.CommitAsync(ct);
        return true;
    }
}