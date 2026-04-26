// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Transactions.Models.Validators.TransactionValidator`2
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using Payhas.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Transactions.Models.Validators;

public class TransactionValidator<T, TLine> : TransactionValidator<T>
  where T : Transaction<TLine>
  where TLine : TransactionLine
{
  public TransactionValidator(
    IValidator<TLine> lineValidator,
    IValidator<CurrencyConvertion> currencyConvertionValidator)
  {
    ((IRuleBuilder<T, IEnumerable<TLine>>) this.RuleFor<WatchedObservableCollection<TLine>>((Expression<Func<T, WatchedObservableCollection<TLine>>>) (x => x.Lines)).Must<T, WatchedObservableCollection<TLine>>((Func<WatchedObservableCollection<TLine>, bool>) (x => x != null && x.Any<TLine>())).WithLocalizationMessageKey<T, WatchedObservableCollection<TLine>>("{PropertyName} can not be empty")).SetCollectionValidator<T, TLine>(lineValidator);
    this.RuleFor<WatchedObservableCollection<TLine>>((Expression<Func<T, WatchedObservableCollection<TLine>>>) (x => x.Lines)).Must<T, WatchedObservableCollection<TLine>>((Func<T, WatchedObservableCollection<TLine>, bool>) ((model, list) => list == null || list.Where<TLine>((Func<TLine, bool>) (x => !string.IsNullOrEmpty(x.CurrencyId))).All<TLine>((Func<TLine, bool>) (x =>
    {
      WatchedObservableCollection<CurrencyConvertion> currencyConvertions = model.CurrencyConvertions;
      return currencyConvertions != null && currencyConvertions.Any<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (z => z.CurrencyId == x.CurrencyId));
    })))).WithLocalizationMessageKey<T, WatchedObservableCollection<TLine>>("Not all currencies in {PropertyName} convertable");
    ((IRuleBuilder<T, IEnumerable<CurrencyConvertion>>) this.RuleFor<WatchedObservableCollection<CurrencyConvertion>>((Expression<Func<T, WatchedObservableCollection<CurrencyConvertion>>>) (x => x.CurrencyConvertions))).SetCollectionValidator<T, CurrencyConvertion>(currencyConvertionValidator).Must<T, IEnumerable<CurrencyConvertion>>((Func<IEnumerable<CurrencyConvertion>, bool>) (list => list == null || list.GroupBy<CurrencyConvertion, string>((Func<CurrencyConvertion, string>) (i => i.CurrencyId)).All<IGrouping<string, CurrencyConvertion>>((Func<IGrouping<string, CurrencyConvertion>, bool>) (g => g.Count<CurrencyConvertion>() == 1)))).WithLocalizationMessageKey<T, IEnumerable<CurrencyConvertion>>("Some convertions in {PropertyName} apear more than once");
  }
}
