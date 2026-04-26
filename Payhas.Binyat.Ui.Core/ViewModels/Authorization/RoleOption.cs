// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Authorization.RoleOption
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using Payhas.Data.Models;
using System;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Authorization;

public class RoleOption : BindableObject
{
  private readonly RoleAction _action;

  public RoleOption(RoleAction action) => this._action = action;

  public virtual string Name { get; set; }

  public virtual int Value { get; set; }

  public virtual bool IsSelected
  {
    get
    {
      return this.Value != 0 ? this._action.SeletedValue.HasBit(this.Value) : this._action.SeletedValue == 0;
    }
    set
    {
      if (value && this.Value == 0)
        this._action.SeletedValue = 0;
      else
        this._action.SeletedValue = value ? this._action.SeletedValue.AddBit(this.Value) : this._action.SeletedValue.RemoveBit(this.Value);
    }
  }
}
