using Npgsql;
using NpgsqlTypes;
using Shop.Application.Catalog;
using Shop.Domain.Catalog;

namespace Shop.Infrastructure.Persistence;

public class CoffeeRepository : ICoffeeRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly NpgsqlUnitOfWork _npgsqlUnitOfWork;

    private readonly Dictionary<Guid, uint> _loadedVersions = new ();

    public CoffeeRepository(NpgsqlDataSource dataSource, NpgsqlUnitOfWork npgsqlUnitOfWork)
    {
        _dataSource = dataSource;
        _npgsqlUnitOfWork = npgsqlUnitOfWork;
    }

    public async Task<IReadOnlyList<Coffee>> ListPublishedAsync(CancellationToken ct)
    {
        await using var cmd = _dataSource.CreateCommand("""
            SELECT c.id, c.name, c.origin, c.roast_style, c.stock_mode, c.status, cp.bag_size, cp.amount, cp.currency, c.xmin
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
        SELECT c.id, c.name, c.origin, c.roast_style, c.stock_mode, c.status, cp.bag_size, cp.amount, cp.currency, c.xmin
            FROM coffees c
            LEFT JOIN coffee_prices cp ON c.id = cp.coffee_id
            WHERE c.id = $1
        """);
        cmd.Parameters.Add(new NpgsqlParameter<Guid>() { TypedValue = id });
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        List<Coffee> coffees = await ReadCoffeesAsync(reader, ct);
        return coffees.FirstOrDefault();
    }

    public async Task<Coffee?> GetByIdForUpdateAsync(Guid id, CancellationToken ct)
    {
        await using var cmd = _npgsqlUnitOfWork.CreateCommand("""
        SELECT c.id, c.name, c.origin, c.roast_style, c.stock_mode, c.status, cp.bag_size, cp.amount, cp.currency, c.xmin
            FROM coffees c
            LEFT JOIN coffee_prices cp ON c.id = cp.coffee_id
            WHERE c.id = $1
            FOR UPDATE OF c
        """);
        cmd.Parameters.Add(new NpgsqlParameter<Guid>() { TypedValue = id });
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        List<Coffee> coffees = await ReadCoffeesAsync(reader, ct);
        return coffees.FirstOrDefault();
    }

    public async Task UpdateAsync(Coffee coffee, CancellationToken ct)
    {
        if (!_loadedVersions.TryGetValue(coffee.Id, out uint expectedVersion)) throw new InvalidOperationException($"Coffee {coffee.Id} was not loaded before saving.");
        // Postgres writes a new row version on every UPDATE, so xmin changes by itself.
        // RETURNING hands back the new xmin; no row back means the WHERE did not match.
        await using var cmd = _npgsqlUnitOfWork.CreateCommand("""
        UPDATE coffees
        SET status = $1
        WHERE id = $2 AND xmin = $3
        RETURNING xmin
        """);
        cmd.Parameters.Add(new NpgsqlParameter<string> { TypedValue = coffee.Status.ToString() });
        cmd.Parameters.Add(new NpgsqlParameter<Guid> { TypedValue = coffee.Id });
        cmd.Parameters.Add(new NpgsqlParameter<uint> { TypedValue = expectedVersion, NpgsqlDbType = NpgsqlDbType.Xid });
        object? newVersion = await cmd.ExecuteScalarAsync(ct);
        if (newVersion is null)
            throw new CoffeeConflictException(coffee.Id);
        _loadedVersions[coffee.Id] = (uint)newVersion;
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
            _loadedVersions[coffee.Id] = reader.GetFieldValue<uint>(9);
        }
        return coffeesById.Values.ToList();
    }
}