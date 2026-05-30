using Couchbase.Core;
using Couchbase.Views;
using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Commerce.Models;
using Mermer.Core.Couch.Common;
using Mermer.CRM.Models;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Enterprise.Models;
using Mermer.FundsManagement.Models;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;
using Mermer.Warehousing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mermer.StockManagement.Models.Extenders;
using Mermer.FundsManagement.Models.Extenders;

namespace Mermer.Core.Couch.StockManagement.Services;

public class StockActionsRepository : CouchView, IStockActionsRepository
{
    private readonly ILoginService _loginService;
    private readonly IAuthorizationService _authService;
    private readonly IReadOnlyListAuthorizer<StockActionWithData> _authorizer;
    private readonly IStocksRepository _stocksRepository;
    private readonly IStockBalancesRepository _balancesRepository;
    private readonly IReadOnlyRepository<Partner> _partnersRepository;
    private readonly IReadOnlyRepository<Currency> _currenciesRepository;
    private readonly IReadOnlyRepository<Warehouse> _warehousesRepository;

    public StockActionsRepository(
        ICouchCluster cluster,
        ILoginService loginService,
        IAuthorizationService authService,
        IReadOnlyListAuthorizer<StockActionWithData> authorizer,
        IStocksRepository stocksRepository,
        IStockBalancesRepository balancesRepository,
        IReadOnlyRepository<Partner> partnersRepository,
        IReadOnlyRepository<Currency> currenciesRepository,
        IReadOnlyRepository<Warehouse> warehousesRepository)
        : base(cluster)
    {
        _loginService = loginService;
        _authService = authService;
        _authorizer = authorizer;
        _stocksRepository = stocksRepository;
        _balancesRepository = balancesRepository;
        _partnersRepository = partnersRepository;
        _currenciesRepository = currenciesRepository;
        _warehousesRepository = warehousesRepository;
    }

    public async Task<int> CountAsync(DateTime? startDate, DateTime? endDate, string stockId, params string[] warehouseIds)
    {
        var records = await GetRecordsAsync<int>(startDate, endDate, warehouseIds, stockId, true);
        return records.Sum();
    }

    public async Task<IEnumerable<StockActionWithData>> GetAsync(DateTime? startDate, DateTime? endDate, string stockId, params string[] warehouseIds)
    {
        var list = (await GetActionsAsync<StockActionWithData>(startDate, endDate, stockId, warehouseIds)).ToList();

        var relatedPartners = new Dictionary<string, string>();
        var partnerIds = list.Select(x => x.ActionRelatedPartnerId).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToArray();
        if (partnerIds.Any())
        {
            relatedPartners = (await _partnersRepository.GetAsync(partnerIds))
                .Where(x => x != null)
                .ToDictionary(x => x.Id, x => x.Name);
        }

        var relatedWarehouses = new Dictionary<string, string>();
        var whIds = list.Select(x => x.ActionRelatedWarehouseId).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToArray();
        if (whIds.Any())
        {
            relatedWarehouses = (await _warehousesRepository.GetAsync(whIds))
                .ToDictionary(x => x.Id, x => x.Name);
        }

        var stockIds = list.Select(x => x.ActionStockId).Distinct().ToArray();
        if (stockIds.Any())
        {
            var relatedStocks = (await _stocksRepository.GetListAsync(stockIds)).ToDictionary(x => x.Id, x => x);
            var currencies = (await _currenciesRepository.GetAsync()).ToDictionary(x => x.Id, x => x);
            var defaultCurrency = currencies.Values.Single(x => x.IsDefault);

            foreach (var action in list)
            {
                var stock = relatedStocks[action.ActionStockId];
                action.StockCode = stock.Code;
                action.StockName = stock.Name;
                action.StockType = stock.Type;
                action.StockGroup = stock.Group;
                action.StockTags = stock.Tags;

                var price = stock.GetPrice(action.TransactionDate);
                var rate = currencies[price.CurrencyId].GetRate(action.TransactionDate);

                action.RecommendedPrice = Math.Round(price.Price * rate.Multiplier / rate.Divider, defaultCurrency.Decimals);

                if (!string.IsNullOrEmpty(action.ActionRelatedPartnerId) && relatedPartners.ContainsKey(action.ActionRelatedPartnerId))
                    action.ActionRelatedObjectName = relatedPartners[action.ActionRelatedPartnerId];

                if (!string.IsNullOrEmpty(action.ActionRelatedWarehouseId) && relatedWarehouses.ContainsKey(action.ActionRelatedWarehouseId))
                    action.ActionRelatedObjectName = relatedWarehouses[action.ActionRelatedWarehouseId];
            }
        }
        return list;
    }

    // ВИПРАВЛЕНО: Прибрано сотні рядків рефлексії. Використано чистий dynamic.
    private async Task<IEnumerable<T>> GetActionsAsync<T>(DateTime? startDate, DateTime? endDate, string stockId, params string[] warehouseIds) where T : StockAction
    {
        int dateIndexOwn = stockId == null ? 3 : 4;
        int stockIndexOwn = stockId == null ? 4 : 3;
        int dateIndexAll = stockId == null ? 2 : 3;
        int stockIndexAll = stockId == null ? 3 : 2;

        return await GetRecordsAsync<T>(startDate, endDate, warehouseIds, stockId, reduce: false,
            projectorAll: x =>
            {
                T instance = Activator.CreateInstance<T>();
                dynamic key = x.Key;
                dynamic val = x.Value;

                instance.TransactionId = (string)val.tId;
                instance.TransactionCode = (string)val.tCode;
                instance.TransactionDate = Convert.ToDateTime(key[dateIndexAll]);
                instance.TransactionType = (string)key[0];
                instance.TransactionUserId = (string)key[4];
                instance.TransactionUserName = (string)val.tUserName;
                instance.TransactionIsCash = (bool?)val.tIsCash ?? false;
                instance.TransactionIsCompleted = (bool?)val.tIsCompleted ?? false;
                instance.TransactionIsDisabled = (bool?)val.tIsDisabled ?? false;
                instance.TransactionGroup = (string)val.tGroup;

                if (val.tTags != null)
                {
                    instance.TransactionTags = val.tTags.ToObject<List<string>>();
                }

                instance.ActionId = (string)val.aId;
                instance.ActionSourceId = (string)val.aSourceId;
                instance.ActionWarehouseId = (string)key[1];
                instance.ActionStockId = (string)key[stockIndexAll];
                instance.ActionRelatedPartnerId = (string)val.aRPId;
                instance.ActionRelatedWarehouseId = (string)val.aRWId;
                instance.ActionPrice = (decimal?)val.aPrice ?? 0m;
                instance.ActionIncome = (decimal?)val.aIncome ?? 0m;
                instance.ActionExpense = (decimal?)val.aExpense ?? 0m;
                instance.ActionDiscount = (decimal?)val.aDiscount ?? 0m;
                instance.ActionOverhead = (decimal?)val.aOverhead ?? 0m;

                return instance;
            },
            projectorOwn: x =>
            {
                T instance = Activator.CreateInstance<T>();
                dynamic key = x.Key;
                dynamic val = x.Value;

                instance.TransactionId = (string)val.tId;
                instance.TransactionCode = (string)val.tCode;
                instance.TransactionDate = Convert.ToDateTime(key[dateIndexOwn]);
                instance.TransactionType = (string)key[0];
                instance.TransactionUserId = (string)key[1];
                instance.TransactionUserName = (string)val.tUserName;
                instance.TransactionIsCash = (bool?)val.tIsCash ?? false;
                instance.TransactionIsCompleted = (bool?)val.tIsCompleted ?? false;
                instance.TransactionIsDisabled = (bool?)val.tIsDisabled ?? false;
                instance.TransactionGroup = (string)val.tGroup;

                if (val.tTags != null)
                {
                    instance.TransactionTags = val.tTags.ToObject<List<string>>();
                }

                instance.ActionId = (string)val.aId;
                instance.ActionSourceId = (string)val.aSourceId;
                instance.ActionWarehouseId = (string)key[2];
                instance.ActionStockId = (string)key[stockIndexOwn];
                instance.ActionRelatedPartnerId = (string)val.aRPId;
                instance.ActionRelatedWarehouseId = (string)val.aRWId;
                instance.ActionPrice = (decimal?)val.aPrice ?? 0m;
                instance.ActionIncome = (decimal?)val.aIncome ?? 0m;
                instance.ActionExpense = (decimal?)val.aExpense ?? 0m;
                instance.ActionDiscount = (decimal?)val.aDiscount ?? 0m;
                instance.ActionOverhead = (decimal?)val.aOverhead ?? 0m;

                return instance;
            });
    }

    private async Task<IEnumerable<T>> GetRecordsAsync<T>(DateTime? startDate, DateTime? endDate, string[] warehouseIds, string stockId, bool reduce = false, Func<ViewRow<object>, T> projectorAll = null, Func<ViewRow<object>, T> projectorOwn = null)
    {
        _authorizer.Authorize();
        if (!warehouseIds.Any()) throw new ArgumentNullException(nameof(warehouseIds));

        var types = Enum.GetValues(typeof(InvoiceType)).Cast<Enum>().Union(Enum.GetValues(typeof(StockSlipType)).Cast<Enum>()).ToArray();
        List<string> ownActions;
        List<string> allActions;

        if (_loginService.Session.IsAdmin)
        {
            allActions = types.Select(x => x.ToString()).ToList();
            allActions.Add("StockTransferSource");
            allActions.Add("StockTransferDestination");
            ownActions = new List<string>();
        }
        else
        {
            var readableAccountIds = _authService.GetAccessableAccounts(AccountAccessLevel.Read);
            warehouseIds = warehouseIds.Where(x => readableAccountIds.Contains(x)).ToArray();
            allActions = _authService.FilterAvailableActions(TransactionAccessLevel.ReadAll, types).ToList();
            ownActions = _authService.FilterAvailableActions(TransactionAccessLevel.ReadOwn, types).Where(x => !allActions.Contains(x)).ToList();

            if (_authService.TryAuthorizeAction(TransactionActions.StockTransfers, TransactionAccessLevel.ReadAll))
            {
                allActions.Add("StockTransferSource");
                allActions.Add("StockTransferDestination");
            }
            else if (_authService.TryAuthorizeAction(TransactionActions.StockTransfers, TransactionAccessLevel.ReadOwn))
            {
                ownActions.Add("StockTransferSource");
                ownActions.Add("StockTransferDestination");
            }
        }

        string userId = _loginService.Session.UserId;
        var list = new List<T>();

        if (stockId != null)
        {
            var array2 = allActions.SelectMany(actionType => warehouseIds.Select(accountId => new Tuple<object, object>(
                new[] { actionType, accountId, stockId, startDate?.ToString("yyyy-MM-dd") ?? "0" },
                new[] { actionType, accountId, stockId, endDate?.ToString("yyyy-MM-dd") ?? "zzz" }
            ))).ToArray();

            if (array2.Any())
            {
                // ВИПРАВЛЕНО: Додано явне іменування аргументу projector (усунено CS1503)
                var records = projectorAll == null
                    ? await GetRecordsAsync<T>("stock-management", "stock-actions-by-warehouse-and-id-all", array2, reduce)
                    : await GetRecordsAsync<object, T>("stock-management", "stock-actions-by-warehouse-and-id-all", array2, reduce, projector: projectorAll);
                list.AddRange(records);
            }

            var array3 = ownActions.SelectMany(actionType => warehouseIds.Select(accountId => new Tuple<object, object>(
                new[] { actionType, userId, accountId, stockId, startDate?.ToString("yyyy-MM-dd") ?? "0" },
                new[] { actionType, userId, accountId, stockId, endDate?.ToString("yyyy-MM-dd") ?? "zzz" }
            ))).ToArray();

            if (array3.Any())
            {
                // ВИПРАВЛЕНО: Додано явне іменування аргументу projector (усунено CS1503)
                var records = projectorOwn == null
                    ? await GetRecordsAsync<T>("stock-management", "stock-actions-by-warehouse-and-id", array3, reduce)
                    : await GetRecordsAsync<object, T>("stock-management", "stock-actions-by-warehouse-and-id", array3, reduce, projector: projectorOwn);
                list.AddRange(records);
            }
        }
        else
        {
            var array4 = allActions.SelectMany(actionType => warehouseIds.Select(accountId => new Tuple<object, object>(
                new[] { actionType, accountId, startDate?.ToString("yyyy-MM-dd") ?? "0" },
                new[] { actionType, accountId, endDate?.ToString("yyyy-MM-dd") ?? "zzz" }
            ))).ToArray();

            if (array4.Any())
            {
                // ВИПРАВЛЕНО: Додано явне іменування аргументу projector (усунено CS1503)
                var records = projectorAll == null
                    ? await GetRecordsAsync<T>("stock-management", "stock-actions-by-warehouse-all", array4, reduce)
                    : await GetRecordsAsync<object, T>("stock-management", "stock-actions-by-warehouse-all", array4, reduce, projector: projectorAll);
                list.AddRange(records);
            }

            var array5 = ownActions.SelectMany(actionType => warehouseIds.Select(accountId => new Tuple<object, object>(
                new[] { actionType, userId, accountId, startDate?.ToString("yyyy-MM-dd") ?? "0" },
                new[] { actionType, userId, accountId, endDate?.ToString("yyyy-MM-dd") ?? "zzz" }
            ))).ToArray();

            if (array5.Any())
            {
                // ВИПРАВЛЕНО: Додано явне іменування аргументу projector (усунено CS1503)
                var records = projectorOwn == null
                    ? await GetRecordsAsync<T>("stock-management", "stock-actions-by-warehouse", array5, reduce)
                    : await GetRecordsAsync<object, T>("stock-management", "stock-actions-by-warehouse", array5, reduce, projector: projectorOwn);
                list.AddRange(records);
            }
        }

        return list;
    }

    public async Task<StockTracking> TrackByLineIdAsync(string lineId)
    {
        return (await TrackActionsAsync(lineId: lineId)).SingleOrDefault();
    }

    public Task<IEnumerable<StockTracking>> TrackByTransactionIdAsync(string transactionId)
    {
        return TrackActionsAsync(transactionId, incomeOnly: true);
    }

    protected async Task<IEnumerable<StockTracking>> TrackActionsAsync(string transactionId = null, string lineId = null, string lineSourceId = null, decimal? expensable = null, bool incomeOnly = false)
    {
        var tracking = new List<StockTracking>();
        var actions = new List<StockActionSimple>();
        Dictionary<string, Stock> stocks;

        using (IBucket bucket = Cluster.OpenDefaultBucket())
        {
            IViewQuery query;
            if (!string.IsNullOrEmpty(transactionId))
                query = new ViewQuery().From("stock-management-reporting", "stock-tracking-by-transactionId").Key(transactionId);
            else if (!string.IsNullOrEmpty(lineId))
                query = new ViewQuery().From("stock-management-reporting", "stock-tracking-by-lineId").Key(lineId);
            else if (!string.IsNullOrEmpty(lineSourceId))
                query = new ViewQuery().From("stock-management-reporting", "stock-tracking-by-lineSourceId").Key(lineSourceId);
            else throw new ArgumentNullException();

            var viewResult = await bucket.QueryAsync<StockActionSimple>(query);
            if (!viewResult.Success) throw viewResult.Exception ?? new Exception(viewResult.Message);

            actions.AddRange(!incomeOnly ? viewResult.Values : viewResult.Values.Where(x => x.Income > x.Expense));
            if (!actions.Any()) return tracking;

            var stockIds = actions.Select(x => x.StockId).Distinct().ToArray();
            stocks = (await bucket.GetDocumentsAsync<Stock>(stockIds)).Select(x => x.Content).ToDictionary(x => x.Id, x => x);
        }

        foreach (var action in actions)
        {
            var stock = stocks[action.StockId];
            decimal income = action.Income - action.Expense;
            decimal sellable = expensable ?? income;

            if (income > sellable) income = sellable;

            decimal prevBalance = (await _balancesRepository.GetAsync(action.StockId, action.Date.AddSeconds(-1.0), action.WarehouseId)).Sum(x => x.Balance);
            sellable += prevBalance;

            decimal directExpenses = await CountAndDetectReturns(tracking, sellable, action.WarehouseId, action.StockId, action.Date, x => x.ActionSourceId == action.Id);
            sellable -= directExpenses;

            decimal num1 = 0M;
            if (sellable > 0M)
                num1 = await CountAndDetectReturns(tracking, sellable, action.WarehouseId, action.StockId, action.Date.AddTicks(1L), x => x.ActionSourceId == null, prevBalance > 0M ? prevBalance : 0M);

            decimal num2 = directExpenses + num1 - prevBalance;
            if (num2 < 0M) num2 = 0M;
            else if (num2 > income) num2 = income;

            tracking.Add(new StockTracking
            {
                StockId = action.StockId,
                StockCode = stock.Code,
                StockName = stock.Name,
                WarehouseId = action.WarehouseId,
                Income = income,
                Expense = num2
            });
        }
        return tracking;
    }

    private async Task<decimal> CountAndDetectReturns(List<StockTracking> tracking, decimal expensable, string warehouseId, string stockId, DateTime date, Func<StockAction, bool> filter, decimal prevBalance = 0M)
    {
        decimal expenses = 0M;
        var stockActionArray = (await GetActionsAsync<StockAction>(date, null, stockId, warehouseId))
            .Where(x => x.ActionExpense > x.ActionIncome)
            .Where(filter)
            .ToArray();

        foreach (var expenseAction in stockActionArray)
        {
            decimal expenseActionQuantity = expenseAction.ActionExpense - expenseAction.ActionIncome;
            if (prevBalance > 0M)
            {
                if (expenseActionQuantity <= prevBalance)
                {
                    prevBalance -= expenseActionQuantity;
                    expensable -= expenseActionQuantity;
                    continue;
                }
                expenseActionQuantity -= prevBalance;
                expensable -= prevBalance;
                prevBalance = 0M;
            }

            foreach (var stockTracking in await TrackActionsAsync(lineSourceId: expenseAction.ActionId, expensable: expensable))
            {
                if (stockTracking.WarehouseId == expenseAction.ActionWarehouseId)
                    expenseActionQuantity -= stockTracking.Left;
                else
                    tracking.Add(stockTracking);
            }

            if (expenseActionQuantity > expensable)
                expenseActionQuantity = expensable;

            expenses += expenseActionQuantity;
            expensable -= expenseActionQuantity;

            if (expensable == 0M) break;
        }
        return expenses;
    }

    internal class StockActionSimple
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string WarehouseId { get; set; }
        public string StockId { get; set; }
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
    }
}