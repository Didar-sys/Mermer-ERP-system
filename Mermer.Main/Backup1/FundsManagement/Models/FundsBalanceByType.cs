// Decompiled with JetBrains decompiler
// Type: Mermer.FundsManagement.Models.FundsBalanceByType
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;

#nullable disable
namespace Mermer.FundsManagement.Models;

public class FundsBalanceByType : FundsBalance
{
  public Decimal FundsOpening { get; set; }

  public Decimal FundsRevisionExceed { get; set; }

  public Decimal FundsRevisionDeficit { get; set; }

  public Decimal FundsTransferSource { get; set; }

  public Decimal FundsTransferDestination { get; set; }

  public Decimal ExpenseSlip { get; set; }

  public Decimal Sales { get; set; }

  public Decimal SalesReturn { get; set; }

  public Decimal Purchase { get; set; }

  public Decimal PurchaseReturn { get; set; }

  public Decimal Payment { get; set; }

  public Decimal Collection { get; set; }
}
