// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Couch.FundsManagement.Services.FundsActionsRepository
// Assembly: Payhas.Binyat.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.Couch.dll

using Payhas.Binyat.Authorization.Enums;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Commerce.Models;
using Payhas.Binyat.Core.Couch.Common;
using Payhas.Binyat.CRM.Models;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.Finance.Models;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.FundsManagement.Services;
using Payhas.Data.Authorizers;
using Payhas.Data.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Core.Couch.FundsManagement.Services;

public class FundsActionsRepository : CouchView, IFundsActionsRepository
{
  private readonly ILoginService _loginService;
  private readonly IAuthorizationService _authService;
  private readonly IReadOnlyListAuthorizer<FundsAction> _authorizer;
  private readonly IReadOnlyRepository<Partner> _partnersRepository;
  private readonly IReadOnlyRepository<Depository> _depositoriesRepository;

  public FundsActionsRepository(
    ICouchCluster cluster,
    ILoginService loginService,
    IAuthorizationService authService,
    IReadOnlyListAuthorizer<FundsAction> authorizer,
    IReadOnlyRepository<Partner> partnersRepository,
    IReadOnlyRepository<Depository> depositoriesRepository)
    : base(cluster)
  {
    this._loginService = loginService;
    this._authService = authService;
    this._partnersRepository = partnersRepository;
    this._depositoriesRepository = depositoriesRepository;
    this._authorizer = authorizer;
  }

  public async Task<int> CountAsync(
    DateTime? startDate,
    DateTime? endDate,
    string currencyId,
    params string[] depositoryIds)
  {
    return (await this.GetRecordsAsync<int>(startDate, endDate, depositoryIds, currencyId, true)).Sum();
  }

  public async Task<IEnumerable<FundsAction>> GetAsync(
    DateTime? startDate,
    DateTime? endDate,
    string currencyId,
    params string[] depositoryIds)
  {
    List<FundsAction> list = (await this.GetRecordsAsync<FundsAction>(startDate, endDate, depositoryIds, currencyId)).ToList<FundsAction>();
    string[] relatedPartnerIds = list.Select<FundsAction, string>((Func<FundsAction, string>) (x => x.ActionRelatedPartnerId)).Where<string>((Func<string, bool>) (x => !string.IsNullOrEmpty(x))).ToArray<string>();
    if (((IEnumerable<string>) relatedPartnerIds).Any<string>())
    {
      Dictionary<string, string> dictionary = (await this._partnersRepository.GetAsync((Expression<Func<Partner, bool>>) (x => relatedPartnerIds.Contains<string>(x.Id)))).ToDictionary<Partner, string, string>((Func<Partner, string>) (x => x.Id), (Func<Partner, string>) (x => x.Name));
      foreach (FundsAction fundsAction in list.Where<FundsAction>((Func<FundsAction, bool>) (x => !string.IsNullOrEmpty(x.ActionRelatedPartnerId))))
        fundsAction.ActionRelatedObjectName = dictionary[fundsAction.ActionRelatedPartnerId];
    }
    string[] relatedDepositoryIds = list.Select<FundsAction, string>((Func<FundsAction, string>) (x => x.ActionRelatedDepositoryId)).Where<string>((Func<string, bool>) (x => !string.IsNullOrEmpty(x))).ToArray<string>();
    if (((IEnumerable<string>) relatedDepositoryIds).Any<string>())
    {
      Dictionary<string, string> dictionary = (await this._depositoriesRepository.GetAsync((Expression<Func<Depository, bool>>) (x => relatedDepositoryIds.Contains<string>(x.Id)))).ToDictionary<Depository, string, string>((Func<Depository, string>) (x => x.Id), (Func<Depository, string>) (x => x.Name));
      foreach (FundsAction fundsAction in list.Where<FundsAction>((Func<FundsAction, bool>) (x => !string.IsNullOrEmpty(x.ActionRelatedDepositoryId))))
        fundsAction.ActionRelatedObjectName = dictionary[fundsAction.ActionRelatedDepositoryId];
    }
    IEnumerable<FundsAction> async = (IEnumerable<FundsAction>) list;
    list = (List<FundsAction>) null;
    return async;
  }

  private Task<IEnumerable<T>> GetRecordsAsync<T>(
    DateTime? startDate,
    DateTime? endDate,
    string[] depositoryIds,
    string currencyId,
    bool reduce = false)
  {
    this._authorizer.Authorize();
    List<Tuple<string, string>> source;
    if (this._loginService.Session.IsAdmin)
    {
      source = new List<Tuple<string, string>>()
      {
        new Tuple<string, string>("all", "all")
      };
    }
    else
    {
      string userId = this._loginService.Session.UserId;
      IEnumerable<string> readableAccountIds = this._authService.GetAccessableAccounts(AccountAccessLevel.Read);
      depositoryIds = depositoryIds == null || !((IEnumerable<string>) depositoryIds).All<string>((Func<string, bool>) (x => !string.IsNullOrEmpty((string) null))) ? readableAccountIds.ToArray<string>() : ((IEnumerable<string>) depositoryIds).Where<string>((Func<string, bool>) (x => readableAccountIds.Contains<string>(x))).ToArray<string>();
      Enum[] array = Enum.GetValues(typeof (InvoiceType)).Cast<Enum>().Union<Enum>(Enum.GetValues(typeof (BillType)).Cast<Enum>()).Union<Enum>(Enum.GetValues(typeof (FundsSlipType)).Cast<Enum>()).ToArray<Enum>();
      List<string> allActions = this._authService.FilterAvailableActions((Enum) TransactionAccessLevel.ReadAll, array).ToList<string>();
      List<string> list = this._authService.FilterAvailableActions((Enum) TransactionAccessLevel.ReadOwn, array).ToList<string>();
      if (this._authService.TryAuthorizeAction((Enum) TransactionActions.FundsTransfers, (Enum) TransactionAccessLevel.ReadAll))
      {
        allActions.Add("FundsTransferSource");
        allActions.Add("FundsTransferDestination");
      }
      if (this._authService.TryAuthorizeAction((Enum) TransactionActions.FundsTransfers, (Enum) TransactionAccessLevel.ReadOwn))
      {
        list.Add("FundsTransferSource");
        list.Add("FundsTransferDestination");
      }
      if (this._authService.TryAuthorizeAction((Enum) TransactionActions.ExpenseSlips, (Enum) TransactionAccessLevel.ReadAll))
        allActions.Add("ExpenseSlip");
      if (this._authService.TryAuthorizeAction((Enum) TransactionActions.ExpenseSlips, (Enum) TransactionAccessLevel.ReadOwn))
        list.Add("ExpenseSlip");
      source = allActions.Select<string, Tuple<string, string>>((Func<string, Tuple<string, string>>) (x => new Tuple<string, string>("all", x))).Union<Tuple<string, string>>(list.Where<string>((Func<string, bool>) (x => !allActions.Contains(x))).Select<string, Tuple<string, string>>((Func<string, Tuple<string, string>>) (x => new Tuple<string, string>(userId, x)))).ToList<Tuple<string, string>>();
    }
    return this.GetRecordsAsync<T>("funds-management", "funds-actions", source.SelectMany<Tuple<string, string>, Tuple<object, object>>((Func<Tuple<string, string>, IEnumerable<Tuple<object, object>>>) (x => ((IEnumerable<string>) depositoryIds).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (accountId => new Tuple<object, object>((object) new string[5]
    {
      x.Item1,
      x.Item2,
      accountId ?? "all",
      currencyId ?? "all",
      startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : "0"
    }, (object) new string[5]
    {
      x.Item1,
      x.Item2,
      accountId ?? "all",
      currencyId ?? "all",
      endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : "zzz"
    }))))).ToArray<Tuple<object, object>>(), reduce);
  }
}
