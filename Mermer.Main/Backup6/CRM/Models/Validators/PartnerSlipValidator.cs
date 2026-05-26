// Decompiled with JetBrains decompiler
// Type: Mermer.CRM.Models.Validators.PartnerSlipValidator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using Mermer.Transactions.Models;
using Mermer.Transactions.Models.Validators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.CRM.Models.Validators;

public class PartnerSlipValidator : TransactionValidator<PartnerSlip>
{
  public PartnerSlipValidator(
    IValidator<PartnerSlipLine> lineValidator,
    IValidator<CurrencyConvertion> currencyConvertionValidator)
  {
    this.RuleFor<string>((Expression<Func<PartnerSlip, string>>) (x => x.OfficeId)).NotEmpty<PartnerSlip, string>();
    ((IRuleBuilder<PartnerSlip, IEnumerable<PartnerSlipLine>>) this.RuleFor<ObservableCollection<PartnerSlipLine>>((Expression<Func<PartnerSlip, ObservableCollection<PartnerSlipLine>>>) (x => x.Lines)).Must<PartnerSlip, ObservableCollection<PartnerSlipLine>>((Func<ObservableCollection<PartnerSlipLine>, bool>) (x => x != null && x.Any<PartnerSlipLine>())).WithLocalizationMessageKey<PartnerSlip, ObservableCollection<PartnerSlipLine>>("{PropertyName} can not be empty")).SetCollectionValidator<PartnerSlip, PartnerSlipLine>(lineValidator).Must<PartnerSlip, IEnumerable<PartnerSlipLine>>((Func<PartnerSlip, IEnumerable<PartnerSlipLine>, bool>) ((model, list) =>
    {
      if (!(list is PartnerSlipLine[] partnerSlipLineArray2))
        partnerSlipLineArray2 = list != null ? list.ToArray<PartnerSlipLine>() : (PartnerSlipLine[]) null;
      PartnerSlipLine[] source = partnerSlipLineArray2;
      return source == null || ((IEnumerable<PartnerSlipLine>) source).Select<PartnerSlipLine, string>((Func<PartnerSlipLine, string>) (x => x.DebitCurrencyId)).Union<string>(((IEnumerable<PartnerSlipLine>) source).Select<PartnerSlipLine, string>((Func<PartnerSlipLine, string>) (x => x.CreditCurrencyId))).Distinct<string>().Where<string>((Func<string, bool>) (x => !string.IsNullOrEmpty(x))).All<string>((Func<string, bool>) (x =>
      {
        ObservableCollection<CurrencyConvertion> currencyConvertions = model.CurrencyConvertions;
        return currencyConvertions != null && currencyConvertions.Any<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (z => z.CurrencyId == x));
      }));
    })).WithLocalizationMessageKey<PartnerSlip, IEnumerable<PartnerSlipLine>>("Not all currencies in {PropertyName} convertable");
    ((IRuleBuilder<PartnerSlip, IEnumerable<CurrencyConvertion>>) this.RuleFor<ObservableCollection<CurrencyConvertion>>((Expression<Func<PartnerSlip, ObservableCollection<CurrencyConvertion>>>) (x => x.CurrencyConvertions))).SetCollectionValidator<PartnerSlip, CurrencyConvertion>(currencyConvertionValidator).Must<PartnerSlip, IEnumerable<CurrencyConvertion>>((Func<IEnumerable<CurrencyConvertion>, bool>) (list => list == null || list.GroupBy<CurrencyConvertion, string>((Func<CurrencyConvertion, string>) (i => i.CurrencyId)).All<IGrouping<string, CurrencyConvertion>>((Func<IGrouping<string, CurrencyConvertion>, bool>) (g => g.Count<CurrencyConvertion>() == 1)))).WithLocalizationMessageKey<PartnerSlip, IEnumerable<CurrencyConvertion>>("Some convertions in {PropertyName} apear more than once");
  }
}
