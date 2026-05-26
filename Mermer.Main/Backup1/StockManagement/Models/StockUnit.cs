// Decompiled with JetBrains decompiler
// Type: Mermer.StockManagement.Models.StockUnit
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Data.Models;
using System;

#nullable disable
namespace Mermer.StockManagement.Models;

public class StockUnit : BindableObject
{
  private string _id;
  private string _name;
  private Decimal _multiplier;
  private Decimal _divider;
  private bool _isDefault;

  public virtual string Id
  {
    get => this._id;
    set => this.SetProperty<string>(ref this._id, value, nameof (Id));
  }

  public virtual string Name
  {
    get => this._name;
    set => this.SetProperty<string>(ref this._name, value, nameof (Name));
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

  public virtual bool IsDefault
  {
    get => this._isDefault;
    set => this.SetProperty<bool>(ref this._isDefault, value, nameof (IsDefault));
  }
}
