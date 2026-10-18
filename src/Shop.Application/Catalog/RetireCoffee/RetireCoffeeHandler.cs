using Shop.Domain.Catalog;

namespace Shop.Application.Catalog.RetireCoffee;

public class RetireCoffeeHandler(ICoffeeRepository coffeeRepository)
{
    public async Task<bool> HandleAsync(Guid id, CancellationToken ct)
    {
        Coffee? coffee = await coffeeRepository.GetByIdAsync(id, ct);
        if (coffee is null) return false;

        coffee.Retire();

        await coffeeRepository.UpdateAsync(coffee, ct);
        return true;
    }
}