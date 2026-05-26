// Decompiled with JetBrains decompiler
// Type: Mermer.FundsManagement.Models.Currency
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Common.Models;
using System.Collections.ObjectModel;

#nullable disable
namespace Mermer.FundsManagement.Models;

public class Currency : Model
{
  private string _name;
  private int _decimals = 2;
  private bool _isDefault;
  private string _description;
  private ObservableCollection<CurrencyRate> _rates;

  public virtual string Name
  {
    get => this._name;
    set => this.SetProperty<string>(ref this._name, value, nameof (Name));
  }

  public virtual int Decimals
  {
    get => this._decimals;
    set => this.SetProperty<int>(ref this._decimals, value, nameof (Decimals));
  }

  public virtual bool IsDefault
  {
    get => this._isDefault;
    set => this.SetProperty<bool>(ref this._isDefault, value, nameof (IsDefault));
  }

  public virtual string Description
  {
    get => this._description;
    set => this.SetProperty<string>(ref this._description, value, nameof (Description));
  }

  public virtual ObservableCollection<CurrencyRate> Rates
  {
    get => this._rates;
    set
    {
      this.SetProperty<ObservableCollection<CurrencyRate>>(ref this._rates, value, nameof (Rates));
    }
  }

  public override string ToString() => this.Name;
}
