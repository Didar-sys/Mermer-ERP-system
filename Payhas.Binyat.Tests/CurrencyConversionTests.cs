using Dapper;
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
/// Verifies that the optional <c>displayCurrencyId</c> parameter on
/// <c>GetInfoAsync</c> performs the correct cross-currency conversion
/// using the per-invoice <c>invoice_currency_convertions</c> snapshot.
///
/// Coefficient definition:
///   "1 unit of currency_X = (multiplier_X / divider_X) units of base"
/// Conversion to display:
///   amount_in_display = amount × (mult_X / div_X) × (div_disp / mult_disp)
/// </summary>
public sealed class CurrencyConversionTests : IAsyncLifetime
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(),
        $"payhas-currency-tests-{Guid.NewGuid():N}.sqlite");
    private string ConnectionString => $"Data Source={_dbPath}";

    private SqliteInvoicesRepository Repo => new(ConnectionString);

    // base = TMT (1:1), USD ≈ 3.5 TMT, EUR ≈ 4 TMT
    private static readonly string TmtId  = Guid.NewGuid().ToString();
    private static readonly string UsdId  = Guid.NewGuid().ToString();
    private static readonly string EurId  = Guid.NewGuid().ToString();

    public async Task InitializeAsync()
    {
        await new SqliteSchemaManager(ConnectionString).EnsureCreatedAsync();

        await using var conn = new SqliteConnection(ConnectionString);
        await conn.OpenAsync();
        await conn.ExecuteAsync(@"
            INSERT INTO currencies (id, name, is_default) VALUES
                (@tmt, 'TMT', 1),
                (@usd, 'USD', 0),
                (@eur, 'EUR', 0);",
            new { tmt = TmtId, usd = UsdId, eur = EurId });
    }

    public Task DisposeAsync()
    {
        SqliteConnection.ClearAllPools();
        try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { /* tmp leak */ }
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetInfoAsync_without_display_currency_returns_raw_amounts()
    {
        // Контроль: без displayCurrencyId возвращаются "сырые" суммы
        // (legacy-поведение должно сохраниться).
        var inv = NewInvoice(lineQty: 2, linePrice: 100m, lineCurrency: TmtId);
        await Repo.CreateAsync(inv);
        await SetupRates(inv.Id!, new[] { (TmtId, 1.0, 1.0) });

        var rows = await Repo.GetInfoAsync(
            from: DateTime.Today.AddDays(-1), till: DateTime.Today.AddDays(1),
            displayCurrencyId: null);

        rows.Should().HaveCount(1);
        rows[0].Subtotal.Should().Be(200m);
    }

    [Fact]
    public async Task USD_invoice_displayed_in_TMT_is_multiplied_by_rate()
    {
        // Накладная в USD: 2 шт × 100 USD = 200 USD
        // Курс: 1 USD = 3.5 TMT  →  должно быть 700 TMT при display=TMT
        var inv = NewInvoice(lineQty: 2, linePrice: 100m, lineCurrency: UsdId);
        await Repo.CreateAsync(inv);
        await SetupRates(inv.Id!, new[]
        {
            (TmtId, 1.0, 1.0),    // 1 TMT = 1 base (TMT)
            (UsdId, 3.5, 1.0)     // 1 USD = 3.5 base
        });

        var rows = await Repo.GetInfoAsync(
            from: DateTime.Today.AddDays(-1), till: DateTime.Today.AddDays(1),
            displayCurrencyId: TmtId);

        rows[0].Subtotal.Should().Be(700m);
    }

    [Fact]
    public async Task USD_invoice_displayed_in_EUR_uses_cross_rate()
    {
        // 200 USD при 1 USD = 3.5 TMT и 1 EUR = 4 TMT
        // → 200 × 3.5 / 4 = 175 EUR
        var inv = NewInvoice(lineQty: 2, linePrice: 100m, lineCurrency: UsdId);
        await Repo.CreateAsync(inv);
        await SetupRates(inv.Id!, new[]
        {
            (TmtId, 1.0, 1.0),
            (UsdId, 3.5, 1.0),
            (EurId, 4.0, 1.0)
        });

        var rows = await Repo.GetInfoAsync(
            from: DateTime.Today.AddDays(-1), till: DateTime.Today.AddDays(1),
            displayCurrencyId: EurId);

        rows[0].Subtotal.Should().Be(175m);
    }

    [Fact]
    public async Task Mixed_currency_lines_are_each_converted_independently()
    {
        // Накладная содержит позиции в двух разных валютах:
        //   1 шт × 100 USD (3.5 TMT/USD)  =  350 TMT
        //   1 шт × 50 EUR  (4.0 TMT/EUR)  =  200 TMT
        // Итого в TMT = 550
        var inv = new Invoice
        {
            Id          = Guid.NewGuid().ToString(),
            Date        = DateTime.UtcNow,
            InvoiceType = InvoiceType.Sales,
            IsCompleted = true
        };
        inv.Lines.Add(new InvoiceLine { Quantity = 1, Price = 100m, CurrencyId = UsdId });
        inv.Lines.Add(new InvoiceLine { Quantity = 1, Price = 50m,  CurrencyId = EurId });

        await Repo.CreateAsync(inv);
        await SetupRates(inv.Id!, new[]
        {
            (TmtId, 1.0, 1.0),
            (UsdId, 3.5, 1.0),
            (EurId, 4.0, 1.0)
        });

        var rows = await Repo.GetInfoAsync(
            from: DateTime.Today.AddDays(-1), till: DateTime.Today.AddDays(1),
            displayCurrencyId: TmtId);

        rows[0].Subtotal.Should().Be(550m);
    }

    [Fact]
    public async Task Lines_without_currency_id_are_treated_as_base()
    {
        // У позиции currency_id IS NULL → считаем что она уже в base (TMT).
        // 1 × 100 (NULL) → 100 TMT при display=TMT.
        var inv = NewInvoice(lineQty: 1, linePrice: 100m, lineCurrency: null);
        await Repo.CreateAsync(inv);
        await SetupRates(inv.Id!, new[] { (TmtId, 1.0, 1.0) });

        var rows = await Repo.GetInfoAsync(
            from: DateTime.Today.AddDays(-1), till: DateTime.Today.AddDays(1),
            displayCurrencyId: TmtId);

        rows[0].Subtotal.Should().Be(100m);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static Invoice NewInvoice(decimal lineQty, decimal linePrice, string? lineCurrency)
    {
        var inv = new Invoice
        {
            Id          = Guid.NewGuid().ToString(),
            Date        = DateTime.UtcNow,
            InvoiceType = InvoiceType.Sales,
            IsCompleted = true
        };
        inv.Lines.Add(new InvoiceLine
        {
            Quantity = lineQty, Price = linePrice, CurrencyId = lineCurrency
        });
        return inv;
    }

    private async Task SetupRates(string invoiceId, (string CurrencyId, double Mult, double Div)[] rates)
    {
        await using var conn = new SqliteConnection(ConnectionString);
        await conn.OpenAsync();
        foreach (var (cur, mult, div) in rates)
        {
            await conn.ExecuteAsync(
                @"INSERT INTO invoice_currency_convertions (id, invoice_id, currency_id, multiplier, divider)
                  VALUES (@id, @inv, @cur, @m, @d)",
                new { id = Guid.NewGuid().ToString(), inv = invoiceId, cur, m = (decimal)mult, d = (decimal)div });
        }
    }
}
