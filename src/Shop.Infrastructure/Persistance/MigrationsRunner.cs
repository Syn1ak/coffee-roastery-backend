using Npgsql;

namespace Shop.Infrastructure.Persistence;

public static class MigrationsRunner
{
    public static async Task RunMigrations(NpgsqlDataSource dataSource, CancellationToken ct)
    {
        string dir = Path.Combine(AppContext.BaseDirectory, "Persistance", "Sql");
        string[] files = Directory.GetFiles(dir, "*.sql");
        Array.Sort(files, StringComparer.Ordinal);
        // create log table if not existed
        await using var cmdCreateLogs = dataSource.CreateCommand("""
        CREATE TABLE IF NOT EXISTS migrations_logs (
            file_name text PRIMARY KEY
        )
        """);
        await cmdCreateLogs.ExecuteNonQueryAsync(ct);

        foreach (string filePath in files)
        {
            string fileName = Path.GetFileName(filePath);
            // check if filename present on log table 
            await using var cmdMigrationExisted = dataSource.CreateCommand("""
            SELECT EXISTS (
                SELECT 1 FROM migrations_logs WHERE file_name = $1
            )
            """);
            cmdMigrationExisted.Parameters.Add(new() {Value = fileName});
            object? found = await cmdMigrationExisted.ExecuteScalarAsync(ct);
            bool alreadyApplied = found is true;

            // if yes nothing happens continue
            if (alreadyApplied) continue;

            // if not exist apply sql and insert to log table in one transaction
            string sql = await File.ReadAllTextAsync(filePath, ct);
            await using var conn = await dataSource.OpenConnectionAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);
            // apply migration
            await using var applyMigration = new NpgsqlCommand(sql, conn, tx);
            await applyMigration.ExecuteNonQueryAsync(ct);

            await using var logApplied = new NpgsqlCommand("""
                INSERT INTO migrations_logs (file_name) VALUES ($1)
            """, conn, tx);
            logApplied.Parameters.Add(new () {Value = fileName});
            await logApplied.ExecuteNonQueryAsync(ct);


            await tx.CommitAsync(ct);

        }
    }
}