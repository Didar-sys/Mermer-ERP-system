// Decompiled with JetBrains decompiler
// Type: Mermer.Warehousing.Ordering.Models.Validators.StockOrderLineValidator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Warehousing.Ordering.Models.Validators;

public class StockOrderLineValidator : AbstractValidator<StockOrderLine>
{
  public StockOrderLineValidator()
  {
    this.RuleFor<string>((Expression<Func<StockOrderLine, string>>) (x => x.Id)).NotEmpty<StockOrderLine, string>();
    this.RuleFor<string>((Expression<Func<StockOrderLine, string>>) (x => x.StockId)).NotEmpty<StockOrderLine, string>();
    this.RuleFor<Decimal>((Expression<Func<StockOrderLine, Decimal>>) (x => x.Quantity)).GreaterThanOrEqualTo<StockOrderLine, Decimal>(0M);
    this.RuleFor<string>((Expression<Func<StockOrderLine, string>>) (x => x.UnitId)).NotEmpty<StockOrderLine, string>();
  }
}
