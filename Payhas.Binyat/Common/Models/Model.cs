// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Common.Models.Model
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Data.Models;

#nullable disable
namespace Payhas.Binyat.Common.Models;

public abstract class Model : BindableObject, IModel
{
  private string _id;
  private bool _isDisabled;

  public virtual string Id
  {
    get => this._id;
    set => this.SetProperty<string>(ref this._id, value, nameof (Id));
  }

  public virtual bool IsDisabled
  {
    get => this._isDisabled;
    set => this.SetProperty<bool>(ref this._isDisabled, value, nameof (IsDisabled));
  }

  public string DocType => this.GetType().Name;
}
