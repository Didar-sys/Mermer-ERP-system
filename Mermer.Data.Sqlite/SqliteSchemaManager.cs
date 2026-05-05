using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Mermer.Data.Sqlite;

/// <summary>
/// Creates the local SQLite database from <c>Scripts/001_create_database.sql</c>
/// the first time we connect. Idempotent — every <c>CREATE TABLE</c> uses
/// <c>IF NOT EXISTS</c>.
/// </summary>
public sealed class SqliteSchemaManager
{
    private readonly string _connectionString;

    public SqliteSchemaManager(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task EnsureCreatedAsync(CancellationToken ct = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(ct);

        // PRAGMAs are connection-scoped — set them on every connection that
        // touches the DB; here we just enable foreign keys before DDL.
        await using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA foreign_keys = ON;";
            await pragma.ExecuteNonQueryAsync(ct);
        }

        var ddl = await LoadEmbeddedScriptAsync("001_create_database.sql", ct);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = ddl;
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task<string> LoadEmbeddedScriptAsync(string name, CancellationToken ct)
    {
        // Try the embedded resource first (release deployment).
        var assembly = typeof(SqliteSchemaManager).Assembly;
        var resourceName = $"Mermer.Data.Sqlite.Scripts.{name}";
        await using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync(ct);
        }

        // Fall back to the on-disk script (dev / unit-test runs).
        var probePath = Path.Combine(
            Path.GetDirectoryName(assembly.Location)!,
            "Scripts", name);
        if (File.Exists(probePath))
            return await File.ReadAllTextAsync(probePath, ct);

        throw new FileNotFoundException(
            $"Could not locate SQLite schema script '{name}' (embedded or on-disk).");
    }
}
