// Decompiled with JetBrains decompiler
// Type: Mermer.Commerce.Models.Validators.InvoiceDiscountValidator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Commerce.Models.Validators;

public class InvoiceDiscountValidator : AbstractValidator<InvoiceDiscount>
{
  public InvoiceDiscountValidator()
  {
    this.RuleFor<string>((Expression<Func<InvoiceDiscount, string>>) (x => x.Id)).NotEmpty<InvoiceDiscount, string>();
    this.RuleFor<Decimal>((Expression<Func<InvoiceDiscount, Decimal>>) (x => x.Amount)).GreaterThanOrEqualTo<InvoiceDiscount, Decimal>(0M);
  }
}
