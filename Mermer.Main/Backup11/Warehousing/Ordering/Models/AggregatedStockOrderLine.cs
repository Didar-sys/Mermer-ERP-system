// Decompiled with JetBrains decompiler
// Type: Mermer.Warehousing.Ordering.Models.AggregatedStockOrderLine
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Data;
using Mermer.Data.Models;
using System;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Warehousing.Ordering.Models;

public class AggregatedStockOrderLine : BindableObject
{
  private string _id;
  private string _stockId;
  private string _unitId;
  private WatchedDictionary<string, Decimal> _orders;

  public AggregatedStockOrderLine() => this.Id = Guid.NewGuid().ToString();

  public string Id
  {
    get => this._id;
    set => this.SetProperty<string>(ref this._id, value, nameof (Id));
  }

  public string StockId
  {
    get => this._stockId;
    set => this.SetProperty<string>(ref this._stockId, value, nameof (StockId));
  }

  public string UnitId
  {
    get => this._unitId;
    set => this.SetProperty<string>(ref this._unitId, value, nameof (UnitId));
  }

  public WatchedDictionary<string, Decimal> Orders
  {
    get => this._orders;
    set
    {
      if (this._orders != null)
      {
        this._orders.ValueChanged -= new ValueChangedEventHandler<string, Decimal>(this.Orders_ValueChanged);
        this._orders.CollectionChanged -= new EventHandler(this.Orders_CollectionChanged);
      }
      this.SetProperty<WatchedDictionary<string, Decimal>>(ref this._orders, value, nameof (Orders));
      if (this._orders != null)
      {
        this._orders.ValueChanged += new ValueChangedEventHandler<string, Decimal>(this.Orders_ValueChanged);
        this._orders.CollectionChanged += new EventHandler(this.Orders_CollectionChanged);
      }
      this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.OrdersTotal));
    }
  }

  private void Orders_CollectionChanged(object sender, EventArgs e)
  {
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.OrdersTotal));
  }

  private void Orders_ValueChanged(object sender, ValueChangedEventArgs<string, Decimal> e)
  {
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.OrdersTotal));
  }

  public Decimal OrdersTotal => this.Orders.Values.Sum();
}
