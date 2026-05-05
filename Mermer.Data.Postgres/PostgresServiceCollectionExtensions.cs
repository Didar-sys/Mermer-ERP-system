using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mermer.Data.Postgres.Abstractions;
using Mermer.Data.Postgres.Repositories;

namespace Mermer.Data.Postgres;

/// <summary>
/// Extension methods for registering the PostgreSQL data layer in a DI
/// container. The layer is fully decoupled from the legacy WPF
/// (.NET Framework 4.6.1) projects — it works with the POCOs and
/// interfaces in <see cref="Abstractions"/> and <c>Models</c>.
/// </summary>
public static class PostgresServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="MermerDbContext"/> and all PostgreSQL services.
    /// Call once at application startup:
    /// <code>services.AddMermerPostgres(connectionString);</code>
    /// </summary>
    public static IServiceCollection AddMermerPostgres(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<MermerDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);

                npgsqlOptions.CommandTimeout(30);
            });
        });

        // Stage 1 — fuzzy search (pg_trgm + tsvector, single round-trip)
        services.AddSingleton<IStockSearchService>(
            _ => new PgStockSearchService(connectionString));

        // Stage 2 — domain repository adapters
        services.AddScoped<IStocksRepository>(sp =>
            new PgStocksRepository(sp.GetRequiredService<MermerDbContext>(), connectionString));

        services.AddScoped<IInvoicesRepository>(sp =>
            new PgInvoicesRepository(sp.GetRequiredService<MermerDbContext>(), connectionString));

        services.AddScoped<IStockBalancesRepository>(
            _ => new PgStockBalancesRepository(connectionString));

        return services;
    }
}
