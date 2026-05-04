using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Payhas.Binyat.Data.Postgres;
using Payhas.Binyat.Data.Postgres.Abstractions;
using Payhas.Binyat.Data.Postgres.Repositories;
using Payhas.Binyat.Data.Sqlite.HybridRepositories;
using Payhas.Binyat.Data.Sqlite.Repositories;
using Payhas.Binyat.Data.Sqlite.Sync;

namespace Payhas.Binyat.Data.Sqlite;

/// <summary>
/// Composition root for the Stage-3 hybrid (online + offline) data layer.
///
/// What this registration does:
///  1. Creates / opens the local SQLite database.
///  2. Registers the PostgreSQL repositories under keyed names so the
///     hybrid wrappers can resolve them while the UI sees only the
///     hybrid <c>I*Repository</c> interfaces.
///  3. Starts the <see cref="SyncService"/> background worker which
///     bidirectionally replicates local changes ↔ central server.
/// </summary>
public static class SqliteServiceCollectionExtensions
{
    public static IServiceCollection AddPayhasHybridStorage(
        this IServiceCollection services,
        string sqliteConnectionString,
        string postgresConnectionString,
        Action<SyncOptions>? configureSync = null)
    {
        // Schema bootstrap (idempotent).
        services.AddSingleton(_ => new SqliteSchemaManager(sqliteConnectionString));

        // Connectivity probe (Npgsql ping with 5s caching).
        services.AddSingleton<IConnectivityProbe>(_ => new NpgsqlConnectivityProbe(postgresConnectionString));

        // PostgreSQL data layer (DbContext + Pg* repositories).
        services.AddPayhasPostgres(postgresConnectionString);

        // We need both Pg* (online) and Sqlite* (local) implementations available.
        // The Pg* are already registered as IStocksRepository/IInvoicesRepository
        // by AddPayhasPostgres. We replace them with the Hybrid wrappers and
        // resolve the Pg implementations via dedicated factory keys.

        // Local SQLite repositories.
        services.AddSingleton(_ => new SqliteStocksRepository(sqliteConnectionString));
        services.AddSingleton(_ => new SqliteInvoicesRepository(sqliteConnectionString));
        services.AddSingleton(_ => new SqliteStockBalancesRepository(sqliteConnectionString));

        // Replace IStocksRepository / IInvoicesRepository with hybrid wrappers.
        // The hybrid wrappers depend on PgStocksRepository / PgInvoicesRepository
        // directly (which AddPayhasPostgres already registered as scoped services).
        services.Replace(ServiceDescriptor.Scoped<IStocksRepository>(sp =>
            new HybridStocksRepository(
                sp.GetRequiredService<PgStocksRepository>(),
                sp.GetRequiredService<SqliteStocksRepository>(),
                sp.GetRequiredService<IConnectivityProbe>())));

        services.Replace(ServiceDescriptor.Scoped<IInvoicesRepository>(sp =>
            new HybridInvoicesRepository(
                sp.GetRequiredService<PgInvoicesRepository>(),
                sp.GetRequiredService<SqliteInvoicesRepository>(),
                sp.GetRequiredService<IConnectivityProbe>())));

        // PgStocksRepository / PgInvoicesRepository need to be resolvable
        // by their concrete type (the AddPayhasPostgres registration only
        // exposes the interface). Add the missing concrete registrations.
        services.AddScoped(sp => new PgStocksRepository(
            sp.GetRequiredService<PayhasDbContext>(), postgresConnectionString));
        services.AddScoped(sp => new PgInvoicesRepository(
            sp.GetRequiredService<PayhasDbContext>(), postgresConnectionString));

        // Background sync.
        var options = new SyncOptions();
        configureSync?.Invoke(options);
        services.AddSingleton(options);

        services.AddHostedService(sp => new SyncService(
            sp.GetRequiredService<SqliteSchemaManager>(),
            sp.GetRequiredService<IConnectivityProbe>(),
            sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SyncService>>(),
            sp.GetRequiredService<SyncOptions>(),
            sqliteConnectionString,
            postgresConnectionString,
            sp.GetRequiredService<PgInvoicesRepository>(),
            sp.GetRequiredService<PgStocksRepository>()));

        return services;
    }
}
