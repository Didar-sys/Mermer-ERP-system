// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.CRM.PartnerMerge
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using Payhas.Data.Models;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.CRM;

public class PartnerMerge : BindableObject
{
  private string _partnerId;
  private bool _isMain;

  public virtual string PartnerId
  {
    get => this._partnerId;
    set => this.SetProperty<string>(ref this._partnerId, value, nameof (PartnerId));
  }

  public virtual bool IsMain
  {
    get => this._isMain;
    set => this.SetProperty<bool>(ref this._isMain, value, nameof (IsMain));
  }
}
