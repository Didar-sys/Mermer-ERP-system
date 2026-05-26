// Decompiled with JetBrains decompiler
// Type: Mermer.Commerce.Models.LastPurchasePrice
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;

#nullable disable
namespace Mermer.Commerce.Models;

public class LastPurchasePrice
{
  public string StockId { get; set; }

  public DateTime Date { get; set; }

  public Decimal Price { get; set; }

  public string CurrencyId { get; set; }
}
