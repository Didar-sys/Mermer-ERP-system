using Couchbase.Views;
using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Core.Couch.Common;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.FundsManagement.Models;
using Mermer.FundsManagement.Models.Extenders;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Models.Extenders;
using Mermer.StockManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mermer.Core.Couch.StockManagement.Services;

public class StockBalancesAggregatedRepository : CouchView, IStockBalancesAggregatedRepository
{
    private readonly ILoginService _loginService;
    private readonly IStocksRepository _stocksRepository;
    private readonly IRepository<Currency> _currenciesRepository;
    private readonly IAuthorizationService _authorizationService;
    private readonly IStockRepriceEffectsRepository _repriceEffectsRepository;
    private readonly IReadOnlyListAuthorizer<StockBalanceWithData> _authorizer;

    public StockBalancesAggregatedRepository(
        ICouchCluster cluster,
        ILoginService loginService,
        IStocksRepository stocksRepository,
        IRepository<Currency> currenciesRepository,
        IAuthorizationService authorizationService,
        IStockRepriceEffectsRepository repriceEffectsRepository,
        IReadOnlyListAuthorizer<StockBalanceWithData> authorizer)
        : base(cluster)
    {
        _loginService = loginService;
        _stocksRepository = stocksRepository;
        _currenciesRepository = currenciesRepository;
        _authorizationService = authorizationService;
        _repriceEffectsRepository = repriceEffectsRepository;
        _authorizer = authorizer;
    }

    public async Task<StockBalanceAggregated> GetByTypeAggregatedAsync(string[] warehouseIds, DateTime dateFrom, DateTime dateTill)
    {
        _authorizer.Authorize();

        if (!_loginService.Session.IsAdmin)
        {
            var accounts = _authorizationService.GetAccessableAccounts(AccountAccessLevel.Read).ToList();
            warehouseIds = warehouseIds.Where(x => accounts.Contains(x)).ToArray();
        }

        if (!warehouseIds.Any())
            return new StockBalanceAggregated(true);

        var array1 = warehouseIds.Select(accountId => new Tuple<object, object>(
            new[] { accountId, "0" },
            new[] { accountId, dateFrom.ToString("yyyy-MM-dd") }
        )).ToArray();

        var startingBalances = (await GetRecordsAsync<StockBalance>("stock-management", "stock-balances-by-warehouse", array1, true, 3, x =>
        {
            var balance = x.Value;
            dynamic key = x.Key;
            balance.WarehouseId = (string)key[0];
            balance.StockId = (string)key[2];
            return balance;
        })).ToList();

        var array2 = warehouseIds.Select(accountId => new Tuple<object, object>(
            new[] { accountId, dateFrom.ToString("yyyy-MM-dd") },
            new[] { accountId, dateTill.ToString("yyyy-MM-dd") }
        )).ToArray();

        var changingBalances = (await GetRecordsAsync<StockBalanceByTypeWithDate>("stock-management", "stock-balances-by-warehouse", array2, true, 3, x =>
        {
            var balance = x.Value;
            dynamic key = x.Key;
            balance.WarehouseId = (string)key[0];
            balance.Date = Convert.ToDateTime(key[1]);
            balance.StockId = (string)key[2];
            return balance;
        })).ToList();

        var stockIds = startingBalances.Select(x => x.StockId)
            .Union(changingBalances.Select(x => x.StockId))
            .Distinct()
            .ToArray();

        if (!stockIds.Any())
            return new StockBalanceAggregated(true);

        var stocks = (await _stocksRepository.GetListAsync(stockIds)).ToArray();
        var currencies = (await _currenciesRepository.GetAsync()).ToDictionary(x => x.Id, x => x);

        var starting = startingBalances.Join(stocks, x => x.StockId, i => i.Id, (x, i) =>
        {
            var price = i.GetPrice(dateFrom);
            var rate = currencies[price.CurrencyId].GetRate(dateFrom);
            return x.Balance * price.Price * rate.Multiplier / rate.Divider;
        });

        var changing = changingBalances.Join(stocks, x => x.StockId, i => i.Id, (x, i) =>
        {
            var price = i.GetPrice(x.Date);
            var rate = currencies[price.CurrencyId].GetRate(x.Date);
            decimal num = price.Price * rate.Multiplier / rate.Divider;
            return new StockBalanceAggregated
            {
                Income = x.Income * num,
                Expense = x.Expense * num,
                Lines = new[]
                {
                    new StockBalanceAggregatedLine("StockOpening", x.StockOpening * num),
                    new StockBalanceAggregatedLine("StockSpoilage", x.StockSpoilage * num),
                    new StockBalanceAggregatedLine("StockUsage", x.StockUsage * num),
                    new StockBalanceAggregatedLine("RevisionExceed", x.RevisionExceed * num),
                    new StockBalanceAggregatedLine("RevisionDeficit", x.RevisionDeficit * num),
                    new StockBalanceAggregatedLine("StockTransferSource", x.StockTransferSource * num),
                    new StockBalanceAggregatedLine("StockTransferDestination", x.StockTransferDestination * num),
                    new StockBalanceAggregatedLine("Sales", x.Sales * num),
                    new StockBalanceAggregatedLine("SalesReturn", x.SalesReturn * num),
                    new StockBalanceAggregatedLine("Purchase", x.Purchase * num),
                    new StockBalanceAggregatedLine("PurchaseReturn", x.PurchaseReturn * num),
                    new StockBalanceAggregatedLine("Repricing", 0M)
                }
            };
        }).ToList();

        StockBalanceAggregatedLine[] balanceAggregatedLineArray;
        try
        {
            balanceAggregatedLineArray = (await _repriceEffectsRepository.GetAsync(dateFrom, dateTill, warehouseIds))
                .Select(x => new StockBalanceAggregatedLine("Repricing", x.BalanceEffect))
                .ToArray();
        }
        catch
        {
            balanceAggregatedLineArray = Array.Empty<StockBalanceAggregatedLine>();
        }

        return new StockBalanceAggregated
        {
            StartingBalance = starting.Sum(),
            Income = changing.Sum(x => x.Income) + balanceAggregatedLineArray.Sum(x => x.Income),
            Expense = changing.Sum(x => x.Expense) + balanceAggregatedLineArray.Sum(x => x.Expense),
            Lines = changing.SelectMany(x => x.Lines)
                .Union(balanceAggregatedLineArray)
                .GroupBy(x => x.Type)
                .Select(g => new StockBalanceAggregatedLine
                {
                    Type = g.Key,
                    Income = g.Sum(x => x.Income),
                    Expense = g.Sum(x => x.Expense)
                })
        };
    }

    private class StockBalanceByTypeWithDate : StockBalanceByType
    {
        public DateTime Date { get; set; }
    }
}