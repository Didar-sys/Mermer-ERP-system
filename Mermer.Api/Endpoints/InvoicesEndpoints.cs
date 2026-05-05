using Mermer.Data.Postgres.Abstractions;
using Mermer.Data.Postgres.Models;

namespace Mermer.Api.Endpoints;

/// <summary>
/// Invoice / financial endpoints. All money math is performed server-side
/// in NUMERIC(18,4) via per-child-table CTEs (no Cartesian inflation),
/// with proper Flat / Percentage discount semantics, overheads, and
/// optional currency conversion through invoice_currency_convertions.
/// </summary>
public static class InvoicesEndpoints
{
    public static IEndpointRouteBuilder MapInvoicesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoices").WithTags("Invoices");

        group.MapGet("/", async (
            DateTime from,
            DateTime till,
            string? displayCurrencyId,
            IInvoicesRepository repo,
            CancellationToken ct) =>
        {
            var info = await repo.GetInfoAsync(from, till, displayCurrencyId, ct);
            return Results.Ok(info);
        })
        .WithName("InvoicesGetInfo")
        .WithSummary("Aggregated invoices for a date range, with optional display currency.");

        group.MapGet("/count", async (
            DateTime from,
            DateTime till,
            IInvoicesRepository repo,
            CancellationToken ct) =>
        {
            var count = await repo.CountInfoAsync(from, till, ct);
            return Results.Ok(new { count });
        })
        .WithName("InvoicesCountInfo");

        group.MapGet("/payment-info", async (
            DateTime from,
            DateTime till,
            string? officeId,
            string? partnerId,
            string? displayCurrencyId,
            IInvoicesRepository repo,
            CancellationToken ct) =>
        {
            var info = await repo.GetPaymentInfoAsync(from, till, officeId, partnerId, displayCurrencyId, ct);
            return Results.Ok(info);
        })
        .WithName("InvoicesGetPaymentInfo")
        .WithSummary("Partner ledger with debit/credit, optional currency conversion.");

        group.MapGet("/payment-info/count", async (
            DateTime from,
            DateTime till,
            string? officeId,
            string? partnerId,
            IInvoicesRepository repo,
            CancellationToken ct) =>
        {
            var count = await repo.CountPaymentInfoAsync(from, till, officeId, partnerId, ct);
            return Results.Ok(new { count });
        })
        .WithName("InvoicesCountPaymentInfo");

        group.MapGet("/revenue", async (
            DateTime from,
            DateTime till,
            string? warehouseId,
            IInvoicesRepository repo,
            CancellationToken ct) =>
        {
            var rows = await repo.GetRevenueReportAsync(from, till, warehouseId, ct);
            return Results.Ok(rows);
        })
        .WithName("InvoicesRevenueReport")
        .WithSummary(
            "Profit-and-loss report. Cost is running weighted-average; " +
            "SalesReturn cost is looked up via invoice_lines.source_id, " +
            "fixing the legacy '100% profit on resold-after-return' bug.");

        group.MapGet("/{id}", async (
            string id,
            IInvoicesRepository repo,
            CancellationToken ct) =>
        {
            var inv = await repo.GetAsync(id, ct);
            return inv is null ? Results.NotFound() : Results.Ok(inv);
        })
        .WithName("InvoicesGetById")
        .WithSummary("Full invoice with lines, discounts, payments, overheads.");

        group.MapPost("/", async (
            Invoice model,
            IInvoicesRepository repo,
            CancellationToken ct) =>
        {
            var created = await repo.CreateAsync(model, ct);
            return Results.Created($"/api/invoices/{created.Id}", created);
        })
        .WithName("InvoicesCreate");

        group.MapPut("/{id}", async (
            string id,
            Invoice model,
            IInvoicesRepository repo,
            CancellationToken ct) =>
        {
            model.Id = id;
            var updated = await repo.UpdateAsync(model, ct);
            return Results.Ok(updated);
        })
        .WithName("InvoicesUpdate");

        group.MapDelete("/{id}", async (
            string id,
            IInvoicesRepository repo,
            CancellationToken ct) =>
        {
            await repo.DeleteAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("InvoicesDelete");

        return app;
    }
}
