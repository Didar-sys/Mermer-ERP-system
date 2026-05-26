// Decompiled with JetBrains decompiler
// Type: Mermer.FundsManagement.Models.Validators.CurrencyValidator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using Mermer.Common.Models.Validators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.FundsManagement.Models.Validators;

public class CurrencyValidator : AbstractModelValidator<Currency>
{
  public CurrencyValidator(IValidator<CurrencyRate> currencyRateValidator)
  {
    this.RuleFor<string>((Expression<Func<Currency, string>>) (x => x.Name)).NotEmpty<Currency, string>();
    this.RuleFor<int>((Expression<Func<Currency, int>>) (x => x.Decimals)).GreaterThanOrEqualTo<Currency, int>(0);
    ((IRuleBuilder<Currency, IEnumerable<CurrencyRate>>) this.RuleFor<ObservableCollection<CurrencyRate>>((Expression<Func<Currency, ObservableCollection<CurrencyRate>>>) (x => x.Rates))).SetCollectionValidator<Currency, CurrencyRate>(currencyRateValidator);
    this.RuleFor<ObservableCollection<CurrencyRate>>((Expression<Func<Currency, ObservableCollection<CurrencyRate>>>) (x => x.Rates)).Must<Currency, ObservableCollection<CurrencyRate>>((Func<ObservableCollection<CurrencyRate>, bool>) (x => x != null && x.Any<CurrencyRate>())).WithLocalizationMessageKey<Currency, ObservableCollection<CurrencyRate>>("{PropertyName} must be specified");
    this.RuleFor<ObservableCollection<CurrencyRate>>((Expression<Func<Currency, ObservableCollection<CurrencyRate>>>) (x => x.Rates)).Must<Currency, ObservableCollection<CurrencyRate>>((Func<ObservableCollection<CurrencyRate>, bool>) (x => x.Count == 1 && x.Count<CurrencyRate>((Func<CurrencyRate, bool>) (z => z.Multiplier == 1M && z.Divider == 1M)) == 1)).When<Currency, ObservableCollection<CurrencyRate>>((Func<Currency, bool>) (x => x.IsDefault)).WithLocalizationMessageKey<Currency, ObservableCollection<CurrencyRate>>("Default currency must have only one rate with multiplier = 1 & divider = 1");
  }
}
