// Decompiled with JetBrains decompiler
// Type: Mermer.StockManagement.Models.Extenders.StockExtender
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Data;
using System;
using System.Linq;

#nullable disable
namespace Mermer.StockManagement.Models.Extenders;

public static class StockExtender
{
  public static StockPrice GetPrice(this Stock stock, DateTime? date = null, string priceGroup = null)
  {
    DateTime priceDate = date ?? DateTime.Now;
    if (!string.IsNullOrEmpty(priceGroup))
    {
      WatchedObservableCollection<StockAdditionalPrice> additionalPrices1 = stock.AdditionalPrices;
      StockAdditionalPrice stockAdditionalPrice = additionalPrices1 != null ? additionalPrices1.Where<StockAdditionalPrice>((Func<StockAdditionalPrice, bool>) (x => x.Group == priceGroup)).OrderByDescending<StockAdditionalPrice, DateTime>((Func<StockAdditionalPrice, DateTime>) (x => x.ValidFrom)).FirstOrDefault<StockAdditionalPrice>((Func<StockAdditionalPrice, bool>) (x => x.ValidFrom <= priceDate)) : (StockAdditionalPrice) null;
      if (stockAdditionalPrice == null)
      {
        WatchedObservableCollection<StockAdditionalPrice> additionalPrices2 = stock.AdditionalPrices;
        stockAdditionalPrice = additionalPrices2 != null ? additionalPrices2.Where<StockAdditionalPrice>((Func<StockAdditionalPrice, bool>) (x => x.Group == priceGroup)).OrderBy<StockAdditionalPrice, DateTime>((Func<StockAdditionalPrice, DateTime>) (x => x.ValidFrom)).FirstOrDefault<StockAdditionalPrice>() : (StockAdditionalPrice) null;
      }
      StockAdditionalPrice price = stockAdditionalPrice;
      if (price != null)
        return (StockPrice) price;
    }
    WatchedObservableCollection<StockPrice> prices1 = stock.Prices;
    StockPrice price1 = prices1 != null ? prices1.OrderByDescending<StockPrice, DateTime>((Func<StockPrice, DateTime>) (x => x.ValidFrom)).FirstOrDefault<StockPrice>((Func<StockPrice, bool>) (x => x.ValidFrom <= priceDate)) : (StockPrice) null;
    if (price1 != null)
      return price1;
    WatchedObservableCollection<StockPrice> prices2 = stock.Prices;
    return prices2 == null ? (StockPrice) null : prices2.OrderBy<StockPrice, DateTime>((Func<StockPrice, DateTime>) (x => x.ValidFrom)).FirstOrDefault<StockPrice>();
  }
}
