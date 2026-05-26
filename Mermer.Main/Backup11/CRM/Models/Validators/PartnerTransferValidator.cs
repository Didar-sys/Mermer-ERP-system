// Decompiled with JetBrains decompiler
// Type: Mermer.CRM.Models.Validators.PartnerTransferValidator
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

public class PartnerTransferValidator : TransactionValidator<PartnerTransfer>
{
  public PartnerTransferValidator(
    IValidator<PartnerTransferLine> lineValidator,
    IValidator<CurrencyConvertion> currencyConvertionValidator)
  {
    this.RuleFor<bool>((Expression<Func<PartnerTransfer, bool>>) (x => x.IsCompleted)).NotEqual<PartnerTransfer, bool>(true).When<PartnerTransfer, bool>((Func<PartnerTransfer, bool>) (x => x.IsConflicted)).WithLocalizationMessageKey<PartnerTransfer, bool>("Tranfer can not be completed while conflicted");
    ((IRuleBuilder<PartnerTransfer, IEnumerable<PartnerTransferLine>>) this.RuleFor<ObservableCollection<PartnerTransferLine>>((Expression<Func<PartnerTransfer, ObservableCollection<PartnerTransferLine>>>) (x => x.Lines)).Must<PartnerTransfer, ObservableCollection<PartnerTransferLine>>((Func<ObservableCollection<PartnerTransferLine>, bool>) (x => x != null && x.Any<PartnerTransferLine>())).WithLocalizationMessageKey<PartnerTransfer, ObservableCollection<PartnerTransferLine>>("{PropertyName} can not be empty")).SetCollectionValidator<PartnerTransfer, PartnerTransferLine>(lineValidator).Must<PartnerTransfer, IEnumerable<PartnerTransferLine>>((Func<PartnerTransfer, IEnumerable<PartnerTransferLine>, bool>) ((model, list) =>
    {
      if (!(list is PartnerTransferLine[] partnerTransferLineArray2))
        partnerTransferLineArray2 = list != null ? list.ToArray<PartnerTransferLine>() : (PartnerTransferLine[]) null;
      PartnerTransferLine[] source = partnerTransferLineArray2;
      return source == null || ((IEnumerable<PartnerTransferLine>) source).Select<PartnerTransferLine, string>((Func<PartnerTransferLine, string>) (x => x.DebitCurrencyId)).Union<string>(((IEnumerable<PartnerTransferLine>) source).Select<PartnerTransferLine, string>((Func<PartnerTransferLine, string>) (x => x.CreditCurrencyId))).Distinct<string>().Where<string>((Func<string, bool>) (x => !string.IsNullOrEmpty(x))).All<string>((Func<string, bool>) (x =>
      {
        ObservableCollection<CurrencyConvertion> currencyConvertions = model.CurrencyConvertions;
        return currencyConvertions != null && currencyConvertions.Any<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (z => z.CurrencyId == x));
      }));
    })).WithLocalizationMessageKey<PartnerTransfer, IEnumerable<PartnerTransferLine>>("Not all currencies in {PropertyName} convertable");
    ((IRuleBuilder<PartnerTransfer, IEnumerable<CurrencyConvertion>>) this.RuleFor<ObservableCollection<CurrencyConvertion>>((Expression<Func<PartnerTransfer, ObservableCollection<CurrencyConvertion>>>) (x => x.CurrencyConvertions))).SetCollectionValidator<PartnerTransfer, CurrencyConvertion>(currencyConvertionValidator).Must<PartnerTransfer, IEnumerable<CurrencyConvertion>>((Func<IEnumerable<CurrencyConvertion>, bool>) (list => list == null || list.GroupBy<CurrencyConvertion, string>((Func<CurrencyConvertion, string>) (i => i.CurrencyId)).All<IGrouping<string, CurrencyConvertion>>((Func<IGrouping<string, CurrencyConvertion>, bool>) (g => g.Count<CurrencyConvertion>() == 1)))).WithLocalizationMessageKey<PartnerTransfer, IEnumerable<CurrencyConvertion>>("Some convertions in {PropertyName} apear more than once");
  }
}
