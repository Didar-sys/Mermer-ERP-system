using Couchbase.Views;
using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Core.Couch.Common;
using Mermer.CRM.Models;
using Mermer.CRM.Services;
using Mermer.Data.Authorizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mermer.Core.Couch.CRM.Services;

public class PartnerBalancesRepository : CouchView, IPartnerBalancesRepository
{
    private readonly ILoginService _loginService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IReadOnlyListAuthorizer<PartnerBalance> _authorizer;

    public PartnerBalancesRepository(
      ICouchCluster cluster,
      ILoginService loginService,
      IAuthorizationService authorizationService,
      IReadOnlyListAuthorizer<PartnerBalance> authorizer)
      : base(cluster)
    {
        _loginService = loginService;
        _authorizationService = authorizationService;
        _authorizer = authorizer;
    }

    public async Task<PartnerBalanceResult> GetBalanceToDateAsync(string officeId, string partnerId, DateTime date, string excludeTransactionId = null)
    {
        if (string.IsNullOrEmpty(officeId)) throw new ArgumentNullException(nameof(officeId));
        if (string.IsNullOrEmpty(partnerId)) throw new ArgumentException(nameof(partnerId));

        if (!_loginService.Session.IsAdmin && !_authorizationService.GetAccessableAccounts(AccountAccessLevel.Read).Contains(officeId))
            return new PartnerBalanceResult();

        var startEndKeys = new[]
        {
            new Tuple<object, object>(
                new[] { "all", "all", officeId, partnerId, "0" },
                new[] { "all", "all", officeId, partnerId, date.ToString("o") }
            )
        };

        var list = (await GetRecordsAsync<PartnerAction>("crm", "partner-actions", startEndKeys)).ToList();

        return new PartnerBalanceResult
        {
            Balance = list.Where(x => x.TransactionId != excludeTransactionId && x.TransactionDate < date && x.TransactionIsCompleted && !x.TransactionIsDisabled)
                          .Sum(x => x.ActionEffect)
        };
    }

    public async Task<IEnumerable<PartnerBalanceByTypeWithBalance>> GetByTypeAsync(DateTime dateFrom, DateTime dateTill, string partnerId, params string[] officeIds)
    {
        _authorizer.Authorize();

        if (dateFrom >= dateTill) throw new ArgumentException("From date should be lower than or equal to till date");

        officeIds = officeIds?.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        // Замість того, щоб "впускати" програму, просто повертаємо пустий список (нульовий баланс), якщо офісів немає
        if (officeIds == null || !officeIds.Any())
        {
            return Array.Empty<PartnerBalanceByTypeWithBalance>();
        }

        if (!_loginService.Session.IsAdmin)
        {
            var accounts = _authorizationService.GetAccessableAccounts(AccountAccessLevel.Read).ToArray();
            officeIds = !officeIds.Any(string.IsNullOrEmpty) ? officeIds.Where(accounts.Contains).ToArray() : accounts;
            if (!officeIds.Any()) return Array.Empty<PartnerBalanceByTypeWithBalance>();
        }

        List<PartnerBalance> startingBalances;
        List<PartnerBalanceByType> changingBalances;

        if (!string.IsNullOrEmpty(partnerId))
        {
            var array1 = officeIds.Select(officeId => new Tuple<object, object>(
                new[] { officeId, partnerId, "0" },
                new[] { officeId, partnerId, dateFrom.ToString("o") })).ToArray();

            startingBalances = (await GetRecordsAsync<PartnerBalance>("crm", "partner-balances-by-office-and-id", array1, true, 2, x =>
            {
                var byTypeAsync = x.Value;
                dynamic key = x.Key;
                byTypeAsync.OfficeId = (string)key[0];
                byTypeAsync.PartnerId = (string)key[1];
                return byTypeAsync;
            })).ToList();

            var array2 = officeIds.Select(officeId => new Tuple<object, object>(
                new[] { officeId, partnerId, dateFrom.ToString("o") },
                new[] { officeId, partnerId, dateTill.ToString("o") })).ToArray();

            changingBalances = (await GetRecordsAsync<PartnerBalanceByType>("crm", "partner-balances-by-office-and-id", array2, true, 2, x =>
            {
                var byTypeAsync = x.Value;
                dynamic key = x.Key;
                byTypeAsync.OfficeId = (string)key[0];
                byTypeAsync.PartnerId = (string)key[1];
                return byTypeAsync;
            })).ToList();
        }
        else
        {
            var array3 = officeIds.Select(officeId => new Tuple<object, object>(
                new[] { officeId, "0" },
                new[] { officeId, dateFrom.ToString("o") })).ToArray();

            startingBalances = (await GetRecordsAsync<PartnerBalance>("crm", "partner-balances-by-office", array3, true, 3, x =>
            {
                var byTypeAsync = x.Value;
                dynamic key = x.Key;
                byTypeAsync.OfficeId = (string)key[0];
                byTypeAsync.PartnerId = (string)key[2];
                return byTypeAsync;
            })).GroupBy(x => new { x.OfficeId, x.PartnerId }).Select(g => new PartnerBalance
            {
                OfficeId = g.Key.OfficeId,
                PartnerId = g.Key.PartnerId,
                Debit = g.Sum(x => x.Debit),
                Credit = g.Sum(x => x.Credit)
            }).ToList();

            var array4 = officeIds.Select(officeId => new Tuple<object, object>(
                new[] { officeId, dateFrom.ToString("o") },
                new[] { officeId, dateTill.ToString("o") })).ToArray();

            changingBalances = (await GetRecordsAsync<PartnerBalanceByType>("crm", "partner-balances-by-office", array4, true, 3, x =>
            {
                var byTypeAsync = x.Value;
                dynamic key = x.Key;
                byTypeAsync.OfficeId = (string)key[0];
                byTypeAsync.PartnerId = (string)key[2];
                return byTypeAsync;
            })).GroupBy(x => new { x.OfficeId, x.PartnerId }).Select(g => new PartnerBalanceByType
            {
                OfficeId = g.Key.OfficeId,
                PartnerId = g.Key.PartnerId,
                Debit = g.Sum(x => x.Debit),
                Credit = g.Sum(x => x.Credit),
                PartnerOpeningBalance = g.Sum(x => x.PartnerOpeningBalance),
                PartnerBalanceRevision = g.Sum(x => x.PartnerBalanceRevision),
                PartnerTransfer = g.Sum(x => x.PartnerTransfer),
                Sales = g.Sum(x => x.Sales),
                SalesReturn = g.Sum(x => x.SalesReturn),
                Purchase = g.Sum(x => x.Purchase),
                PurchaseReturn = g.Sum(x => x.PurchaseReturn),
                Payment = g.Sum(x => x.Payment),
                Collection = g.Sum(x => x.Collection)
            }).ToList();
        }

        return startingBalances.Select(x => new { x.OfficeId, x.PartnerId })
            .Union(changingBalances.Select(x => new { x.OfficeId, x.PartnerId }))
            .Distinct()
            .Select(x => new
            {
                item = x,
                startingBalances = startingBalances.Where(z => z.OfficeId == x.OfficeId && z.PartnerId == x.PartnerId),
                changingBalances = changingBalances.Where(z => z.OfficeId == x.OfficeId && z.PartnerId == x.PartnerId)
            })
            .Select(x => new PartnerBalanceByTypeWithBalance
            {
                OfficeId = x.item.OfficeId,
                PartnerId = x.item.PartnerId,
                StartingBalance = x.startingBalances.Sum(z => z.Balance),
                Debit = x.changingBalances.Sum(z => z.Debit),
                Credit = x.changingBalances.Sum(z => z.Credit),
                PartnerOpeningBalance = x.changingBalances.Sum(z => z.PartnerOpeningBalance),
                PartnerBalanceRevision = x.changingBalances.Sum(z => z.PartnerBalanceRevision),
                PartnerTransfer = x.changingBalances.Sum(z => z.PartnerTransfer),
                Sales = x.changingBalances.Sum(z => z.Sales),
                SalesReturn = x.changingBalances.Sum(z => z.SalesReturn),
                Purchase = x.changingBalances.Sum(z => z.Purchase),
                PurchaseReturn = x.changingBalances.Sum(z => z.PurchaseReturn),
                Payment = x.changingBalances.Sum(z => z.Payment),
                Collection = x.changingBalances.Sum(z => z.Collection)
            });
    }

    public async Task<PartnerBalanceAggregated> GetByTypeAggregatedAsync(string[] officeIds, DateTime dateFrom, DateTime dateTill)
    {
        var list = (await GetByTypeAsync(dateFrom, dateTill, null, officeIds)).ToList();

        return new PartnerBalanceAggregated
        {
            Debit = list.Sum(x => x.Debit),
            Credit = list.Sum(x => x.Credit),
            StartingBalance = list.Sum(x => x.StartingBalance),
            Lines = new[]
            {
                new PartnerBalanceAggregatedLine("PartnerOpeningBalance", list.Sum(z => z.PartnerOpeningBalance)),
                new PartnerBalanceAggregatedLine("PartnerBalanceRevision", list.Sum(z => z.PartnerBalanceRevision)),
                new PartnerBalanceAggregatedLine("PartnerTransfer", list.Sum(z => z.PartnerTransfer)),
                new PartnerBalanceAggregatedLine("Sales", list.Sum(z => z.Sales)),
                new PartnerBalanceAggregatedLine("SalesReturn", list.Sum(z => z.SalesReturn)),
                new PartnerBalanceAggregatedLine("Purchase", list.Sum(z => z.Purchase)),
                new PartnerBalanceAggregatedLine("PurchaseReturn", list.Sum(z => z.PurchaseReturn)),
                new PartnerBalanceAggregatedLine("Payment", list.Sum(z => z.Payment)),
                new PartnerBalanceAggregatedLine("Collection", list.Sum(z => z.Collection))
            }
        };
    }
}