// Decompiled with JetBrains decompiler
// Type: Mermer.Warehousing.Models.StockTransfer
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Transactions.Models;
using Mermer.Data;
using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Warehousing.Models;

public class StockTransfer : StockTransaction<StockTransferLine>
{
  private string _destinationWarehouseId;

  public StockTransfer()
  {
    this.AutoRaisePropertyChanged("Lines", nameof (ActionReceivedTotal), nameof (IsConflicted));
    this.AutoRaisePropertyChanged("CurrencyConvertions", nameof (ActionReceivedTotal), nameof (IsConflicted));
    this.AutoRaisePropertyChanged("StockUnitConvertions", nameof (ActionReceivedTotal));
  }

  public string DestinationWarehouseId
  {
    get => this._destinationWarehouseId;
    set
    {
      this.SetProperty<string>(ref this._destinationWarehouseId, value, nameof (DestinationWarehouseId));
    }
  }

  public override bool IsStockIncome => false;

  public override string Type => nameof (StockTransfer);

  public Decimal ActionReceivedTotal
  {
    get
    {
      WatchedObservableCollection<StockTransferLine> lines = this.Lines;
      return lines == null ? 0M : lines.Sum<StockTransferLine>((Func<StockTransferLine, Decimal>) (x => x.ActionReceivedTotal));
    }
  }

  public bool IsConflicted
  {
    get
    {
      WatchedObservableCollection<StockTransferLine> lines = this.Lines;
      return lines != null && lines.Any<StockTransferLine>((Func<StockTransferLine, bool>) (x => x.IsConflicted));
    }
  }

  protected override void LinePropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    base.LinePropertyChanged(sender, e);
    if (e.PropertyName == "ActionReceivedTotal")
      this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.ActionReceivedTotal));
    if (!(e.PropertyName == "IsConflicted"))
      return;
    this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsConflicted));
  }
}
