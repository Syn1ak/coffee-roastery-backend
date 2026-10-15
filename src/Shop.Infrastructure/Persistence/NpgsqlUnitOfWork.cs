using Npgsql;
using Shop.Application;

namespace Shop.Infrastructure.Persistence;

public class NpgsqlUnitOfWork(NpgsqlDataSource dataSource) : IUnitOfWork, IAsyncDisposable
{
    private NpgsqlConnection? _connection;
    private NpgsqlTransaction? _transaction;

    public async Task BeginAsync(CancellationToken ct)
    {
        if (_transaction is not null) throw new InvalidOperationException("Begin was already called");
        _connection = await dataSource.OpenConnectionAsync(ct);
        _transaction = await _connection.BeginTransactionAsync(ct);
    }

    public async Task CommitAsync(CancellationToken ct)
    {
        if (_transaction == null) throw new InvalidOperationException("Begin wasn't called");
        await _transaction.CommitAsync(ct);
    }

    public NpgsqlCommand CreateCommand(string sql)
    {
        if (_connection == null || _transaction == null)
            throw new InvalidOperationException("Begin wasn't called");
        return new NpgsqlCommand(sql, _connection, _transaction);
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null) await _transaction.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
    }
}