// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Enterprise.Models.Depository
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Common.Models;
using System.Collections.Generic;

#nullable disable
namespace Payhas.Binyat.Enterprise.Models;

public class Depository : Model
{
  private string _officeId;
  private string _name;
  private string _description;
  private IEnumerable<string> _tags;

  public virtual string OfficeId
  {
    get => this._officeId;
    set => this.SetProperty<string>(ref this._officeId, value, nameof (OfficeId));
  }

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

  public virtual IEnumerable<string> Tags
  {
    get => this._tags;
    set => this.SetProperty<IEnumerable<string>>(ref this._tags, value, nameof (Tags));
  }

  public override string ToString() => this.Name;
}
