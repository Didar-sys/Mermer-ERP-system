// Decompiled with JetBrains decompiler
// Type: Mermer.Commerce.Models.Validators.BillLineValidator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Commerce.Models.Validators;

public class BillLineValidator : AbstractValidator<BillLine>
{
  public BillLineValidator()
  {
    this.RuleFor<Decimal>((Expression<Func<BillLine, Decimal>>) (x => x.Amount)).GreaterThan<BillLine, Decimal>(0M);
    this.RuleFor<string>((Expression<Func<BillLine, string>>) (x => x.CurrencyId)).NotEmpty<BillLine, string>();
  }
}
