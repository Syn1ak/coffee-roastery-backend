
namespace Shop.Domain.Catalog;

public enum CoffeeStatus 
{ 
    Draft, 
    Published, 
    Retired 
}

public enum RoastStyle 
{ 
    Filter, 
    Espresso 
}

public enum StockMode 
{ 
    Shelf, 
    RoastToOrder
};

public record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0) throw new ArgumentException("Money can't be negative.");
        if (decimal.Round(amount, 2) != amount) throw new ArgumentException("Max 2 decimal places.");
        if (currency != "EUR") throw new ArgumentException("Only EUR is sold.");
        Amount = amount;
        Currency = currency;
    }
}

public enum BagSize { Grams250, Kilogram1 }
public record BagPrice(BagSize Size, Money Price);


public class Coffee
{
    public string Name { get; private set; }

    public string Origin { get;private set; }

    public RoastStyle RoastStyle { get; private set; }

    private readonly List<BagPrice> _prices = [];
    public IReadOnlyList<BagPrice> Prices => _prices;

    public CoffeeStatus Status { get; private set; } = CoffeeStatus.Draft;

    public StockMode StockMode { get; private set; }

    public Coffee(
        string name, 
        string origin,
        RoastStyle roastStyle,
        StockMode stockMode)
    {
        Name = name;
        Origin = origin;
        RoastStyle = roastStyle;
        StockMode = stockMode;
    }

    public void SetPrice(BagSize size, Money price)
    {
        if (Status == CoffeeStatus.Retired) throw new InvalidOperationException("Retired coffee can't change.");
        if (price.Amount == 0) throw new ArgumentException("A bag must cost something.");
        _prices.RemoveAll(p => p.Size == size);
        _prices.Add(new BagPrice(size, price));
    }

    public void Retire()
    {
        if (Status == CoffeeStatus.Retired)
            throw new InvalidOperationException("Already retired.");
        Status = CoffeeStatus.Retired;
    }

    public void Publish()
    {
        if (Status == CoffeeStatus.Retired)
            throw new InvalidOperationException("Retired coffee can't be published.");
        if (_prices.Count <= 0) 
            throw new InvalidOperationException("Coffee needs a price and bag size to be published.");
        Status = CoffeeStatus.Published;
    }
}