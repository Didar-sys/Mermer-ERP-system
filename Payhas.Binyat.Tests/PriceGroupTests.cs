using FluentAssertions;
using Microsoft.Data.Sqlite;
using Payhas.Binyat.Data.Sqlite;
using Payhas.Binyat.Data.Sqlite.Repositories;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Payhas.Binyat.Tests;

/// <summary>
/// Verifies that the Stock Balances report honors the <c>priceGroup</c>
/// selector — clients can ask for "wholesale" / "retail" / etc. and get
/// the matching price column. Null = default (no-group) price.
/// </summary>
public sealed class PriceGroupTests : IAsyncLifetime
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(),
        $"payhas-pricegroup-tests-{Guid.NewGuid():N}.sqlite");
    private string ConnectionString => $"Data Source={_dbPath}";

    private static readonly string OfficeId    = Guid.NewGuid().ToString();
    private static readonly string WarehouseId = Guid.NewGuid().ToString();
    private static readonly string StockId     = Guid.NewGuid().ToString();

    public async Task InitializeAsync()
    {
        await new SqliteSchemaManager(ConnectionString).EnsureCreatedAsync();

        await using var conn = new SqliteConnection(ConnectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO offices    (id, name) VALUES ($oid, 'O');
            INSERT INTO warehouses (id, office_id, name) VALUES ($wid, $oid, 'W');
            INSERT INTO stocks     (id, name) VALUES ($sid, 'YD 209 nude');

            -- Three price entries: default, wholesale, retail.
            INSERT INTO stock_prices (id, stock_id, price, price_group, valid_from)
                VALUES ($pid1, $sid, 10.00, NULL,        '2026-01-01'),
                       ($pid2, $sid, 7.00,  'wholesale', '2026-01-01'),
                       ($pid3, $sid, 15.00, 'retail',    '2026-01-01');

            -- A balance row so the GROUP BY returns this stock.
            INSERT INTO stock_balances (warehouse_id, stock_id, income, expense)
                VALUES ($wid, $sid, 10, 0);
        ";
        cmd.Parameters.AddWithValue("$oid",  OfficeId);
        cmd.Parameters.AddWithValue("$wid",  WarehouseId);
        cmd.Parameters.AddWithValue("$sid",  StockId);
        cmd.Parameters.AddWithValue("$pid1", Guid.NewGuid().ToString());
        cmd.Parameters.AddWithValue("$pid2", Guid.NewGuid().ToString());
        cmd.Parameters.AddWithValue("$pid3", Guid.NewGuid().ToString());
        await cmd.ExecuteNonQueryAsync();
    }

    public Task DisposeAsync()
    {
        SqliteConnection.ClearAllPools();
        try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { /* tmp leak */ }
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Default_price_group_returns_no_group_price()
    {
        var repo = new SqliteStockBalancesRepository(ConnectionString);
        var rows = await repo.GetByDateAndWarehousesAsync(
            DateTime.UtcNow, warehouseIds: null, displayCurrencyId: null,
            stockIds: null, priceGroup: null);

        rows.Should().HaveCount(1);
        rows[0].Price.Should().Be(10.00m, "по умолчанию выбирается цена без price_group");
    }

    [Fact]
    public async Task Wholesale_price_group_returns_wholesale_price()
    {
        var repo = new SqliteStockBalancesRepository(ConnectionString);
        var rows = await repo.GetByDateAndWarehousesAsync(
            DateTime.UtcNow, warehouseIds: null, displayCurrencyId: null,
            stockIds: null, priceGroup: "wholesale");

        rows.Should().HaveCount(1);
        rows[0].Price.Should().Be(7.00m);
    }

    [Fact]
    public async Task Retail_price_group_returns_retail_price()
    {
        var repo = new SqliteStockBalancesRepository(ConnectionString);
        var rows = await repo.GetByDateAndWarehousesAsync(
            DateTime.UtcNow, warehouseIds: null, displayCurrencyId: null,
            stockIds: null, priceGroup: "retail");

        rows.Should().HaveCount(1);
        rows[0].Price.Should().Be(15.00m);
    }

    [Fact]
    public async Task Unknown_price_group_returns_zero_price_not_a_random_other_one()
    {
        var repo = new SqliteStockBalancesRepository(ConnectionString);
        var rows = await repo.GetByDateAndWarehousesAsync(
            DateTime.UtcNow, warehouseIds: null, displayCurrencyId: null,
            stockIds: null, priceGroup: "nonexistent");

        rows.Should().HaveCount(1);
        rows[0].Price.Should().Be(0m,
            "если запросили несуществующую группу — цена 0, чтобы UI явно показал отсутствие");
    }
}
