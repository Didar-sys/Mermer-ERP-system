// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.StockManagement.Services.StockSearchResult
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using System;

#nullable disable
namespace Payhas.Binyat.StockManagement.Services;

public class StockSearchResult
{
  public string Id { get; set; }

  public string Code { get; set; }

  public string Name { get; set; }

  public string CodeHtml { get; set; }

  public string NameHtml { get; set; }

  public Decimal Price { get; set; }

  public string Currency { get; set; }

  public string CurrencyId { get; set; }

  public Decimal? LastPurchasePrice { get; set; }

  public string LastPurchaseCurrency { get; set; }

  public string LastPurchaseCurrencyId { get; set; }

  public Decimal Balance { get; set; }

  public string Unit { get; set; }

  public string UnitId { get; set; }

  public bool IsDisabled { get; set; }
}
