// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.StockManagement.Models.Validators.StockValidator
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using Payhas.Binyat.Common.Models.Validators;
using Payhas.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.StockManagement.Models.Validators;

public class StockValidator : AbstractModelValidator<Stock>
{
  public StockValidator(
    IValidator<StockUnit> stockUnitValidator,
    IValidator<StockPrice> stockPriceValidator,
    IValidator<StockAdditionalPrice> stockAdditionalPriceValidator)
  {
    this.RuleFor<string>((Expression<Func<Stock, string>>) (x => x.Code)).NotEmpty<Stock, string>();
    this.RuleFor<string>((Expression<Func<Stock, string>>) (x => x.Name)).NotEmpty<Stock, string>();
    ((IRuleBuilder<Stock, IEnumerable<StockUnit>>) this.RuleFor<ObservableCollection<StockUnit>>((Expression<Func<Stock, ObservableCollection<StockUnit>>>) (x => x.Units)).Must<Stock, ObservableCollection<StockUnit>>((Func<ObservableCollection<StockUnit>, bool>) (x => x != null && x.Any<StockUnit>())).WithLocalizationMessageKey<Stock, ObservableCollection<StockUnit>>("{PropertyName} must be specified").Must<Stock, ObservableCollection<StockUnit>>((Func<ObservableCollection<StockUnit>, bool>) (x => x != null && x.Count<StockUnit>((Func<StockUnit, bool>) (z => z.IsDefault)) == 1)).WithLocalizationMessageKey<Stock, ObservableCollection<StockUnit>>("{PropertyName} must contain (only) one default unit").Must<Stock, ObservableCollection<StockUnit>>((Func<ObservableCollection<StockUnit>, bool>) (x =>
    {
      StockUnit stockUnit = x != null ? x.FirstOrDefault<StockUnit>((Func<StockUnit, bool>) (z => z.IsDefault)) : (StockUnit) null;
      if (stockUnit == null)
        return true;
      return stockUnit.Multiplier == 1M && stockUnit.Divider == 1M;
    })).WithLocalizationMessageKey<Stock, ObservableCollection<StockUnit>>("Default unit must have multiplier = 1 & divider = 1")).SetCollectionValidator<Stock, StockUnit>(stockUnitValidator);
    ((IRuleBuilder<Stock, IEnumerable<StockPrice>>) this.RuleFor<WatchedObservableCollection<StockPrice>>((Expression<Func<Stock, WatchedObservableCollection<StockPrice>>>) (x => x.Prices)).Must<Stock, WatchedObservableCollection<StockPrice>>((Func<WatchedObservableCollection<StockPrice>, bool>) (x => x != null && x.Any<StockPrice>())).WithLocalizationMessageKey<Stock, WatchedObservableCollection<StockPrice>>("{PropertyName} must be specified")).SetCollectionValidator<Stock, StockPrice>(stockPriceValidator);
    ((IRuleBuilder<Stock, IEnumerable<StockAdditionalPrice>>) this.RuleFor<WatchedObservableCollection<StockAdditionalPrice>>((Expression<Func<Stock, WatchedObservableCollection<StockAdditionalPrice>>>) (x => x.AdditionalPrices))).SetCollectionValidator<Stock, StockAdditionalPrice>(stockAdditionalPriceValidator);
  }
}
