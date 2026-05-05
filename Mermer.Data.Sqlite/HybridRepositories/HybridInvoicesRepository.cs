using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mermer.Data.Postgres.Abstractions;
using Mermer.Data.Postgres.Models;
using Mermer.Data.Sqlite.Sync;

namespace Mermer.Data.Sqlite.HybridRepositories;

/// <summary>
/// Hybrid invoice repository.
///
/// Reads:    always served from SQLite (local cache).
/// Writes:   always to SQLite, queued for replication via the outbox.
/// Reports:  payment-info (partner_actions ledger) requires the online
///           PostgreSQL repo because the ledger isn't replicated yet.
/// </summary>
public sealed class HybridInvoicesRepository : IInvoicesRepository
{
    private readonly IInvoicesRepository _online;
    private readonly IInvoicesRepository _local;
    private readonly IConnectivityProbe _probe;

    public HybridInvoicesRepository(
        IInvoicesRepository online,
        IInvoicesRepository local,
        IConnectivityProbe probe)
    {
        _online = online;
        _local  = local;
        _probe  = probe;
    }

    public Task<Invoice?> GetAsync(string id, CancellationToken ct = default)
        => _local.GetAsync(id, ct);

    public Task<IReadOnlyList<InvoiceInfo>> GetInfoAsync(
        DateTime from, DateTime till,
        string? displayCurrencyId = null,
        CancellationToken ct = default)
        => _local.GetInfoAsync(from, till, displayCurrencyId, ct);

    public Task<int> CountInfoAsync(DateTime from, DateTime till, CancellationToken ct = default)
        => _local.CountInfoAsync(from, till, ct);

    public async Task<IReadOnlyList<InvoicePaymentInfo>> GetPaymentInfoAsync(
        DateTime from, DateTime till, string? officeId, string? partnerId,
        string? displayCurrencyId = null,
        CancellationToken ct = default)
    {
        // The partner_actions ledger isn't replicated to SQLite — fall back
        // to the online repo when we have connectivity, otherwise return
        // empty (UI shows a "feature requires connection" notice).
        if (await _probe.IsOnlineAsync(ct))
            return await _online.GetPaymentInfoAsync(from, till, officeId, partnerId, displayCurrencyId, ct);
        return await _local.GetPaymentInfoAsync(from, till, officeId, partnerId, displayCurrencyId, ct);
    }

    public async Task<int> CountPaymentInfoAsync(
        DateTime from, DateTime till, string? officeId, string? partnerId, CancellationToken ct = default)
    {
        if (await _probe.IsOnlineAsync(ct))
            return await _online.CountPaymentInfoAsync(from, till, officeId, partnerId, ct);
        return 0;
    }

    public Task<Invoice> CreateAsync(Invoice model, CancellationToken ct = default)
        => _local.CreateAsync(model, ct);
    public Task<Invoice> UpdateAsync(Invoice model, CancellationToken ct = default)
        => _local.UpdateAsync(model, ct);
    public Task          DeleteAsync(string id, CancellationToken ct = default)
        => _local.DeleteAsync(id, ct);

    public Task<IReadOnlyList<RevenueReportRow>> GetRevenueReportAsync(
        DateTime from, DateTime till, string? warehouseId = null, CancellationToken ct = default)
        => _local.GetRevenueReportAsync(from, till, warehouseId, ct);
}
