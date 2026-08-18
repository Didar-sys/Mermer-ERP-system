using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Mermer.Data.Postgres;
using Mermer.Data.Postgres.Abstractions;
using Mermer.Data.Postgres.Entities;

namespace Mermer.Api.Endpoints;

public static class StocksEndpoints
{
    public static IEndpointRouteBuilder MapStocksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stocks").WithTags("Stocks");

        group.MapGet("/", async (MermerDbContext db, CancellationToken ct) =>
        {
            var list = await db.Stocks
                .Include(s => s.Prices)
                .Include(s => s.Units)
                .AsSplitQuery()
                .AsNoTracking()
                .Where(s => !s.IsDisabled)
                .ToListAsync(ct);

            var result = list.Select(s =>
            {
                var currentPrice = s.Prices?
                    .OrderByDescending(p => p.ValidFrom)
                    .FirstOrDefault();

                var defaultUnit = s.Units?.FirstOrDefault(u => u.IsDefault) ?? s.Units?.FirstOrDefault();

                var pricesList = s.Prices != null && s.Prices.Any()
                    ? s.Prices.Select(p => (object)new
                    {
                        Id = p.Id.ToString(),
                        Price = p.Price,
                        CurrencyId = p.CurrencyId?.ToString(),
                        PriceGroup = p.PriceGroup,
                        ValidFrom = p.ValidFrom
                    }).ToList()
                    : new List<object>();

                var unitsList = s.Units != null && s.Units.Any()
                    ? s.Units.Select(u => (object)new
                    {
                        Id = u.Id.ToString(),
                        Name = u.Name,
                        Multiplier = u.Multiplier,
                        Divider = u.Divider,
                        IsDefault = u.IsDefault
                    }).ToList()
                    : new List<object>();

                return new
                {
                    Id = s.Id.ToString(),
                    Code = s.Code ?? string.Empty,
                    Name = s.Name ?? string.Empty,
                    ShortName = s.ShortName ?? string.Empty,
                    IsDisabled = s.IsDisabled,
                    Type = s.Type ?? string.Empty,
                    Group = s.Group ?? string.Empty,
                    Barcodes = s.Barcodes ?? Array.Empty<string>(),
                    Tags = s.Tags ?? Array.Empty<string>(),

                    Price = currentPrice?.Price ?? 0m,
                    CurrencyId = currentPrice?.CurrencyId?.ToString(),
                    Unit = defaultUnit?.Name ?? string.Empty,
                    UnitId = defaultUnit?.Id.ToString(),

                    Prices = pricesList,
                    Units = unitsList
                };
            });

            return Results.Ok(result);
        })
        .WithName("StocksList");

        group.MapGet("/search", async (string q, string? warehouseId, string? priceGroup, int? limit, double? minSimilarity, IStockSearchService search, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(q)) return Results.BadRequest(new { error = "Query parameter 'q' is required." });
            var result = await search.SearchAsync(q, warehouseId, priceGroup, limit ?? 32, minSimilarity ?? 0.1, ct);
            return Results.Ok(result);
        })
        .WithName("StocksSearch");

        group.MapGet("/next-code", async (MermerDbContext db) =>
        {
            var count = await db.Stocks.CountAsync();
            return Results.Ok(new { code = $"ST-{(count + 1):D6}" });
        })
        .WithName("StocksGetNextCode");

        group.MapGet("/{id}", async (string id, IStocksRepository repo, CancellationToken ct) =>
        {
            var stock = await repo.GetAsync(id, ct);
            return stock is null ? Results.NotFound() : Results.Ok(stock);
        })
        .WithName("StocksGetById");

        group.MapGet("/facets", async (string fields, IStocksRepository repo, CancellationToken ct) =>
        {
            var fieldList = fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (fieldList.Length == 0) return Results.BadRequest(new { error = "Provide fields." });
            var facets = await repo.GetFacetsAsync(fieldList, ct);
            return Results.Ok(facets);
        })
        .WithName("StocksFacets");

        // --- ЖУРНАЛ ДВИЖЕНИЯ ТОВАРОВ (STOCK ACTIONS) ---
        group.MapGet("/actions", async (DateTime? from, DateTime? till, string? stockId, HttpRequest req, MermerDbContext db, CancellationToken ct) =>
        {
            DateTimeOffset startDate = from.HasValue ? new DateTimeOffset(from.Value.ToUniversalTime()) : DateTimeOffset.MinValue;
            DateTimeOffset endDate = till.HasValue ? new DateTimeOffset(till.Value.ToUniversalTime()) : DateTimeOffset.MaxValue;

            var whIds = req.Query["warehouseId"].Select(x => Guid.TryParse(x, out var g) ? (Guid?)g : null).Where(x => x.HasValue).Select(x => x!.Value).ToList();
            Guid? filterStockGuid = Guid.TryParse(stockId, out var sG) ? sG : null;

            var actions = new List<object>();

            // 1. Из складских ордеров (StockSlips) — IsDisabled убран
            var slipsQuery = db.StockSlips.Include(s => s.Lines).ThenInclude(l => l.Stock).AsSplitQuery().AsNoTracking()
                .Where(s => s.Date >= startDate && s.Date <= endDate);

            if (whIds.Any()) slipsQuery = slipsQuery.Where(s => s.WarehouseId.HasValue && whIds.Contains(s.WarehouseId.Value));

            var slips = await slipsQuery.ToListAsync(ct);
            foreach (var s in slips)
            {
                foreach (var l in s.Lines ?? Enumerable.Empty<StockSlipLineEntity>())
                {
                    if (filterStockGuid.HasValue && l.StockId != filterStockGuid) continue;

                    bool isIncome = s.SlipType == "StockOpening" || s.SlipType == "RevisionExceed";
                    actions.Add(new
                    {
                        TransactionId = s.Id.ToString(),
                        TransactionCode = s.Code ?? "",
                        TransactionDate = s.Date.UtcDateTime,
                        TransactionType = s.SlipType,
                        TransactionIsCompleted = s.IsCompleted,
                        TransactionIsDisabled = false,
                        ActionId = l.Id.ToString(),
                        ActionWarehouseId = s.WarehouseId?.ToString(),
                        ActionStockId = l.StockId?.ToString(),
                        StockCode = l.Stock?.Code ?? "",
                        StockName = l.Stock?.Name ?? "",
                        ActionPrice = l.Price,
                        ActionIncome = isIncome ? l.Quantity : 0m,
                        ActionExpense = !isIncome ? l.Quantity : 0m,
                        GrandTotal = l.Price * l.Quantity
                    });
                }
            }

            // 2. Из перемещений (StockTransfers)
            var transfersQuery = db.StockTransfers.Include(t => t.Lines).ThenInclude(l => l.Stock).AsSplitQuery().AsNoTracking()
                .Where(t => t.Date >= startDate && t.Date <= endDate && !t.IsDisabled);

            var transfers = await transfersQuery.ToListAsync(ct);
            foreach (var t in transfers)
            {
                foreach (var l in t.Lines ?? Enumerable.Empty<StockTransferLineEntity>())
                {
                    if (filterStockGuid.HasValue && l.StockId != filterStockGuid) continue;

                    // Списание со склада-источника
                    if (!whIds.Any() || (t.WarehouseId.HasValue && whIds.Contains(t.WarehouseId.Value)))
                    {
                        actions.Add(new
                        {
                            TransactionId = t.Id.ToString(),
                            TransactionCode = t.Code ?? "",
                            TransactionDate = t.Date.UtcDateTime,
                            TransactionType = "StockTransferSource",
                            TransactionIsCompleted = t.IsCompleted,
                            TransactionIsDisabled = t.IsDisabled,
                            ActionId = l.Id.ToString(),
                            ActionWarehouseId = t.WarehouseId?.ToString(),
                            ActionRelatedWarehouseId = t.DestinationWarehouseId?.ToString(),
                            ActionStockId = l.StockId?.ToString(),
                            StockCode = l.Stock?.Code ?? "",
                            StockName = l.Stock?.Name ?? "",
                            ActionPrice = l.Price,
                            ActionIncome = 0m,
                            ActionExpense = l.Quantity,
                            GrandTotal = l.Price * l.Quantity
                        });
                    }

                    // Приход на склад-получатель
                    if (!whIds.Any() || (t.DestinationWarehouseId.HasValue && whIds.Contains(t.DestinationWarehouseId.Value)))
                    {
                        actions.Add(new
                        {
                            TransactionId = t.Id.ToString(),
                            TransactionCode = t.Code ?? "",
                            TransactionDate = t.Date.UtcDateTime,
                            TransactionType = "StockTransferDestination",
                            TransactionIsCompleted = t.IsCompleted,
                            TransactionIsDisabled = t.IsDisabled,
                            ActionId = l.Id.ToString(),
                            ActionWarehouseId = t.DestinationWarehouseId?.ToString(),
                            ActionRelatedWarehouseId = t.WarehouseId?.ToString(),
                            ActionStockId = l.StockId?.ToString(),
                            StockCode = l.Stock?.Code ?? "",
                            StockName = l.Stock?.Name ?? "",
                            ActionPrice = l.Price,
                            ActionIncome = l.ReceivedQuantity,
                            ActionExpense = 0m,
                            GrandTotal = l.Price * l.ReceivedQuantity
                        });
                    }
                }
            }

            // 3. Из продаж и закупок (Invoices)
            var invQuery = db.Invoices.Include(i => i.Lines).ThenInclude(l => l.Stock).AsSplitQuery().AsNoTracking()
                .Where(i => i.Date >= startDate && i.Date <= endDate && !i.IsDisabled && i.IsCompleted);

            if (whIds.Any()) invQuery = invQuery.Where(i => i.WarehouseId.HasValue && whIds.Contains(i.WarehouseId.Value));

            var invoices = await invQuery.ToListAsync(ct);
            foreach (var i in invoices)
            {
                foreach (var l in i.Lines ?? Enumerable.Empty<InvoiceLineEntity>())
                {
                    if (filterStockGuid.HasValue && l.StockId != filterStockGuid) continue;

                    bool isIncome = i.InvoiceType == "Purchase" || i.InvoiceType == "SalesReturn";
                    actions.Add(new
                    {
                        TransactionId = i.Id.ToString(),
                        TransactionCode = i.Code ?? "",
                        TransactionDate = i.Date.UtcDateTime,
                        TransactionType = i.InvoiceType,
                        TransactionIsCompleted = i.IsCompleted,
                        TransactionIsDisabled = i.IsDisabled,
                        ActionId = l.Id.ToString(),
                        ActionWarehouseId = i.WarehouseId?.ToString(),
                        ActionRelatedPartnerId = i.PartnerId?.ToString(),
                        ActionStockId = l.StockId?.ToString(),
                        StockCode = l.Stock?.Code ?? "",
                        StockName = l.Stock?.Name ?? "",
                        ActionPrice = l.Price,
                        ActionIncome = isIncome ? l.Quantity : 0m,
                        ActionExpense = !isIncome ? l.Quantity : 0m,
                        GrandTotal = l.Price * l.Quantity
                    });
                }
            }

            return Results.Ok(actions.OrderByDescending(a => ((dynamic)a).TransactionDate));
        });

        // --- СОХРАНЕНИЕ ---
        Func<HttpRequest, MermerDbContext, Task<IResult>> saveStockHandler = async (request, db) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(body)) return Results.BadRequest("Empty body");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            string idStr = root.TryGetProperty("id", out var idProp) || root.TryGetProperty("Id", out idProp) ? idProp.GetString() : null;
            Guid stockId = Guid.TryParse(idStr, out var parsedGuid) ? parsedGuid : Guid.NewGuid();

            string code = root.TryGetProperty("code", out var codeProp) || root.TryGetProperty("Code", out codeProp) ? codeProp.GetString() : $"ST-{DateTime.UtcNow:yyMMddHHmmss}";
            string name = root.TryGetProperty("name", out var nameProp) || root.TryGetProperty("Name", out nameProp) ? nameProp.GetString() : "Новый товар";
            string type = root.TryGetProperty("type", out var typeProp) || root.TryGetProperty("Type", out typeProp) ? typeProp.GetString() : "";

            var existing = await db.Stocks.FirstOrDefaultAsync(p => p.Id == stockId);
            if (existing == null)
            {
                await db.Stocks.AddAsync(new StockEntity
                {
                    Id = stockId,
                    Code = code,
                    Name = name,
                    Type = type,
                    IsDisabled = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.Code = code;
                existing.Name = name;
                existing.Type = type;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();
            return Results.Ok(new { id = stockId, code });
        };

        group.MapPost("/", saveStockHandler);
        group.MapPut("/{id}", saveStockHandler);

        group.MapDelete("/{id}", async (string id, IStocksRepository repo, CancellationToken ct) =>
        {
            await repo.DeleteAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("StocksDelete");

        return app;
    }
}