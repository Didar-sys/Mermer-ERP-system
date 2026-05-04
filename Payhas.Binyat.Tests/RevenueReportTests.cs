using FluentAssertions;
using Microsoft.Data.Sqlite;
using Payhas.Binyat.Data.Postgres.Models;
using Payhas.Binyat.Data.Sqlite;
using Payhas.Binyat.Data.Sqlite.Repositories;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Payhas.Binyat.Tests;

/// <summary>
/// Tests for the Revenue / P&L report — specifically the "100% profit on a
/// resold-after-return item" bug from the legacy system.
///
/// Bug reproduction (legacy behaviour):
///   1. Buy 1 item for 7.50.
///   2. Sell  1 item for 15.00 → cost = 7.50, profit = 7.50 (correct).
///   3. SalesReturn → item back on the shelf.
///   4. Sell that same item again for 15.00 → legacy showed cost = 0,
///      profit = 15.00 (i.e. 100% profit). WRONG.
///
/// Expected behaviour after the fix: cost on the second sale must equal
/// 7.50 (same as the first sale), profit = 7.50 again.
/// </summary>
public sealed class RevenueReportTests : IAsyncLifetime
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(),
        $"payhas-revenue-tests-{Guid.NewGuid():N}.sqlite");
    private string ConnectionString => $"Data Source={_dbPath}";

    private SqliteInvoicesRepository Repo => new(ConnectionString);

    // Stable IDs used across the test (so we can wire SalesReturn.source_id
    // explicitly without first reading back generated GUIDs).
    private static readonly string Warehouse = Guid.NewGuid().ToString();
    private static readonly string Stock     = Guid.NewGuid().ToString();

    public async Task InitializeAsync()
    {
        await new SqliteSchemaManager(ConnectionString).EnsureCreatedAsync();

        // Seed the FK targets the schema needs (foreign keys are enforced).
        await using var conn = new SqliteConnection(ConnectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO offices    (id, name) VALUES ($oid, 'O');
            INSERT INTO warehouses (id, office_id, name) VALUES ($wid, $oid, 'W');
            INSERT INTO stocks     (id, name) VALUES ($sid, 'YD 209 nude');
        ";
        cmd.Parameters.AddWithValue("$oid", Guid.NewGuid().ToString());
        cmd.Parameters.AddWithValue("$wid", Warehouse);
        cmd.Parameters.AddWithValue("$sid", Stock);
        await cmd.ExecuteNonQueryAsync();
    }

    public Task DisposeAsync()
    {
        SqliteConnection.ClearAllPools();
        try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { /* tmp leak */ }
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Sell_then_return_then_sell_again_does_not_produce_100_percent_profit()
    {
        // 1. Purchase: 1 шт по 7.50 — себестоимость партии
        await Repo.CreateAsync(MakeInvoice(InvoiceType.Purchase, qty: 1, price: 7.50m,
            date: new DateTime(2026, 04, 01)));

        // 2. First Sales: 1 шт по 15.00 → ожидаем cost = 7.50, profit = 7.50
        var firstSaleLineId = Guid.NewGuid().ToString();
        await Repo.CreateAsync(MakeInvoice(InvoiceType.Sales, qty: 1, price: 15.00m,
            date: new DateTime(2026, 04, 02), lineId: firstSaleLineId));

        // 3. SalesReturn: товар вернули, source_id указывает на исходную Sales-строку
        await Repo.CreateAsync(MakeInvoice(InvoiceType.SalesReturn, qty: 1, price: 15.00m,
            date: new DateTime(2026, 04, 03), sourceLineId: firstSaleLineId));

        // 4. Second Sales: тот же товар снова продали по 15.00
        var secondSaleLineId = Guid.NewGuid().ToString();
        await Repo.CreateAsync(MakeInvoice(InvoiceType.Sales, qty: 1, price: 15.00m,
            date: new DateTime(2026, 04, 04), lineId: secondSaleLineId));

        var report = await Repo.GetRevenueReportAsync(
            new DateTime(2026, 03, 01), new DateTime(2026, 04, 30));

        var sales = report.Where(r => r.InvoiceType == InvoiceType.Sales)
                          .OrderBy(r => r.Date)
                          .ToList();

        sales.Should().HaveCount(2, "обе продажи должны попасть в отчёт");

        // Главная проверка — починили ли баг:
        sales[0].UnitCost.Should().Be(7.50m, "первая продажа берёт cost из Purchase");
        sales[0].Profit  .Should().Be(7.50m);
        sales[0].ProfitPercent.Should().Be(50m);

        sales[1].UnitCost.Should().Be(7.50m,
            "ВТОРАЯ продажа после возврата должна сохранить ту же себестоимость, " +
            "а не показать cost=0 / 100% прибыли как в старой системе");
        sales[1].Profit  .Should().Be(7.50m);
        sales[1].ProfitPercent.Should().Be(50m);
    }

    [Fact]
    public async Task Multiple_purchases_use_running_weighted_average_cost()
    {
        // Закупка 1: 10 × 5.00 → склад: qty=10, value=50, avg=5.00
        await Repo.CreateAsync(MakeInvoice(InvoiceType.Purchase, qty: 10, price: 5.00m,
            date: new DateTime(2026, 04, 01)));
        // Закупка 2: 10 × 7.00 → склад: qty=20, value=120, avg=6.00
        await Repo.CreateAsync(MakeInvoice(InvoiceType.Purchase, qty: 10, price: 7.00m,
            date: new DateTime(2026, 04, 02)));
        // Продажа: 5 шт по 10.00 → cost = 5 × 6.00 = 30.00, profit = 50 - 30 = 20.00
        await Repo.CreateAsync(MakeInvoice(InvoiceType.Sales, qty: 5, price: 10.00m,
            date: new DateTime(2026, 04, 03)));

        var report = await Repo.GetRevenueReportAsync(
            new DateTime(2026, 03, 01), new DateTime(2026, 04, 30));

        report.Should().HaveCount(1);
        report[0].UnitCost .Should().Be(6.00m, "weighted average of (10×5 + 10×7)/20");
        report[0].CostTotal.Should().Be(30.00m);
        report[0].Profit   .Should().Be(20.00m);
    }

    [Fact]
    public async Task Purchase_return_keeps_correct_cost_via_source_id()
    {
        // Купили: 5 × 4.00
        var purchaseLineId = Guid.NewGuid().ToString();
        await Repo.CreateAsync(MakeInvoice(InvoiceType.Purchase, qty: 5, price: 4.00m,
            date: new DateTime(2026, 04, 01), lineId: purchaseLineId));
        // Вернули поставщику 2 шт — это PurchaseReturn, расход. Цена в накладной
        // = та же, что мы заплатили (4.00). Cost берём из running WA = 4.00.
        await Repo.CreateAsync(MakeInvoice(InvoiceType.PurchaseReturn, qty: 2, price: 4.00m,
            date: new DateTime(2026, 04, 02), sourceLineId: purchaseLineId));

        var report = await Repo.GetRevenueReportAsync(
            new DateTime(2026, 03, 01), new DateTime(2026, 04, 30));

        var pr = report.Single(r => r.InvoiceType == InvoiceType.PurchaseReturn);
        pr.UnitCost.Should().Be(4.00m);
        pr.Profit  .Should().Be(0m, "возврат поставщику по той же цене → нулевая прибыль/убыток");
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static Invoice MakeInvoice(
        InvoiceType type, decimal qty, decimal price, DateTime date,
        string? lineId = null, string? sourceLineId = null)
    {
        var inv = new Invoice
        {
            Id          = Guid.NewGuid().ToString(),
            Code        = $"{type}-{Guid.NewGuid():N}".Substring(0, 12),
            Date        = date,
            InvoiceType = type,
            IsCompleted = true,
            WarehouseId = Warehouse
        };
        inv.Lines.Add(new InvoiceLine
        {
            Id       = lineId ?? Guid.NewGuid().ToString(),
            SourceId = sourceLineId,
            StockId  = Stock,
            Quantity = qty,
            Price    = price
        });
        return inv;
    }
}
