using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Payhas.Binyat.Data.Postgres.Abstractions;
using Payhas.Binyat.Data.Postgres.Models;
using Payhas.Binyat.Data.Sqlite.Sync;

namespace Payhas.Binyat.Data.Sqlite.HybridRepositories;

/// <summary>
/// Repository facade that picks PostgreSQL when reachable, SQLite otherwise.
/// Reads always prefer the local cache (instant response, no network round-
/// trip); writes go to the local cache and are queued for replication.
///
/// This is the single repository the WPF UI is intended to depend on. It
/// implements <see cref="IStocksRepository"/> so the UI doesn't need to
/// know about online/offline at all.
/// </summary>
public sealed class HybridStocksRepository : IStocksRepository
{
    private readonly IStocksRepository _online;
    private readonly IStocksRepository _local;
    private readonly IConnectivityProbe _probe;

    public HybridStocksRepository(
        IStocksRepository online,
        IStocksRepository local,
        IConnectivityProbe probe)
    {
        _online = online;
        _local  = local;
        _probe  = probe;
    }

    public Task<Stock?>                   GetAsync(string id, CancellationToken ct = default)
        => _local.GetAsync(id, ct);
    public Task<IReadOnlyList<Stock>>     GetAllAsync(CancellationToken ct = default)
        => _local.GetAllAsync(ct);
    public Task<IReadOnlyList<Stock>>     GetListAsync(string[] stockIds, CancellationToken ct = default)
        => _local.GetListAsync(stockIds, ct);
    public Task<IReadOnlyList<StockInfo>> GetInfoAsync(string[]? stockIds = null, CancellationToken ct = default)
        => _local.GetInfoAsync(stockIds, ct);
    public Task<IReadOnlyList<StockInfo>> GetInfoAsync(string? additionalPriceCurrencyId, string? additionalPriceGroup, CancellationToken ct = default)
        => _local.GetInfoAsync(additionalPriceCurrencyId, additionalPriceGroup, ct);

    public Task<Stock> CreateAsync(Stock model, CancellationToken ct = default)
        => _local.CreateAsync(model, ct);     // local dirty + outbox; SyncService pushes
    public Task<Stock> UpdateAsync(Stock model, CancellationToken ct = default)
        => _local.UpdateAsync(model, ct);
    public Task        DeleteAsync(string id, CancellationToken ct = default)
        => _local.DeleteAsync(id, ct);

    public async Task MergeAsync(string mainStockId, string[] mergeStockIds, bool disableMergedItems, CancellationToken ct = default)
    {
        // Merge changes invoice_lines globally — it MUST run online.
        if (!await _probe.IsOnlineAsync(ct))
            throw new InvalidOperationException(
                "Stock merge requires an online connection to the central server.");

        await _online.MergeAsync(mainStockId, mergeStockIds, disableMergedItems, ct);
    }

    public Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>>> GetFacetsAsync(
        string[] fields, CancellationToken ct = default)
        => _local.GetFacetsAsync(fields, ct);
}
