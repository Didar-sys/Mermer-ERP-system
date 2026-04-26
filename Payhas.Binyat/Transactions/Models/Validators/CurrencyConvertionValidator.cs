// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Transactions.Models.Validators.CurrencyConvertionValidator
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Transactions.Models.Validators;

public class CurrencyConvertionValidator : AbstractValidator<CurrencyConvertion>
{
  public CurrencyConvertionValidator()
  {
    this.RuleFor<string>((Expression<Func<CurrencyConvertion, string>>) (x => x.Id)).NotEmpty<CurrencyConvertion, string>();
    this.RuleFor<string>((Expression<Func<CurrencyConvertion, string>>) (x => x.CurrencyId)).NotEmpty<CurrencyConvertion, string>();
    this.RuleFor<Decimal>((Expression<Func<CurrencyConvertion, Decimal>>) (x => x.Multiplier)).GreaterThan<CurrencyConvertion, Decimal>(0M);
    this.RuleFor<Decimal>((Expression<Func<CurrencyConvertion, Decimal>>) (x => x.Divider)).GreaterThan<CurrencyConvertion, Decimal>(0M);
  }
}
