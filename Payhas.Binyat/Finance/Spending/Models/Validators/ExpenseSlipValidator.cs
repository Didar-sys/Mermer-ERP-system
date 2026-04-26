// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Finance.Spending.Models.Validators.ExpenseSlipValidator
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
namespace Payhas.Binyat.Finance.Spending.Models.Validators;

public class ExpenseSlipValidator : TransactionValidator<ExpenseSlip>
{
  public ExpenseSlipValidator(
    IValidator<ExpenseSlipLine> lineValidator,
    IValidator<CurrencyConvertion> currencyConvertionValidator)
  {
    this.RuleFor<string>((Expression<Func<ExpenseSlip, string>>) (x => x.DepositoryId)).NotEmpty<ExpenseSlip, string>();
    ((IRuleBuilder<ExpenseSlip, IEnumerable<ExpenseSlipLine>>) this.RuleFor<WatchedObservableCollection<ExpenseSlipLine>>((Expression<Func<ExpenseSlip, WatchedObservableCollection<ExpenseSlipLine>>>) (x => x.Lines)).Must<ExpenseSlip, WatchedObservableCollection<ExpenseSlipLine>>((Func<WatchedObservableCollection<ExpenseSlipLine>, bool>) (x => x != null && x.Any<ExpenseSlipLine>())).WithLocalizationMessageKey<ExpenseSlip, WatchedObservableCollection<ExpenseSlipLine>>("{PropertyName} can not be empty")).SetCollectionValidator<ExpenseSlip, ExpenseSlipLine>(lineValidator).Must<ExpenseSlip, IEnumerable<ExpenseSlipLine>>((Func<ExpenseSlip, IEnumerable<ExpenseSlipLine>, bool>) ((model, list) => list == null || list.Where<ExpenseSlipLine>((Func<ExpenseSlipLine, bool>) (x => !string.IsNullOrEmpty(x.CurrencyId))).All<ExpenseSlipLine>((Func<ExpenseSlipLine, bool>) (x =>
    {
      WatchedObservableCollection<CurrencyConvertion> currencyConvertions = model.CurrencyConvertions;
      return currencyConvertions != null && currencyConvertions.Any<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (z => z.CurrencyId == x.CurrencyId));
    })))).WithLocalizationMessageKey<ExpenseSlip, IEnumerable<ExpenseSlipLine>>("Not all currencies in {PropertyName} convertable");
    ((IRuleBuilder<ExpenseSlip, IEnumerable<CurrencyConvertion>>) this.RuleFor<WatchedObservableCollection<CurrencyConvertion>>((Expression<Func<ExpenseSlip, WatchedObservableCollection<CurrencyConvertion>>>) (x => x.CurrencyConvertions))).SetCollectionValidator<ExpenseSlip, CurrencyConvertion>(currencyConvertionValidator).Must<ExpenseSlip, IEnumerable<CurrencyConvertion>>((Func<IEnumerable<CurrencyConvertion>, bool>) (list => list == null || list.GroupBy<CurrencyConvertion, string>((Func<CurrencyConvertion, string>) (i => i.CurrencyId)).All<IGrouping<string, CurrencyConvertion>>((Func<IGrouping<string, CurrencyConvertion>, bool>) (g => g.Count<CurrencyConvertion>() == 1)))).WithLocalizationMessageKey<ExpenseSlip, IEnumerable<CurrencyConvertion>>("Some convertions in {PropertyName} apear more than once");
  }
}
