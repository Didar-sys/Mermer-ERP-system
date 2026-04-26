// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Commerce.Models.Validators.InvoicePaymentValidator
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Commerce.Models.Validators;

public class InvoicePaymentValidator : AbstractValidator<InvoicePayment>
{
  public InvoicePaymentValidator()
  {
    this.RuleFor<string>((Expression<Func<InvoicePayment, string>>) (x => x.Id)).NotEmpty<InvoicePayment, string>();
    this.RuleFor<Decimal>((Expression<Func<InvoicePayment, Decimal>>) (x => x.Amount)).GreaterThanOrEqualTo<InvoicePayment, Decimal>(0M);
    this.RuleFor<string>((Expression<Func<InvoicePayment, string>>) (x => x.CurrencyId)).NotEmpty<InvoicePayment, string>().When<InvoicePayment, string>((Func<InvoicePayment, bool>) (x => x.Amount > 0M));
  }
}
