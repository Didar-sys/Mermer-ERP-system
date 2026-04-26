// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Commerce.Models.Validators.BillValidator
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using Payhas.Binyat.Transactions.Models;
using Payhas.Binyat.Transactions.Models.Validators;
using Payhas.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Commerce.Models.Validators;

public class BillValidator : TransactionValidator<Bill>
{
  public BillValidator(
    IValidator<BillLine> lineValidator,
    IValidator<CurrencyConvertion> rateValidator)
  {
    this.RuleFor<string>((Expression<Func<Bill, string>>) (x => x.PartnerId)).NotEmpty<Bill, string>();
    this.RuleFor<string>((Expression<Func<Bill, string>>) (x => x.DepositoryId)).NotEmpty<Bill, string>();
    ((IRuleBuilder<Bill, IEnumerable<BillLine>>) this.RuleFor<WatchedObservableCollection<BillLine>>((Expression<Func<Bill, WatchedObservableCollection<BillLine>>>) (x => x.Lines)).Must<Bill, WatchedObservableCollection<BillLine>>((Func<WatchedObservableCollection<BillLine>, bool>) (x => x != null && x.Any<BillLine>())).WithLocalizationMessageKey<Bill, WatchedObservableCollection<BillLine>>("{PropertyName} can not be empty")).SetCollectionValidator<Bill, BillLine>(lineValidator).Must<Bill, IEnumerable<BillLine>>((Func<Bill, IEnumerable<BillLine>, bool>) ((model, list) => list == null || list.Where<BillLine>((Func<BillLine, bool>) (x => !string.IsNullOrEmpty(x.CurrencyId))).All<BillLine>((Func<BillLine, bool>) (x =>
    {
      WatchedObservableCollection<CurrencyConvertion> currencyConvertions = model.CurrencyConvertions;
      return currencyConvertions != null && currencyConvertions.Any<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (z => z.CurrencyId == x.CurrencyId));
    })))).WithLocalizationMessageKey<Bill, IEnumerable<BillLine>>("Not all currencies in {PropertyName} convertable");
    ((IRuleBuilder<Bill, IEnumerable<CurrencyConvertion>>) this.RuleFor<WatchedObservableCollection<CurrencyConvertion>>((Expression<Func<Bill, WatchedObservableCollection<CurrencyConvertion>>>) (x => x.CurrencyConvertions))).SetCollectionValidator<Bill, CurrencyConvertion>(rateValidator).Must<Bill, IEnumerable<CurrencyConvertion>>((Func<IEnumerable<CurrencyConvertion>, bool>) (list => list == null || list.GroupBy<CurrencyConvertion, string>((Func<CurrencyConvertion, string>) (i => i.CurrencyId)).All<IGrouping<string, CurrencyConvertion>>((Func<IGrouping<string, CurrencyConvertion>, bool>) (g => g.Count<CurrencyConvertion>() == 1)))).WithLocalizationMessageKey<Bill, IEnumerable<CurrencyConvertion>>("Some convertions in {PropertyName} apear more than once");
  }
}
