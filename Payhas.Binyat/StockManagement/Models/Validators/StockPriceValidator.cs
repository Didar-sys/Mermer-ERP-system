// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.StockManagement.Models.Validators.StockPriceValidator
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.StockManagement.Models.Validators;

public class StockPriceValidator : AbstractValidator<StockPrice>
{
  public StockPriceValidator()
  {
    this.RuleFor<string>((Expression<Func<StockPrice, string>>) (x => x.Id)).NotEmpty<StockPrice, string>();
    this.RuleFor<DateTime>((Expression<Func<StockPrice, DateTime>>) (x => x.ValidFrom)).NotEmpty<StockPrice, DateTime>();
    this.RuleFor<Decimal>((Expression<Func<StockPrice, Decimal>>) (x => x.Price)).GreaterThan<StockPrice, Decimal>(0M);
    this.RuleFor<string>((Expression<Func<StockPrice, string>>) (x => x.CurrencyId)).NotEmpty<StockPrice, string>();
  }
}
