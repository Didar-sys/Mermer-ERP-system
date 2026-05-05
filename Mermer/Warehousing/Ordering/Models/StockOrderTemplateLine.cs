// Decompiled with JetBrains decompiler
// Type: Mermer.Warehousing.Ordering.Models.StockOrderTemplateLine
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Data.Models;
using System;

#nullable disable
namespace Mermer.Warehousing.Ordering.Models;

public class StockOrderTemplateLine : BindableObject
{
  private string _id;
  private string _stockId;

  public StockOrderTemplateLine() => this.Id = Guid.NewGuid().ToString();

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
}
