// Decompiled with JetBrains decompiler
// Type: Mermer.Warehousing.Ordering.Models.StockOrder
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Transactions.Models;
using System.Collections.ObjectModel;

#nullable disable
namespace Mermer.Warehousing.Ordering.Models;

public class StockOrder : TransactionModel
{
  private string _warehouseId;
  private ObservableCollection<StockOrderLine> _lines;
  private ObservableCollection<StockUnitConvertion> _stockUnitConvertions;

  public virtual string WarehouseId
  {
    get => this._warehouseId;
    set => this.SetProperty<string>(ref this._warehouseId, value, nameof (WarehouseId));
  }

  public override string Type => nameof (StockOrder);

  public virtual ObservableCollection<StockOrderLine> Lines
  {
    get => this._lines;
    set
    {
      this.SetProperty<ObservableCollection<StockOrderLine>>(ref this._lines, value, nameof (Lines));
    }
  }

  public virtual ObservableCollection<StockUnitConvertion> StockUnitConvertions
  {
    get => this._stockUnitConvertions;
    set
    {
      this.SetProperty<ObservableCollection<StockUnitConvertion>>(ref this._stockUnitConvertions, value, nameof (StockUnitConvertions));
    }
  }
}
