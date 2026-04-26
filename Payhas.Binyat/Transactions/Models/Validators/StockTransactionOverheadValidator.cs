// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Transactions.Models.Validators.StockTransactionOverheadValidator
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Transactions.Models.Validators;

public class StockTransactionOverheadValidator : AbstractValidator<StockTransactionOverhead>
{
  public StockTransactionOverheadValidator()
  {
    this.RuleFor<string>((Expression<Func<StockTransactionOverhead, string>>) (x => x.Id)).NotEmpty<StockTransactionOverhead, string>();
    this.RuleFor<Decimal>((Expression<Func<StockTransactionOverhead, Decimal>>) (x => x.Amount)).GreaterThanOrEqualTo<StockTransactionOverhead, Decimal>(0M);
    this.RuleFor<string>((Expression<Func<StockTransactionOverhead, string>>) (x => x.CurrencyId)).NotEmpty<StockTransactionOverhead, string>().When<StockTransactionOverhead, string>((Func<StockTransactionOverhead, bool>) (x => x.Amount > 0M));
  }
}
