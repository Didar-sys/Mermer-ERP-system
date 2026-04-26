// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.StockManagement.Models.Stock
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Common.Models;
using Payhas.Binyat.StockManagement.Models.Extenders;
using Payhas.Data;
using Payhas.Data.Patcher;
using Payhas.Data.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.StockManagement.Models;

public class Stock : Model
{
  private string _code;
  private string _name;
  private string _shortName;
  private string _type;
  private string _group;
  private IEnumerable<string> _tags;
  private IEnumerable<string> _barcodes;
  private Decimal? _limitMin;
  private Decimal? _limitMax;
  private string _description;
  private ObservableCollection<StockUnit> _units;
  private WatchedObservableCollection<StockPrice> _prices;
  private WatchedObservableCollection<StockAdditionalPrice> _additionalPrices;

  public virtual string Code
  {
    get => this._code;
    set => this.SetProperty<string>(ref this._code, value, nameof (Code));
  }

  public virtual string Name
  {
    get => this._name;
    set => this.SetProperty<string>(ref this._name, value, nameof (Name));
  }

  public virtual string ShortName
  {
    get => this._shortName;
    set => this.SetProperty<string>(ref this._shortName, value, nameof (ShortName));
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

  public IEnumerable<string> Barcodes
  {
    get => this._barcodes;
    set => this.SetProperty<IEnumerable<string>>(ref this._barcodes, value, nameof (Barcodes));
  }

  public virtual Decimal? LimitMin
  {
    get => this._limitMin;
    set => this.SetProperty<Decimal?>(ref this._limitMin, value, nameof (LimitMin));
  }

  public virtual Decimal? LimitMax
  {
    get => this._limitMax;
    set => this.SetProperty<Decimal?>(ref this._limitMax, value, nameof (LimitMax));
  }

  public virtual string Description
  {
    get => this._description;
    set => this.SetProperty<string>(ref this._description, value, nameof (Description));
  }

  [IgnorePatch]
  public virtual string Unit
  {
    get
    {
      ObservableCollection<StockUnit> units = this.Units;
      if (units == null)
        return (string) null;
      return units.FirstOrDefault<StockUnit>((Func<StockUnit, bool>) (x => x.IsDefault))?.Name;
    }
    set
    {
      if (this.Units == null)
        return;
      StockUnit stockUnit = this.Units.FirstOrDefault<StockUnit>((Func<StockUnit, bool>) (x => x.IsDefault));
      if (stockUnit == null)
      {
        stockUnit = new StockUnit()
        {
          Id = Guid.NewGuid().ToString(),
          IsDefault = true,
          Multiplier = 1M,
          Divider = 1M
        };
        this.Units.Add(stockUnit);
      }
      stockUnit.Name = value;
      this.RaisePropertyChanged(nameof (Unit));
    }
  }

  [IgnorePatch]
  public virtual string UnitId
  {
    get
    {
      ObservableCollection<StockUnit> units = this.Units;
      if (units == null)
        return (string) null;
      return units.FirstOrDefault<StockUnit>((Func<StockUnit, bool>) (x => x.IsDefault))?.Id;
    }
  }

  public virtual ObservableCollection<StockUnit> Units
  {
    get => this._units;
    set
    {
      this.SetProperty<ObservableCollection<StockUnit>>(ref this._units, value, nameof (Units));
    }
  }

  [IgnorePatch]
  public Decimal Price
  {
    get
    {
      StockPrice price = this.GetPrice();
      return price == null ? 0M : price.Price;
    }
    set
    {
      if (this.Prices == null)
        return;
      StockPrice stockPrice = this.GetPrice();
      if (stockPrice == null || stockPrice.ValidFrom != DateTime.Today)
      {
        stockPrice = new StockPrice()
        {
          ValidFrom = DateTime.Today
        };
        this.Prices.Add(stockPrice);
      }
      stockPrice.Price = value;
      this.RaisePropertyChanged(nameof (Price));
    }
  }

  [IgnorePatch]
  public string CurrencyId
  {
    get => this.GetPrice()?.CurrencyId;
    set
    {
      if (this.Prices == null)
        return;
      StockPrice stockPrice = this.GetPrice();
      if (stockPrice == null || stockPrice.ValidFrom != DateTime.Today)
      {
        stockPrice = new StockPrice()
        {
          ValidFrom = DateTime.Today
        };
        this.Prices.Add(stockPrice);
      }
      stockPrice.CurrencyId = value;
      this.RaisePropertyChanged(nameof (CurrencyId));
    }
  }

  public virtual WatchedObservableCollection<StockPrice> Prices
  {
    get => this._prices;
    set
    {
      if (this._prices != null)
        this._prices.Watcher.ItemsChanged -= new ItemsChangedEventHandler(this.OnPricesChanged);
      this.SetProperty<WatchedObservableCollection<StockPrice>>(ref this._prices, value, nameof (Prices));
      if (this._prices == null)
        return;
      this._prices.Watcher.ItemsChanged += new ItemsChangedEventHandler(this.OnPricesChanged);
    }
  }

  public virtual WatchedObservableCollection<StockAdditionalPrice> AdditionalPrices
  {
    get => this._additionalPrices;
    set
    {
      this.SetProperty<WatchedObservableCollection<StockAdditionalPrice>>(ref this._additionalPrices, value, nameof (AdditionalPrices));
    }
  }

  private void OnPricesChanged()
  {
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.Price));
    this.RaisePropertyChanged<string>((Expression<Func<string>>) (() => this.CurrencyId));
  }

  public string Fullname => $"{this.Code} | {this.Name}";

  public override string ToString() => this.Fullname;
}
