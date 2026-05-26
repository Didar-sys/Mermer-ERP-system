// Decompiled with JetBrains decompiler
// Type: Mermer.Authorization.Models.User
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Authorization.Enums;
using Mermer.Common.Models;
using System.Collections.Generic;

#nullable disable
namespace Mermer.Authorization.Models;

public class User : Model
{
  private string _username;
  private bool _isAdmin;
  private string _password;
  private string _description;
  private IEnumerable<string> _roles;
  private Dictionary<string, AccountAccessLevel> _accountPrivileges;

  public virtual string Username
  {
    get => this._username;
    set => this.SetProperty<string>(ref this._username, value, nameof (Username));
  }

  public virtual bool IsAdmin
  {
    get => this._isAdmin;
    set => this.SetProperty<bool>(ref this._isAdmin, value, nameof (IsAdmin));
  }

  public virtual string Password
  {
    get => this._password;
    set => this.SetProperty<string>(ref this._password, value, nameof (Password));
  }

  public virtual string Description
  {
    get => this._description;
    set => this.SetProperty<string>(ref this._description, value, nameof (Description));
  }

  public virtual IEnumerable<string> Roles
  {
    get => this._roles;
    set => this.SetProperty<IEnumerable<string>>(ref this._roles, value, nameof (Roles));
  }

  public virtual Dictionary<string, AccountAccessLevel> AccountPrivileges
  {
    get => this._accountPrivileges;
    set
    {
      this.SetProperty<Dictionary<string, AccountAccessLevel>>(ref this._accountPrivileges, value, nameof (AccountPrivileges));
    }
  }

  public override string ToString() => this.Username;
}
