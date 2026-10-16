using Npgsql;
using Shop.Application.Catalog;
using Shop.Domain.Catalog;

namespace Shop.Infrastructure.Persistence;

public class CoffeeRepository : ICoffeeRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly NpgsqlUnitOfWork _npgsqlUnitOfWork;

    private readonly Dictionary<Guid, int> _loadedVersions = new ();
    private readonly Dictionary<Guid, List<BagPrice>> _loadedPrices = new ();

    public CoffeeRepository(NpgsqlDataSource dataSource, NpgsqlUnitOfWork npgsqlUnitOfWork)
    {
        _dataSource = dataSource;
        _npgsqlUnitOfWork = npgsqlUnitOfWork;
    }

    public async Task<IReadOnlyList<Coffee>> ListPublishedAsync(CancellationToken ct)
    {
        await using var cmd = _dataSource.CreateCommand("""
            SELECT c.id, c.name, c.origin, c.roast_style, c.stock_mode, c.status, cp.bag_size, cp.amount, cp.currency, c.version
            FROM coffees c 
            LEFT JOIN coffee_prices cp ON c.id = cp.coffee_id 
            WHERE c.status = 'Published'
            """);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await ReadCoffeesAsync(reader, ct);
    }

    public async Task<Coffee?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        await using var cmd = _dataSource.CreateCommand("""
        SELECT c.id, c.name, c.origin, c.roast_style, c.stock_mode, c.status, cp.bag_size, cp.amount, cp.currency, c.version
            FROM coffees c
            LEFT JOIN coffee_prices cp ON c.id = cp.coffee_id
            WHERE c.id = $1
        """);
        cmd.Parameters.Add(new NpgsqlParameter<Guid>() { TypedValue = id });
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        List<Coffee> coffees = await ReadCoffeesAsync(reader, ct);
        return coffees.FirstOrDefault();
    }

    public async Task UpdateAsync(Coffee coffee, CancellationToken ct)
    {
        if (!_loadedVersions.TryGetValue(coffee.Id, out int expectedVersion)) throw new InvalidOperationException($"Coffee {coffee.Id} was not loaded before saving.");
        if (!_loadedPrices.TryGetValue(coffee.Id, out List<BagPrice>? beforePrices)) throw new InvalidOperationException($"Coffee {coffee.Id} prices list missing.");
        await using var cmd = _npgsqlUnitOfWork.CreateCommand("""
        UPDATE coffees 
        SET status = $1, version = version + 1
        WHERE id = $2 AND version = $3
        """);
        cmd.Parameters.Add(new NpgsqlParameter<string> { TypedValue = coffee.Status.ToString() });
        cmd.Parameters.Add(new NpgsqlParameter<Guid> { TypedValue = coffee.Id });
        cmd.Parameters.Add(new NpgsqlParameter<int> { TypedValue = expectedVersion });
        int rows = await cmd.ExecuteNonQueryAsync(ct);
        if (rows != 1)
            throw new CoffeeConflictException(coffee.Id);

        IReadOnlyList<BagPrice> after = coffee.Prices;
        var added = after.Where(a => beforePrices.All(b => b.Size != a.Size));
        var removed = beforePrices.Where(b => after.All(a => a.Size != b.Size));
        var changed = after.Where(a => beforePrices.Any(b => b.Size == a.Size && b != a));

        foreach (BagPrice price in added)
        {
            await using var insert = _npgsqlUnitOfWork.CreateCommand("""
            INSERT INTO coffee_prices (coffee_id, bag_size, amount, currency)
            VALUES ($1, $2, $3, $4)
            """);
            insert.Parameters.Add(new NpgsqlParameter<Guid> { TypedValue = coffee.Id });
            insert.Parameters.Add(new NpgsqlParameter<string> { TypedValue = price.Size.ToString() });
            insert.Parameters.Add(new NpgsqlParameter<decimal> { TypedValue = price.Price.Amount });
            insert.Parameters.Add(new NpgsqlParameter<string> { TypedValue = price.Price.Currency });
            await insert.ExecuteNonQueryAsync(ct);
        }

        foreach (BagPrice price in changed)
        {
            await using var update = _npgsqlUnitOfWork.CreateCommand("""
            UPDATE coffee_prices SET amount = $1, currency = $2
            WHERE coffee_id = $3 AND bag_size = $4
            """);
            update.Parameters.Add(new NpgsqlParameter<decimal> { TypedValue = price.Price.Amount });
            update.Parameters.Add(new NpgsqlParameter<string> { TypedValue = price.Price.Currency });
            update.Parameters.Add(new NpgsqlParameter<Guid> { TypedValue = coffee.Id });
            update.Parameters.Add(new NpgsqlParameter<string> { TypedValue = price.Size.ToString() });
            await update.ExecuteNonQueryAsync(ct);
        }

        foreach (BagPrice price in removed)
        {
            await using var delete = _npgsqlUnitOfWork.CreateCommand("""
            DELETE FROM coffee_prices
            WHERE coffee_id = $1 AND bag_size = $2
            """);
            delete.Parameters.Add(new NpgsqlParameter<Guid> { TypedValue = coffee.Id });
            delete.Parameters.Add(new NpgsqlParameter<string> { TypedValue = price.Size.ToString() });
            await delete.ExecuteNonQueryAsync(ct);
        }

        _loadedVersions[coffee.Id] = expectedVersion + 1;
        _loadedPrices[coffee.Id] = coffee.Prices.ToList();
    }

    private async Task<List<Coffee>> ReadCoffeesAsync(NpgsqlDataReader reader, CancellationToken ct)
    {
        var coffeesById = new Dictionary<Guid, Coffee>();
        while (await reader.ReadAsync(ct))
        {
            var id = reader.GetGuid(0);
            if (!coffeesById.TryGetValue(id, out var coffee))
            {
                string name = reader.GetString(1);
                string origin = reader.GetString(2);
                RoastStyle roastStyle = Enum.Parse<RoastStyle>(reader.GetString(3));
                StockMode stockMode = Enum.Parse<StockMode>(reader.GetString(4));
                CoffeeStatus status = Enum.Parse<CoffeeStatus>(reader.GetString(5));
                coffee = new Coffee(
                    id,
                    name,
                    origin,
                    roastStyle,
                    stockMode,
                    status
                );
                coffeesById.Add(id, coffee);
            }
            if (!reader.IsDBNull(6))
            {
                BagSize bagSize = Enum.Parse<BagSize>(reader.GetString(6));
                Money money = new Money(reader.GetDecimal(7), reader.GetString(8));
                coffee.RestorePrice(bagSize, money);
            }
            _loadedVersions[coffee.Id] = reader.GetInt32(9);
        }

        foreach (Coffee coffee in coffeesById.Values)
            _loadedPrices[coffee.Id] = coffee.Prices.ToList();

        return coffeesById.Values.ToList();
    }
}