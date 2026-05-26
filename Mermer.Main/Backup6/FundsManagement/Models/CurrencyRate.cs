// Decompiled with JetBrains decompiler
// Type: Mermer.FundsManagement.Models.CurrencyRate
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Data.Models;
using System;

#nullable disable
namespace Mermer.FundsManagement.Models;

public class CurrencyRate : BindableObject
{
  private string _id;
  private DateTime _validFrom = DateTime.Today;
  private Decimal _multiplier;
  private Decimal _divider;

  public CurrencyRate() => this.Id = Guid.NewGuid().ToString();

  public string Id
  {
    get => this._id;
    set => this.SetProperty<string>(ref this._id, value, nameof (Id));
  }

  public virtual DateTime ValidFrom
  {
    get => this._validFrom;
    set => this.SetProperty<DateTime>(ref this._validFrom, value, nameof (ValidFrom));
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
