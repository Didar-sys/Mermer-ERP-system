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
        _loginService = loginService;
        _authService = authService;
        _authorizer = authorizer;
    }

    public async Task<int> CountAsync(DateTime? startDate, DateTime? endDate, string partnerId, params string[] officeIds)
    {
        var records = await GetRecordsAsync<int>(startDate, endDate, new[] { partnerId }, officeIds, true);
        return records.Sum();
    }

    public Task<IEnumerable<PartnerAction>> GetAsync(DateTime? startDate, DateTime? endDate, string partnerId, params string[] officeIds)
    {
        return GetRecordsAsync<PartnerAction>(startDate, endDate, new[] { partnerId }, officeIds);
    }

    public async Task<Dictionary<string, PartnerActionInfo[]>> GetByPartnersAsync(string officeId, params string[] partners)
    {
        if (partners == null || !partners.Any())
            return new Dictionary<string, PartnerActionInfo[]>();

        var records = await GetRecordsAsync<PartnerAction>(null, null, partners, new[] { officeId });

        return records
            .Where(x => x.TransactionIsCompleted && !x.TransactionIsDisabled)
            .GroupBy(x => x.ActionPartnerId)
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(x => new { x.TransactionId, x.TransactionDate })
                      .Select(z => new PartnerActionInfo
                      {
                          TransactionId = z.Key.TransactionId,
                          TransactionDate = z.Key.TransactionDate,
                          ActionDebit = z.Sum(x => x.ActionDebit),
                          ActionCredit = z.Sum(x => x.ActionCredit)
                      }).ToArray()
            );
    }

    private Task<IEnumerable<T>> GetRecordsAsync<T>(DateTime? startDate, DateTime? endDate, string[] partnerIds, string[] officeIds, bool reduce = false)
    {
        _authorizer.Authorize();
        List<Tuple<string, string>> source;

        if (_loginService.Session.IsAdmin)
        {
            source = new List<Tuple<string, string>> { new Tuple<string, string>("all", "all") };
        }
        else
        {
            string userId = _loginService.Session.UserId;
            var readableAccountIds = _authService.GetAccessableAccounts(AccountAccessLevel.Read).ToArray();

            officeIds = officeIds == null || officeIds.All(string.IsNullOrEmpty)
                ? readableAccountIds
                : officeIds.Where(readableAccountIds.Contains).ToArray();

            var array = Enum.GetValues(typeof(InvoiceType)).Cast<Enum>().Union(Enum.GetValues(typeof(BillType)).Cast<Enum>()).ToArray();
            var allActions = _authService.FilterAvailableActions(TransactionAccessLevel.ReadAll, array).ToList();
            var list = _authService.FilterAvailableActions(TransactionAccessLevel.ReadOwn, array).ToList();

            if (_authService.TryAuthorizeAction(TransactionActions.PartnerSlips, TransactionAccessLevel.ReadAll))
                allActions.AddRange(Enum.GetValues(typeof(PartnerSlipType)).Cast<Enum>().Select(x => x.ToString()));
            if (_authService.TryAuthorizeAction(TransactionActions.PartnerSlips, TransactionAccessLevel.ReadOwn))
                list.AddRange(Enum.GetValues(typeof(PartnerSlipType)).Cast<Enum>().Select(x => x.ToString()));
            if (_authService.TryAuthorizeAction(TransactionActions.PartnerTransfers, TransactionAccessLevel.ReadAll))
                allActions.Add("PartnerTransfer");
            if (_authService.TryAuthorizeAction(TransactionActions.PartnerTransfers, TransactionAccessLevel.ReadOwn))
                list.Add("PartnerTransfer");

            source = allActions.Select(x => new Tuple<string, string>("all", x))
                .Union(list.Where(x => !allActions.Contains(x)).Select(x => new Tuple<string, string>(userId, x)))
                .ToList();
        }

        var startEndKeys = source.SelectMany(x => officeIds.SelectMany(accountId => partnerIds.Select(partnerId =>
            new Tuple<object, object>(
                new[] { x.Item1, x.Item2, accountId ?? "all", partnerId ?? "all", startDate?.ToString("yyyy-MM-dd") ?? "0" },
                new[] { x.Item1, x.Item2, accountId ?? "all", partnerId ?? "all", endDate?.ToString("yyyy-MM-dd") ?? "zzz" }
            )
        ))).ToArray();

        return GetRecordsAsync<T>("crm", "partner-actions", startEndKeys, reduce);
    }
}