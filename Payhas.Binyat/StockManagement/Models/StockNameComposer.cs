// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.StockManagement.Models.StockNameComposer
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Common.Models;
using System.Collections.ObjectModel;

#nullable disable
namespace Payhas.Binyat.StockManagement.Models;

public class StockNameComposer : Model
{
  private int _order;
  private string _name;
  private string _description;
  private ObservableCollection<StockNameComposerValue> _values;

  public virtual int Order
  {
    get => this._order;
    set => this.SetProperty<int>(ref this._order, value, nameof (Order));
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

  public virtual ObservableCollection<StockNameComposerValue> Values
  {
    get => this._values;
    set
    {
      this.SetProperty<ObservableCollection<StockNameComposerValue>>(ref this._values, value, nameof (Values));
    }
  }
}
