// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.StockManagement.Models.StockBalanceByType
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using System;

#nullable disable
namespace Payhas.Binyat.StockManagement.Models;

public class StockBalanceByType : StockBalance
{
  public Decimal StockOpening { get; set; }

  public Decimal StockSpoilage { get; set; }

  public Decimal StockUsage { get; set; }

  public Decimal RevisionExceed { get; set; }

  public Decimal RevisionDeficit { get; set; }

  public Decimal StockTransferSource { get; set; }

  public Decimal StockTransferDestination { get; set; }

  public Decimal Sales { get; set; }

  public Decimal SalesReturn { get; set; }

  public Decimal Purchase { get; set; }

  public Decimal PurchaseReturn { get; set; }
}
