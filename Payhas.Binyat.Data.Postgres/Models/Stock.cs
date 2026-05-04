using System;
using System.Collections.Generic;

namespace Payhas.Binyat.Data.Postgres.Models;

/// <summary>
/// Stock (product / item) — POCO model decoupled from the legacy WPF MVVM
/// <c>Payhas.Binyat.StockManagement.Models.Stock</c>. The PG repository layer
/// works with these POCOs; an adapter on the WPF side will project them back
/// into the MVVM model when the UI is integrated.
/// </summary>
public class Stock
{
    public string? Id { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Type { get; set; }
    public string? Group { get; set; }
    public List<string>? Tags { get; set; }
    public List<string>? Barcodes { get; set; }
    public decimal? LimitMin { get; set; }
    public decimal? LimitMax { get; set; }
    public string? Description { get; set; }
    public bool IsDisabled { get; set; }

    public List<StockUnit> Units { get; set; } = new();
    public List<StockPrice> Prices { get; set; } = new();
    public List<StockAdditionalPrice> AdditionalPrices { get; set; } = new();
}

public class StockUnit
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public decimal Multiplier { get; set; } = 1m;
    public decimal Divider { get; set; } = 1m;
}

public class StockPrice
{
    public string? Id { get; set; }
    public DateTime ValidFrom { get; set; } = DateTime.UtcNow.Date;
    public decimal Price { get; set; }
    public string? CurrencyId { get; set; }
    public string? PriceGroup { get; set; }
}

public class StockAdditionalPrice
{
    public string? Id { get; set; }
    public DateTime ValidFrom { get; set; } = DateTime.UtcNow.Date;
    public decimal Price { get; set; }
    public string? CurrencyId { get; set; }
    public string? PriceGroup { get; set; }
}

/// <summary>
/// Lightweight aggregated info for stock listings — replaces the
/// Couchbase StocksInfoView projection.
/// </summary>
public class StockInfo
{
    public string? Id { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Unit { get; set; }
    public decimal Price { get; set; }
    public string? CurrencyId { get; set; }
    public decimal? AdditionalPrice { get; set; }
    public string? AdditionalPriceCurrencyId { get; set; }
    public string? Type { get; set; }
    public string? Group { get; set; }
    public List<string>? Tags { get; set; }
    public List<string>? Barcodes { get; set; }
    public bool IsDisabled { get; set; }
}

/// <summary>
/// Raw warehouse balance (income − expense).
/// </summary>
public class StockBalance
{
    public string WarehouseId { get; set; } = string.Empty;
    public string StockId { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Balance => Income - Expense;
}

/// <summary>
/// Balance variant carrying display info from the parent stock.
/// </summary>
public class StockBalanceWithCodeAndName : StockBalance
{
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Time-series balance breakdown row (running balance, per-day, per-warehouse,
/// split by invoice type). Replaces the
/// StockActionsToStockBalanceByTypeByDay Couchbase view.
/// </summary>
public class StockBalanceByTypeWithBalanceAndData
{
    public DateTime Date { get; set; }
    public string InvoiceType { get; set; } = string.Empty;
    public string WarehouseId { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Total { get; set; }
    public decimal RunningBalance { get; set; }
}

/// <summary>
/// Aggregated balance across warehouses for a stock (used in the
/// "balances by date and warehouses" report).
/// </summary>
public class StockBalanceByWarehouses
{
    public string StockId { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Group { get; set; }
    public string? Type { get; set; }
    public string? Unit { get; set; }
    public decimal Price { get; set; }
    public string? CurrencyId { get; set; }

    /// <summary>
    /// JSON-encoded array of <c>{ warehouseId, warehouseName, balance }</c>.
    /// Stored as a string for fidelity with the Couchbase contract; callers
    /// can deserialize on demand.
    /// </summary>
    public string? WarehouseBalances { get; set; }
}
