// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.Converters.BooleanNegateConverter
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Platform.Converters;
using System;
using System.Globalization;

#nullable disable
namespace Payhas.Binyat.Ui.Core.Converters;

public class BooleanNegateConverter : MvxValueConverter<bool, bool>
{
  protected override bool Convert(
    bool value,
    Type targetType,
    object parameter,
    CultureInfo culture)
  {
    return !value;
  }

  protected override bool ConvertBack(
    bool value,
    Type targetType,
    object parameter,
    CultureInfo culture)
  {
    return !value;
  }
}
