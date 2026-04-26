// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.StockManagement.Models.GeneralStockRevenueReport
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using System;

#nullable disable
namespace Payhas.Binyat.StockManagement.Models;

public class GeneralStockRevenueReport : StockRevenueReport
{
  public DateTime Date { get; set; }

  public Decimal SalesReturn { get; set; }

  public Decimal CreditSalesReturn { get; set; }

  public Decimal PurchaseReturn { get; set; }

  public Decimal CreditPurchaseReturn { get; set; }

  public override Decimal Sum => base.Sum + this.PurchaseReturn - this.SalesReturn;

  public override Decimal CreditSum
  {
    get => base.CreditSum + this.CreditPurchaseReturn - this.CreditSalesReturn;
  }
}
