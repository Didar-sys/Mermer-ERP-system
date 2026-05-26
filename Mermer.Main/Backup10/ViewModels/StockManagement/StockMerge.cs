// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.StockManagement.StockMerge
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using Mermer.Data.Models;
using System;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.StockManagement;

public class StockMerge : BindableObject
{
  private bool _isMain;

  public string Id { get; set; }

  public string Code { get; set; }

  public string Name { get; set; }

  public Decimal Price { get; set; }

  public string Currency { get; set; }

  public string Unit { get; set; }

  public bool IsDisabled { get; set; }

  public virtual bool IsMain
  {
    get => this._isMain;
    set => this.SetProperty<bool>(ref this._isMain, value, nameof (IsMain));
  }
}
