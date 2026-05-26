// Decompiled with JetBrains decompiler
// Type: Mermer.CRM.Models.PartnerBalanceResult
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Data.Models;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.CRM.Models;

public class PartnerBalanceResult : BindableObject
{
  private Decimal _balance;

  public virtual Decimal Balance
  {
    get => this._balance;
    set
    {
      this.SetProperty<Decimal>(ref this._balance, value, nameof (Balance));
      this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.Debit));
      this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.Credit));
    }
  }

  public Decimal Debit => !(this.Balance > 0M) ? 0M : this.Balance;

  public Decimal Credit => !(this.Balance < 0M) ? 0M : this.Balance * -1M;
}
