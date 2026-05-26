// Decompiled with JetBrains decompiler
// Type: Mermer.Finance.Models.Authorizers.FundsTransferAuthorizer
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Common.Exceptions;
using Mermer.Common.Services;
using Mermer.Transactions.Models.Authorizers;
using Mermer.Data.Tools.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Finance.Models.Authorizers;

public class FundsTransferAuthorizer(
  ILoginService loginService,
  ILocalizationService localizationService,
  IAuthorizationService authService) : TransactionAuthorizer<FundsTransfer>(loginService, localizationService, authService, (Enum) TransactionActions.FundsTransfers)
{
  protected override string[] GetAccessedAccounts(FundsTransfer item)
  {
    return new string[2]
    {
      item.DepositoryId,
      item.DestinationDepositoryId
    };
  }

  public override void AuthorizeCreate(FundsTransfer item, string errorMessage = null)
  {
    if (this.LoginService.Session.IsAdmin)
      return;
    this.Authorize((Enum) TransactionAccessLevel.Create, errorMessage);
    if (item.IsCompleted)
      this.Authorize(item, TransactionAccessLevel.CompleteOwn, TransactionAccessLevel.CompleteAll, errorMessage ?? this.LocalizationService.GetText("You are not authorized to create completed transaction"));
    if (item.IsDisabled)
      this.Authorize(item, TransactionAccessLevel.DeleteOwn, TransactionAccessLevel.DeleteAll, errorMessage ?? this.LocalizationService.GetText("You are not authorized to create deleted transaction"));
    string[] array = this.AuthService.GetAccessableAccounts(AccountAccessLevel.Operate).ToArray<string>();
    if (!((IEnumerable<string>) array).Contains<string>(item.DepositoryId) && this.GetSentAmounts(item).Any<KeyValuePair<string, Decimal>>((Func<KeyValuePair<string, Decimal>, bool>) (x => x.Value > 0M)))
      throw new AuthorizationFailedException(errorMessage ?? this.LocalizationService.GetText("You are not authorized to modify sent amounts"));
    if (!((IEnumerable<string>) array).Contains<string>(item.DestinationDepositoryId) && this.GetReceivedAmounts(item).Any<KeyValuePair<string, Decimal>>((Func<KeyValuePair<string, Decimal>, bool>) (x => x.Value > 0M)))
      throw new AuthorizationFailedException(errorMessage ?? this.LocalizationService.GetText("You are not authorized to modify received amounts"));
  }

  public override void AuthorizeUpdate(
    FundsTransfer oldItem,
    FundsTransfer newItem,
    string errorMessage = null)
  {
    if (this.LoginService.Session.IsAdmin)
      return;
    this.Authorize(newItem, TransactionAccessLevel.UpdateOwn, TransactionAccessLevel.UpdateAll, errorMessage);
    if (newItem.IsCompleted || oldItem.IsCompleted != newItem.IsCompleted)
      this.Authorize(newItem, TransactionAccessLevel.CompleteOwn, TransactionAccessLevel.CompleteAll, errorMessage ?? this.LocalizationService.GetText("You are not authorized to (un)complete this transaction, or modify completed transaction"));
    if (newItem.IsDisabled || oldItem.IsDisabled != newItem.IsDisabled)
      this.Authorize(newItem, TransactionAccessLevel.DeleteOwn, TransactionAccessLevel.DeleteAll, errorMessage ?? this.LocalizationService.GetText("You are not authorized to (un)delete this transaction, or modify deleted transaction"));
    string[] array = this.AuthService.GetAccessableAccounts(AccountAccessLevel.Operate).ToArray<string>();
    if (!((IEnumerable<string>) array).Contains<string>(newItem.DepositoryId))
    {
      Dictionary<string, Decimal> oldValues = this.GetSentAmounts(oldItem);
      Dictionary<string, Decimal> newValues = this.GetSentAmounts(newItem);
      if (oldValues.Any<KeyValuePair<string, Decimal>>((Func<KeyValuePair<string, Decimal>, bool>) (x => !newValues.ContainsKey(x.Key) || newValues[x.Key] != x.Value)) || newValues.Any<KeyValuePair<string, Decimal>>((Func<KeyValuePair<string, Decimal>, bool>) (x => !oldValues.ContainsKey(x.Key))))
        throw new AuthorizationFailedException(errorMessage ?? this.LocalizationService.GetText("You are not authorized to modify sent amounts"));
    }
    if (((IEnumerable<string>) array).Contains<string>(newItem.DestinationDepositoryId))
      return;
    Dictionary<string, Decimal> oldValues1 = this.GetReceivedAmounts(oldItem);
    Dictionary<string, Decimal> newValues1 = this.GetReceivedAmounts(newItem);
    if (oldValues1.Any<KeyValuePair<string, Decimal>>((Func<KeyValuePair<string, Decimal>, bool>) (x => !newValues1.ContainsKey(x.Key) || newValues1[x.Key] != x.Value)) || newValues1.Any<KeyValuePair<string, Decimal>>((Func<KeyValuePair<string, Decimal>, bool>) (x => !oldValues1.ContainsKey(x.Key))))
      throw new AuthorizationFailedException(errorMessage ?? this.LocalizationService.GetText("You are not authorized to modify received amounts"));
  }

  private Dictionary<string, Decimal> GetSentAmounts(FundsTransfer item)
  {
    return item.Lines.GroupBy<FundsTransferLine, string>((Func<FundsTransferLine, string>) (x => x.CurrencyId)).ToDictionary<IGrouping<string, FundsTransferLine>, string, Decimal>((Func<IGrouping<string, FundsTransferLine>, string>) (g => g.Key), (Func<IGrouping<string, FundsTransferLine>, Decimal>) (g => g.Sum<FundsTransferLine>((Func<FundsTransferLine, Decimal>) (x => x.ActionTotal))));
  }

  private Dictionary<string, Decimal> GetReceivedAmounts(FundsTransfer item)
  {
    return item.Lines.GroupBy<FundsTransferLine, string>((Func<FundsTransferLine, string>) (x => x.CurrencyId)).ToDictionary<IGrouping<string, FundsTransferLine>, string, Decimal>((Func<IGrouping<string, FundsTransferLine>, string>) (g => g.Key), (Func<IGrouping<string, FundsTransferLine>, Decimal>) (g => g.Sum<FundsTransferLine>((Func<FundsTransferLine, Decimal>) (x => x.ActionReceivedTotal))));
  }

  protected override Expression<Func<FundsTransfer, bool>> GetFilter(IEnumerable<string> accounts)
  {
    if (!(accounts is string[] strArray))
      strArray = accounts.ToArray<string>();
    string[] accountsArray = strArray;
    return Predicate.Create<FundsTransfer>((Expression<Func<FundsTransfer, bool>>) (x => accountsArray.Contains<string>(x.DepositoryId) || accountsArray.Contains<string>(x.DestinationDepositoryId)));
  }
}
