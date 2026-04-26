// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Settings.MockObjectsViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using LoremNET;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Commerce.Models;
using Payhas.Binyat.CRM.Models;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.Finance.Models;
using Payhas.Binyat.Finance.Spending.Models;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.FundsManagement.Models.Extenders;
using Payhas.Binyat.StockManagement.Models;
using Payhas.Binyat.Transactions.Models;
using Payhas.Binyat.Warehousing.Models;
using Payhas.Data;
using Payhas.Data.Storage;
using Payhas.Data.Tools.Barcode;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Settings;

public class MockObjectsViewModel : DialogViewModel
{
  private readonly ILoginService _loginService;
  private readonly IRepository<Stock> _stocksRepository;
  private readonly IRepository<Expense> _expensesRepository;
  private readonly IRepository<Currency> _currenciesRepository;
  private readonly IRepository<Partner> _partnersRepository;
  private readonly IRepository<Bill> _billsRepository;
  private readonly IRepository<Invoice> _invoicesRepository;
  private readonly IRepository<StockSlip> _stockSlipsRepository;
  private readonly IRepository<StockTransfer> _stockTransfersRepository;
  private readonly IRepository<FundsSlip> _fundsSlipsRepository;
  private readonly IRepository<FundsTransfer> _fundsTransfersRepository;
  private readonly IRepository<ExpenseSlip> _expenseSlipsRepository;
  private readonly IRepository<PartnerSlip> _partnerSlipsRepository;
  private readonly IRepository<PartnerTransfer> _partnerTransfersRepository;
  private readonly IRepository<Office> _officesRepository;
  private readonly IRepository<Warehouse> _warehousesRepository;
  private readonly IRepository<Depository> _depositoriesRepository;
  private string _status;
  private int _createBillsCount;
  private int _createInvoicesCount;
  private int _createStocksCount;
  private int _createStockSlipsCount;
  private int _createStockTransfersCount;
  private int _createFundsSlipsCount;
  private int _createFundsTransfersCount;
  private int _createExpensesCount;
  private int _createExpenseSlipsCount;
  private int _createPartnersCount;
  private int _createPartnerSlipsCount;
  private int _createPartnerTransfersCount;

  public MockObjectsViewModel(
    ILoginService loginService,
    IRepository<Stock> stocksRepository,
    IRepository<Expense> expensesRepository,
    IRepository<Currency> currenciesRepository,
    IRepository<Partner> partnersRepository,
    IRepository<Bill> billsRepository,
    IRepository<Invoice> invoicesRepository,
    IRepository<StockSlip> stockSlipsRepository,
    IRepository<StockTransfer> stockTransfersRepository,
    IRepository<FundsSlip> fundsSlipsRepository,
    IRepository<FundsTransfer> fundsTransfersRepository,
    IRepository<ExpenseSlip> expenseSlipsRepository,
    IRepository<PartnerSlip> partnerSlipsRepository,
    IRepository<PartnerTransfer> partnerTransfersRepository,
    IRepository<Office> officesRepository,
    IRepository<Warehouse> warehousesRepository,
    IRepository<Depository> depositoriesRepository,
    IMvxMessenger messenger,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._loginService = loginService;
    this._stocksRepository = stocksRepository;
    this._expensesRepository = expensesRepository;
    this._currenciesRepository = currenciesRepository;
    this._partnersRepository = partnersRepository;
    this._billsRepository = billsRepository;
    this._invoicesRepository = invoicesRepository;
    this._stockSlipsRepository = stockSlipsRepository;
    this._stockTransfersRepository = stockTransfersRepository;
    this._fundsSlipsRepository = fundsSlipsRepository;
    this._fundsTransfersRepository = fundsTransfersRepository;
    this._expenseSlipsRepository = expenseSlipsRepository;
    this._partnerSlipsRepository = partnerSlipsRepository;
    this._partnerTransfersRepository = partnerTransfersRepository;
    this._officesRepository = officesRepository;
    this._warehousesRepository = warehousesRepository;
    this._depositoriesRepository = depositoriesRepository;
  }

  public new string Status
  {
    get => this._status;
    set => this.SetProperty<string>(ref this._status, value, nameof (Status));
  }

  public int CreateBillsCount
  {
    get => this._createBillsCount;
    set => this.SetProperty<int>(ref this._createBillsCount, value, nameof (CreateBillsCount));
  }

  public ICommand CreateBills
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnCreateBillsAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  protected virtual async Task OnCreateBillsAsync()
  {
    MockObjectsViewModel objectsViewModel = this;
    if (objectsViewModel.CreateBillsCount < 1)
      return;
    objectsViewModel.IsBusy = true;
    objectsViewModel.Status = $"Starting {objectsViewModel.CreateBillsCount} Bill(s) creation...";
    try
    {
      string[] depositories = (await objectsViewModel._depositoriesRepository.GetAsync()).Select<Depository, string>((Func<Depository, string>) (x => x.Id)).ToArray<string>();
      string[] partners = (await objectsViewModel._partnersRepository.GetAsync()).Select<Partner, string>((Func<Partner, string>) (x => x.Id)).ToArray<string>();
      Currency[] currenciesList = (await objectsViewModel._currenciesRepository.GetAsync()).ToArray<Currency>();
      string[] groups = Lorem.Words(10).Split(' ');
      for (int i = 1; i <= objectsViewModel.CreateBillsCount; ++i)
      {
        string barcode = $"99{i:D10}";
        string str = barcode + Symbology.CalculateChecksumDigit(barcode);
        Bill bill = new Bill();
        bill.Id = Guid.NewGuid().ToString();
        bill.Code = str;
        bill.Date = Lorem.DateTime(DateTime.Today.AddDays((double) -DateTime.Today.DayOfYear), DateTime.Now);
        bill.DepositoryId = Lorem.Random<string>(depositories);
        bill.PartnerId = Lorem.Random<string>(partners);
        bill.UserId = objectsViewModel._loginService.Session.UserId;
        bill.UserName = objectsViewModel._loginService.Session.Username;
        bill.IsCompleted = Lorem.Chance(7, 10);
        bill.IsDisabled = Lorem.Chance(2, 10);
        bill.BillType = Lorem.Enum<BillType>();
        bill.Group = Lorem.Random<string>(groups);
        bill.Description = Lorem.Sentence(2, 20);
        bill.Lines = new WatchedObservableCollection<BillLine>();
        bill.CurrencyConvertions = new WatchedObservableCollection<CurrencyConvertion>();
        Bill model = bill;
        for (int index = 0; index < Lorem.Integer(1, 2); ++index)
        {
          Currency currency = Lorem.Random<Currency>(currenciesList);
          BillLine billLine = new BillLine();
          billLine.Amount = (Decimal) Lorem.Integer(1, 100);
          billLine.CurrencyId = currency.Id;
          BillLine line = billLine;
          if (model.CurrencyConvertions.All<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (x => x.CurrencyId != line.CurrencyId)))
          {
            CurrencyRate rate = currency.GetRate(new DateTime?(model.Date));
            model.CurrencyConvertions.Add(new CurrencyConvertion()
            {
              CurrencyId = currency.Id,
              Multiplier = rate.Multiplier,
              Divider = rate.Divider
            });
          }
          model.Lines.Add(line);
        }
        await objectsViewModel._billsRepository.CreateAsync(model);
        objectsViewModel.Status = $"Created Bill(s): {i}";
      }
      depositories = (string[]) null;
      partners = (string[]) null;
      currenciesList = (Currency[]) null;
      groups = (string[]) null;
    }
    catch (Exception ex)
    {
      objectsViewModel.UserInteractionService.ShowExceptionMessage(ex);
      objectsViewModel.Status = "Error occurred while creating Bill(s)";
    }
    objectsViewModel.IsBusy = false;
  }

  public int CreateInvoicesCount
  {
    get => this._createInvoicesCount;
    set
    {
      this.SetProperty<int>(ref this._createInvoicesCount, value, nameof (CreateInvoicesCount));
    }
  }

  public ICommand CreateInvoices
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnCreateInvoicesAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  protected virtual async Task OnCreateInvoicesAsync()
  {
    MockObjectsViewModel objectsViewModel = this;
    if (objectsViewModel.CreateInvoicesCount < 1)
      return;
    objectsViewModel.IsBusy = true;
    objectsViewModel.Status = $"Starting {objectsViewModel.CreateInvoicesCount} Invoice(s) creation...";
    try
    {
      string[] warehouses = (await objectsViewModel._warehousesRepository.GetAsync()).Select<Warehouse, string>((Func<Warehouse, string>) (x => x.Id)).ToArray<string>();
      string[] depositories = (await objectsViewModel._depositoriesRepository.GetAsync()).Select<Depository, string>((Func<Depository, string>) (x => x.Id)).ToArray<string>();
      string[] partners = (await objectsViewModel._partnersRepository.GetAsync()).Select<Partner, string>((Func<Partner, string>) (x => x.Id)).ToArray<string>();
      Currency[] currenciesList = (await objectsViewModel._currenciesRepository.GetAsync()).ToArray<Currency>();
      Stock[] stocks = (await objectsViewModel._stocksRepository.GetAsync()).ToArray<Stock>();
      string[] groups = Lorem.Words(10).Split(' ');
      for (int i = 1; i <= objectsViewModel.CreateInvoicesCount; ++i)
      {
        string barcode = $"99{i:D10}";
        string str = barcode + Symbology.CalculateChecksumDigit(barcode);
        Invoice invoice1 = new Invoice();
        Guid guid = Guid.NewGuid();
        invoice1.Id = guid.ToString();
        invoice1.Code = str;
        DateTime dateTime1 = DateTime.Today;
        invoice1.Date = Lorem.DateTime(dateTime1.AddDays((double) -DateTime.Today.DayOfYear), DateTime.Now);
        invoice1.UserId = objectsViewModel._loginService.Session.UserId;
        invoice1.UserName = objectsViewModel._loginService.Session.Username;
        invoice1.PartnerId = Lorem.Random<string>(partners);
        invoice1.DebitCreditLeftAmount = true;
        invoice1.WarehouseId = Lorem.Random<string>(warehouses);
        invoice1.DepositoryId = Lorem.Random<string>(depositories);
        invoice1.IsCompleted = Lorem.Chance(7, 10);
        invoice1.IsDisabled = Lorem.Chance(2, 10);
        invoice1.InvoiceType = Lorem.Enum<InvoiceType>();
        invoice1.Group = Lorem.Random<string>(groups);
        invoice1.Description = Lorem.Sentence(2, 20);
        invoice1.Lines = new WatchedObservableCollection<InvoiceLine>();
        invoice1.CurrencyConvertions = new WatchedObservableCollection<CurrencyConvertion>();
        invoice1.StockUnitConvertions = new WatchedObservableCollection<StockUnitConvertion>();
        Invoice model = invoice1;
        Invoice invoice2 = model;
        dateTime1 = model.Date;
        DateTime dateTime2 = dateTime1.AddDays(30.0);
        invoice2.DueDate = dateTime2;
        for (int index = 0; index < Lorem.Integer(1, 100); ++index)
        {
          Stock stock = Lorem.Random<Stock>(stocks);
          StockUnit stockUnit = Lorem.Random<StockUnit>(stock.Units.ToArray<StockUnit>());
          int num1 = 0;
          Decimal num2;
          do
          {
            ++num1;
            num2 = num1 <= 10 ? (Decimal) Lorem.Integer(Convert.ToInt32(stock.Price * 0.8M), Convert.ToInt32(stock.Price * 1.5M)) : stock.Price;
          }
          while (num2 <= 0M);
          InvoiceLine invoiceLine = new InvoiceLine();
          guid = Guid.NewGuid();
          invoiceLine.Id = guid.ToString();
          invoiceLine.StockId = stock.Id;
          invoiceLine.Quantity = (Decimal) Lorem.Integer(1, 100);
          invoiceLine.UnitId = stockUnit.Id;
          invoiceLine.Price = num2;
          invoiceLine.CurrencyId = stock.CurrencyId;
          InvoiceLine line = invoiceLine;
          if (model.CurrencyConvertions.All<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (x => x.CurrencyId != line.CurrencyId)))
          {
            Currency currency = ((IEnumerable<Currency>) currenciesList).Single<Currency>((Func<Currency, bool>) (x => x.Id == line.CurrencyId));
            CurrencyRate rate = currency.GetRate(new DateTime?(model.Date));
            model.CurrencyConvertions.Add(new CurrencyConvertion()
            {
              CurrencyId = currency.Id,
              Multiplier = rate.Multiplier,
              Divider = rate.Divider
            });
          }
          if (model.StockUnitConvertions.All<StockUnitConvertion>((Func<StockUnitConvertion, bool>) (x => x.StockId != line.StockId || x.UnitId != line.UnitId)))
            model.StockUnitConvertions.Add(new StockUnitConvertion()
            {
              StockId = line.StockId,
              UnitId = stockUnit.Id,
              Multiplier = stockUnit.Multiplier,
              Divider = stockUnit.Divider
            });
          model.Lines.Add(line);
        }
        if (Lorem.Chance(1, 10))
        {
          Invoice invoice3 = model;
          WatchedObservableCollection<InvoiceDiscount> observableCollection = new WatchedObservableCollection<InvoiceDiscount>();
          observableCollection.Add(new InvoiceDiscount()
          {
            Type = InvoiceDiscountType.Percentage,
            Amount = (Decimal) Lorem.Integer(1, 10)
          });
          invoice3.Discounts = observableCollection;
        }
        CurrencyConvertion[] array = model.CurrencyConvertions.ToArray<CurrencyConvertion>();
        if (!string.IsNullOrEmpty(model.PartnerId) && Lorem.Chance(3, 10))
        {
          model.DebitCreditLeftAmount = true;
          CurrencyConvertion currencyConvertion = Lorem.Random<CurrencyConvertion>(array);
          Invoice invoice4 = model;
          WatchedObservableCollection<InvoicePayment> observableCollection = new WatchedObservableCollection<InvoicePayment>();
          observableCollection.Add(new InvoicePayment()
          {
            Amount = (Decimal) Lorem.Integer(1, Convert.ToInt32((model.ActionGrandTotal + model.ActionChangesTotal) / currencyConvertion.Multiplier * currencyConvertion.Divider)),
            CurrencyId = currencyConvertion.CurrencyId
          });
          invoice4.Payments = observableCollection;
        }
        else
        {
          CurrencyConvertion currencyConvertion = Lorem.Random<CurrencyConvertion>(array);
          int num = 0;
          if (Lorem.Chance(1, 2))
          {
            Invoice invoice5 = model;
            WatchedObservableCollection<InvoicePayment> observableCollection = new WatchedObservableCollection<InvoicePayment>();
            observableCollection.Add(new InvoicePayment()
            {
              Amount = (Decimal) (num = Lorem.Integer(0, 100)),
              CurrencyId = currencyConvertion.CurrencyId
            });
            invoice5.Changes = observableCollection;
          }
          Invoice invoice6 = model;
          WatchedObservableCollection<InvoicePayment> observableCollection1 = new WatchedObservableCollection<InvoicePayment>();
          observableCollection1.Add(new InvoicePayment()
          {
            Amount = (Decimal) num + model.ActionGrandTotal / currencyConvertion.Multiplier * currencyConvertion.Divider,
            CurrencyId = currencyConvertion.CurrencyId
          });
          invoice6.Payments = observableCollection1;
        }
        await objectsViewModel._invoicesRepository.CreateAsync(model);
        objectsViewModel.Status = $"Created Invoice(s): {i}";
      }
      warehouses = (string[]) null;
      depositories = (string[]) null;
      partners = (string[]) null;
      currenciesList = (Currency[]) null;
      stocks = (Stock[]) null;
      groups = (string[]) null;
    }
    catch (Exception ex)
    {
      objectsViewModel.UserInteractionService.ShowExceptionMessage(ex);
      objectsViewModel.Status = "Error occurred while creating Invoice(s)";
    }
    objectsViewModel.IsBusy = false;
  }

  public int CreateStocksCount
  {
    get => this._createStocksCount;
    set => this.SetProperty<int>(ref this._createStocksCount, value, nameof (CreateStocksCount));
  }

  public ICommand CreateStocks
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnCreateStocksAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  protected virtual async Task OnCreateStocksAsync()
  {
    MockObjectsViewModel objectsViewModel = this;
    if (objectsViewModel.CreateStocksCount < 1)
      return;
    objectsViewModel.IsBusy = true;
    objectsViewModel.Status = $"Starting {objectsViewModel.CreateStocksCount} Stock(s) creation...";
    try
    {
      string[] currencies = (await objectsViewModel._currenciesRepository.GetAsync()).Select<Currency, string>((Func<Currency, string>) (x => x.Id)).ToArray<string>();
      string[] types = Lorem.Words(10).Split(' ');
      string[] groups = Lorem.Words(10).Split(' ');
      for (int i = 1; i <= objectsViewModel.CreateStocksCount; ++i)
      {
        string barcode = $"99{i:D5}";
        string str = barcode + Symbology.CalculateChecksumDigit(barcode);
        Stock stock = new Stock();
        stock.Id = Guid.NewGuid().ToString();
        stock.Code = str;
        stock.Name = Lorem.Words(1, 5);
        ObservableCollection<StockUnit> observableCollection1 = new ObservableCollection<StockUnit>();
        observableCollection1.Add(new StockUnit()
        {
          Id = Guid.NewGuid().ToString(),
          Name = Lorem.Words(1),
          IsDefault = true,
          Multiplier = 1M,
          Divider = 1M
        });
        observableCollection1.Add(new StockUnit()
        {
          Id = Guid.NewGuid().ToString(),
          Name = Lorem.Words(1),
          Multiplier = (Decimal) Lorem.Integer(1, 10),
          Divider = (Decimal) Lorem.Integer(1, 10)
        });
        stock.Units = observableCollection1;
        WatchedObservableCollection<StockPrice> observableCollection2 = new WatchedObservableCollection<StockPrice>();
        observableCollection2.Add(new StockPrice()
        {
          ValidFrom = Lorem.DateTime(DateTime.Today.AddDays((double) -DateTime.Today.DayOfYear), DateTime.Now),
          CurrencyId = Lorem.Random<string>(currencies),
          Price = (Decimal) Lorem.Integer(10, 10000)
        });
        stock.Prices = observableCollection2;
        stock.Type = Lorem.Random<string>(types);
        stock.Group = Lorem.Random<string>(groups);
        stock.Description = Lorem.Sentence(2, 20);
        Stock model = stock;
        await objectsViewModel._stocksRepository.CreateAsync(model);
        objectsViewModel.Status = $"Created stocks: {i}";
      }
      currencies = (string[]) null;
      types = (string[]) null;
      groups = (string[]) null;
    }
    catch (Exception ex)
    {
      objectsViewModel.UserInteractionService.ShowExceptionMessage(ex);
      objectsViewModel.Status = "Error occurred while creating Stock(s)";
    }
    objectsViewModel.IsBusy = false;
  }

  public int CreateStockSlipsCount
  {
    get => this._createStockSlipsCount;
    set
    {
      this.SetProperty<int>(ref this._createStockSlipsCount, value, nameof (CreateStockSlipsCount));
    }
  }

  public ICommand CreateStockSlips
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnCreateStockSlipsAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  protected virtual async Task OnCreateStockSlipsAsync()
  {
    MockObjectsViewModel objectsViewModel = this;
    if (objectsViewModel.CreateStockSlipsCount < 1)
      return;
    objectsViewModel.IsBusy = true;
    objectsViewModel.Status = $"Starting {objectsViewModel.CreateStockSlipsCount} StockSlip(s) creation...";
    try
    {
      string[] warehouses = (await objectsViewModel._warehousesRepository.GetAsync()).Select<Warehouse, string>((Func<Warehouse, string>) (x => x.Id)).ToArray<string>();
      Currency[] currenciesList = (await objectsViewModel._currenciesRepository.GetAsync()).ToArray<Currency>();
      Stock[] stocks = (await objectsViewModel._stocksRepository.GetAsync()).ToArray<Stock>();
      string[] groups = Lorem.Words(10).Split(' ');
      for (int i = 1; i <= objectsViewModel.CreateStockSlipsCount; ++i)
      {
        string barcode = $"99{i:D10}";
        string str = barcode + Symbology.CalculateChecksumDigit(barcode);
        StockSlip stockSlip = new StockSlip();
        Guid guid = Guid.NewGuid();
        stockSlip.Id = guid.ToString();
        stockSlip.Code = str;
        stockSlip.Date = Lorem.DateTime(DateTime.Today.AddDays((double) -DateTime.Today.DayOfYear), DateTime.Now);
        stockSlip.WarehouseId = Lorem.Random<string>(warehouses);
        stockSlip.UserId = objectsViewModel._loginService.Session.UserId;
        stockSlip.UserName = objectsViewModel._loginService.Session.Username;
        stockSlip.IsCompleted = Lorem.Chance(7, 10);
        stockSlip.IsDisabled = Lorem.Chance(2, 10);
        stockSlip.SlipType = Lorem.Enum<StockSlipType>();
        stockSlip.Group = Lorem.Random<string>(groups);
        stockSlip.Description = Lorem.Sentence(2, 20);
        stockSlip.Lines = new WatchedObservableCollection<StockSlipLine>();
        stockSlip.Overheads = new WatchedObservableCollection<StockTransactionOverhead>();
        stockSlip.CurrencyConvertions = new WatchedObservableCollection<CurrencyConvertion>();
        stockSlip.StockUnitConvertions = new WatchedObservableCollection<StockUnitConvertion>();
        StockSlip model = stockSlip;
        for (int index = 0; index < Lorem.Integer(1, 100); ++index)
        {
          Stock stock = Lorem.Random<Stock>(stocks);
          StockUnit stockUnit = Lorem.Random<StockUnit>(stock.Units.ToArray<StockUnit>());
          StockSlipLine stockSlipLine = new StockSlipLine();
          guid = Guid.NewGuid();
          stockSlipLine.Id = guid.ToString();
          stockSlipLine.StockId = stock.Id;
          stockSlipLine.Quantity = (Decimal) Lorem.Integer(1, 100);
          stockSlipLine.UnitId = stockUnit.Id;
          stockSlipLine.Price = stock.Price;
          stockSlipLine.CurrencyId = stock.CurrencyId;
          StockSlipLine line = stockSlipLine;
          model.Lines.Add(line);
          if (model.CurrencyConvertions.All<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (x => x.CurrencyId != line.CurrencyId)))
          {
            Currency currency = ((IEnumerable<Currency>) currenciesList).Single<Currency>((Func<Currency, bool>) (x => x.Id == line.CurrencyId));
            CurrencyRate rate = currency.GetRate(new DateTime?(model.Date));
            model.CurrencyConvertions.Add(new CurrencyConvertion()
            {
              CurrencyId = currency.Id,
              Multiplier = rate.Multiplier,
              Divider = rate.Divider
            });
          }
          if (model.StockUnitConvertions.All<StockUnitConvertion>((Func<StockUnitConvertion, bool>) (x => x.StockId != line.StockId || x.UnitId != line.UnitId)))
            model.StockUnitConvertions.Add(new StockUnitConvertion()
            {
              StockId = line.StockId,
              UnitId = stockUnit.Id,
              Multiplier = stockUnit.Multiplier,
              Divider = stockUnit.Divider
            });
        }
        await objectsViewModel._stockSlipsRepository.CreateAsync(model);
        objectsViewModel.Status = $"Created StockSlip(s): {i}";
      }
      warehouses = (string[]) null;
      currenciesList = (Currency[]) null;
      stocks = (Stock[]) null;
      groups = (string[]) null;
    }
    catch (Exception ex)
    {
      objectsViewModel.UserInteractionService.ShowExceptionMessage(ex);
      objectsViewModel.Status = "Error occurred while creating StockSlip(s)";
    }
    objectsViewModel.IsBusy = false;
  }

  public virtual int CreateStockTransfersCount
  {
    get => this._createStockTransfersCount;
    set
    {
      this.SetProperty<int>(ref this._createStockTransfersCount, value, nameof (CreateStockTransfersCount));
    }
  }

  public ICommand CreateStockTransfers
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnCreateStockTransfersAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private async Task OnCreateStockTransfersAsync()
  {
    MockObjectsViewModel objectsViewModel = this;
    if (objectsViewModel.CreateStockTransfersCount < 1)
      return;
    objectsViewModel.IsBusy = true;
    objectsViewModel.Status = $"Starting {objectsViewModel.CreateStockTransfersCount} StockTransfer(s) creation...";
    try
    {
      string[] warehouses = (await objectsViewModel._warehousesRepository.GetAsync()).Select<Warehouse, string>((Func<Warehouse, string>) (x => x.Id)).ToArray<string>();
      if (warehouses.Length < 2)
        throw new Exception(objectsViewModel["There must be at least two warehouses to create transfer", Array.Empty<object>()]);
      Currency[] currenciesList = (await objectsViewModel._currenciesRepository.GetAsync()).ToArray<Currency>();
      Stock[] stocks = (await objectsViewModel._stocksRepository.GetAsync()).ToArray<Stock>();
      string[] groups = Lorem.Words(10).Split(' ');
      for (int i = 1; i <= objectsViewModel.CreateStockTransfersCount; ++i)
      {
        string barcode = $"99{i:D10}";
        string str1 = barcode + Symbology.CalculateChecksumDigit(barcode);
        string sourceWarehouse = Lorem.Random<string>(warehouses);
        string str2 = Lorem.Random<string>(((IEnumerable<string>) warehouses).Where<string>((Func<string, bool>) (x => x != sourceWarehouse)).ToArray<string>());
        StockTransfer stockTransfer = new StockTransfer();
        stockTransfer.Id = Guid.NewGuid().ToString();
        stockTransfer.Code = str1;
        stockTransfer.Date = Lorem.DateTime(DateTime.Today.AddDays((double) -DateTime.Today.DayOfYear), DateTime.Now);
        stockTransfer.WarehouseId = sourceWarehouse;
        stockTransfer.DestinationWarehouseId = str2;
        stockTransfer.UserId = objectsViewModel._loginService.Session.UserId;
        stockTransfer.UserName = objectsViewModel._loginService.Session.Username;
        stockTransfer.IsCompleted = Lorem.Chance(7, 10);
        stockTransfer.IsDisabled = Lorem.Chance(2, 10);
        stockTransfer.Group = Lorem.Random<string>(groups);
        stockTransfer.Description = Lorem.Sentence(2, 20);
        stockTransfer.Lines = new WatchedObservableCollection<StockTransferLine>();
        stockTransfer.Overheads = new WatchedObservableCollection<StockTransactionOverhead>();
        stockTransfer.CurrencyConvertions = new WatchedObservableCollection<CurrencyConvertion>();
        stockTransfer.StockUnitConvertions = new WatchedObservableCollection<StockUnitConvertion>();
        StockTransfer model = stockTransfer;
        for (int index = 0; index < Lorem.Integer(1, 100); ++index)
        {
          Stock stock = Lorem.Random<Stock>(stocks);
          int num1 = Lorem.Integer(1, 100);
          StockUnit stockUnit1 = Lorem.Random<StockUnit>(stock.Units.ToArray<StockUnit>());
          int num2 = model.IsCompleted ? num1 : Lorem.Integer(1, 100);
          StockUnit stockUnit2 = model.IsCompleted ? stockUnit1 : Lorem.Random<StockUnit>(stock.Units.ToArray<StockUnit>());
          StockTransferLine stockTransferLine = new StockTransferLine();
          stockTransferLine.StockId = stock.Id;
          stockTransferLine.Id = Guid.NewGuid().ToString();
          stockTransferLine.Quantity = (Decimal) num1;
          stockTransferLine.UnitId = stockUnit1.Id;
          stockTransferLine.ReceivedId = Guid.NewGuid().ToString();
          stockTransferLine.ReceivedQuantity = (Decimal) num2;
          stockTransferLine.ReceivedUnitId = stockUnit2.Id;
          stockTransferLine.Price = stock.Price;
          stockTransferLine.CurrencyId = stock.CurrencyId;
          StockTransferLine line = stockTransferLine;
          model.Lines.Add(line);
          if (model.CurrencyConvertions.All<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (x => x.CurrencyId != line.CurrencyId)))
          {
            Currency currency = ((IEnumerable<Currency>) currenciesList).Single<Currency>((Func<Currency, bool>) (x => x.Id == line.CurrencyId));
            CurrencyRate rate = currency.GetRate(new DateTime?(model.Date));
            model.CurrencyConvertions.Add(new CurrencyConvertion()
            {
              CurrencyId = currency.Id,
              Multiplier = rate.Multiplier,
              Divider = rate.Divider
            });
          }
          if (model.StockUnitConvertions.All<StockUnitConvertion>((Func<StockUnitConvertion, bool>) (x => x.StockId != line.StockId || x.UnitId != line.UnitId)))
            model.StockUnitConvertions.Add(new StockUnitConvertion()
            {
              StockId = line.StockId,
              UnitId = stockUnit1.Id,
              Multiplier = stockUnit1.Multiplier,
              Divider = stockUnit1.Divider
            });
          if (model.StockUnitConvertions.All<StockUnitConvertion>((Func<StockUnitConvertion, bool>) (x => x.StockId != line.StockId || x.UnitId != line.ReceivedUnitId)))
            model.StockUnitConvertions.Add(new StockUnitConvertion()
            {
              StockId = line.StockId,
              UnitId = stockUnit2.Id,
              Multiplier = stockUnit2.Multiplier,
              Divider = stockUnit2.Divider
            });
        }
        await objectsViewModel._stockTransfersRepository.CreateAsync(model);
        objectsViewModel.Status = $"Created StockTransfer(s): {i}";
      }
      warehouses = (string[]) null;
      currenciesList = (Currency[]) null;
      stocks = (Stock[]) null;
      groups = (string[]) null;
    }
    catch (Exception ex)
    {
      objectsViewModel.UserInteractionService.ShowExceptionMessage(ex);
      objectsViewModel.Status = "Error occurred while creating StockTransfer(s)";
    }
    objectsViewModel.IsBusy = false;
  }

  public int CreateFundsSlipsCount
  {
    get => this._createFundsSlipsCount;
    set
    {
      this.SetProperty<int>(ref this._createFundsSlipsCount, value, nameof (CreateFundsSlipsCount));
    }
  }

  public ICommand CreateFundsSlips
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnCreateFundsSlipsAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  protected virtual async Task OnCreateFundsSlipsAsync()
  {
    MockObjectsViewModel objectsViewModel = this;
    if (objectsViewModel.CreateFundsSlipsCount < 1)
      return;
    objectsViewModel.IsBusy = true;
    objectsViewModel.Status = $"Starting {objectsViewModel.CreateFundsSlipsCount} FundsSlip(s) creation...";
    try
    {
      string[] depositories = (await objectsViewModel._depositoriesRepository.GetAsync()).Select<Depository, string>((Func<Depository, string>) (x => x.Id)).ToArray<string>();
      Currency[] currenciesList = (await objectsViewModel._currenciesRepository.GetAsync()).ToArray<Currency>();
      string[] groups = Lorem.Words(10).Split(' ');
      for (int i = 1; i <= objectsViewModel.CreateFundsSlipsCount; ++i)
      {
        string barcode = $"99{i:D10}";
        string str = barcode + Symbology.CalculateChecksumDigit(barcode);
        FundsSlip fundsSlip = new FundsSlip();
        fundsSlip.Id = Guid.NewGuid().ToString();
        fundsSlip.Code = str;
        fundsSlip.Date = Lorem.DateTime(DateTime.Today.AddDays((double) -DateTime.Today.DayOfYear), DateTime.Now);
        fundsSlip.DepositoryId = Lorem.Random<string>(depositories);
        fundsSlip.UserId = objectsViewModel._loginService.Session.UserId;
        fundsSlip.UserName = objectsViewModel._loginService.Session.Username;
        fundsSlip.IsCompleted = Lorem.Chance(7, 10);
        fundsSlip.IsDisabled = Lorem.Chance(2, 10);
        fundsSlip.SlipType = Lorem.Enum<FundsSlipType>();
        fundsSlip.Group = Lorem.Random<string>(groups);
        fundsSlip.Description = Lorem.Sentence(2, 20);
        fundsSlip.Lines = new WatchedObservableCollection<FundsSlipLine>();
        fundsSlip.CurrencyConvertions = new WatchedObservableCollection<CurrencyConvertion>();
        FundsSlip model = fundsSlip;
        for (int index = 0; index < Lorem.Integer(1, 2); ++index)
        {
          Currency currency = Lorem.Random<Currency>(currenciesList);
          FundsSlipLine fundsSlipLine = new FundsSlipLine();
          fundsSlipLine.Amount = (Decimal) Lorem.Integer(1, 100);
          fundsSlipLine.CurrencyId = currency.Id;
          FundsSlipLine line = fundsSlipLine;
          if (model.CurrencyConvertions.All<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (x => x.CurrencyId != line.CurrencyId)))
          {
            CurrencyRate rate = currency.GetRate(new DateTime?(model.Date));
            model.CurrencyConvertions.Add(new CurrencyConvertion()
            {
              CurrencyId = currency.Id,
              Multiplier = rate.Multiplier,
              Divider = rate.Divider
            });
          }
          model.Lines.Add(line);
        }
        await objectsViewModel._fundsSlipsRepository.CreateAsync(model);
        objectsViewModel.Status = $"Created FundsSlip(s): {i}";
      }
      depositories = (string[]) null;
      currenciesList = (Currency[]) null;
      groups = (string[]) null;
    }
    catch (Exception ex)
    {
      objectsViewModel.UserInteractionService.ShowExceptionMessage(ex);
      objectsViewModel.Status = "Error occurred while creating FundsSlip(s)";
    }
    objectsViewModel.IsBusy = false;
  }

  public int CreateFundsTransfersCount
  {
    get => this._createFundsTransfersCount;
    set
    {
      this.SetProperty<int>(ref this._createFundsTransfersCount, value, nameof (CreateFundsTransfersCount));
    }
  }

  public ICommand CreateFundsTransfers
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnCreateFundsTransfersAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  protected virtual async Task OnCreateFundsTransfersAsync()
  {
    MockObjectsViewModel objectsViewModel = this;
    if (objectsViewModel.CreateFundsTransfersCount < 1)
      return;
    objectsViewModel.IsBusy = true;
    objectsViewModel.Status = $"Starting {objectsViewModel.CreateFundsTransfersCount} FundsTransfer(s) creation...";
    try
    {
      string[] depositories = (await objectsViewModel._depositoriesRepository.GetAsync()).Select<Depository, string>((Func<Depository, string>) (x => x.Id)).ToArray<string>();
      if (depositories.Length < 2)
        throw new Exception(objectsViewModel["There must be at least two depositories to create transfer", Array.Empty<object>()]);
      Currency[] currenciesList = (await objectsViewModel._currenciesRepository.GetAsync()).ToArray<Currency>();
      string[] groups = Lorem.Words(10).Split(' ');
      for (int i = 1; i <= objectsViewModel.CreateFundsTransfersCount; ++i)
      {
        string barcode = $"99{i:D10}";
        string str1 = barcode + Symbology.CalculateChecksumDigit(barcode);
        string sourceDepository = Lorem.Random<string>(depositories);
        string str2 = Lorem.Random<string>(((IEnumerable<string>) depositories).Where<string>((Func<string, bool>) (x => x != sourceDepository)).ToArray<string>());
        FundsTransfer fundsTransfer = new FundsTransfer();
        fundsTransfer.Id = Guid.NewGuid().ToString();
        fundsTransfer.Code = str1;
        fundsTransfer.Date = Lorem.DateTime(DateTime.Today.AddDays((double) -DateTime.Today.DayOfYear), DateTime.Now);
        fundsTransfer.DepositoryId = sourceDepository;
        fundsTransfer.DestinationDepositoryId = str2;
        fundsTransfer.UserId = objectsViewModel._loginService.Session.UserId;
        fundsTransfer.UserName = objectsViewModel._loginService.Session.Username;
        fundsTransfer.IsCompleted = Lorem.Chance(7, 10);
        fundsTransfer.IsDisabled = Lorem.Chance(2, 10);
        fundsTransfer.Group = Lorem.Random<string>(groups);
        fundsTransfer.Description = Lorem.Sentence(2, 20);
        fundsTransfer.Lines = new WatchedObservableCollection<FundsTransferLine>();
        fundsTransfer.CurrencyConvertions = new WatchedObservableCollection<CurrencyConvertion>();
        FundsTransfer model = fundsTransfer;
        for (int index = 0; index < Lorem.Integer(1, 2); ++index)
        {
          Currency currency = Lorem.Random<Currency>(currenciesList);
          int num1 = Lorem.Integer(1, 100);
          int num2 = model.IsCompleted ? num1 : Lorem.Integer(1, 100);
          FundsTransferLine fundsTransferLine = new FundsTransferLine();
          fundsTransferLine.CurrencyId = currency.Id;
          fundsTransferLine.Amount = (Decimal) num1;
          fundsTransferLine.ReceivedAmount = (Decimal) num2;
          FundsTransferLine line = fundsTransferLine;
          if (model.CurrencyConvertions.All<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (x => x.CurrencyId != line.CurrencyId)))
          {
            CurrencyRate rate = currency.GetRate(new DateTime?(model.Date));
            model.CurrencyConvertions.Add(new CurrencyConvertion()
            {
              CurrencyId = currency.Id,
              Multiplier = rate.Multiplier,
              Divider = rate.Divider
            });
          }
          model.Lines.Add(line);
        }
        await objectsViewModel._fundsTransfersRepository.CreateAsync(model);
        objectsViewModel.Status = $"Created FundsTransfer(s): {i}";
      }
      depositories = (string[]) null;
      currenciesList = (Currency[]) null;
      groups = (string[]) null;
    }
    catch (Exception ex)
    {
      objectsViewModel.UserInteractionService.ShowExceptionMessage(ex);
      objectsViewModel.Status = "Error occurred while creating FundsTransfer(s)";
    }
    objectsViewModel.IsBusy = false;
  }

  public int CreateExpensesCount
  {
    get => this._createExpensesCount;
    set
    {
      this.SetProperty<int>(ref this._createExpensesCount, value, nameof (CreateExpensesCount));
    }
  }

  public ICommand CreateExpenses
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnCreateExpensesAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  protected virtual async Task OnCreateExpensesAsync()
  {
    MockObjectsViewModel objectsViewModel = this;
    if (objectsViewModel.CreateExpensesCount < 1)
      return;
    objectsViewModel.IsBusy = true;
    objectsViewModel.Status = $"Starting {objectsViewModel.CreateExpensesCount} Expense(s) creation...";
    try
    {
      string[] types = Lorem.Words(10).Split(' ');
      string[] groups = Lorem.Words(10).Split(' ');
      for (int i = 1; i <= objectsViewModel.CreateExpensesCount; ++i)
      {
        Expense expense = new Expense();
        expense.Id = Guid.NewGuid().ToString();
        expense.Name = Lorem.Words(1, 5);
        expense.Type = Lorem.Random<string>(types);
        expense.Group = Lorem.Random<string>(groups);
        expense.Description = Lorem.Sentence(2, 20);
        Expense model = expense;
        await objectsViewModel._expensesRepository.CreateAsync(model);
        objectsViewModel.Status = $"Created Expenses: {i}";
      }
      types = (string[]) null;
      groups = (string[]) null;
    }
    catch (Exception ex)
    {
      objectsViewModel.UserInteractionService.ShowExceptionMessage(ex);
      objectsViewModel.Status = "Error occurred while creating Expense(s)";
    }
    objectsViewModel.IsBusy = false;
  }

  public int CreateExpenseSlipsCount
  {
    get => this._createExpenseSlipsCount;
    set
    {
      this.SetProperty<int>(ref this._createExpenseSlipsCount, value, nameof (CreateExpenseSlipsCount));
    }
  }

  public ICommand CreateExpenseSlips
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnCreateExpenseSlipsAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  protected virtual async Task OnCreateExpenseSlipsAsync()
  {
    MockObjectsViewModel objectsViewModel = this;
    if (objectsViewModel.CreateExpenseSlipsCount < 1)
      return;
    objectsViewModel.IsBusy = true;
    objectsViewModel.Status = $"Starting {objectsViewModel.CreateExpenseSlipsCount} ExpenseSlip(s) creation...";
    try
    {
      string[] depositories = (await objectsViewModel._depositoriesRepository.GetAsync()).Select<Depository, string>((Func<Depository, string>) (x => x.Id)).ToArray<string>();
      Currency[] currenciesList = (await objectsViewModel._currenciesRepository.GetAsync()).ToArray<Currency>();
      Expense[] expenses = (await objectsViewModel._expensesRepository.GetAsync()).ToArray<Expense>();
      string[] groups = Lorem.Words(10).Split(' ');
      for (int i = 1; i <= objectsViewModel.CreateExpenseSlipsCount; ++i)
      {
        string barcode = $"99{i:D10}";
        string str = barcode + Symbology.CalculateChecksumDigit(barcode);
        ExpenseSlip expenseSlip = new ExpenseSlip();
        expenseSlip.Id = Guid.NewGuid().ToString();
        expenseSlip.Code = str;
        expenseSlip.Date = Lorem.DateTime(DateTime.Today.AddDays((double) -DateTime.Today.DayOfYear), DateTime.Now);
        expenseSlip.DepositoryId = Lorem.Random<string>(depositories);
        expenseSlip.UserId = objectsViewModel._loginService.Session.UserId;
        expenseSlip.UserName = objectsViewModel._loginService.Session.Username;
        expenseSlip.IsCompleted = Lorem.Chance(7, 10);
        expenseSlip.IsDisabled = Lorem.Chance(2, 10);
        expenseSlip.Group = Lorem.Random<string>(groups);
        expenseSlip.Description = Lorem.Sentence(2, 20);
        expenseSlip.Lines = new WatchedObservableCollection<ExpenseSlipLine>();
        expenseSlip.CurrencyConvertions = new WatchedObservableCollection<CurrencyConvertion>();
        ExpenseSlip model = expenseSlip;
        for (int index = 0; index < Lorem.Integer(1, 100); ++index)
        {
          Expense expense = Lorem.Random<Expense>(expenses);
          Currency currency = Lorem.Random<Currency>(currenciesList);
          ExpenseSlipLine expenseSlipLine1 = new ExpenseSlipLine();
          expenseSlipLine1.ExpenseId = expense.Id;
          expenseSlipLine1.Amount = (Decimal) Lorem.Integer(1, 100);
          expenseSlipLine1.CurrencyId = currency.Id;
          ExpenseSlipLine expenseSlipLine2 = expenseSlipLine1;
          if (model.CurrencyConvertions.All<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (x => x.CurrencyId != currency.Id)))
          {
            CurrencyRate rate = currency.GetRate(new DateTime?(model.Date));
            model.CurrencyConvertions.Add(new CurrencyConvertion()
            {
              CurrencyId = currency.Id,
              Multiplier = rate.Multiplier,
              Divider = rate.Divider
            });
          }
          model.Lines.Add(expenseSlipLine2);
        }
        await objectsViewModel._expenseSlipsRepository.CreateAsync(model);
        objectsViewModel.Status = $"Created ExpenseSlip(s): {i}";
      }
      depositories = (string[]) null;
      currenciesList = (Currency[]) null;
      expenses = (Expense[]) null;
      groups = (string[]) null;
    }
    catch (Exception ex)
    {
      objectsViewModel.UserInteractionService.ShowExceptionMessage(ex);
      objectsViewModel.Status = "Error occurred while creating ExpenseSlip(s)";
    }
    objectsViewModel.IsBusy = false;
  }

  public int CreatePartnersCount
  {
    get => this._createPartnersCount;
    set
    {
      this.SetProperty<int>(ref this._createPartnersCount, value, nameof (CreatePartnersCount));
    }
  }

  public ICommand CreatePartners
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnCreatePartnersAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  protected virtual async Task OnCreatePartnersAsync()
  {
    MockObjectsViewModel objectsViewModel = this;
    if (objectsViewModel.CreatePartnersCount < 1)
      return;
    objectsViewModel.IsBusy = true;
    objectsViewModel.Status = $"Starting {objectsViewModel.CreatePartnersCount} Partner(s) creation...";
    try
    {
      string[] groups = Lorem.Words(10).Split(' ');
      for (int i = 1; i <= objectsViewModel.CreatePartnersCount; ++i)
      {
        Partner partner = new Partner();
        partner.Id = Guid.NewGuid().ToString();
        partner.Code = Lorem.Words(1);
        partner.Name = Lorem.Words(1, 5);
        partner.Group = Lorem.Random<string>(groups);
        partner.Description = Lorem.Sentence(2, 20);
        Partner model = partner;
        await objectsViewModel._partnersRepository.CreateAsync(model);
        objectsViewModel.Status = $"Created Partners: {i}";
      }
      groups = (string[]) null;
    }
    catch (Exception ex)
    {
      objectsViewModel.UserInteractionService.ShowExceptionMessage(ex);
      objectsViewModel.Status = "Error occurred while creating Partner(s)";
    }
    objectsViewModel.IsBusy = false;
  }

  public int CreatePartnerSlipsCount
  {
    get => this._createPartnerSlipsCount;
    set
    {
      this.SetProperty<int>(ref this._createPartnerSlipsCount, value, nameof (CreatePartnerSlipsCount));
    }
  }

  public ICommand CreatePartnerSlips
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnCreatePartnerSlipsAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  protected virtual async Task OnCreatePartnerSlipsAsync()
  {
    MockObjectsViewModel objectsViewModel = this;
    if (objectsViewModel.CreatePartnerSlipsCount < 1)
      return;
    objectsViewModel.IsBusy = true;
    objectsViewModel.Status = $"Starting {objectsViewModel.CreatePartnerSlipsCount} PartnerSlip(s) creation...";
    try
    {
      string[] offices = (await objectsViewModel._officesRepository.GetAsync()).Select<Office, string>((Func<Office, string>) (x => x.Id)).ToArray<string>();
      Currency[] currenciesList = (await objectsViewModel._currenciesRepository.GetAsync()).ToArray<Currency>();
      Partner[] partners = (await objectsViewModel._partnersRepository.GetAsync()).ToArray<Partner>();
      string[] groups = Lorem.Words(10).Split(' ');
      for (int i = 1; i <= objectsViewModel.CreatePartnerSlipsCount; ++i)
      {
        string barcode = $"99{i:D10}";
        string str = barcode + Symbology.CalculateChecksumDigit(barcode);
        PartnerSlip partnerSlip = new PartnerSlip();
        partnerSlip.Id = Guid.NewGuid().ToString();
        partnerSlip.Code = str;
        partnerSlip.Date = Lorem.DateTime(DateTime.Today.AddDays((double) -DateTime.Today.DayOfYear), DateTime.Now);
        partnerSlip.OfficeId = Lorem.Random<string>(offices);
        partnerSlip.UserId = objectsViewModel._loginService.Session.UserId;
        partnerSlip.UserName = objectsViewModel._loginService.Session.Username;
        partnerSlip.IsCompleted = Lorem.Chance(7, 10);
        partnerSlip.IsDisabled = Lorem.Chance(2, 10);
        partnerSlip.Group = Lorem.Random<string>(groups);
        partnerSlip.Description = Lorem.Sentence(2, 20);
        partnerSlip.Lines = new ObservableCollection<PartnerSlipLine>();
        partnerSlip.CurrencyConvertions = new ObservableCollection<CurrencyConvertion>();
        PartnerSlip model = partnerSlip;
        for (int index = 0; index < Lorem.Integer(1, 100); ++index)
        {
          Partner partner = Lorem.Random<Partner>(partners);
          Currency debitCurrency = Lorem.Random<Currency>(currenciesList);
          Currency creditCurrency = Lorem.Random<Currency>(currenciesList);
          PartnerSlipLine partnerSlipLine = new PartnerSlipLine()
          {
            PartnerId = partner.Id,
            DebitAmount = (Decimal) Lorem.Integer(1, 100),
            DebitCurrencyId = debitCurrency.Id,
            CreditAmount = (Decimal) Lorem.Integer(1, 100),
            CreditCurrencyId = creditCurrency.Id
          };
          if (model.CurrencyConvertions.All<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (x => x.CurrencyId != debitCurrency.Id)))
          {
            CurrencyRate rate = debitCurrency.GetRate(new DateTime?(model.Date));
            model.CurrencyConvertions.Add(new CurrencyConvertion()
            {
              CurrencyId = debitCurrency.Id,
              Multiplier = rate.Multiplier,
              Divider = rate.Divider
            });
          }
          if (model.CurrencyConvertions.All<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (x => x.CurrencyId != creditCurrency.Id)))
          {
            CurrencyRate rate = creditCurrency.GetRate(new DateTime?(model.Date));
            model.CurrencyConvertions.Add(new CurrencyConvertion()
            {
              CurrencyId = creditCurrency.Id,
              Multiplier = rate.Multiplier,
              Divider = rate.Divider
            });
          }
          model.Lines.Add(partnerSlipLine);
        }
        await objectsViewModel._partnerSlipsRepository.CreateAsync(model);
        objectsViewModel.Status = $"Created PartnerSlip(s): {i}";
      }
      offices = (string[]) null;
      currenciesList = (Currency[]) null;
      partners = (Partner[]) null;
      groups = (string[]) null;
    }
    catch (Exception ex)
    {
      objectsViewModel.UserInteractionService.ShowExceptionMessage(ex);
      objectsViewModel.Status = "Error occurred while creating PartnerSlip(s)";
    }
    objectsViewModel.IsBusy = false;
  }

  public int CreatePartnerTransfersCount
  {
    get => this._createPartnerTransfersCount;
    set
    {
      this.SetProperty<int>(ref this._createPartnerTransfersCount, value, nameof (CreatePartnerTransfersCount));
    }
  }

  public ICommand CreatePartnerTransfers
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnCreatePartnerTransfersAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  protected virtual async Task OnCreatePartnerTransfersAsync()
  {
    MockObjectsViewModel objectsViewModel = this;
    if (objectsViewModel.CreatePartnerTransfersCount < 1)
      return;
    objectsViewModel.IsBusy = true;
    objectsViewModel.Status = $"Starting {objectsViewModel.CreatePartnerTransfersCount} PartnerTransfer(s) creation...";
    try
    {
      string[] offices = (await objectsViewModel._officesRepository.GetAsync()).Select<Office, string>((Func<Office, string>) (x => x.Id)).ToArray<string>();
      if (offices.Length < 2)
        throw new Exception(objectsViewModel["There must be at least two offices to create transfer", Array.Empty<object>()]);
      Partner[] partners = (await objectsViewModel._partnersRepository.GetAsync()).ToArray<Partner>();
      if (partners.Length < 2)
        throw new Exception(objectsViewModel["There must be at least two partners to create transfer", Array.Empty<object>()]);
      Currency[] currenciesList = (await objectsViewModel._currenciesRepository.GetAsync()).ToArray<Currency>();
      string[] groups = Lorem.Words(10).Split(' ');
      for (int i = 1; i <= objectsViewModel.CreatePartnerTransfersCount; ++i)
      {
        string barcode = $"99{i:D10}";
        string str1 = barcode + Symbology.CalculateChecksumDigit(barcode);
        PartnerTransfer partnerTransfer = new PartnerTransfer();
        partnerTransfer.Id = Guid.NewGuid().ToString();
        partnerTransfer.Code = str1;
        partnerTransfer.Date = Lorem.DateTime(DateTime.Today.AddDays((double) -DateTime.Today.DayOfYear), DateTime.Now);
        partnerTransfer.UserId = objectsViewModel._loginService.Session.UserId;
        partnerTransfer.UserName = objectsViewModel._loginService.Session.Username;
        partnerTransfer.IsCompleted = Lorem.Chance(7, 10);
        partnerTransfer.IsDisabled = Lorem.Chance(2, 10);
        partnerTransfer.Group = Lorem.Random<string>(groups);
        partnerTransfer.Description = Lorem.Sentence(2, 20);
        partnerTransfer.Lines = new ObservableCollection<PartnerTransferLine>();
        partnerTransfer.CurrencyConvertions = new ObservableCollection<CurrencyConvertion>();
        PartnerTransfer model = partnerTransfer;
        int num = Lorem.Integer(1, 100);
        Currency currency = Lorem.Random<Currency>(currenciesList);
        Partner debitPartner = Lorem.Random<Partner>(partners);
        Partner partner = Lorem.Random<Partner>(((IEnumerable<Partner>) partners).Where<Partner>((Func<Partner, bool>) (x => x.Id != debitPartner.Id)).ToArray<Partner>());
        string debitOffice = Lorem.Random<string>(offices);
        string str2 = Lorem.Random<string>(((IEnumerable<string>) offices).Where<string>((Func<string, bool>) (x => x != debitOffice)).ToArray<string>());
        PartnerTransferLine partnerTransferLine1 = new PartnerTransferLine()
        {
          OfficeId = debitOffice,
          PartnerId = debitPartner.Id,
          DebitAmount = (Decimal) num,
          DebitCurrencyId = currency.Id
        };
        PartnerTransferLine partnerTransferLine2 = new PartnerTransferLine()
        {
          OfficeId = str2,
          PartnerId = partner.Id,
          CreditAmount = (Decimal) num,
          CreditCurrencyId = currency.Id
        };
        if (model.CurrencyConvertions.All<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (x => x.CurrencyId != currency.Id)))
        {
          CurrencyRate rate = currency.GetRate(new DateTime?(model.Date));
          model.CurrencyConvertions.Add(new CurrencyConvertion()
          {
            CurrencyId = currency.Id,
            Multiplier = rate.Multiplier,
            Divider = rate.Divider
          });
        }
        model.Lines.Add(partnerTransferLine1);
        model.Lines.Add(partnerTransferLine2);
        await objectsViewModel._partnerTransfersRepository.CreateAsync(model);
        objectsViewModel.Status = $"Created PartnerTransfer(s): {i}";
      }
      offices = (string[]) null;
      partners = (Partner[]) null;
      currenciesList = (Currency[]) null;
      groups = (string[]) null;
    }
    catch (Exception ex)
    {
      objectsViewModel.UserInteractionService.ShowExceptionMessage(ex);
      objectsViewModel.Status = "Error occurred while creating PartnerTransfer(s)";
    }
    objectsViewModel.IsBusy = false;
  }
}
