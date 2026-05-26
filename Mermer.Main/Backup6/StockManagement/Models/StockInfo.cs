// Decompiled with JetBrains decompiler
// Type: Mermer.StockManagement.Models.StockInfo
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace Mermer.StockManagement.Models;

public class StockInfo
{
  public string Id { get; set; }

  public string Code { get; set; }

  public string Name { get; set; }

  public string ShortName { get; set; }

  public bool IsDisabled { get; set; }

  public string Unit { get; set; }

  public Decimal Price { get; set; }

  public string CurrencyId { get; set; }

  public Decimal AdditionalPrice { get; set; }

  public string AdditionalPriceCurrencyId { get; set; }

  public string Type { get; set; }

  public string Group { get; set; }

  public IEnumerable<string> Tags { get; set; }

  public IEnumerable<string> Barcodes { get; set; }
}
