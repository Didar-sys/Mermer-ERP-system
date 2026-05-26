// Decompiled with JetBrains decompiler
// Type: Mermer.StockManagement.Models.StockNameComposerValue
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Data.Models;

#nullable disable
namespace Mermer.StockManagement.Models;

public class StockNameComposerValue : BindableObject
{
  private int _order;
  private string _name;
  private string _shortName;

  public virtual int Order
  {
    get => this._order;
    set => this.SetProperty<int>(ref this._order, value, nameof (Order));
  }

  public virtual string Name
  {
    get => this._name;
    set => this.SetProperty<string>(ref this._name, value, nameof (Name));
  }

  public virtual string ShortName
  {
    get => this._shortName;
    set => this.SetProperty<string>(ref this._shortName, value, nameof (ShortName));
  }

  public string Fullname => $"{this.Name} ({this.ShortName})";
}
