// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Finance.Spending.Models.Expense
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Common.Models;
using System.Collections.Generic;

#nullable disable
namespace Payhas.Binyat.Finance.Spending.Models;

public class Expense : Model
{
  private string _name;
  private string _type;
  private string _group;
  private IEnumerable<string> _tags;
  private string _description;

  public virtual string Name
  {
    get => this._name;
    set => this.SetProperty<string>(ref this._name, value, nameof (Name));
  }

  public virtual string Type
  {
    get => this._type;
    set => this.SetProperty<string>(ref this._type, value, nameof (Type));
  }

  public virtual string Group
  {
    get => this._group;
    set => this.SetProperty<string>(ref this._group, value, nameof (Group));
  }

  public virtual IEnumerable<string> Tags
  {
    get => this._tags;
    set => this.SetProperty<IEnumerable<string>>(ref this._tags, value, nameof (Tags));
  }

  public virtual string Description
  {
    get => this._description;
    set => this.SetProperty<string>(ref this._description, value, nameof (Description));
  }

  public override string ToString() => this.Name;
}
