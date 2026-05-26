// Decompiled with JetBrains decompiler
// Type: Mermer.Authorization.Models.Role
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Common.Models;
using System.Collections.Generic;

#nullable disable
namespace Mermer.Authorization.Models;

public class Role : Model
{
  private string _name;
  private string _description;
  private Dictionary<string, int> _authorizations;

  public virtual string Name
  {
    get => this._name;
    set => this.SetProperty<string>(ref this._name, value, nameof (Name));
  }

  public virtual string Description
  {
    get => this._description;
    set => this.SetProperty<string>(ref this._description, value, nameof (Description));
  }

  public virtual Dictionary<string, int> Authorizations
  {
    get => this._authorizations;
    set
    {
      this.SetProperty<Dictionary<string, int>>(ref this._authorizations, value, nameof (Authorizations));
    }
  }

  public override string ToString() => this.Name;
}
