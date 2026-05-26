// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Authorization.AccountAssignment
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using Mermer.Authorization.Enums;
using Mermer.Data.Models;
using System;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Authorization;

public class AccountAssignment : BindableObject
{
  private string _accountId;
  private string _officeId;
  private AccountAccessLevel _accessLevel;

  public virtual string AccountId
  {
    get => this._accountId;
    set => this.SetProperty<string>(ref this._accountId, value, nameof (AccountId));
  }

  public virtual string OfficeId
  {
    get => this._officeId;
    set => this.SetProperty<string>(ref this._officeId, value, nameof (OfficeId));
  }

  public virtual AccountAccessLevel AccessLevel
  {
    get => this._accessLevel;
    set
    {
      if (!this.SetProperty<AccountAccessLevel>(ref this._accessLevel, value, nameof (AccessLevel)))
        return;
      this.RaisePropertyChanged("NoAccess");
      this.RaisePropertyChanged("ReadAccess");
      this.RaisePropertyChanged("OperationAccess");
    }
  }

  public virtual bool NoAccess
  {
    get => this.AccessLevel == AccountAccessLevel.None;
    set
    {
      if (!value)
        return;
      this.AccessLevel = AccountAccessLevel.None;
    }
  }

  public virtual bool ReadAccess
  {
    get => this.AccessLevel.HasFlag((Enum) AccountAccessLevel.Read);
    set
    {
      if (value)
        this.AccessLevel |= AccountAccessLevel.Read;
      else
        this.AccessLevel &= ~AccountAccessLevel.Read;
    }
  }

  public virtual bool OperationAccess
  {
    get => this.AccessLevel.HasFlag((Enum) AccountAccessLevel.Operate);
    set
    {
      if (value)
        this.AccessLevel |= AccountAccessLevel.Operate;
      else
        this.AccessLevel &= ~AccountAccessLevel.Operate;
    }
  }
}
