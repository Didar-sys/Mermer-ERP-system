// Decompiled with JetBrains decompiler
// Type: Mermer.Transactions.Models.FundsTransactionLine
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Transactions.Models;

public abstract class FundsTransactionLine : TransactionLine
{
  private Decimal _amount;

  public Decimal Amount
  {
    get => this._amount;
    set => this.SetProperty<Decimal>(ref this._amount, value, nameof (Amount), "ActionTotal");
  }

  public override Decimal ActionTotal
  {
    get
    {
      if (string.IsNullOrEmpty(this.CurrencyId))
        return 0M;
      CurrencyConvertion currencyConvertion = this.GetCurrencyConvertion(this.CurrencyId);
      return currencyConvertion == null ? 0M : this.Amount * currencyConvertion.Multiplier / currencyConvertion.Divider;
    }
  }

  public override void UpdateCurrencyConvertion()
  {
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.ActionTotal));
  }
}
