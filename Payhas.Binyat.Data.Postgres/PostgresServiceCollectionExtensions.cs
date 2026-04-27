using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Payhas.Binyat.Data.Postgres.Repositories;

namespace Payhas.Binyat.Data.Postgres;

/// <summary>
/// Extension methods for registering PostgreSQL data layer services in DI container.
/// </summary>
public static class PostgresServiceCollectionExtensions
{
    /// <summary>
    /// Registers PayhasDbContext and all PostgreSQL repository services.
    /// Call once at application startup: services.AddPayhasPostgres(connectionString)
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="connectionString">PostgreSQL connection string</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddPayhasPostgres(
        this IServiceCollection services,
        string connectionString)
    {
        // Register EF Core DbContext with Npgsql
        services.AddDbContext<PayhasDbContext>(options =>
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

        // ── Stage 1: Search engine (Dapper + pg_trgm) ────────────────────────
        // High-performance fuzzy search — replaces 3 Couchbase round-trips with 1 SQL query
        services.AddSingleton<IPgStockSearchService>(
            _ => new PgStockSearchService(connectionString));

        // ── Stage 2: Domain repository adapters ──────────────────────────────

        // Stocks — replaces Couchbase StocksRepository
        services.AddScoped<Payhas.Binyat.StockManagement.Services.IStocksRepository>(sp =>
            new PgStocksRepository(
                sp.GetRequiredService<PayhasDbContext>(),
                connectionString));

        // Invoices — replaces Couchbase InvoicesRepository
        // Financial totals calculated in SQL NUMERIC(18,4) — fixes race conditions and precision bugs
        services.AddScoped<Payhas.Binyat.Commerce.Services.IInvoicesRepository>(sp =>
            new PgInvoicesRepository(
                sp.GetRequiredService<PayhasDbContext>(),
                connectionString));

        // Stock Balances — replaces 5 Couchbase Map/Reduce views atomically
        services.AddScoped<Payhas.Binyat.StockManagement.Services.IStockBalancesRepository>(
            _ => new PgStockBalancesRepository(connectionString));

        return services;
    }
}
