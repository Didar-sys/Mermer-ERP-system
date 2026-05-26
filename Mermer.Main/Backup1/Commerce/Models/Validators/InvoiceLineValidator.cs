// Decompiled with JetBrains decompiler
// Type: Mermer.Commerce.Models.Validators.InvoiceLineValidator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Commerce.Models.Validators;

public class InvoiceLineValidator : AbstractValidator<InvoiceLine>
{
  public InvoiceLineValidator()
  {
    this.RuleFor<string>((Expression<Func<InvoiceLine, string>>) (x => x.Id)).NotEmpty<InvoiceLine, string>();
    this.RuleFor<string>((Expression<Func<InvoiceLine, string>>) (x => x.StockId)).NotEmpty<InvoiceLine, string>();
    this.RuleFor<Decimal>((Expression<Func<InvoiceLine, Decimal>>) (x => x.Quantity)).GreaterThanOrEqualTo<InvoiceLine, Decimal>(0M);
    this.RuleFor<string>((Expression<Func<InvoiceLine, string>>) (x => x.UnitId)).NotEmpty<InvoiceLine, string>().When<InvoiceLine, string>((Func<InvoiceLine, bool>) (x => x.Quantity > 0M));
    this.RuleFor<Decimal>((Expression<Func<InvoiceLine, Decimal>>) (x => x.Price)).GreaterThanOrEqualTo<InvoiceLine, Decimal>(0M);
    this.RuleFor<string>((Expression<Func<InvoiceLine, string>>) (x => x.CurrencyId)).NotEmpty<InvoiceLine, string>().When<InvoiceLine, string>((Func<InvoiceLine, bool>) (x => x.Price > 0M));
  }
}
