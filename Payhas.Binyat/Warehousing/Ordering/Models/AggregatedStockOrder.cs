// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Warehousing.Ordering.Models.AggregatedStockOrder
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Transactions.Models;
using Payhas.Data;

#nullable disable
namespace Payhas.Binyat.Warehousing.Ordering.Models;

public class AggregatedStockOrder : TransactionModel
{
  private string _warehouseId;
  private WatchedObservableCollection<AggregatedStockOrderLine> _lines;

  public string WarehouseId
  {
    get => this._warehouseId;
    set => this.SetProperty<string>(ref this._warehouseId, value, nameof (WarehouseId));
  }

  public override string Type => nameof (AggregatedStockOrder);

  public WatchedObservableCollection<AggregatedStockOrderLine> Lines
  {
    get => this._lines;
    set
    {
      this.SetProperty<WatchedObservableCollection<AggregatedStockOrderLine>>(ref this._lines, value, nameof (Lines));
    }
  }
}
