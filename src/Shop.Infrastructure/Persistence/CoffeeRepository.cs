using Npgsql;
using Shop.Application.Catalog;
using Shop.Domain.Catalog;

namespace Shop.Infrastructure.Persistence;

public class CoffeeRepository : ICoffeeRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public CoffeeRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<IReadOnlyList<Coffee>> ListPublishedAsync(CancellationToken ct)
    {
        await using var cmd = _dataSource.CreateCommand("""
            SELECT c.id, c.name, c.origin, c.roast_style, c.stock_mode, c.status, cp.bag_size, cp.amount, cp.currency 
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
        SELECT c.id, c.name, c.origin, c.roast_style, c.stock_mode, c.status, cp.bag_size, cp.amount, cp.currency 
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
        await using var cmd = _dataSource.CreateCommand("UPDATE coffees SET status = $1 WHERE id = $2");
        cmd.Parameters.Add(new NpgsqlParameter<string> { TypedValue = coffee.Status.ToString() });
        cmd.Parameters.Add(new NpgsqlParameter<Guid> { TypedValue = coffee.Id });
        int rows = await cmd.ExecuteNonQueryAsync(ct);
        if (rows != 1)
            throw new InvalidOperationException($"Expected to update 1 coffee row for {coffee.Id}, updated {rows}.");
    }

    private static async Task<List<Coffee>> ReadCoffeesAsync(NpgsqlDataReader reader, CancellationToken ct)
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
        }
        return coffeesById.Values.ToList();
    }
}