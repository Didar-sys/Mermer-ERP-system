// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Transactions.Models.Validators.FundsTransactionLineValidator`1
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Transactions.Models.Validators;

public class FundsTransactionLineValidator<T> : TransactionLineValidator<T> where T : FundsTransactionLine
{
  public FundsTransactionLineValidator()
  {
    this.RuleFor<Decimal>((Expression<Func<T, Decimal>>) (x => x.Amount)).GreaterThanOrEqualTo<T, Decimal>(0M);
  }
}
