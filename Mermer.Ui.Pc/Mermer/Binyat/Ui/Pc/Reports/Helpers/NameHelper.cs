// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Reports.Helpers.NameHelper
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

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
    return (await this.Get<Mermer.FundsManagement.Models.Currency>(currencyId, this._currencies, this._currenciesRepository)).Name;
  }

  public async Task<string> GetStockName(string stockId)
  {
    return (await this.Get<Stock>(stockId, this._stocks, this._stocksRepository)).Fullname;
  }

  public async Task<string> GetExpenseName(string expenseId)
  {
    return (await this.Get<Expense>(expenseId, this._expenses, this._expensesRepository)).Name;
  }

  public async Task<string> GetPartnerName(string partnerId)
  {
    return (await this.Get<Partner>(partnerId, this._partners, this._partnersRepository)).Fullname;
  }

  public async Task<string> GetDepositoryName(string depositoryId)
  {
    return (await this.Get<Depository>(depositoryId, this._depositories, this._depositoriesRepository)).Name;
  }

  public async Task<string> GetWarehouseName(string warehouseId)
  {
    return (await this.Get<Warehouse>(warehouseId, this._warehouses, this._warehousesRepository)).Name;
  }

  public async Task<string> GetStockUnitName(string stockId, string unitId)
  {
    return (await this.Get<Stock>(stockId, this._stocks, this._stocksRepository)).Units.SingleOrDefault<StockUnit>((Func<StockUnit, bool>) (x => x.Id == unitId))?.Name;
  }

  private async Task<T> Get<T>(
    string id,
    Dictionary<string, T> items,
    IRepository<T> itemsRepository)
    where T : IModel
  {
    if (string.IsNullOrEmpty(id))
      throw new ArgumentNullException(nameof (id));
    if (!items.ContainsKey(id))
      items.Add(id, await ((IReadOnlyRepository<T>) itemsRepository).GetAsync(id) ?? throw new Exception($"{typeof (T).Name} with id {id} not found"));
    return items[id];
  }
}
