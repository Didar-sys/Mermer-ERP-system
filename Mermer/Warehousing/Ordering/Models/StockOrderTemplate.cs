// Decompiled with JetBrains decompiler
// Type: Mermer.Warehousing.Ordering.Models.StockOrderTemplate
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Common.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#nullable disable
namespace Mermer.Warehousing.Ordering.Models;

public class StockOrderTemplate : Model
{
  private string _name;
  private bool _isDisabled;
  private string _group;
  private IEnumerable<string> _tags;
  private string _description;
  private ObservableCollection<StockOrderTemplateLine> _lines;

  public virtual string Name
  {
    get => this._name;
    set => this.SetProperty<string>(ref this._name, value, nameof (Name));
  }

  public new virtual bool IsDisabled
  {
    get => this._isDisabled;
    set => this.SetProperty<bool>(ref this._isDisabled, value, nameof (IsDisabled));
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

  public virtual ObservableCollection<StockOrderTemplateLine> Lines
  {
    get => this._lines;
    set
    {
      this.SetProperty<ObservableCollection<StockOrderTemplateLine>>(ref this._lines, value, nameof (Lines));
    }
  }

  public override string ToString() => this.Name;
}
