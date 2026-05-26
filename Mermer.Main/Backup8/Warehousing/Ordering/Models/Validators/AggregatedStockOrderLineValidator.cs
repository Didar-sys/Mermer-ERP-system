// Decompiled with JetBrains decompiler
// Type: Mermer.Warehousing.Ordering.Models.Validators.AggregatedStockOrderLineValidator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using Mermer.Data;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Warehousing.Ordering.Models.Validators;

public class AggregatedStockOrderLineValidator : AbstractValidator<AggregatedStockOrderLine>
{
  public AggregatedStockOrderLineValidator()
  {
    this.RuleFor<string>((Expression<Func<AggregatedStockOrderLine, string>>) (x => x.Id)).NotEmpty<AggregatedStockOrderLine, string>();
    this.RuleFor<string>((Expression<Func<AggregatedStockOrderLine, string>>) (x => x.StockId)).NotEmpty<AggregatedStockOrderLine, string>();
    this.RuleFor<string>((Expression<Func<AggregatedStockOrderLine, string>>) (x => x.UnitId)).NotEmpty<AggregatedStockOrderLine, string>();
    this.RuleFor<WatchedDictionary<string, Decimal>>((Expression<Func<AggregatedStockOrderLine, WatchedDictionary<string, Decimal>>>) (x => x.Orders)).NotEmpty<AggregatedStockOrderLine, WatchedDictionary<string, Decimal>>();
  }
}
