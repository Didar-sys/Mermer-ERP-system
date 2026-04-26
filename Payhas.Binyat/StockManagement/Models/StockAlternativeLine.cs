// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.StockManagement.Models.StockAlternativeLine
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Data.Models;
using System;

#nullable disable
namespace Payhas.Binyat.StockManagement.Models;

public class StockAlternativeLine : BindableObject
{
  private string _id;
  private string _stockId;

  public StockAlternativeLine() => this._id = Guid.NewGuid().ToString();

  public virtual string Id
  {
    get => this._id;
    set => this.SetProperty<string>(ref this._id, value, nameof (Id));
  }

  public string StockId
  {
    get => this._stockId;
    set => this.SetProperty<string>(ref this._stockId, value, nameof (StockId));
  }
}
