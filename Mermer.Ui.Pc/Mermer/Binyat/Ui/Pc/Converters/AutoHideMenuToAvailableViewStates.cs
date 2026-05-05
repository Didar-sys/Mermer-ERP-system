// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Converters.AutoHideMenuToAvailableViewStates
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using DevExpress.Xpf.WindowsUI;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

#nullable disable
namespace Mermer.Ui.Pc.Converters;

public class AutoHideMenuToAvailableViewStates : MarkupExtension, IValueConverter
{
  public HamburgerMenuAvailableViewStates TrueAvailableViewStates { get; set; }

  public HamburgerMenuAvailableViewStates FalseAvailableViewStates { get; set; }

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    return (object) (HamburgerMenuAvailableViewStates) ((bool) value ? (int) this.TrueAvailableViewStates : (int) this.FalseAvailableViewStates);
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }

  public override object ProvideValue(IServiceProvider serviceProvider) => (object) this;
}
