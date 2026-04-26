// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.StockManagement.Models.StockAdditionalPrice
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

#nullable disable
namespace Payhas.Binyat.StockManagement.Models;

public class StockAdditionalPrice : StockPrice
{
  private string _group;

  public virtual string Group
  {
    get => this._group;
    set => this.SetProperty<string>(ref this._group, value, nameof (Group));
  }
}
