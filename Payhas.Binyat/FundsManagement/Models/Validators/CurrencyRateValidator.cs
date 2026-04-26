// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.FundsManagement.Models.Validators.CurrencyRateValidator
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.FundsManagement.Models.Validators;

public class CurrencyRateValidator : AbstractValidator<CurrencyRate>
{
  public CurrencyRateValidator()
  {
    this.RuleFor<string>((Expression<Func<CurrencyRate, string>>) (x => x.Id)).NotEmpty<CurrencyRate, string>();
    this.RuleFor<DateTime>((Expression<Func<CurrencyRate, DateTime>>) (x => x.ValidFrom)).NotEmpty<CurrencyRate, DateTime>();
    this.RuleFor<Decimal>((Expression<Func<CurrencyRate, Decimal>>) (x => x.Multiplier)).GreaterThan<CurrencyRate, Decimal>(0M);
    this.RuleFor<Decimal>((Expression<Func<CurrencyRate, Decimal>>) (x => x.Divider)).GreaterThan<CurrencyRate, Decimal>(0M);
  }
}
