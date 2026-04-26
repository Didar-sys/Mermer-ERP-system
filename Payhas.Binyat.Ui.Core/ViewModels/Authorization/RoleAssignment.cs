// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Authorization.RoleAssignment
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using Payhas.Data.Models;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Authorization;

public class RoleAssignment : BindableObject
{
  private string _roleId;

  public virtual string RoleId
  {
    get => this._roleId;
    set => this.SetProperty<string>(ref this._roleId, value, nameof (RoleId));
  }
}
