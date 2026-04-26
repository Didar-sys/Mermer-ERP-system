// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Warehousing.Ordering.Models.Validators.StockOrderValidator
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using Payhas.Binyat.Transactions.Models;
using Payhas.Binyat.Transactions.Models.Validators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Warehousing.Ordering.Models.Validators;

public class StockOrderValidator : TransactionValidator<StockOrder>
{
  public StockOrderValidator(
    StockOrderLineValidator lineValidator,
    IValidator<StockUnitConvertion> stockUnitConvertionValidator)
  {
    this.RuleFor<string>((Expression<Func<StockOrder, string>>) (x => x.WarehouseId)).NotEmpty<StockOrder, string>();
    ((IRuleBuilder<StockOrder, IEnumerable<StockOrderLine>>) this.RuleFor<ObservableCollection<StockOrderLine>>((Expression<Func<StockOrder, ObservableCollection<StockOrderLine>>>) (x => x.Lines)).Must<StockOrder, ObservableCollection<StockOrderLine>>((Func<ObservableCollection<StockOrderLine>, bool>) (x => x != null && x.Any<StockOrderLine>())).WithLocalizationMessageKey<StockOrder, ObservableCollection<StockOrderLine>>("{PropertyName} can not be empty")).SetCollectionValidator<StockOrder, StockOrderLine>((IValidator<StockOrderLine>) lineValidator).Must<StockOrder, IEnumerable<StockOrderLine>>((Func<StockOrder, IEnumerable<StockOrderLine>, bool>) ((model, list) => list == null || list.All<StockOrderLine>((Func<StockOrderLine, bool>) (x =>
    {
      ObservableCollection<StockUnitConvertion> stockUnitConvertions = model.StockUnitConvertions;
      return stockUnitConvertions != null && stockUnitConvertions.Any<StockUnitConvertion>((Func<StockUnitConvertion, bool>) (z => z.StockId == x.StockId && z.UnitId == x.UnitId));
    })))).WithLocalizationMessageKey<StockOrder, IEnumerable<StockOrderLine>>("Not all stock units in {PropertyName} convertable");
    ((IRuleBuilder<StockOrder, IEnumerable<StockUnitConvertion>>) this.RuleFor<ObservableCollection<StockUnitConvertion>>((Expression<Func<StockOrder, ObservableCollection<StockUnitConvertion>>>) (x => x.StockUnitConvertions))).SetCollectionValidator<StockOrder, StockUnitConvertion>(stockUnitConvertionValidator).Must<StockOrder, IEnumerable<StockUnitConvertion>>((Func<IEnumerable<StockUnitConvertion>, bool>) (list => list == null || list.GroupBy(i => new
    {
      StockId = i.StockId,
      UnitId = i.UnitId
    }).All<IGrouping<\u003C\u003Ef__AnonymousType2<string, string>, StockUnitConvertion>>(g => g.Count<StockUnitConvertion>() == 1))).WithLocalizationMessageKey<StockOrder, IEnumerable<StockUnitConvertion>>("Some convertions in {PropertyName} apear more than once");
  }
}
