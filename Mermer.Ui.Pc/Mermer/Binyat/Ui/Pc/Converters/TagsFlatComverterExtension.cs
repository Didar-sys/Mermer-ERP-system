// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Converters.TagsFlatComverterExtension
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

#nullable disable
namespace Mermer.Ui.Pc.Converters;

public class TagsFlatComverterExtension : MarkupExtension, IValueConverter
{
  private const string Seperator = ", ";

  public override object ProvideValue(IServiceProvider serviceProvider) => (object) this;

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    return (object) string.Join(", ", (IEnumerable<string>) ((object) (value as IEnumerable<string>) ?? (object) new string[0]));
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (!(value is string str))
      return (object) null;
    string[] separator = new string[1]{ ", " };
    return (object) str.Split(separator, StringSplitOptions.RemoveEmptyEntries);
  }
}
