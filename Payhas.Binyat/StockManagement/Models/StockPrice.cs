// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.StockManagement.Models.StockPrice
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Data.Models;
using System;

#nullable disable
namespace Payhas.Binyat.StockManagement.Models;

public class StockPrice : BindableObject
{
  private string _id;
  private DateTime _validFrom;
  private Decimal _price;
  private string _currencyId;

  public StockPrice() => this.Id = Guid.NewGuid().ToString();

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

  public virtual Decimal Price
  {
    get => this._price;
    set => this.SetProperty<Decimal>(ref this._price, value, nameof (Price));
  }

  public virtual string CurrencyId
  {
    get => this._currencyId;
    set => this.SetProperty<string>(ref this._currencyId, value, nameof (CurrencyId));
  }
}
