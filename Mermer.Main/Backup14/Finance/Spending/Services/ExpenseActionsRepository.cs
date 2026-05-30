// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Finance.Spending.Services.ExpenseActionsRepository
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Core.Couch.Common;
using Mermer.Finance.Spending.Models;
using Mermer.Finance.Spending.Services;
using Mermer.Data.Authorizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Finance.Spending.Services;

public class ExpenseActionsRepository : CouchView, IExpenseActionsRepository
{
  private readonly ILoginService _loginService;
  private readonly IAuthorizationService _authService;
  private readonly IReadOnlyListAuthorizer<ExpenseAction> _authorizer;

  public ExpenseActionsRepository(
    ICouchCluster cluster,
    ILoginService loginService,
    IAuthorizationService authService,
    IReadOnlyListAuthorizer<ExpenseAction> authorizer)
    : base(cluster)
  {
    this._loginService = loginService;
    this._authService = authService;
    this._authorizer = authorizer;
  }

  public async Task<int> CountAsync(
    DateTime? startDate,
    DateTime? endDate,
    string[] depositoryIds,
    string expenseId)
  {
    return (await this.GetRecordsAsync<int>(startDate, endDate, depositoryIds, expenseId, true)).Sum();
  }

  public Task<IEnumerable<ExpenseAction>> GetAsync(
    DateTime? startDate,
    DateTime? endDate,
    string[] depositoryIds,
    string expenseId)
  {
    return this.GetRecordsAsync<ExpenseAction>(startDate, endDate, depositoryIds, expenseId);
  }

  private Task<IEnumerable<T>> GetRecordsAsync<T>(
    DateTime? startDate,
    DateTime? endDate,
    string[] depositoryIds,
    string expenseId,
    bool reduce = false)
  {
    this._authorizer.Authorize();
    List<string> source;
    if (this._loginService.Session.IsAdmin)
    {
      source = new List<string>() { "all" };
    }
    else
    {
      string userId = this._loginService.Session.UserId;
      IEnumerable<string> readableAccountIds = this._authService.GetAccessableAccounts(AccountAccessLevel.Read);
      depositoryIds = ((IEnumerable<string>) depositoryIds).Where<string>((Func<string, bool>) (x => readableAccountIds.Contains<string>(x))).ToArray<string>();
      if (this._authService.TryAuthorizeAction((Enum) TransactionActions.ExpenseSlips, (Enum) TransactionAccessLevel.ReadAll))
      {
        source = new List<string>() { "all" };
      }
      else
      {
        if (!this._authService.TryAuthorizeAction((Enum) TransactionActions.ExpenseSlips, (Enum) TransactionAccessLevel.ReadOwn))
          return Task.FromResult<IEnumerable<T>>(((IEnumerable<T>) Array.Empty<T>()).AsEnumerable<T>());
        source = new List<string>() { userId };
      }
    }
    return this.GetRecordsAsync<T>("finance-spending", "expense-actions", source.SelectMany<string, Tuple<object, object>>((Func<string, IEnumerable<Tuple<object, object>>>) (x => ((IEnumerable<string>) depositoryIds).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (accountId => new Tuple<object, object>((object) new string[4]
    {
      x,
      accountId ?? "all",
      expenseId ?? "all",
      startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : "0"
    }, (object) new string[4]
    {
      x,
      accountId ?? "all",
      expenseId ?? "all",
      endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : "zzz"
    }))))).ToArray<Tuple<object, object>>(), reduce);
  }
}
