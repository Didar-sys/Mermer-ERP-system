// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Warehousing.Ordering.Models.StockOrderLine
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Data.Models;
using System;

#nullable disable
namespace Payhas.Binyat.Warehousing.Ordering.Models;

public class StockOrderLine : BindableObject
{
  private string _id;
  private string _stockId;
  private Decimal _quantity;
  private string _unitId;

  public StockOrderLine() => this.Id = Guid.NewGuid().ToString();

  public string Id
  {
    get => this._id;
    set => this.SetProperty<string>(ref this._id, value, nameof (Id));
  }

  public virtual string StockId
  {
    get => this._stockId;
    set => this.SetProperty<string>(ref this._stockId, value, nameof (StockId));
  }

  public virtual Decimal Quantity
  {
    get => this._quantity;
    set => this.SetProperty<Decimal>(ref this._quantity, value, nameof (Quantity));
  }

  public virtual string UnitId
  {
    get => this._unitId;
    set => this.SetProperty<string>(ref this._unitId, value, nameof (UnitId));
  }
}
