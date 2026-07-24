using Mermer.CRM.Models;
using Mermer.Enterprise.Models;
using Mermer.Finance.Spending.Models;
using Mermer.StockManagement.Models;
using Mermer.Data.Models;
using Mermer.Data.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Pc.Reports.Helpers;

public class NameHelper
{
    private readonly Dictionary<string, Stock> _stocks;
    private readonly Dictionary<string, Expense> _expenses;
    private readonly Dictionary<string, Partner> _partners;
    private readonly Dictionary<string, Mermer.FundsManagement.Models.Currency> _currencies;
    private readonly Dictionary<string, Warehouse> _warehouses;
    private readonly Dictionary<string, Depository> _depositories;
    private readonly IRepository<Stock> _stocksRepository;
    private readonly IRepository<Expense> _expensesRepository;
    private readonly IRepository<Partner> _partnersRepository;
    private readonly IRepository<Mermer.FundsManagement.Models.Currency> _currenciesRepository;
    private readonly IRepository<Warehouse> _warehousesRepository;
    private readonly IRepository<Depository> _depositoriesRepository;

    public NameHelper(
      IRepository<Stock> stocksRepository,
      IRepository<Expense> expensesRepository,
      IRepository<Partner> partnersRepository,
      IRepository<Mermer.FundsManagement.Models.Currency> currenciesRepository,
      IRepository<Warehouse> warehousesRepository,
      IRepository<Depository> depositoriesRepository)
    {
        this._stocks = new Dictionary<string, Stock>();
        this._expenses = new Dictionary<string, Expense>();
        this._partners = new Dictionary<string, Partner>();
        this._currencies = new Dictionary<string, Mermer.FundsManagement.Models.Currency>();
        this._warehouses = new Dictionary<string, Warehouse>();
        this._depositories = new Dictionary<string, Depository>();
        this._stocksRepository = stocksRepository;
        this._expensesRepository = expensesRepository;
        this._partnersRepository = partnersRepository;
        this._currenciesRepository = currenciesRepository;
        this._warehousesRepository = warehousesRepository;
        this._depositoriesRepository = depositoriesRepository;
    }

    public async Task<string> GetCurrencyName(string currencyId)
    {
        var item = await this.Get<Mermer.FundsManagement.Models.Currency>(currencyId, this._currencies, this._currenciesRepository);
        return item?.Name ?? string.Empty;
    }

    public async Task<string> GetStockName(string stockId)
    {
        var item = await this.Get<Stock>(stockId, this._stocks, this._stocksRepository);
        return item?.Fullname ?? string.Empty;
    }

    public async Task<string> GetExpenseName(string expenseId)
    {
        var item = await this.Get<Expense>(expenseId, this._expenses, this._expensesRepository);
        return item?.Name ?? string.Empty;
    }

    public async Task<string> GetPartnerName(string partnerId)
    {
        var item = await this.Get<Partner>(partnerId, this._partners, this._partnersRepository);
        return item?.Fullname ?? string.Empty;
    }

    public async Task<string> GetDepositoryName(string depositoryId)
    {
        var item = await this.Get<Depository>(depositoryId, this._depositories, this._depositoriesRepository);
        return item?.Name ?? string.Empty;
    }

    public async Task<string> GetWarehouseName(string warehouseId)
    {
        var item = await this.Get<Warehouse>(warehouseId, this._warehouses, this._warehousesRepository);
        return item?.Name ?? string.Empty;
    }

    public async Task<string> GetStockUnitName(string stockId, string unitId)
    {
        if (string.IsNullOrEmpty(stockId) || string.IsNullOrEmpty(unitId)) return string.Empty;
        var stock = await this.Get<Stock>(stockId, this._stocks, this._stocksRepository);
        return stock?.Units?.SingleOrDefault(x => x.Id == unitId)?.Name ?? string.Empty;
    }

    private async Task<T> Get<T>(
      string id,
      Dictionary<string, T> items,
      IRepository<T> itemsRepository)
      where T : IModel
    {
        if (string.IsNullOrEmpty(id))
            return default(T);

        if (!items.ContainsKey(id))
        {
            var entity = await ((IReadOnlyRepository<T>)itemsRepository).GetAsync(id);
            if (entity == null) return default(T);
            items.Add(id, entity);
        }
        return items[id];
    }
}