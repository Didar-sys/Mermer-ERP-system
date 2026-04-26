// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.CRM.Models.PartnerBalanceByType
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using System;

#nullable disable
namespace Payhas.Binyat.CRM.Models;

public class PartnerBalanceByType : PartnerBalance
{
  public Decimal PartnerOpeningBalance { get; set; }

  public Decimal PartnerBalanceRevision { get; set; }

  public Decimal PartnerTransfer { get; set; }

  public Decimal Sales { get; set; }

  public Decimal SalesReturn { get; set; }

  public Decimal Purchase { get; set; }

  public Decimal PurchaseReturn { get; set; }

  public Decimal Payment { get; set; }

  public Decimal Collection { get; set; }
}
