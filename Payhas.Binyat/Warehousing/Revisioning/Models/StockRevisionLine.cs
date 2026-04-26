// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Warehousing.Revisioning.Models.StockRevisionLine
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Common.Models;
using System;

#nullable disable
namespace Payhas.Binyat.Warehousing.Revisioning.Models;

public class StockRevisionLine : Model
{
  private string _stockRevisionId;
  private string _stockId;
  private DateTime _date;
  private Decimal _quantity;
  private string _unitId;
  private Decimal? _price;
  private string _currencyId;
  private string _userId;
  private string _userName;

  public string StockRevisionId
  {
    get => this._stockRevisionId;
    set => this.SetProperty<string>(ref this._stockRevisionId, value, nameof (StockRevisionId));
  }

  public string StockId
  {
    get => this._stockId;
    set => this.SetProperty<string>(ref this._stockId, value, nameof (StockId));
  }

  public DateTime Date
  {
    get => this._date;
    set => this.SetProperty<DateTime>(ref this._date, value, nameof (Date));
  }

  public Decimal Quantity
  {
    get => this._quantity;
    set => this.SetProperty<Decimal>(ref this._quantity, value, nameof (Quantity));
  }

  public string UnitId
  {
    get => this._unitId;
    set => this.SetProperty<string>(ref this._unitId, value, nameof (UnitId));
  }

  public Decimal? Price
  {
    get => this._price;
    set => this.SetProperty<Decimal?>(ref this._price, value, nameof (Price));
  }

  public string CurrencyId
  {
    get => this._currencyId;
    set => this.SetProperty<string>(ref this._currencyId, value, nameof (CurrencyId));
  }

  public string UserId
  {
    get => this._userId;
    set => this.SetProperty<string>(ref this._userId, value, nameof (UserId));
  }

  public string UserName
  {
    get => this._userName;
    set => this.SetProperty<string>(ref this._userName, value, nameof (UserName));
  }
}
