// Decompiled with JetBrains decompiler
// Type: Mermer.FundsManagement.Models.FundsBalance
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;

#nullable disable
namespace Mermer.FundsManagement.Models;

public class FundsBalance
{
  public string DepositoryId { get; set; }

  public Decimal Income { get; set; }

  public Decimal Expense { get; set; }

  public Decimal Balance => this.Income - this.Expense;
}
