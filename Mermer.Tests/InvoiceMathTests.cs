using FluentAssertions;
using Microsoft.Data.Sqlite;
using Mermer.Data.Postgres.Models;
using Mermer.Data.Sqlite;
using Mermer.Data.Sqlite.Repositories;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Mermer.Tests;

/// <summary>
/// End-to-end financial-math tests using a real SQLite database.
/// The same SQL CTEs run on PostgreSQL — the test guards both code paths
/// against regression of the bugs we fixed:
///  • Cartesian-product inflation across child tables.
///  • Percentage discount being treated as a flat amount.
///  • Overheads being silently dropped from the grand total.
/// </summary>
public sealed class InvoiceMathTests : IAsyncLifetime
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(),
        $"payhas-tests-{Guid.NewGuid():N}.sqlite");
    private string ConnectionString => $"Data Source={_dbPath}";

    private SqliteInvoicesRepository Repo => new(ConnectionString);

    public async Task InitializeAsync()
    {
        var schema = new SqliteSchemaManager(ConnectionString);
        await schema.EnsureCreatedAsync();
    }

    public Task DisposeAsync()
    {
        // Microsoft.Data.Sqlite pools connections per-string; on Windows the
        // file stays locked until the pool is drained. ClearAllPools() makes
        // the test cleanup deterministic.
        SqliteConnection.ClearAllPools();
        try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { /* tmp leak */ }
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Multi_line_invoice_with_no_discounts_aggregates_subtotal_correctly()
    {
        // 5 lines × 100 TMT each = 500 TMT subtotal
        var invoice = NewInvoice(lines: 5, qty: 1, price: 100m);
        await Repo.CreateAsync(invoice);

        var info = (await Repo.GetInfoAsync(DateTime.Today.AddDays(-1), DateTime.Today)).Single();
        info.Subtotal      .Should().Be(500m);
        info.DiscountsTotal.Should().Be(0m);
        info.GrandTotal    .Should().Be(500m);
    }

    [Fact]
    public async Task Multiple_payments_do_not_inflate_subtotal_via_cartesian_product()
    {
        // 3 lines + 4 payments would produce 12 rows with a naive LEFT JOIN
        // and inflate the line subtotal by 4×.
        var invoice = NewInvoice(lines: 3, qty: 1, price: 50m);
        for (var i = 0; i < 4; i++)
            invoice.Payments.Add(new InvoicePayment { Amount = 25m });

        await Repo.CreateAsync(invoice);
        var info = (await Repo.GetInfoAsync(DateTime.Today.AddDays(-1), DateTime.Today)).Single();

        info.Subtotal.Should().Be(150m);
        info.PaymentsTotal.Should().Be(100m);
    }

    [Fact]
    public async Task Multiple_discounts_do_not_inflate_payments_via_cartesian_product()
    {
        var invoice = NewInvoice(lines: 1, qty: 10, price: 100m); // subtotal 1000
        invoice.Discounts.Add(new InvoiceDiscount { Type = InvoiceDiscountType.Flat, Amount = 50m });
        invoice.Discounts.Add(new InvoiceDiscount { Type = InvoiceDiscountType.Flat, Amount = 30m });
        invoice.Payments.Add(new InvoicePayment { Amount = 200m });
        invoice.Payments.Add(new InvoicePayment { Amount = 200m });

        await Repo.CreateAsync(invoice);
        var info = (await Repo.GetInfoAsync(DateTime.Today.AddDays(-1), DateTime.Today)).Single();

        info.DiscountsTotal.Should().Be(80m);
        info.PaymentsTotal .Should().Be(400m);  // not 800 as naive JOIN would give
    }

    [Fact]
    public async Task Percentage_discount_is_applied_as_percent_of_subtotal()
    {
        var invoice = NewInvoice(lines: 1, qty: 1, price: 1000m);
        invoice.Discounts.Add(new InvoiceDiscount
        {
            Type   = InvoiceDiscountType.Percentage,
            Amount = 10m            // 10% — should subtract 100, not 10
        });

        await Repo.CreateAsync(invoice);
        var info = (await Repo.GetInfoAsync(DateTime.Today.AddDays(-1), DateTime.Today)).Single();

        info.DiscountsTotal.Should().Be(100m);
        info.GrandTotal    .Should().Be(900m);
    }

    [Fact]
    public async Task Mixed_flat_and_percentage_discounts_compose_correctly()
    {
        var invoice = NewInvoice(lines: 1, qty: 1, price: 1000m);
        invoice.Discounts.Add(new InvoiceDiscount { Type = InvoiceDiscountType.Percentage, Amount = 5m });
        invoice.Discounts.Add(new InvoiceDiscount { Type = InvoiceDiscountType.Flat,       Amount = 25m });

        await Repo.CreateAsync(invoice);
        var info = (await Repo.GetInfoAsync(DateTime.Today.AddDays(-1), DateTime.Today)).Single();

        // 5% of 1000 = 50; flat 25 → total discount 75
        info.DiscountsTotal.Should().Be(75m);
        info.GrandTotal    .Should().Be(925m);
    }

    [Fact]
    public async Task Overheads_are_added_to_grand_total()
    {
        var invoice = NewInvoice(lines: 1, qty: 1, price: 1000m);
        invoice.Overheads.Add(new InvoiceOverhead { Amount = 50m, Description = "Доставка" });
        invoice.Overheads.Add(new InvoiceOverhead { Amount = 25m, Description = "Таможня" });

        await Repo.CreateAsync(invoice);
        var info = (await Repo.GetInfoAsync(DateTime.Today.AddDays(-1), DateTime.Today)).Single();

        info.OverheadsTotal.Should().Be(75m);
        info.GrandTotal    .Should().Be(1075m);
    }

    [Fact]
    public async Task Left_total_never_goes_negative_when_overpaid()
    {
        var invoice = NewInvoice(lines: 1, qty: 1, price: 100m);
        invoice.Payments.Add(new InvoicePayment { Amount = 500m });    // wildly overpaid
        invoice.Changes .Add(new InvoicePayment { Amount = 400m });    // change given back

        await Repo.CreateAsync(invoice);
        var info = (await Repo.GetInfoAsync(DateTime.Today.AddDays(-1), DateTime.Today)).Single();

        info.GrandTotal   .Should().Be(100m);
        info.PaymentsTotal.Should().Be(500m);
        info.LeftTotal    .Should().Be(0m, "100 grand − 500 paid + 400 change = 0, clamped at 0");
    }

    [Fact]
    public async Task Decimal_precision_survives_round_trip()
    {
        // 0.1 + 0.2 disaster doesn't apply because we use NUMERIC. Verify
        // that 1/3-style fractions persist with 4 decimals of precision.
        var invoice = NewInvoice(lines: 3, qty: 1, price: 33.3333m);
        await Repo.CreateAsync(invoice);

        var info = (await Repo.GetInfoAsync(DateTime.Today.AddDays(-1), DateTime.Today)).Single();
        info.Subtotal.Should().Be(99.9999m);
    }

    private static Invoice NewInvoice(int lines, decimal qty, decimal price)
    {
        var inv = new Invoice
        {
            Id          = Guid.NewGuid().ToString(),
            Code        = "T-" + Guid.NewGuid().ToString("N")[..6],
            Date        = DateTime.UtcNow.AddMinutes(-1),
            InvoiceType = InvoiceType.Sales,
            IsCompleted = true
        };
        for (var i = 0; i < lines; i++)
            inv.Lines.Add(new InvoiceLine { Quantity = qty, Price = price, SortOrder = i });
        return inv;
    }
}
