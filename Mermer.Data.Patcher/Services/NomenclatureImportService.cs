using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mermer.Data.Postgres;
using Mermer.Data.Postgres.Entities;

namespace Mermer.Data.Patcher.Services;

public class NomenclatureImportService
{
    private readonly MermerDbContext _dbContext;

    public NomenclatureImportService(MermerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Полный импорт справочника Номенклатуры (Stock), Единиц измерения (StockUnit) и Цен (StockPrice)
    /// </summary>
    public async Task MigrateStocksAsync(string jsonFilePath)
    {
        Console.WriteLine("Начинаем импорт справочника Номенклатуры (Stock)...");

        // Выгружаем существующие CurrencyId, чтобы не нарушать Foreign Key у цен
        var validCurrencyIds = (await _dbContext.Currencies.Select(c => c.Id).ToListAsync()).ToHashSet();

        using var stream = File.OpenRead(jsonFilePath);
        using var reader = new StreamReader(stream);

        var stocksBatch = new List<StockEntity>();
        var unitsBatch = new List<StockUnitEntity>();
        var pricesBatch = new List<StockPriceEntity>();

        var processedStockIds = new HashSet<Guid>();
        var processedUnitIds = new HashSet<Guid>();
        var processedPriceIds = new HashSet<Guid>();

        string? line;
        int totalStocks = 0;
        int totalUnits = 0;
        int totalPrices = 0;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement;

            if (IsTargetDocType(root, "Stock"))
            {
                var targetContainer = GetTargetContainer(root);
                if (!TryGetGuidProperty(root, targetContainer, "id", out var stockId)) continue;

                if (!processedStockIds.Add(stockId)) continue; // Пропуск дубликатов товара

                // 1. Извлекаем главный объект Stock
                var stock = new StockEntity
                {
                    Id = stockId,
                    Code = GetStringProperty(targetContainer, "code"),
                    Name = GetStringProperty(targetContainer, "name") ?? "Без названия",
                    ShortName = GetStringProperty(targetContainer, "shortName"),
                    Type = GetStringProperty(targetContainer, "type"),
                    Group = GetStringProperty(targetContainer, "group"),
                    Description = GetStringProperty(targetContainer, "description"),
                    IsDisabled = GetBoolProperty(targetContainer, "isDisabled"),
                    LimitMin = GetDecimalProperty(targetContainer, "limitMin"),
                    LimitMax = GetDecimalProperty(targetContainer, "limitMax"),
                    Tags = GetStringArrayProperty(targetContainer, "tags"),
                    Barcodes = GetStringArrayProperty(targetContainer, "barcodes"),
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                stocksBatch.Add(stock);

                // 2. Извлекаем вложенные Единицы Измерения (units)
                ExtractUnits(targetContainer, stockId, unitsBatch, processedUnitIds);

                // 3. Извлекаем вложенные Цены (prices)
                ExtractPrices(targetContainer, stockId, validCurrencyIds, pricesBatch, processedPriceIds);

                // Сохранение пачками по 500 штук
                if (stocksBatch.Count >= 500)
                {
                    await SaveNomenclatureBatchAsync(stocksBatch, unitsBatch, pricesBatch);
                    totalStocks += stocksBatch.Count;
                    totalUnits += unitsBatch.Count;
                    totalPrices += pricesBatch.Count;

                    stocksBatch.Clear();
                    unitsBatch.Clear();
                    pricesBatch.Clear();

                    Console.WriteLine($"Сохранено товаров: {totalStocks}...");
                }
            }
        }

        if (stocksBatch.Any())
        {
            await SaveNomenclatureBatchAsync(stocksBatch, unitsBatch, pricesBatch);
            totalStocks += stocksBatch.Count;
            totalUnits += unitsBatch.Count;
            totalPrices += pricesBatch.Count;
        }

        Console.WriteLine($"Готово! Импортировано Складов/Товаров: {totalStocks}, Единиц: {totalUnits}, Цен: {totalPrices}");
    }

    private async Task SaveNomenclatureBatchAsync(
        List<StockEntity> stocks,
        List<StockUnitEntity> units,
        List<StockPriceEntity> prices)
    {
        if (stocks.Any()) await _dbContext.Stocks.AddRangeAsync(stocks);
        if (units.Any()) await _dbContext.StockUnits.AddRangeAsync(units);
        if (prices.Any()) await _dbContext.StockPrices.AddRangeAsync(prices);

        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
    }

    #region Вспомогательные методы парсинга

    private static void ExtractUnits(JsonElement container, Guid stockId, List<StockUnitEntity> unitsBatch, HashSet<Guid> processedUnitIds)
    {
        if (container.TryGetProperty("units", out var unitsArray) && unitsArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var unitElem in unitsArray.EnumerateArray())
            {
                if (!unitElem.TryGetProperty("id", out var idProp) || !Guid.TryParse(idProp.GetString(), out var unitId))
                    continue;

                if (!processedUnitIds.Add(unitId)) continue;

                unitsBatch.Add(new StockUnitEntity
                {
                    Id = unitId,
                    StockId = stockId,
                    Name = GetStringProperty(unitElem, "name") ?? "шт",
                    Multiplier = GetDecimalProperty(unitElem, "multiplier") ?? 1m,
                    Divider = GetDecimalProperty(unitElem, "divider") ?? 1m,
                    IsDefault = GetBoolProperty(unitElem, "isDefault")
                });
            }
        }
    }

    private static void ExtractPrices(JsonElement container, Guid stockId, HashSet<Guid> validCurrencyIds, List<StockPriceEntity> pricesBatch, HashSet<Guid> processedPriceIds)
    {
        if (container.TryGetProperty("prices", out var pricesArray) && pricesArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var priceElem in pricesArray.EnumerateArray())
            {
                if (!priceElem.TryGetProperty("id", out var idProp) || !Guid.TryParse(idProp.GetString(), out var priceId))
                    continue;

                if (!processedPriceIds.Add(priceId)) continue;

                Guid? currencyId = null;
                if (TryGetGuidProperty(priceElem, priceElem, "currencyId", out var cId) && validCurrencyIds.Contains(cId))
                {
                    currencyId = cId;
                }

                DateTime validFrom = DateTime.UtcNow;
                if (priceElem.TryGetProperty("validFrom", out var vfProp) && vfProp.ValueKind == JsonValueKind.String)
                {
                    DateTime.TryParse(vfProp.GetString(), out validFrom);
                }

                pricesBatch.Add(new StockPriceEntity
                {
                    Id = priceId,
                    StockId = stockId,
                    CurrencyId = currencyId,
                    Price = GetDecimalProperty(priceElem, "price") ?? 0m,
                    PriceGroup = GetStringProperty(priceElem, "priceGroup"),
                    ValidFrom = DateTime.SpecifyKind(validFrom, DateTimeKind.Utc),
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
        }
    }

    private static bool IsTargetDocType(JsonElement root, string targetDocType)
    {
        if (root.ValueKind != JsonValueKind.Object) return false;
        if (root.TryGetProperty("docType", out var dt) && dt.ValueKind == JsonValueKind.String)
            return dt.GetString() == targetDocType;
        if (root.TryGetProperty("patch", out var patch) && patch.ValueKind == JsonValueKind.Object &&
            patch.TryGetProperty("docType", out var pdt) && pdt.ValueKind == JsonValueKind.String)
            return pdt.GetString() == targetDocType;

        return false;
    }

    private static JsonElement GetTargetContainer(JsonElement root)
    {
        if (root.TryGetProperty("patch", out var patch) && patch.ValueKind == JsonValueKind.Object)
        {
            if (patch.TryGetProperty("propertyPatches", out var props) && props.ValueKind == JsonValueKind.Object)
                return props;
            return patch;
        }
        return root;
    }

    private static bool TryGetGuidProperty(JsonElement root, JsonElement container, string propertyName, out Guid result)
    {
        result = Guid.Empty;
        if (container.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
            return Guid.TryParse(prop.GetString(), out result);

        if (root.TryGetProperty("patch", out var patch) && patch.ValueKind == JsonValueKind.Object &&
            patch.TryGetProperty(propertyName, out var patchProp) && patchProp.ValueKind == JsonValueKind.String)
            return Guid.TryParse(patchProp.GetString(), out result);

        if (root.TryGetProperty(propertyName, out var rootProp) && rootProp.ValueKind == JsonValueKind.String)
            return Guid.TryParse(rootProp.GetString(), out result);

        return false;
    }

    private static string? GetStringProperty(JsonElement container, string propertyName)
    {
        if (container.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
            return prop.GetString();
        return null;
    }

    private static string[]? GetStringArrayProperty(JsonElement container, string propertyName)
    {
        if (container.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Array)
        {
            return prop.EnumerateArray()
                       .Where(x => x.ValueKind == JsonValueKind.String)
                       .Select(x => x.GetString()!)
                       .ToArray();
        }
        return null;
    }

    private static decimal? GetDecimalProperty(JsonElement container, string propertyName)
    {
        if (container.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number)
            return prop.GetDecimal();
        return null;
    }

    private static bool GetBoolProperty(JsonElement container, string propertyName)
    {
        if (container.TryGetProperty(propertyName, out var prop))
        {
            if (prop.ValueKind == JsonValueKind.True) return true;
            if (prop.ValueKind == JsonValueKind.False) return false;
        }
        return false;
    }

    #endregion
}