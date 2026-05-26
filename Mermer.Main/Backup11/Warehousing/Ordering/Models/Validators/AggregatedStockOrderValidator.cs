// Decompiled with JetBrains decompiler
// Type: Mermer.Warehousing.Ordering.Models.Validators.AggregatedStockOrderValidator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using Mermer.Transactions.Models.Validators;
using Mermer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Warehousing.Ordering.Models.Validators;

public class AggregatedStockOrderValidator : TransactionValidator<AggregatedStockOrder>
{
  public AggregatedStockOrderValidator(IValidator<AggregatedStockOrderLine> lineValidator)
  {
    this.RuleFor<string>((Expression<Func<AggregatedStockOrder, string>>) (x => x.WarehouseId)).NotEmpty<AggregatedStockOrder, string>();
    ((IRuleBuilder<AggregatedStockOrder, IEnumerable<AggregatedStockOrderLine>>) this.RuleFor<WatchedObservableCollection<AggregatedStockOrderLine>>((Expression<Func<AggregatedStockOrder, WatchedObservableCollection<AggregatedStockOrderLine>>>) (x => x.Lines)).NotEmpty<AggregatedStockOrder, WatchedObservableCollection<AggregatedStockOrderLine>>().Must<AggregatedStockOrder, WatchedObservableCollection<AggregatedStockOrderLine>>((Func<WatchedObservableCollection<AggregatedStockOrderLine>, bool>) (lines => lines != null && lines.Any<AggregatedStockOrderLine>())).WithLocalizationMessageKey<AggregatedStockOrder, WatchedObservableCollection<AggregatedStockOrderLine>>("{PropertyName} should not be empty!")).SetCollectionValidator<AggregatedStockOrder, AggregatedStockOrderLine>(lineValidator);
  }
}
