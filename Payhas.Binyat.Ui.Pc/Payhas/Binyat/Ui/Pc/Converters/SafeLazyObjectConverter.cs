// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Converters.SafeLazyObjectConverter
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using DevExpress.Data;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Converters;

public class SafeLazyObjectConverter : MarkupExtension, IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    switch (value)
    {
      case null:
      case NotLoadedObject _:
        return (object) null;
      default:
        return value;
    }
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    return value;
  }

  public override object ProvideValue(IServiceProvider serviceProvider) => (object) this;
}
