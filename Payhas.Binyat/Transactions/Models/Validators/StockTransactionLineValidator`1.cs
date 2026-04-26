// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Transactions.Models.Validators.StockTransactionLineValidator`1
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Transactions.Models.Validators;

public class StockTransactionLineValidator<T> : TransactionLineValidator<T> where T : StockTransactionLine
{
  public StockTransactionLineValidator()
  {
    this.RuleFor<string>((Expression<Func<T, string>>) (x => x.StockId)).NotEmpty<T, string>();
    this.RuleFor<Decimal>((Expression<Func<T, Decimal>>) (x => x.Quantity)).GreaterThanOrEqualTo<T, Decimal>(0M);
    this.RuleFor<string>((Expression<Func<T, string>>) (x => x.UnitId)).NotEmpty<T, string>();
    this.RuleFor<Decimal>((Expression<Func<T, Decimal>>) (x => x.Price)).GreaterThanOrEqualTo<T, Decimal>(0M);
  }
}
