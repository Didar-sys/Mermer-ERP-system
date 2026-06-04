using Couchbase.Views;
using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Commerce.Models;
using Mermer.Core.Couch.Common;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Enterprise.Models;
using Mermer.FundsManagement.Models;
using Mermer.FundsManagement.Models.Extenders;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Models.Extenders;
using Mermer.StockManagement.Services;
using Mermer.Warehousing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mermer.Core.Couch.StockManagement.Services;

public class StockBalancesRepository : CouchView, IStockBalancesRepository
{
    private readonly ILoginService _loginService;
    private readonly IStocksRepository _stocksRepository;
    private readonly IRepository<Currency> _currenciesRepository;
    private readonly IRepository<Warehouse> _warehousesRepository;
    private readonly IAuthorizationService _authorizationService;
    private readonly IReadOnlyListAuthorizer<StockBalanceWithData> _authorizer;

    public StockBalancesRepository(
        ICouchCluster cluster,
        ILoginService loginService,
        IStocksRepository stocksRepository,
        IRepository<Currency> currenciesRepository,
        IRepository<Warehouse> warehousesRepository,
        IAuthorizationService authorizationService,
        IReadOnlyListAuthorizer<StockBalanceWithData> authorizer)
        : base(cluster)
    {
        _loginService = loginService;
        _stocksRepository = stocksRepository;
        _currenciesRepository = currenciesRepository;
        _warehousesRepository = warehousesRepository;
        _authorizationService = authorizationService;
        _authorizer = authorizer;
    }

    public async Task<IEnumerable<StockBalance>> GetAsync(string stockId, DateTime date, params string[] warehouses)
    {
        _authorizer.Authorize();

        if (_loginService.Session.IsAdmin)
        {
            if (warehouses == null || !warehouses.Any())
                warehouses = (await _warehousesRepository.GetAsync()).Select(x => x.Id).ToArray();
        }
        else
        {
            var accounts = _authorizationService.GetAccessableAccounts(AccountAccessLevel.Read).ToArray();
            warehouses = warehouses.Where(x => accounts.Contains(x)).ToArray();
        }

        if (!warehouses.Any())
            return Array.Empty<StockBalance>();

        if (!string.IsNullOrEmpty(stockId))
        {
            var array = warehouses.Select(accountId => new Tuple<object, object>(
                new[] { accountId, stockId, "0" },
                new[] { accountId, stockId, date.ToString("o") }
            )).ToArray();

            return await GetRecordsAsync<StockBalance>("stock-management", "stock-balances-by-warehouse-and-id", array, true, 2, x =>
            {
                var balance = x.Value;
                dynamic key = x.Key;
                balance.WarehouseId = (string)key[0];
                balance.StockId = (string)key[1];
                return balance;
            });
        }

        var array1 = warehouses.Select(accountId => new Tuple<object, object>(
            new[] { accountId, "0" },
            new[] { accountId, date.ToString("o") }
        )).ToArray();

        return (await GetRecordsAsync<StockBalance>("stock-management", "stock-balances-by-warehouse", array1, true, 3, x =>
        {
            var balance = x.Value;
            dynamic key = x.Key;
            balance.WarehouseId = (string)key[0];
            balance.StockId = (string)key[2];
            return balance;
        })).GroupBy(x => new { x.WarehouseId, x.StockId }).Select(g => new StockBalance
        {
            WarehouseId = g.Key.WarehouseId,
            StockId = g.Key.StockId,
            Income = g.Sum(x => x.Income),
            Expense = g.Sum(x => x.Expense)
        });
    }

    public async Task<IEnumerable<StockBalanceWithCodeAndName>> GetAsync(string warehouseId, string[] stockIds, string excludedTransactionId)
    {
        _authorizer.Authorize();

        if (!_loginService.Session.IsAdmin && !_authorizationService.GetAccessableAccounts(AccountAccessLevel.Read).Contains(warehouseId))
            return Array.Empty<StockBalanceWithCodeAndName>();

        var actionTypes = Enum.GetValues(typeof(InvoiceType)).Cast<Enum>()
            .Concat(Enum.GetValues(typeof(StockSlipType)).Cast<Enum>())
            .Select(x => x.ToString())
            .Concat(new[] { "StockTransferSource", "StockTransferDestination" })
            .ToArray();

        var array = actionTypes.SelectMany(actionType => stockIds.Select(stockId => new Tuple<object, object>(
            new[] { actionType, warehouseId, stockId, DateTime.MinValue.ToString("o") },
            new[] { actionType, warehouseId, stockId, DateTime.MaxValue.ToString("o") }
        ))).ToArray();

        var stockActions = await GetRecordsAsync<object, StockActionTempData>("stock-management", "stock-actions-by-warehouse-and-id-all", array, projector: x =>
        {
            var action = new StockActionTempData();
            dynamic key = x.Key;
            dynamic val = x.Value;

            action.StockId = (string)key[2];
            action.TransactionId = (string)val.tId;
            action.TransactionIsDisabled = (bool?)val.tIsDisabled ?? false;
            action.TransactionIsCompleted = (bool?)val.tIsCompleted ?? false;
            action.ActionIncome = (decimal?)val.aIncome ?? 0m;
            action.ActionExpense = (decimal?)val.aExpense ?? 0m;
            return action;
        });

        var stocks = await _stocksRepository.GetListAsync(stockIds);

        return stocks.GroupJoin(
            stockActions.Where(x => x.TransactionId != excludedTransactionId && x.TransactionIsCompleted && !x.TransactionIsDisabled),
            x => x.Id,
            x => x.StockId,
            (stock, g) => new StockBalanceWithCodeAndName
            {
                StockId = stock.Id,
                StockCode = stock.Code,
                StockName = stock.Name,
                WarehouseId = warehouseId,
                Income = g.Sum(x => x.ActionIncome),
                Expense = g.Sum(x => x.ActionExpense)
            }
        );
    }

    public Task<IEnumerable<StockBalance>> GetAsync(string warehouseId, string[] stockIds, DateTime? date = null)
    {
        if (string.IsNullOrEmpty(warehouseId))
            throw new ArgumentNullException(nameof(warehouseId));
        return GetAsync(new[] { warehouseId }, stockIds, date);
    }

    public async Task<IEnumerable<StockBalance>> GetAsync(string[] warehouseIds, string[] stockIds, DateTime? date = null)
    {
        _authorizer.Authorize();

        // ВИПРАВЛЕНО: М'яка перевірка на null замість throw new ArgumentNullException
        if (warehouseIds == null || !warehouseIds.Any() || stockIds == null || !stockIds.Any())
        {
            return Array.Empty<StockBalance>();
        }

        if (!_loginService.Session.IsAdmin)
        {
            var accounts = _authorizationService.GetAccessableAccounts(AccountAccessLevel.Read).ToArray();
            warehouseIds = !warehouseIds.Any() || warehouseIds.Any(string.IsNullOrEmpty)
                ? accounts
                : warehouseIds.Where(accounts.Contains).ToArray();

            if (!warehouseIds.Any()) return Array.Empty<StockBalance>();
        }

        var array = warehouseIds.SelectMany(accountId => stockIds.Select(stockId => new Tuple<object, object>(
            new[] { accountId, stockId, "0" },
            new[] { accountId, stockId, date?.ToString("o") ?? "zzz" }
        ))).ToArray();

        return await GetRecordsAsync<StockBalance>("stock-management", "stock-balances-by-warehouse-and-id", array, true, 2, x =>
        {
            var balance = x.Value;
            dynamic key = x.Key;
            balance.WarehouseId = (string)key[0];
            balance.StockId = (string)key[1];
            return balance;
        });
    }

    public Task<IEnumerable<StockBalance>> GetAsync(string warehouseId, (string stockId, DateTime? balanceDate)[] stockBalanceDates)
    {
        if (string.IsNullOrEmpty(warehouseId)) throw new ArgumentNullException(nameof(warehouseId));
        return GetAsync(new[] { warehouseId }, stockBalanceDates);
    }

    public async Task<IEnumerable<StockBalance>> GetAsync(string[] warehouseIds, (string stockId, DateTime? balanceDate)[] stockBalanceDates)
    {
        _authorizer.Authorize();

        // ВИПРАВЛЕНО: М'яка перевірка на null замість throw new ArgumentNullException
        if (warehouseIds == null || !warehouseIds.Any() || stockBalanceDates == null || !stockBalanceDates.Any())
        {
            return Array.Empty<StockBalance>();
        }

        if (!_loginService.Session.IsAdmin)
        {
            var accounts = _authorizationService.GetAccessableAccounts(AccountAccessLevel.Read).ToArray();
            warehouseIds = !warehouseIds.Any() || warehouseIds.Any(string.IsNullOrEmpty)
                ? accounts
                : warehouseIds.Where(accounts.Contains).ToArray();

            if (!warehouseIds.Any()) return Array.Empty<StockBalance>();
        }

        var array = warehouseIds.SelectMany(accountId => stockBalanceDates.Select(x => new Tuple<object, object>(
            new[] { accountId, x.stockId, "0" },
            new[] { accountId, x.stockId, x.balanceDate?.ToString("o") ?? "zzz" }
        ))).ToArray();

        return await GetRecordsAsync<StockBalance>("stock-management", "stock-balances-by-warehouse-and-id", array, true, 2, x =>
        {
            var balance = x.Value;
            dynamic key = x.Key;
            balance.WarehouseId = (string)key[0];
            balance.StockId = (string)key[1];
            return balance;
        });
    }

    public async Task<IEnumerable<StockBalanceByTypeWithBalanceAndData>> GetByTypeAsync(string[] warehouseIds, string stockId, DateTime dateFrom, DateTime dateTill, bool aggregate)
    {
        _authorizer.Authorize();

        if (dateFrom >= dateTill) throw new ArgumentException("From date should be lower than or equal to till date");

        if (!_loginService.Session.IsAdmin)
        {
            var accounts = _authorizationService.GetAccessableAccounts(AccountAccessLevel.Read).ToList();
            warehouseIds = warehouseIds.Where(accounts.Contains).ToArray();
        }

        if (warehouseIds == null || !warehouseIds.Any()) return Array.Empty<StockBalanceByTypeWithBalanceAndData>();

        List<StockBalance> startingBalances;
        List<StockBalanceByType> changingBalances;

        if (!string.IsNullOrEmpty(stockId))
        {
            var array1 = warehouseIds.Select(accountId => new Tuple<object, object>(
                new[] { accountId, stockId, "0" },
                new[] { accountId, stockId, dateFrom.ToString("yyyy-MM-dd") }
            )).ToArray();

            startingBalances = (await GetRecordsAsync<StockBalance>("stock-management", "stock-balances-by-warehouse-and-id", array1, true, 2, x =>
            {
                var balance = x.Value;
                dynamic key = x.Key;
                balance.WarehouseId = (string)key[0];
                balance.StockId = (string)key[1];
                return balance;
            })).ToList();

            var array2 = warehouseIds.Select(accountId => new Tuple<object, object>(
                new[] { accountId, stockId, dateFrom.ToString("yyyy-MM-dd") },
                new[] { accountId, stockId, dateTill.ToString("yyyy-MM-dd") }
            )).ToArray();

            changingBalances = (await GetRecordsAsync<StockBalanceByType>("stock-management", "stock-balances-by-warehouse-and-id", array2, true, 2, x =>
            {
                var balance = x.Value;
                dynamic key = x.Key;
                balance.WarehouseId = (string)key[0];
                balance.StockId = (string)key[1];
                return balance;
            })).ToList();
        }
        else
        {
            var array3 = warehouseIds.Select(accountId => new Tuple<object, object>(
                new[] { accountId, "0" },
                new[] { accountId, dateFrom.ToString("yyyy-MM-dd") }
            )).ToArray();

            startingBalances = (await GetRecordsAsync<StockBalance>("stock-management", "stock-balances-by-warehouse", array3, true, 3, x =>
            {
                var balance = x.Value;
                dynamic key = x.Key;
                balance.WarehouseId = (string)key[0];
                balance.StockId = (string)key[2];
                return balance;
            })).ToList();

            var array4 = warehouseIds.Select(accountId => new Tuple<object, object>(
                new[] { accountId, dateFrom.ToString("yyyy-MM-dd") },
                new[] { accountId, dateTill.ToString("yyyy-MM-dd") }
            )).ToArray();

            changingBalances = (await GetRecordsAsync<StockBalanceByType>("stock-management", "stock-balances-by-warehouse", array4, true, 3, x =>
            {
                var balance = x.Value;
                dynamic key = x.Key;
                balance.WarehouseId = (string)key[0];
                balance.StockId = (string)key[2];
                return balance;
            })).ToList();
        }

        var stockIdsArray = startingBalances.Select(x => x.StockId).Union(changingBalances.Select(x => x.StockId)).Distinct().ToArray();

        if (!stockIdsArray.Any()) return Array.Empty<StockBalanceByTypeWithBalanceAndData>();

        var stocks = await _stocksRepository.GetInfoAsync(stockIdsArray);

        if (!aggregate)
        {
            return startingBalances.Select(x => new { x.WarehouseId, x.StockId })
                .Union(changingBalances.Select(x => new { x.WarehouseId, x.StockId }))
                .Distinct()
                .Select(x => new
                {
                    item = x,
                    stock = stocks.Single(y => y.Id == x.StockId),
                    startingBalances = startingBalances.Where(z => z.WarehouseId == x.WarehouseId && z.StockId == x.StockId),
                    changingBalances = changingBalances.Where(z => z.WarehouseId == x.WarehouseId && z.StockId == x.StockId)
                })
                .Select(x => new StockBalanceByTypeWithBalanceAndData
                {
                    WarehouseId = x.item.WarehouseId,
                    StockId = x.item.StockId,
                    StockCode = x.stock.Code,
                    StockName = x.stock.Name,
                    StockShortName = x.stock.ShortName,
                    StockUnit = x.stock.Unit,
                    StockPrice = x.stock.Price,
                    StockCurrencyId = x.stock.CurrencyId,
                    StockType = x.stock.Type,
                    StockGroup = x.stock.Group,
                    StockTags = x.stock.Tags,
                    StartingBalance = x.startingBalances.Sum(z => z.Balance),
                    Income = x.changingBalances.Sum(z => z.Income),
                    Expense = x.changingBalances.Sum(z => z.Expense),
                    StockOpening = x.changingBalances.Sum(z => z.StockOpening),
                    StockSpoilage = x.changingBalances.Sum(z => z.StockSpoilage),
                    StockUsage = x.changingBalances.Sum(z => z.StockUsage),
                    RevisionExceed = x.changingBalances.Sum(z => z.RevisionExceed),
                    RevisionDeficit = x.changingBalances.Sum(z => z.RevisionDeficit),
                    StockTransferSource = x.changingBalances.Sum(z => z.StockTransferSource),
                    StockTransferDestination = x.changingBalances.Sum(z => z.StockTransferDestination),
                    Sales = x.changingBalances.Sum(z => z.Sales),
                    SalesReturn = x.changingBalances.Sum(z => z.SalesReturn),
                    Purchase = x.changingBalances.Sum(z => z.Purchase),
                    PurchaseReturn = x.changingBalances.Sum(z => z.PurchaseReturn)
                });
        }
        else
        {
            return stocks.Select(x => new
            {
                stock = x,
                startingBalances = startingBalances.Where(z => z.StockId == x.Id),
                changingBalances = changingBalances.Where(z => z.StockId == x.Id)
            }).Select(x => new StockBalanceByTypeWithBalanceAndData
            {
                WarehouseId = null,
                StockId = x.stock.Id,
                StockCode = x.stock.Code,
                StockName = x.stock.Name,
                StockShortName = x.stock.ShortName,
                StockUnit = x.stock.Unit,
                StockPrice = x.stock.Price,
                StockCurrencyId = x.stock.CurrencyId,
                StockType = x.stock.Type,
                StockGroup = x.stock.Group,
                StockTags = x.stock.Tags,
                StartingBalance = x.startingBalances.Sum(z => z.Balance),
                Income = x.changingBalances.Sum(z => z.Income),
                Expense = x.changingBalances.Sum(z => z.Expense),
                StockOpening = x.changingBalances.Sum(z => z.StockOpening),
                StockSpoilage = x.changingBalances.Sum(z => z.StockSpoilage),
                StockUsage = x.changingBalances.Sum(z => z.StockUsage),
                RevisionExceed = x.changingBalances.Sum(z => z.RevisionExceed),
                RevisionDeficit = x.changingBalances.Sum(z => z.RevisionDeficit),
                StockTransferSource = x.changingBalances.Sum(z => z.StockTransferSource),
                StockTransferDestination = x.changingBalances.Sum(z => z.StockTransferDestination),
                Sales = x.changingBalances.Sum(z => z.Sales),
                SalesReturn = x.changingBalances.Sum(z => z.SalesReturn),
                Purchase = x.changingBalances.Sum(z => z.Purchase),
                PurchaseReturn = x.changingBalances.Sum(z => z.PurchaseReturn)
            });
        }
    }

    public async Task<IEnumerable<StockBalanceByWarehouses>> GetByDateAndWarehousesAsync(DateTime date, IEnumerable<string> warehouseIds, string displayCurrencyId, IEnumerable<string> stockIds = null)
    {
        _authorizer.Authorize();

        var source1 = warehouseIds.ToArray();

        if (!_loginService.Session.IsAdmin)
        {
            var accounts = _authorizationService.GetAccessableAccounts(AccountAccessLevel.Read).ToArray();
            source1 = source1.Where(accounts.Contains).ToArray();
        }

        if (!source1.Any()) return Array.Empty<StockBalanceByWarehouses>();

        var stockIdsArray = stockIds?.ToArray();
        IEnumerable<StockBalance> recordsAsync;

        if (stockIdsArray != null)
        {
            if (!stockIdsArray.Any()) return Array.Empty<StockBalanceByWarehouses>();

            var array = source1.SelectMany(accountId => stockIdsArray.Select(stockId => new Tuple<object, object>(
                new[] { accountId, stockId, "0" },
                new[] { accountId, stockId, date.ToString("o") }
            ))).ToArray();

            recordsAsync = await GetRecordsAsync<StockBalance>("stock-management", "stock-balances-by-warehouse-and-id", array, true, 2, x =>
            {
                var balance = x.Value;
                dynamic key = x.Key;
                balance.WarehouseId = (string)key[0];
                balance.StockId = (string)key[1];
                return balance;
            });
        }
        else
        {
            var array = source1.Select(accountId => new Tuple<object, object>(
                new[] { accountId, "0" },
                new[] { accountId, date.ToString("o") }
            )).ToArray();

            recordsAsync = await GetRecordsAsync<StockBalance>("stock-management", "stock-balances-by-warehouse", array, true, 3, x =>
            {
                var balance = x.Value;
                dynamic key = x.Key;
                balance.WarehouseId = (string)key[0];
                balance.StockId = (string)key[2];
                return balance;
            });
        }

        var balancesByStockAndWarehouse = recordsAsync.GroupBy(x => new { x.StockId, x.WarehouseId })
            .Select(g => new
            {
                StockId = g.Key.StockId,
                WarehouseId = g.Key.WarehouseId,
                Balance = g.Sum(x => x.Balance)
            }).ToArray();

        Stock[] stocks;
        if (stockIdsArray != null)
            stocks = (await _stocksRepository.GetAsync(stockIdsArray)).ToArray();
        else
            stocks = (await _stocksRepository.GetAsync()).ToArray();

        var currencies = (await _currenciesRepository.GetAsync()).ToArray();
        var currency = currencies.Single(x => x.Id == displayCurrencyId);
        var displayCurrencyRate = currency.GetRate(date);
        int displayCurrencyDecimals = currency.Decimals;

        return stocks.Select(x => new
        {
            stock = x,
            price = x.GetPrice(date)
        }).Join(currencies, x => x.price.CurrencyId, c => c.Id, (x, c) => new
        {
            stock = x.stock,
            price = x.price,
            currencyRate = c.GetRate(date)
        }).GroupJoin(balancesByStockAndWarehouse, x => x.stock.Id, x => x.StockId, (x, b) => new StockBalanceByWarehouses
        {
            StockId = x.stock.Id,
            StockCode = x.stock.Code,
            StockName = x.stock.Name,
            StockUnit = x.stock.Unit,
            StockPrice = Math.Round(x.price.Price * x.currencyRate.Multiplier / x.currencyRate.Divider / displayCurrencyRate.Multiplier * displayCurrencyRate.Divider, displayCurrencyDecimals),
            StockPriceCurrencyId = displayCurrencyId,
            StockGroup = x.stock.Group,
            StockType = x.stock.Type,
            StockTags = x.stock.Tags == null ? "" : string.Join(" ", x.stock.Tags),
            Balances = b.ToDictionary(y => y.WarehouseId, y => y.Balance)
        });
    }

    private class StockActionTempData
    {
        public string StockId { get; set; }
        public string TransactionId { get; set; }
        public bool TransactionIsDisabled { get; set; }
        public bool TransactionIsCompleted { get; set; }
        public decimal ActionIncome { get; set; }
        public decimal ActionExpense { get; set; }
    }
}