// Decompiled with JetBrains decompiler
// Type: Mermer.Transactions.Models.IRequestCurrencyConverter
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

#nullable disable
namespace Mermer.Transactions.Models;

public interface IRequestCurrencyConverter
{
  void UpdateDisplayCurrencyId(bool raiseChangeEvent = false);

  void UpdateDisplayCurrencyConvertion(CurrencyConvertion convertion, bool raiseChangeEvent = false);

  event CurrencyId DisplayCurrencyIdRequested;

  void UpdateDefaultCurrencyId();

  event CurrencyId DefaultCurrencyIdRequested;

  void UpdateCurrencyConvertion();

  event CurrencyConverter CurrencyConverterRequested;

  event AmountFormatter AmountFormatterRequested;
}
