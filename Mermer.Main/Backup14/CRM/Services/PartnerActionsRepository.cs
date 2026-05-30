// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.CRM.Services.PartnerActionsRepository
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Commerce.Models;
using Mermer.Core.Couch.Common;
using Mermer.CRM.Models;
using Mermer.CRM.Services;
using Mermer.Data.Authorizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.CRM.Services;

public class PartnerActionsRepository : CouchView, IPartnerActionsRepository
{
  private readonly ILoginService _loginService;
  private readonly IAuthorizationService _authService;
  private readonly IReadOnlyListAuthorizer<PartnerAction> _authorizer;

  public PartnerActionsRepository(
    ICouchCluster cluster,
    ILoginService loginService,
    IAuthorizationService authService,
    IReadOnlyListAuthorizer<PartnerAction> authorizer)
    : base(cluster)
  {
    this._loginService = loginService;
    this._authService = authService;
    this._authorizer = authorizer;
  }

  public async Task<int> CountAsync(
    DateTime? startDate,
    DateTime? endDate,
    string partnerId,
    params string[] officeIds)
  {
    return (await this.GetRecordsAsync<int>(startDate, endDate, new string[1]
    {
      partnerId
    }, officeIds, true)).Sum();
  }

  public Task<IEnumerable<PartnerAction>> GetAsync(
    DateTime? startDate,
    DateTime? endDate,
    string partnerId,
    params string[] officeIds)
  {
    return this.GetRecordsAsync<PartnerAction>(startDate, endDate, new string[1]
    {
      partnerId
    }, officeIds);
  }

  public async Task<Dictionary<string, PartnerActionInfo[]>> GetByPartnersAsync(
    string officeId,
    params string[] partners)
  {
    PartnerActionsRepository actionsRepository = this;
    string[] source = partners;
    if ((source != null ? (((IEnumerable<string>) source).Any<string>() ? 1 : 0) : 0) == 0)
      return new Dictionary<string, PartnerActionInfo[]>();
    return (await actionsRepository.GetRecordsAsync<PartnerAction>(new DateTime?(), new DateTime?(), partners, new string[1]
    {
      officeId
    })).Where<PartnerAction>((Func<PartnerAction, bool>) (x => x.TransactionIsCompleted && !x.TransactionIsDisabled)).GroupBy<PartnerAction, string>((Func<PartnerAction, string>) (x => x.ActionPartnerId)).ToDictionary<IGrouping<string, PartnerAction>, string, PartnerActionInfo[]>((Func<IGrouping<string, PartnerAction>, string>) (g => g.Key), (Func<IGrouping<string, PartnerAction>, PartnerActionInfo[]>) (g => g.GroupBy(x => new
    {
      TransactionId = x.TransactionId,
      TransactionDate = x.TransactionDate
    }).Select<IGrouping<\u003C\u003Ef__AnonymousType0<string, DateTime>, PartnerAction>, PartnerActionInfo>(z => new PartnerActionInfo()
    {
      TransactionId = z.Key.TransactionId,
      TransactionDate = z.Key.TransactionDate,
      ActionDebit = z.Sum<PartnerAction>((Func<PartnerAction, Decimal>) (x => x.ActionDebit)),
      ActionCredit = z.Sum<PartnerAction>((Func<PartnerAction, Decimal>) (x => x.ActionCredit))
    }).ToArray<PartnerActionInfo>()));
  }

  private Task<IEnumerable<T>> GetRecordsAsync<T>(
    DateTime? startDate,
    DateTime? endDate,
    string[] partnerIds,
    string[] officeIds,
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
      officeIds = officeIds == null || !((IEnumerable<string>) officeIds).All<string>((Func<string, bool>) (x => !string.IsNullOrEmpty((string) null))) ? readableAccountIds.ToArray<string>() : ((IEnumerable<string>) officeIds).Where<string>((Func<string, bool>) (x => readableAccountIds.Contains<string>(x))).ToArray<string>();
      Enum[] array = Enum.GetValues(typeof (InvoiceType)).Cast<Enum>().Union<Enum>(Enum.GetValues(typeof (BillType)).Cast<Enum>()).ToArray<Enum>();
      List<string> allActions = this._authService.FilterAvailableActions((Enum) TransactionAccessLevel.ReadAll, array).ToList<string>();
      List<string> list = this._authService.FilterAvailableActions((Enum) TransactionAccessLevel.ReadOwn, array).ToList<string>();
      if (this._authService.TryAuthorizeAction((Enum) TransactionActions.PartnerSlips, (Enum) TransactionAccessLevel.ReadAll))
        allActions.AddRange(Enum.GetValues(typeof (PartnerSlipType)).Cast<Enum>().Select<Enum, string>((Func<Enum, string>) (x => x.ToString())));
      if (this._authService.TryAuthorizeAction((Enum) TransactionActions.PartnerSlips, (Enum) TransactionAccessLevel.ReadOwn))
        list.AddRange(Enum.GetValues(typeof (PartnerSlipType)).Cast<Enum>().Select<Enum, string>((Func<Enum, string>) (x => x.ToString())));
      if (this._authService.TryAuthorizeAction((Enum) TransactionActions.PartnerTransfers, (Enum) TransactionAccessLevel.ReadAll))
        allActions.Add("PartnerTransfer");
      if (this._authService.TryAuthorizeAction((Enum) TransactionActions.PartnerTransfers, (Enum) TransactionAccessLevel.ReadOwn))
        list.Add("PartnerTransfer");
      source = allActions.Select<string, Tuple<string, string>>((Func<string, Tuple<string, string>>) (x => new Tuple<string, string>("all", x))).Union<Tuple<string, string>>(list.Where<string>((Func<string, bool>) (x => !allActions.Contains(x))).Select<string, Tuple<string, string>>((Func<string, Tuple<string, string>>) (x => new Tuple<string, string>(userId, x)))).ToList<Tuple<string, string>>();
    }
    return this.GetRecordsAsync<T>("crm", "partner-actions", source.SelectMany<Tuple<string, string>, Tuple<object, object>>((Func<Tuple<string, string>, IEnumerable<Tuple<object, object>>>) (x => ((IEnumerable<string>) officeIds).SelectMany<string, Tuple<object, object>>((Func<string, IEnumerable<Tuple<object, object>>>) (accountId => ((IEnumerable<string>) partnerIds).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (partnerId => new Tuple<object, object>((object) new string[5]
    {
      x.Item1,
      x.Item2,
      accountId ?? "all",
      partnerId ?? "all",
      startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : "0"
    }, (object) new string[5]
    {
      x.Item1,
      x.Item2,
      accountId ?? "all",
      partnerId ?? "all",
      endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : "zzz"
    }))))))).ToArray<Tuple<object, object>>(), reduce);
  }
}
