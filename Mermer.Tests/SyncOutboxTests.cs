using Dapper;
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
/// Verifies that every local mutation lands in <c>sync_outbox</c> and that
/// the affected row's <c>sync_state</c> flips to <c>'dirty'</c>. This is
/// what the SyncService relies on to drive replication, and it's easy to
/// regress when adding new fields/tables.
/// </summary>
public sealed class SyncOutboxTests : IAsyncLifetime
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(),
        $"payhas-sync-tests-{Guid.NewGuid():N}.sqlite");
    private string ConnectionString => $"Data Source={_dbPath}";

    public async Task InitializeAsync()
    {
        await new SqliteSchemaManager(ConnectionString).EnsureCreatedAsync();
    }

    public Task DisposeAsync()
    {
        SqliteConnection.ClearAllPools();
        try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { /* tmp leak */ }
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Creating_an_invoice_enqueues_an_insert_and_marks_row_dirty()
    {
        var repo = new SqliteInvoicesRepository(ConnectionString);
        var inv  = new Invoice
        {
            Id          = Guid.NewGuid().ToString(),
            Code        = "INV-001",
            Date        = DateTime.UtcNow,
            InvoiceType = InvoiceType.Sales
        };
        inv.Lines.Add(new InvoiceLine { Quantity = 1, Price = 10m });

        await repo.CreateAsync(inv);

        await using var conn = new SqliteConnection(ConnectionString);
        await conn.OpenAsync();

        var outbox = (await conn.QueryAsync<(string TableName, string RowId, string Operation)>(
            "SELECT table_name, row_id, operation FROM sync_outbox")).ToList();
        outbox.Should().ContainSingle(o => o.RowId == inv.Id && o.Operation == "insert" && o.TableName == "invoices");

        var syncState = await conn.ExecuteScalarAsync<string>(
            "SELECT sync_state FROM invoices WHERE id = @id", new { id = inv.Id });
        syncState.Should().Be("dirty");
    }

    [Fact]
    public async Task Updating_an_invoice_bumps_row_version_and_enqueues_update()
    {
        var repo = new SqliteInvoicesRepository(ConnectionString);
        var inv  = new Invoice { Id = Guid.NewGuid().ToString(), InvoiceType = InvoiceType.Sales };
        inv.Lines.Add(new InvoiceLine { Quantity = 1, Price = 10m });
        await repo.CreateAsync(inv);

        inv.Code = "UPDATED";
        await repo.UpdateAsync(inv);

        await using var conn = new SqliteConnection(ConnectionString);
        await conn.OpenAsync();

        var rowVersion = await conn.ExecuteScalarAsync<long>(
            "SELECT row_version FROM invoices WHERE id = @id", new { id = inv.Id });
        rowVersion.Should().BeGreaterThan(1);

        var ops = (await conn.QueryAsync<string>(
            "SELECT operation FROM sync_outbox WHERE row_id = @id", new { id = inv.Id })).ToList();
        ops.Should().Contain(new[] { "insert", "update" });
    }

    [Fact]
    public async Task Deleting_a_stock_marks_disabled_and_queues_delete_op()
    {
        var repo = new SqliteStocksRepository(ConnectionString);
        var stock = new Stock { Id = Guid.NewGuid().ToString(), Name = "Test" };
        await repo.CreateAsync(stock);
        await repo.DeleteAsync(stock.Id);

        await using var conn = new SqliteConnection(ConnectionString);
        await conn.OpenAsync();

        var disabled = await conn.ExecuteScalarAsync<long>(
            "SELECT is_disabled FROM stocks WHERE id = @id", new { id = stock.Id });
        disabled.Should().Be(1);

        var ops = (await conn.QueryAsync<string>(
            "SELECT operation FROM sync_outbox WHERE row_id = @id ORDER BY id", new { id = stock.Id })).ToList();
        ops.Should().EndWith("delete");
    }
}
