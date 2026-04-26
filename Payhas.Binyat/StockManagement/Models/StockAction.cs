// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.StockManagement.Models.StockAction
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace Payhas.Binyat.StockManagement.Models;

public class StockAction
{
  public string TransactionId { get; set; }

  public string TransactionCode { get; set; }

  public DateTime TransactionDate { get; set; }

  public string TransactionType { get; set; }

  public string TransactionUserId { get; set; }

  public string TransactionUserName { get; set; }

  public bool TransactionIsCompleted { get; set; }

  public bool TransactionIsCash { get; set; }

  public bool TransactionIsDisabled { get; set; }

  public string TransactionGroup { get; set; }

  public IEnumerable<string> TransactionTags { get; set; }

  public string ActionId { get; set; }

  public string ActionSourceId { get; set; }

  public string ActionWarehouseId { get; set; }

  public string ActionRelatedPartnerId { get; set; }

  public string ActionRelatedWarehouseId { get; set; }

  public string ActionRelatedObjectName { get; set; }

  public string ActionStockId { get; set; }

  public Decimal ActionPrice { get; set; }

  public Decimal RecommendedPrice { get; set; }

  public bool IsCheaperThanRecommended => this.ActionPrice < this.RecommendedPrice;

  public Decimal ActionIncome { get; set; }

  public Decimal ActionExpense { get; set; }

  public Decimal ActionEffect => this.ActionIncome - this.ActionExpense;

  public Decimal ActionDiscount { get; set; }

  public Decimal ActionOverhead { get; set; }

  public Decimal RecommendedTotal => this.RecommendedPrice * this.ActionEffect;

  public Decimal GrandTotal => this.ActionPrice * this.ActionEffect;
}
