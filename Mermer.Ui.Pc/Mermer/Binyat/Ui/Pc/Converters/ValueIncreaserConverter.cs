// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Converters.ValueIncreaserConverter
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

#nullable disable
namespace Mermer.Ui.Pc.Converters;

public class ValueIncreaserConverter : MarkupExtension, IValueConverter
{
  private const int IgnoredValue = -2147483647 /*0x80000001*/;

  public int IncreaseBy { get; set; } = 1;

  public override object ProvideValue(IServiceProvider serviceProvider) => (object) this;

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    int num = value as int? ?? -2147483647 /*0x80000001*/;
    return num != -2147483647 /*0x80000001*/ ? (object) $"{num + this.IncreaseBy}" : (object) "";
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }
}
