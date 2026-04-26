// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Warehousing.Models.StockTransferLine
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Transactions.Models;
using System;

#nullable disable
namespace Payhas.Binyat.Warehousing.Models;

public class StockTransferLine : StockTransactionLine
{
  private string _receivedId;
  private Decimal _receivedQuantity;
  private string _receivedUnitId;

  public StockTransferLine()
  {
    this.AutoRaisePropertyChanged("StockId", nameof (ActionReceivedQuantity));
    this.AutoRaisePropertyChanged(nameof (ActionReceivedQuantity), nameof (IsConflicted), nameof (ActionReceivedTotal));
    this.AutoRaisePropertyChanged("ActionQuantity", nameof (IsConflicted));
    this.AutoRaisePropertyChanged("ActionPrice", nameof (ActionReceivedTotal));
    this.AutoRaisePropertyChanged(nameof (ActionReceivedTotal), nameof (DisplayReceivedTotal));
    this.AutoRaisePropertyChanged(nameof (DisplayReceivedTotal), nameof (DisplayReceivedTotalString));
  }

  public string ReceivedId
  {
    get => this._receivedId;
    set => this.SetProperty<string>(ref this._receivedId, value, nameof (ReceivedId));
  }

  public virtual Decimal ReceivedQuantity
  {
    get => this._receivedQuantity;
    set
    {
      this.SetProperty<Decimal>(ref this._receivedQuantity, value, nameof (ReceivedQuantity), "ActionReceivedQuantity");
    }
  }

  public virtual string ReceivedUnitId
  {
    get => this._receivedUnitId;
    set
    {
      this.SetProperty<string>(ref this._receivedUnitId, value, nameof (ReceivedUnitId), "ActionReceivedQuantity");
    }
  }

  public Decimal ActionReceivedQuantity
  {
    get
    {
      if (string.IsNullOrEmpty(this.StockId) || string.IsNullOrEmpty(this.ReceivedUnitId))
        return 0M;
      StockUnitConvertion stockUnitConvertion = this.GetStockUnitConvertion(this.StockId, this.ReceivedUnitId);
      return stockUnitConvertion == null ? 0M : this.ReceivedQuantity * stockUnitConvertion.Multiplier / stockUnitConvertion.Divider;
    }
  }

  public bool IsConflicted => this.ActionQuantity != this.ActionReceivedQuantity;

  public Decimal ActionReceivedTotal => this.ActionPrice * this.ActionReceivedQuantity;

  public Decimal DisplayReceivedTotal => this.GetDisplayAmount(this.ActionReceivedTotal);

  public string DisplayReceivedTotalString
  {
    get => this.GetDisplayAmountString(this.DisplayReceivedTotal);
  }
}
