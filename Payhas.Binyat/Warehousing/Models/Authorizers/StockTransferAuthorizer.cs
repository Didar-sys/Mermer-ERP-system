// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Warehousing.Models.Authorizers.StockTransferAuthorizer
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Authorization.Enums;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Common.Exceptions;
using Payhas.Binyat.Common.Services;
using Payhas.Binyat.Transactions.Models.Authorizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Warehousing.Models.Authorizers;

public class StockTransferAuthorizer(
  ILoginService loginService,
  ILocalizationService localizationService,
  IAuthorizationService authService) : TransactionAuthorizer<StockTransfer>(loginService, localizationService, authService, (Enum) TransactionActions.StockTransfers)
{
  protected override string[] GetAccessedAccounts(StockTransfer item)
  {
    return new string[2]
    {
      item.WarehouseId,
      item.DestinationWarehouseId
    };
  }

  public override void AuthorizeCreate(StockTransfer item, string errorMessage = null)
  {
    if (this.LoginService.Session.IsAdmin)
      return;
    this.Authorize((Enum) TransactionAccessLevel.Create, errorMessage);
    if (item.IsCompleted)
      this.Authorize(item, TransactionAccessLevel.CompleteOwn, TransactionAccessLevel.CompleteAll, errorMessage ?? this.LocalizationService.GetText("You are not authorized to create completed transaction"));
    if (item.IsDisabled)
      this.Authorize(item, TransactionAccessLevel.DeleteOwn, TransactionAccessLevel.DeleteAll, errorMessage ?? this.LocalizationService.GetText("You are not authorized to create deleted transaction"));
    string[] array = this.AuthService.GetAccessableAccounts(AccountAccessLevel.Operate).ToArray<string>();
    if (!((IEnumerable<string>) array).Contains<string>(item.WarehouseId) && this.GetSentQuantites(item).Any<KeyValuePair<string, Decimal>>((Func<KeyValuePair<string, Decimal>, bool>) (x => x.Value > 0M)))
      throw new AuthorizationFailedException(errorMessage ?? this.LocalizationService.GetText("You are not authorized to modify sent quantities"));
    if (!((IEnumerable<string>) array).Contains<string>(item.DestinationWarehouseId) && this.GetReceivedQuantites(item).Any<KeyValuePair<string, Decimal>>((Func<KeyValuePair<string, Decimal>, bool>) (x => x.Value > 0M)))
      throw new AuthorizationFailedException(errorMessage ?? this.LocalizationService.GetText("You are not authorized to modify received quantities"));
  }

  public override void AuthorizeUpdate(
    StockTransfer oldItem,
    StockTransfer newItem,
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
    if (!((IEnumerable<string>) array).Contains<string>(newItem.WarehouseId))
    {
      Dictionary<string, Decimal> oldQuantities = this.GetSentQuantites(oldItem);
      Dictionary<string, Decimal> newQuantities = this.GetSentQuantites(newItem);
      if (oldQuantities.Any<KeyValuePair<string, Decimal>>((Func<KeyValuePair<string, Decimal>, bool>) (x => !newQuantities.ContainsKey(x.Key) || newQuantities[x.Key] != x.Value)) || newQuantities.Any<KeyValuePair<string, Decimal>>((Func<KeyValuePair<string, Decimal>, bool>) (x => !oldQuantities.ContainsKey(x.Key))))
        throw new AuthorizationFailedException(errorMessage ?? this.LocalizationService.GetText("You are not authorized to modify sent quantities"));
    }
    if (((IEnumerable<string>) array).Contains<string>(newItem.DestinationWarehouseId))
      return;
    Dictionary<string, Decimal> oldQuantities1 = this.GetReceivedQuantites(oldItem);
    Dictionary<string, Decimal> newQuantities1 = this.GetReceivedQuantites(newItem);
    if (oldQuantities1.Any<KeyValuePair<string, Decimal>>((Func<KeyValuePair<string, Decimal>, bool>) (x => !newQuantities1.ContainsKey(x.Key) || newQuantities1[x.Key] != x.Value)) || newQuantities1.Any<KeyValuePair<string, Decimal>>((Func<KeyValuePair<string, Decimal>, bool>) (x => !oldQuantities1.ContainsKey(x.Key))))
      throw new AuthorizationFailedException(errorMessage ?? this.LocalizationService.GetText("You are not authorized to modify received quantities"));
  }

  private Dictionary<string, Decimal> GetSentQuantites(StockTransfer item)
  {
    return item.Lines.GroupBy<StockTransferLine, string>((Func<StockTransferLine, string>) (x => x.StockId)).ToDictionary<IGrouping<string, StockTransferLine>, string, Decimal>((Func<IGrouping<string, StockTransferLine>, string>) (g => g.Key), (Func<IGrouping<string, StockTransferLine>, Decimal>) (g => g.Sum<StockTransferLine>((Func<StockTransferLine, Decimal>) (x => x.ActionQuantity))));
  }

  private Dictionary<string, Decimal> GetReceivedQuantites(StockTransfer item)
  {
    return item.Lines.GroupBy<StockTransferLine, string>((Func<StockTransferLine, string>) (x => x.StockId)).ToDictionary<IGrouping<string, StockTransferLine>, string, Decimal>((Func<IGrouping<string, StockTransferLine>, string>) (g => g.Key), (Func<IGrouping<string, StockTransferLine>, Decimal>) (g => g.Sum<StockTransferLine>((Func<StockTransferLine, Decimal>) (x => x.ActionReceivedQuantity))));
  }

  protected override Expression<Func<StockTransfer, bool>> GetFilter(IEnumerable<string> accounts)
  {
    return (Expression<Func<StockTransfer, bool>>) null;
  }
}
