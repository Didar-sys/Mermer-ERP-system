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

        // Register Dapper-based search service (raw SQL for max performance)
        services.AddSingleton<IPgStockSearchService>(
            _ => new PgStockSearchService(connectionString));

        return services;
    }
}
