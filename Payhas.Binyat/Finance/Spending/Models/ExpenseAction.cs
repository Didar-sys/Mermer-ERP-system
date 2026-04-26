// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Finance.Spending.Models.ExpenseAction
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace Payhas.Binyat.Finance.Spending.Models;

public class ExpenseAction
{
  public string TransactionId { get; set; }

  public string TransactionCode { get; set; }

  public DateTime TransactionDate { get; set; }

  public string TransactionType { get; set; }

  public string TransactionUserId { get; set; }

  public string TransactionUserName { get; set; }

  public bool TransactionIsCompleted { get; set; }

  public bool TransactionIsDisabled { get; set; }

  public string TransactionGroup { get; set; }

  public IEnumerable<string> TransactionTags { get; set; }

  public string ActionDepositoryId { get; set; }

  public string ActionExpenseId { get; set; }

  public Decimal ActionAmount { get; set; }
}
