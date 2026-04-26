// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Converters.StockUnitsFilterConverter
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using Payhas.Binyat.StockManagement.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Converters;

public class StockUnitsFilterConverter : MarkupExtension, IMultiValueConverter
{
  public override object ProvideValue(IServiceProvider serviceProvider) => (object) this;

  public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
  {
    if (!(values[1] is ObservableCollection<Stock> source))
      return (object) null;
    if ((values[2] as bool?).GetValueOrDefault())
    {
      string stockId = values[0] as string;
      if (stockId != null)
      {
        Stock stock = source.SingleOrDefault<Stock>((Func<Stock, bool>) (x => x.Id == stockId));
        return stock == null ? (object) null : (object) stock.Units;
      }
    }
    return (object) source.SelectMany<Stock, StockUnit>((Func<Stock, IEnumerable<StockUnit>>) (x => (IEnumerable<StockUnit>) x.Units));
  }

  public object[] ConvertBack(
    object value,
    Type[] targetTypes,
    object parameter,
    CultureInfo culture)
  {
    throw new NotImplementedException();
  }
}
