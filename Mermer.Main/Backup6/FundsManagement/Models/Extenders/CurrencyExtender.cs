// Decompiled with JetBrains decompiler
// Type: Mermer.FundsManagement.Models.Extenders.CurrencyExtender
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;
using System.Collections.ObjectModel;
using System.Linq;

#nullable disable
namespace Mermer.FundsManagement.Models.Extenders;

public static class CurrencyExtender
{
  public static CurrencyRate GetRate(this Currency currency, DateTime? date = null)
  {
    DateTime rateDate = date ?? DateTime.Now;
    ObservableCollection<CurrencyRate> rates1 = currency.Rates;
    CurrencyRate rate = rates1 != null ? rates1.OrderByDescending<CurrencyRate, DateTime>((Func<CurrencyRate, DateTime>) (x => x.ValidFrom)).FirstOrDefault<CurrencyRate>((Func<CurrencyRate, bool>) (x => x.ValidFrom <= rateDate)) : (CurrencyRate) null;
    if (rate != null)
      return rate;
    ObservableCollection<CurrencyRate> rates2 = currency.Rates;
    return rates2 == null ? (CurrencyRate) null : rates2.OrderBy<CurrencyRate, DateTime>((Func<CurrencyRate, DateTime>) (x => x.ValidFrom)).FirstOrDefault<CurrencyRate>();
  }
}
