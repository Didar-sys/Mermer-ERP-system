using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Payhas.Binyat.Data.Postgres.Models;
using Payhas.Binyat.Data.Sqlite;
using Payhas.Binyat.Data.Sqlite.Repositories;

// =============================================================================
// Payhas Binyat — load benchmark
// =============================================================================
// Generates 100 000 invoices with synthetic lines/discounts/payments/overheads,
// inserts them into a fresh SQLite database, and measures the time of the
// aggregated InvoiceInfo report (the same CTE-based SQL that runs on
// PostgreSQL in production).
//
// Goal from TZ:
//   * Search responds in < 100 ms on 100 k items
//   * Reports respond in seconds, not minutes
//
// Usage:  dotnet run -c Release --project Payhas.Binyat.Benchmarks
// =============================================================================

var dbPath = Path.Combine(Path.GetTempPath(), $"payhas-bench-{Guid.NewGuid():N}.sqlite");
var connectionString = $"Data Source={dbPath}";
Console.WriteLine($"Bench DB: {dbPath}");

try
{
    await new SqliteSchemaManager(connectionString).EnsureCreatedAsync();

    const int InvoiceCount = 100_000;
    const int LinesPerInvoice = 5;

    Console.WriteLine($"Inserting {InvoiceCount:N0} invoices × {LinesPerInvoice} lines …");

    var sw = Stopwatch.StartNew();
    await InsertInvoicesAsync(connectionString, InvoiceCount, LinesPerInvoice);
    sw.Stop();

    var totalLines = (long)InvoiceCount * LinesPerInvoice;
    Console.WriteLine($"  Inserted in {sw.Elapsed.TotalSeconds:F1} s "
                    + $"({InvoiceCount / sw.Elapsed.TotalSeconds:N0} inv/s, "
                    + $"{totalLines / sw.Elapsed.TotalSeconds:N0} lines/s)");

    Console.WriteLine();
    Console.WriteLine("Running aggregated GetInfoAsync — full year window …");
    var repo = new SqliteInvoicesRepository(connectionString);
    var from = DateTime.UtcNow.AddYears(-1);
    var till = DateTime.UtcNow.AddDays(1);

    // Warm-up
    await repo.GetInfoAsync(from, till);

    var times = new List<double>();
    for (var i = 0; i < 5; i++)
    {
        sw.Restart();
        var rows = await repo.GetInfoAsync(from, till);
        sw.Stop();
        times.Add(sw.Elapsed.TotalMilliseconds);
        Console.WriteLine($"  run {i + 1}: {sw.Elapsed.TotalMilliseconds:F1} ms ({rows.Count:N0} rows)");
    }

    times.Sort();
    Console.WriteLine();
    Console.WriteLine($"Median GetInfoAsync over {InvoiceCount:N0} invoices: {times[times.Count / 2]:F1} ms");
    Console.WriteLine($"Best:                                                {times[0]:F1} ms");
    Console.WriteLine($"Worst:                                               {times[^1]:F1} ms");

    // Search-style point lookup by partner over a 30-day window
    Console.WriteLine();
    Console.WriteLine("Running 30-day window report …");
    sw.Restart();
    var recent = await repo.GetInfoAsync(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(1));
    sw.Stop();
    Console.WriteLine($"  30-day report: {sw.Elapsed.TotalMilliseconds:F1} ms ({recent.Count:N0} rows)");
}
finally
{
    SqliteConnection.ClearAllPools();
    try { File.Delete(dbPath); } catch { /* tmp leak */ }
}

static async Task InsertInvoicesAsync(string connectionString, int invoiceCount, int linesPerInvoice)
{
    // Bulk-insert pattern: open one connection, one transaction, prepared
    // statements. Avoids per-row overhead of the high-level repository.
    await using var conn = new SqliteConnection(connectionString);
    await conn.OpenAsync();
    await using var pragma = conn.CreateCommand();
    pragma.CommandText = "PRAGMA foreign_keys = OFF; PRAGMA synchronous = OFF; PRAGMA journal_mode = MEMORY;";
    await pragma.ExecuteNonQueryAsync();

    await using var tx = (SqliteTransaction)await conn.BeginTransactionAsync();

    await using var headerCmd = conn.CreateCommand();
    headerCmd.Transaction = tx;
    headerCmd.CommandText = @"
        INSERT INTO invoices (id, code, date, invoice_type, is_completed, is_disabled,
                              created_at, updated_at, row_version, sync_state)
        VALUES ($id, $code, $date, $type, 1, 0, datetime('now'), datetime('now'), 1, 'synced')";
    var pId   = headerCmd.CreateParameter(); pId.ParameterName   = "$id";   headerCmd.Parameters.Add(pId);
    var pCode = headerCmd.CreateParameter(); pCode.ParameterName = "$code"; headerCmd.Parameters.Add(pCode);
    var pDate = headerCmd.CreateParameter(); pDate.ParameterName = "$date"; headerCmd.Parameters.Add(pDate);
    var pType = headerCmd.CreateParameter(); pType.ParameterName = "$type"; headerCmd.Parameters.Add(pType);

    await using var lineCmd = conn.CreateCommand();
    lineCmd.Transaction = tx;
    lineCmd.CommandText = @"
        INSERT INTO invoice_lines (id, invoice_id, quantity, price, sort_order)
        VALUES ($id, $invoiceId, $qty, $price, $sort)";
    var lId  = lineCmd.CreateParameter(); lId.ParameterName  = "$id";        lineCmd.Parameters.Add(lId);
    var lInv = lineCmd.CreateParameter(); lInv.ParameterName = "$invoiceId"; lineCmd.Parameters.Add(lInv);
    var lQty = lineCmd.CreateParameter(); lQty.ParameterName = "$qty";       lineCmd.Parameters.Add(lQty);
    var lPr  = lineCmd.CreateParameter(); lPr.ParameterName  = "$price";     lineCmd.Parameters.Add(lPr);
    var lSo  = lineCmd.CreateParameter(); lSo.ParameterName  = "$sort";      lineCmd.Parameters.Add(lSo);

    await using var discCmd = conn.CreateCommand();
    discCmd.Transaction = tx;
    discCmd.CommandText = @"
        INSERT INTO invoice_discounts (id, invoice_id, discount_type, amount, sort_order)
        VALUES ($id, $invoiceId, $type, $amt, 0)";
    discCmd.Parameters.AddWithValue("$id", "");
    discCmd.Parameters.AddWithValue("$invoiceId", "");
    discCmd.Parameters.AddWithValue("$type", "Flat");
    discCmd.Parameters.AddWithValue("$amt", 0m);

    await using var payCmd = conn.CreateCommand();
    payCmd.Transaction = tx;
    payCmd.CommandText = @"
        INSERT INTO invoice_payments (id, invoice_id, payment_type, amount, sort_order)
        VALUES ($id, $invoiceId, 'Payment', $amt, 0)";
    payCmd.Parameters.AddWithValue("$id", "");
    payCmd.Parameters.AddWithValue("$invoiceId", "");
    payCmd.Parameters.AddWithValue("$amt", 0m);

    var rng       = new Random(42);
    var startDate = DateTime.UtcNow.AddYears(-1);

    for (var i = 0; i < invoiceCount; i++)
    {
        var invId = Guid.NewGuid().ToString();
        pId.Value   = invId;
        pCode.Value = $"INV-{i:D7}";
        pDate.Value = startDate.AddMinutes(i).ToString("o");
        pType.Value = (i % 4) switch
        {
            0 => "Sales",
            1 => "Purchase",
            2 => "SalesReturn",
            _ => "PurchaseReturn"
        };
        await headerCmd.ExecuteNonQueryAsync();

        for (var j = 0; j < linesPerInvoice; j++)
        {
            lId.Value  = Guid.NewGuid().ToString();
            lInv.Value = invId;
            lQty.Value = 1 + rng.Next(0, 10);
            lPr.Value  = (decimal)(10 + rng.NextDouble() * 1000);
            lSo.Value  = j;
            await lineCmd.ExecuteNonQueryAsync();
        }

        // 30% of invoices have a discount.
        if (rng.Next(100) < 30)
        {
            discCmd.Parameters["$id"].Value        = Guid.NewGuid().ToString();
            discCmd.Parameters["$invoiceId"].Value = invId;
            discCmd.Parameters["$type"].Value      = rng.Next(2) == 0 ? "Flat" : "Percentage";
            discCmd.Parameters["$amt"].Value       = (decimal)(rng.NextDouble() * 50);
            await discCmd.ExecuteNonQueryAsync();
        }

        // 70% of invoices have at least one payment.
        if (rng.Next(100) < 70)
        {
            payCmd.Parameters["$id"].Value        = Guid.NewGuid().ToString();
            payCmd.Parameters["$invoiceId"].Value = invId;
            payCmd.Parameters["$amt"].Value       = (decimal)(rng.NextDouble() * 5000);
            await payCmd.ExecuteNonQueryAsync();
        }

        if (i % 10_000 == 0 && i > 0)
            Console.WriteLine($"  … {i:N0} invoices");
    }

    await tx.CommitAsync();
}
