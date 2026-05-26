// Decompiled with JetBrains decompiler
// Type: Mermer.Finance.Models.FundsTransferLine
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Transactions.Models;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Finance.Models;

public class FundsTransferLine : FundsTransactionLine
{
  private Decimal _receivedAmount;

  public FundsTransferLine()
  {
    this.AutoRaisePropertyChanged(nameof (ActionReceivedTotal), nameof (DisplayReceivedTotal));
  }

  public virtual Decimal ReceivedAmount
  {
    get => this._receivedAmount;
    set
    {
      this.SetProperty<Decimal>(ref this._receivedAmount, value, nameof (ReceivedAmount), "ActionReceivedTotal");
    }
  }

  public Decimal ActionReceivedTotal
  {
    get
    {
      if (string.IsNullOrEmpty(this.CurrencyId))
        return 0M;
      CurrencyConvertion currencyConvertion = this.GetCurrencyConvertion(this.CurrencyId);
      return currencyConvertion == null ? 0M : this.ReceivedAmount * currencyConvertion.Multiplier / currencyConvertion.Divider;
    }
  }

  public Decimal DisplayReceivedTotal => this.GetDisplayAmount(this.ActionReceivedTotal);

  public override void UpdateCurrencyConvertion()
  {
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.ActionTotal));
  }
}
