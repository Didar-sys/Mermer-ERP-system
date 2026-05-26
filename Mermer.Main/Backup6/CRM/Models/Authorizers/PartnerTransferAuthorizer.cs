// Decompiled with JetBrains decompiler
// Type: Mermer.CRM.Models.Authorizers.PartnerTransferAuthorizer
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Common.Exceptions;
using Mermer.Common.Services;
using Mermer.Transactions.Models;
using Mermer.Transactions.Models.Authorizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.CRM.Models.Authorizers;

public class PartnerTransferAuthorizer(
  ILoginService loginService,
  ILocalizationService localizationService,
  IAuthorizationService authService) : TransactionAuthorizer<PartnerTransfer>(loginService, localizationService, authService, (Enum) TransactionActions.PartnerTransfers)
{
  protected override string[] GetAccessedAccounts(PartnerTransfer item)
  {
    IEnumerable<string> officeIds = item.OfficeIds;
    return (officeIds != null ? officeIds.ToArray<string>() : (string[]) null) ?? new string[0];
  }

  public override void AuthorizeCreate(PartnerTransfer item, string errorMessage = null)
  {
    if (this.LoginService.Session.IsAdmin)
      return;
    this.Authorize((Enum) TransactionAccessLevel.Create, errorMessage);
    if (item.IsCompleted)
      this.Authorize(item, TransactionAccessLevel.CompleteOwn, TransactionAccessLevel.CompleteAll, errorMessage ?? this.LocalizationService.GetText("You are not authorized to create completed transaction"));
    if (item.IsDisabled)
      this.Authorize(item, TransactionAccessLevel.DeleteOwn, TransactionAccessLevel.DeleteAll, errorMessage ?? this.LocalizationService.GetText("You are not authorized to create deleted transaction"));
    string[] operatableAccounts = this.AuthService.GetAccessableAccounts(AccountAccessLevel.Operate).ToArray<string>();
    foreach (string officeId in ((IEnumerable<string>) this.GetAccessedAccounts(item)).Where<string>((Func<string, bool>) (x => !((IEnumerable<string>) operatableAccounts).Contains<string>(x))))
    {
      if (this.GeDebitAmounts(item, officeId).Any<KeyValuePair<string, Decimal>>((Func<KeyValuePair<string, Decimal>, bool>) (x => x.Value != 0M)))
        throw new AuthorizationFailedException(errorMessage ?? this.LocalizationService.GetText("You are not authorized"));
      if (this.GeCreditAmounts(item, officeId).Any<KeyValuePair<string, Decimal>>((Func<KeyValuePair<string, Decimal>, bool>) (x => x.Value != 0M)))
        throw new AuthorizationFailedException(errorMessage ?? this.LocalizationService.GetText("You are not authorized"));
    }
  }

  public override void AuthorizeUpdate(
    PartnerTransfer oldItem,
    PartnerTransfer newItem,
    string errorMessage = null)
  {
    if (this.LoginService.Session.IsAdmin)
      return;
    this.Authorize(newItem, TransactionAccessLevel.UpdateOwn, TransactionAccessLevel.UpdateAll, errorMessage);
    if (newItem.IsCompleted || oldItem.IsCompleted != newItem.IsCompleted)
      this.Authorize(newItem, TransactionAccessLevel.CompleteOwn, TransactionAccessLevel.CompleteAll, errorMessage ?? this.LocalizationService.GetText("You are not authorized to (un)complete this transaction, or modify completed transaction"));
    if (newItem.IsDisabled || oldItem.IsDisabled != newItem.IsDisabled)
      this.Authorize(newItem, TransactionAccessLevel.DeleteOwn, TransactionAccessLevel.DeleteAll, errorMessage ?? this.LocalizationService.GetText("You are not authorized to (un)delete this transaction, or modify deleted transaction"));
    string[] operatableAccounts = this.AuthService.GetAccessableAccounts(AccountAccessLevel.Operate).ToArray<string>();
    foreach (string officeId in ((IEnumerable<string>) ((IEnumerable<string>) this.GetAccessedAccounts(oldItem)).Union<string>((IEnumerable<string>) this.GetAccessedAccounts(newItem)).Distinct<string>().ToArray<string>()).Where<string>((Func<string, bool>) (x => !((IEnumerable<string>) operatableAccounts).Contains<string>(x))))
    {
      Dictionary<string, Decimal> oldDebits = this.GeDebitAmounts(oldItem, officeId);
      Dictionary<string, Decimal> newDebits = this.GeDebitAmounts(newItem, officeId);
      if (oldDebits.Any<KeyValuePair<string, Decimal>>((Func<KeyValuePair<string, Decimal>, bool>) (x => !newDebits.ContainsKey(x.Key) || newDebits[x.Key] != x.Value)) || newDebits.Any<KeyValuePair<string, Decimal>>((Func<KeyValuePair<string, Decimal>, bool>) (x => !oldDebits.ContainsKey(x.Key))))
        throw new AuthorizationFailedException(errorMessage ?? this.LocalizationService.GetText("You are not authorized"));
      Dictionary<string, Decimal> oldCredits = this.GeCreditAmounts(oldItem, officeId);
      Dictionary<string, Decimal> newCredits = this.GeCreditAmounts(newItem, officeId);
      if (oldCredits.Any<KeyValuePair<string, Decimal>>((Func<KeyValuePair<string, Decimal>, bool>) (x => !newCredits.ContainsKey(x.Key) || newCredits[x.Key] != x.Value)) || newCredits.Any<KeyValuePair<string, Decimal>>((Func<KeyValuePair<string, Decimal>, bool>) (x => !oldCredits.ContainsKey(x.Key))))
        throw new AuthorizationFailedException(errorMessage ?? this.LocalizationService.GetText("You are not authorized"));
    }
  }

  private Dictionary<string, Decimal> GeDebitAmounts(PartnerTransfer item, string officeId)
  {
    Dictionary<string, Decimal> dictionary = new Dictionary<string, Decimal>();
    foreach (IGrouping<string, PartnerTransferLine> grouping in item.Lines.Where<PartnerTransferLine>((Func<PartnerTransferLine, bool>) (x => x.OfficeId == officeId && x.DebitAmount > 0M)).GroupBy<PartnerTransferLine, string>((Func<PartnerTransferLine, string>) (x => x.PartnerId)))
    {
      Decimal num = 0M;
      foreach (PartnerTransferLine partnerTransferLine in (IEnumerable<PartnerTransferLine>) grouping)
      {
        PartnerTransferLine line = partnerTransferLine;
        CurrencyConvertion currencyConvertion = item.CurrencyConvertions.Single<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (x => x.CurrencyId == line.DebitCurrencyId));
        num += line.DebitAmount * currencyConvertion.Multiplier / currencyConvertion.Divider;
      }
      if (num > 0M)
        dictionary.Add(grouping.Key, num);
    }
    return dictionary;
  }

  private Dictionary<string, Decimal> GeCreditAmounts(PartnerTransfer item, string officeId)
  {
    Dictionary<string, Decimal> dictionary = new Dictionary<string, Decimal>();
    foreach (IGrouping<string, PartnerTransferLine> grouping in item.Lines.Where<PartnerTransferLine>((Func<PartnerTransferLine, bool>) (x => x.OfficeId == officeId && x.CreditAmount > 0M)).GroupBy<PartnerTransferLine, string>((Func<PartnerTransferLine, string>) (x => x.PartnerId)))
    {
      Decimal num = 0M;
      foreach (PartnerTransferLine partnerTransferLine in (IEnumerable<PartnerTransferLine>) grouping)
      {
        PartnerTransferLine line = partnerTransferLine;
        CurrencyConvertion currencyConvertion = item.CurrencyConvertions.Single<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (x => x.CurrencyId == line.CreditCurrencyId));
        num += line.CreditAmount * currencyConvertion.Multiplier / currencyConvertion.Divider;
      }
      if (num > 0M)
        dictionary.Add(grouping.Key, num);
    }
    return dictionary;
  }

  public override Expression<Func<PartnerTransfer, bool>> AuthorizedListFilter()
  {
    throw new Exception(this.LocalizationService.GetText("Report to application vendor!!!"));
  }

  protected override Expression<Func<PartnerTransfer, bool>> GetFilter(IEnumerable<string> accounts)
  {
    throw new NotImplementedException();
  }
}
