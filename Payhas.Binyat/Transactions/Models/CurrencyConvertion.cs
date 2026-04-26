// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Transactions.Models.CurrencyConvertion
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Data.Models;
using System;

#nullable disable
namespace Payhas.Binyat.Transactions.Models;

public class CurrencyConvertion : BindableObject
{
  private string _id;
  private string _currencyId;
  private Decimal _multiplier;
  private Decimal _divider;

  public CurrencyConvertion() => this.Id = Guid.NewGuid().ToString();

  public string Id
  {
    get => this._id;
    set => this.SetProperty<string>(ref this._id, value, nameof (Id));
  }

  public virtual string CurrencyId
  {
    get => this._currencyId;
    set => this.SetProperty<string>(ref this._currencyId, value, nameof (CurrencyId));
  }

  public virtual Decimal Multiplier
  {
    get => this._multiplier;
    set => this.SetProperty<Decimal>(ref this._multiplier, value, nameof (Multiplier));
  }

  public virtual Decimal Divider
  {
    get => this._divider;
    set => this.SetProperty<Decimal>(ref this._divider, value, nameof (Divider));
  }
}
