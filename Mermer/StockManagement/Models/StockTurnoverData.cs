using System;
using System.Collections.Generic;

namespace Mermer.StockManagement.Models;

public class StockTurnoverData
{
    public string WarehouseId { get; set; }
    public string StockId { get; set; }
    public string StockCode { get; set; }
    public string StockName { get; set; }
    public string StockType { get; set; }
    public string StockGroup { get; set; }
    public IEnumerable<string> StockTags { get; set; }

    // Свойство для отображения в таблице
    public string DisplayTags => StockTags != null ? string.Join(", ", StockTags) : string.Empty;

    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Sellable => this.Income - this.Expense;
    public decimal Sold { get; set; }

    public int Turnover => !(this.Sellable > 0m) ? 0 : Convert.ToInt32(100m * this.Sold / this.Sellable);
}