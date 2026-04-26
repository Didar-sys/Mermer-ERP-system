// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Converters.TokenConverter
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Converters;

public class TokenConverter : MarkupExtension, IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    return !(value is IEnumerable<string> source) ? (object) null : (object) source.Cast<object>().ToList<object>();
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    return !(value is IEnumerable<object> source) ? (object) null : (object) source.Cast<string>().ToList<string>();
  }

  public override object ProvideValue(IServiceProvider serviceProvider) => (object) this;
}
