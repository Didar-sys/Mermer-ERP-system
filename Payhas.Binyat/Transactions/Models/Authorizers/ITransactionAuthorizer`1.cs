// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Transactions.Models.Authorizers.ITransactionAuthorizer`1
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Authorization.Enums;
using Payhas.Binyat.Authorization.Models;
using Payhas.Data.Authorizers;
using System.Collections.Generic;

#nullable disable
namespace Payhas.Binyat.Transactions.Models.Authorizers;

public interface ITransactionAuthorizer<T> : 
  IListAuthorizer<T>,
  IReadOnlyListAuthorizer<T>,
  IAuthorizer
{
  UserSession GetCurrentSession();

  IEnumerable<string> GetAvailableAccounts(AccountAccessLevel accessLevel);

  IEnumerable<string> GetAvailableActions(TransactionAccessLevel accessLevel);

  bool CanChangeDate();
}
