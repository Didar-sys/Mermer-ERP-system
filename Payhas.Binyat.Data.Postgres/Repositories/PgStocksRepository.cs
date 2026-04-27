using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Payhas.Binyat.StockManagement.Models;
using Payhas.Binyat.StockManagement.Services;
using Payhas.Binyat.Data.Postgres.Entities;
using Payhas.Data.Storage;

namespace Payhas.Binyat.Data.Postgres.Repositories;

/// <summary>
/// PostgreSQL implementation of IStocksRepository.
/// Replaces Couchbase StocksRepository: eliminates View queries and bucket round-trips.
/// All IDs are bridged: domain models use string IDs, PostgreSQL uses UUID.
/// </summary>
public class PgStocksRepository : IStocksRepository
{
    private readonly PayhasDbContext _db;
    private readonly string _connectionString;

    public PgStocksRepository(PayhasDbContext db, string connectionString)
    {
        _db = db;
        _connectionString = connectionString;
    }

    // ─── IReadOnlyRepository<Stock> ──────────────────────────────────────────

    public async Task<Stock> GetAsync(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return null;

        var entity = await _db.Stocks
            .Include(s => s.Units)
            .Include(s => s.Prices)
            .Include(s => s.AdditionalPrices)
            .FirstOrDefaultAsync(s => s.Id == guid);

        return entity == null ? null : MapToModel(entity);
    }

    public async Task<IEnumerable<Stock>> GetAsync(
        params Expression<Func<Stock, bool>>[] filters)
    {
        // For full list — load all stocks and filter in memory
        // (filters are LINQ expressions on domain model, not EF entities)
        var entities = await _db.Stocks
            .Include(s => s.Units)
            .Include(s => s.Prices)
            .Include(s => s.AdditionalPrices)
            .Where(s => !s.IsDisabled)
            .OrderBy(s => s.Name)
            .ToListAsync();

        var stocks = entities.Select(MapToModel);

        foreach (var filter in filters)
            stocks = stocks.AsQueryable().Where(filter.Compile());

        return stocks;
    }

    // ─── IStocksRepository ───────────────────────────────────────────────────

    public async Task<IEnumerable<Stock>> GetListAsync(params string[] stockIds)
    {
        var guids = stockIds
            .Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .ToArray();

        var entities = await _db.Stocks
            .Include(s => s.Units)
            .Include(s => s.Prices)
            .Include(s => s.AdditionalPrices)
            .Where(s => guids.Contains(s.Id))
            .ToListAsync();

        return entities.Select(MapToModel);
    }

    public async Task<IEnumerable<StockInfo>> GetInfoAsync(params string[] stockIds)
    {
        const string sql = """
            SELECT
                s.id,
                s.code,
                s.name,
                s.short_name,
                s.type,
                s.group_name AS "group",
                s.tags,
                s.barcodes,
                s.is_disabled,
                u.name        AS unit,
                u.id          AS unit_id,
                p.price,
                p.currency_id
            FROM stocks s
            LEFT JOIN LATERAL (
                SELECT name, id FROM stock_units
                WHERE stock_id = s.id AND is_default = true
                LIMIT 1
            ) u ON true
            LEFT JOIN LATERAL (
                SELECT price, currency_id FROM stock_prices
                WHERE stock_id = s.id
                ORDER BY valid_from DESC
                LIMIT 1
            ) p ON true
            WHERE (@ids::uuid[] IS NULL OR s.id = ANY(@ids))
            ORDER BY s.name
            """;

        await using var conn = new NpgsqlConnection(_connectionString);

        Guid[]? guidIds = stockIds.Length > 0
            ? stockIds.Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
                      .Where(g => g != Guid.Empty).ToArray()
            : null;

        var rows = await conn.QueryAsync(sql, new { ids = guidIds });

        return rows.Select(r => new StockInfo
        {
            Id           = r.id.ToString(),
            Code         = r.code,
            Name         = r.name,
            ShortName    = r.short_name,
            Unit         = r.unit,
            Price        = r.price ?? 0m,
            CurrencyId   = r.currency_id?.ToString(),
            Type         = r.type,
            Group        = r.group,
            Tags         = ((string[])r.tags)?.ToList(),
            Barcodes     = ((string[])r.barcodes)?.ToList(),
            IsDisabled   = r.is_disabled
        });
    }

    public async Task<IEnumerable<StockInfo>> GetInfoAsync(
        string additionalPriceCurrencyId,
        string additionalPriceGroup)
    {
        // Load base info
        var infos = (await GetInfoAsync(Array.Empty<string>())).ToList();

        if (string.IsNullOrEmpty(additionalPriceCurrencyId) &&
            string.IsNullOrEmpty(additionalPriceGroup))
            return infos;

        // Load additional prices per stock for the requested group
        const string sql = """
            SELECT
                ap.stock_id,
                ap.price,
                ap.currency_id
            FROM stock_additional_prices ap
            WHERE (@group IS NULL OR ap.price_group = @group)
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        var additionalPrices = (await conn.QueryAsync(sql,
                new { group = string.IsNullOrEmpty(additionalPriceGroup) ? null : additionalPriceGroup }))
            .ToDictionary(r => ((Guid)r.stock_id).ToString());

        foreach (var info in infos)
        {
            if (additionalPrices.TryGetValue(info.Id, out var ap))
            {
                info.AdditionalPrice           = ap.price ?? 0m;
                info.AdditionalPriceCurrencyId = ap.currency_id?.ToString();
            }
        }

        return infos;
    }

    public async Task MergeAsync(string mainStockId, string[] mergeStockIds, bool disableMergedItems)
    {
        if (!Guid.TryParse(mainStockId, out var mainGuid))
            throw new ArgumentException("Invalid mainStockId", nameof(mainStockId));

        var mergeGuids = mergeStockIds
            .Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .ToArray();

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Update all invoice lines referencing merged stocks
            await _db.Database.ExecuteSqlRawAsync("""
                UPDATE invoice_lines
                SET stock_id = {0}
                WHERE stock_id = ANY({1})
                """, mainGuid, mergeGuids);

            if (disableMergedItems)
            {
                // Collect barcodes from merged stocks
                var mergedStocks = await _db.Stocks
                    .Where(s => mergeGuids.Contains(s.Id))
                    .ToListAsync();

                var mainStock = await _db.Stocks.FindAsync(mainGuid);
                if (mainStock != null)
                {
                    var allBarcodes = (mainStock.Barcodes?.ToList() ?? new List<string>());
                    foreach (var s in mergedStocks)
                    {
                        allBarcodes.Add(s.Code ?? s.Id.ToString());
                        if (s.Barcodes != null) allBarcodes.AddRange(s.Barcodes);
                        s.IsDisabled = true;
                        s.UpdatedAt  = DateTimeOffset.UtcNow;
                    }
                    mainStock.Barcodes  = allBarcodes.Distinct().ToArray();
                    mainStock.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ─── IRepository<Stock> ──────────────────────────────────────────────────

    public async Task<Stock> CreateAsync(Stock model)
    {
        model.Id ??= Guid.NewGuid().ToString();
        var entity = MapToEntity(model);
        entity.CreatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        _db.Stocks.Add(entity);
        await _db.SaveChangesAsync();
        return model;
    }

    public async Task<Stock> UpdateAsync(Stock model)
    {
        if (!Guid.TryParse(model.Id, out var guid))
            throw new ArgumentException("Invalid stock ID");

        var entity = await _db.Stocks
            .Include(s => s.Units)
            .Include(s => s.Prices)
            .Include(s => s.AdditionalPrices)
            .FirstOrDefaultAsync(s => s.Id == guid)
            ?? throw new InvalidOperationException($"Stock {guid} not found");

        // Apply domain changes to entity
        entity.Code        = model.Code;
        entity.Name        = model.Name;
        entity.ShortName   = model.ShortName;
        entity.Type        = model.Type;
        entity.Group       = model.Group;
        entity.Tags        = model.Tags?.ToArray();
        entity.Barcodes    = model.Barcodes?.ToArray();
        entity.LimitMin    = model.LimitMin;
        entity.LimitMax    = model.LimitMax;
        entity.Description = model.Description;
        entity.IsDisabled  = model.IsDisabled;
        entity.UpdatedAt   = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();
        return model;
    }

    public async Task DeleteAsync(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return;
        var entity = await _db.Stocks.FindAsync(guid);
        if (entity != null)
        {
            // Soft delete
            entity.IsDisabled = true;
            entity.UpdatedAt  = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    public async Task ValidateAsync(Stock model)
    {
        // Validate unique code
        if (!string.IsNullOrEmpty(model.Code))
        {
            Guid.TryParse(model.Id, out var selfGuid);
            var exists = await _db.Stocks
                .AnyAsync(s => s.Code == model.Code && s.Id != selfGuid);
            if (exists)
                throw new InvalidOperationException(
                    $"Stock with code '{model.Code}' already exists.");
        }
    }

    public async Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
    {
        // Returns group/type/tags facets for filter UI
        var result = new Dictionary<string, Dictionary<string, int>>();

        if (fields.Contains("group", StringComparer.OrdinalIgnoreCase))
        {
            result["group"] = await _db.Stocks
                .Where(s => s.Group != null && !s.IsDisabled)
                .GroupBy(s => s.Group!)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        if (fields.Contains("type", StringComparer.OrdinalIgnoreCase))
        {
            result["type"] = await _db.Stocks
                .Where(s => s.Type != null && !s.IsDisabled)
                .GroupBy(s => s.Type!)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        return result;
    }

    // ─── Mapping helpers ─────────────────────────────────────────────────────

    private static Stock MapToModel(StockEntity e)
    {
        var stock = new Stock
        {
            Id          = e.Id.ToString(),
            Code        = e.Code,
            Name        = e.Name,
            ShortName   = e.ShortName,
            Type        = e.Type,
            Group       = e.Group,
            Tags        = e.Tags?.ToList<string>(),
            Barcodes    = e.Barcodes?.ToList<string>(),
            LimitMin    = e.LimitMin,
            LimitMax    = e.LimitMax,
            Description = e.Description,
            IsDisabled  = e.IsDisabled
        };

        if (e.Units?.Any() == true)
        {
            stock.Units = new System.Collections.ObjectModel.ObservableCollection<StockUnit>(
                e.Units.Select(u => new StockUnit
                {
                    Id         = u.Id.ToString(),
                    Name       = u.Name,
                    IsDefault  = u.IsDefault,
                    Multiplier = u.Multiplier,
                    Divider    = u.Divider
                }));
        }

        return stock;
    }

    private static StockEntity MapToEntity(Stock m)
    {
        Guid.TryParse(m.Id, out var guid);
        return new StockEntity
        {
            Id          = guid == Guid.Empty ? Guid.NewGuid() : guid,
            Code        = m.Code,
            Name        = m.Name,
            ShortName   = m.ShortName,
            Type        = m.Type,
            Group       = m.Group,
            Tags        = m.Tags?.ToArray(),
            Barcodes    = m.Barcodes?.ToArray(),
            LimitMin    = m.LimitMin,
            LimitMax    = m.LimitMax,
            Description = m.Description,
            IsDisabled  = m.IsDisabled,
            CreatedAt   = DateTimeOffset.UtcNow,
            UpdatedAt   = DateTimeOffset.UtcNow
        };
    }
}
