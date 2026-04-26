// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Commerce.Models.InvoiceInfo
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace Payhas.Binyat.Commerce.Models;

public class InvoiceInfo
{
  public string Id { get; set; }

  public string Code { get; set; }

  public string Type { get; set; }

  public DateTime Date { get; set; }

  public string UserId { get; set; }

  public string UserName { get; set; }

  public bool IsCash { get; set; }

  public bool IsCompleted { get; set; }

  public bool IsDisabled { get; set; }

  public string Group { get; set; }

  public IEnumerable<string> Tags { get; set; }

  public string OfficeId { get; set; }

  public string WarehouseId { get; set; }

  public string DepositoryId { get; set; }

  public string PartnerId { get; set; }

  public Decimal ActionTotal { get; set; }

  public Decimal ActionDiscountsTotal { get; set; }

  public Decimal ActionGrandTotal { get; set; }
}
