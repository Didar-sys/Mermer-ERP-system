// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Commerce.InvoiceDetailsViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Commerce.Models;
using Payhas.Binyat.CRM.Models;
using Payhas.Binyat.CRM.Services;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.StockManagement.Models;
using Payhas.Binyat.StockManagement.Services;
using Payhas.Binyat.Transactions.Models;
using Payhas.Binyat.Transactions.Models.Authorizers;
using Payhas.Binyat.Transactions.Services;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.Services;
using Payhas.Binyat.Ui.Core.ViewModels.Common;
using Payhas.Binyat.Ui.Core.ViewModels.StockManagement;
using Payhas.Binyat.Ui.Core.ViewModels.Transactions;
using Payhas.Binyat.Ui.Core.ViewModels.Warehousing.Ordering;
using Payhas.Data;
using Payhas.Data.Authorizers;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using Payhas.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Commerce;

public class InvoiceDetailsViewModel : 
  StockTransactionDetailsViewModel<Invoice, InvoiceLine, InvoiceType>,
  IMvxViewModel<InvoiceType>,
  IMvxViewModel
{
  private readonly IPrintingService _printingService;
  private readonly IStocksRepository _stocksRepository;
  private readonly IPartnerBalancesRepository _partnerBalancesRepository;
  private InvoiceType _newInvoiceType = InvoiceType.Sales;
  private PartnerBalanceResult _partnerBalanceToDate;
  private string[] _priceGroupNames;

  public InvoiceDetailsViewModel(
    CopyCreate copyCreate,
    IRepository<Invoice> repository,
    ITransactionAuthorizer<Invoice> authorizer,
    IConfigurator configurator,
    ILoginService loginService,
    StockSearcher stockSearcher,
    Reference<Office> offices,
    Reference<Partner> partners,
    Reference<Currency> currencies,
    Reference<Warehouse> warehouses,
    IPrintingService printingService,
    Reference<Depository> depositories,
    IStocksRepository stocksRepository,
    IMvxNavigationService navigationService,
    ITransactionCodeGenerationService codegentor,
    IUserInteractionService userInteractionService,
    IPartnerBalancesRepository partnerBalancesRepository)
    : base(copyCreate, repository, (IListAuthorizer<Invoice>) authorizer, configurator, loginService, stockSearcher, currencies, warehouses, stocksRepository, navigationService, codegentor, userInteractionService)
  {
    this._printingService = printingService;
    this._stocksRepository = stocksRepository;
    this._partnerBalancesRepository = partnerBalancesRepository;
    this.Offices = offices;
    this.Partners = partners;
    this.Depositories = depositories;
    this.DiscountTypes = Enum.GetValues(typeof (InvoiceDiscountType)).Cast<InvoiceDiscountType>().Select<InvoiceDiscountType, ListHelper<InvoiceDiscountType>>((Func<InvoiceDiscountType, ListHelper<InvoiceDiscountType>>) (x => new ListHelper<InvoiceDiscountType>()
    {
      Text = this[x.ToString(), Array.Empty<object>()],
      Value = x
    })).ToArray<ListHelper<InvoiceDiscountType>>();
  }

  public Reference<Office> Offices { get; }

  public Reference<Partner> Partners { get; }

  public Reference<Depository> Depositories { get; }

  public ListHelper<InvoiceDiscountType>[] DiscountTypes { get; set; }

  public override Invoice Details
  {
    get => base.Details;
    set
    {
      base.Details = value;
      this.UpdatePartnerBalance();
    }
  }

  public virtual PartnerBalanceResult PartnerBalanceToDate
  {
    get => this._partnerBalanceToDate;
    set
    {
      this.SetProperty<PartnerBalanceResult>(ref this._partnerBalanceToDate, value, nameof (PartnerBalanceToDate));
      this.RaisePropertyChanged<PartnerBalanceResult>((Expression<Func<PartnerBalanceResult>>) (() => this.PartnerBalanceResult));
    }
  }

  public virtual PartnerBalanceResult PartnerBalanceResult
  {
    get
    {
      if (this.PartnerBalanceToDate == null)
        return (PartnerBalanceResult) null;
      return new PartnerBalanceResult()
      {
        Balance = this.PartnerBalanceToDate.Balance + this.Details.DisplayDebitCreditTotal
      };
    }
  }

  public virtual string[] PriceGroupNames
  {
    get => this._priceGroupNames;
    set => this.SetProperty<string[]>(ref this._priceGroupNames, value, nameof (PriceGroupNames));
  }

  public void Prepare(InvoiceType parameter) => this._newInvoiceType = parameter;

  protected override async Task LoadFacetsAsync()
  {
    await base.LoadFacetsAsync();
    this.PriceGroupNames = (await this._stocksRepository.GetFacets("PriceGroupNames"))["PriceGroupNames"].Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (x => x.Key)).ToArray<string>();
  }

  protected override Task PreLoad()
  {
    return Task.WhenAll(base.PreLoad(), this.Offices.Initialize(), this.Partners.Initialize(), this.Depositories.Initialize());
  }

  protected override async Task OnLoad()
  {
    InvoiceDetailsViewModel detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__1();
    if (!string.IsNullOrEmpty(detailsViewModel.ItemId))
      return;
    detailsViewModel.Details.InvoiceType = detailsViewModel._newInvoiceType;
    detailsViewModel.Details.OfficeId = detailsViewModel.AppSettings.DefaultOfficeId;
    detailsViewModel.Details.DepositoryId = detailsViewModel.AppSettings.DefaultDepositoryId;
    detailsViewModel.Details.StockPriceGroup = detailsViewModel.AppSettings.DefaultStockPriceGroup;
    Invoice details = detailsViewModel.Details;
    DateTime dateTime = DateTime.Now;
    dateTime = dateTime.AddDays((double) detailsViewModel.AppSettings.DefaultDueDateInDays);
    DateTime date = dateTime.Date;
    details.DueDate = date;
  }

  protected override async Task PostLoad()
  {
    InvoiceDetailsViewModel detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__2();
    if (detailsViewModel.Details.Payments == null)
      detailsViewModel.Details.Payments = new WatchedObservableCollection<InvoicePayment>();
    if (detailsViewModel.Details.Changes == null)
      detailsViewModel.Details.Changes = new WatchedObservableCollection<InvoicePayment>();
    if (detailsViewModel.Details.Discounts == null)
      detailsViewModel.Details.Discounts = new WatchedObservableCollection<InvoiceDiscount>();
    // ISSUE: explicit non-virtual call
    __nonvirtual (detailsViewModel.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => detailsViewModel.CanCreateAggregatedStockOrder)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (detailsViewModel.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => detailsViewModel.CanShowNewPrices)));
    detailsViewModel.Details.RaisePropertyChanged("DisplayDiscountsTotal");
    detailsViewModel.Details.RaisePropertyChanged("DisplayGrandTotal");
    detailsViewModel.Details.RaisePropertyChanged("DisplayPaymentsTotal");
    detailsViewModel.Details.RaisePropertyChanged("DisplayChangesTotal");
    detailsViewModel.Details.RaisePropertyChanged("DisplayLeftTotal");
    detailsViewModel.Details.RaisePropertyChanged("DisplayDebitCreditTotal");
    detailsViewModel.Details.RaisePropertyChanged("DisplayCreditTotal");
    detailsViewModel.Details.RaisePropertyChanged("DisplayDebitTotal");
    // ISSUE: explicit non-virtual call
    __nonvirtual (detailsViewModel.RaisePropertyChanged<PartnerBalanceResult>((Expression<Func<PartnerBalanceResult>>) (() => detailsViewModel.PartnerBalanceToDate)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (detailsViewModel.RaisePropertyChanged<PartnerBalanceResult>((Expression<Func<PartnerBalanceResult>>) (() => detailsViewModel.PartnerBalanceResult)));
    detailsViewModel.StockSearcher.PriceGroup = detailsViewModel.Details.StockPriceGroup;
    // ISSUE: reference to a compiler-generated method
    detailsViewModel.Partners.Filter = new Func<Partner, bool>(detailsViewModel.\u003CPostLoad\u003Eb__35_4);
    // ISSUE: reference to a compiler-generated method
    detailsViewModel.Offices.Filter = new Func<Office, bool>(detailsViewModel.\u003CPostLoad\u003Eb__35_5);
    detailsViewModel.UpdateFacilityFilters();
  }

  protected override void Details_PropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    base.Details_PropertyChanged(sender, e);
    if (e.PropertyName == "Date" || e.PropertyName == "PartnerId" || e.PropertyName == "WarehouseId" || e.PropertyName == "DisplayCurrencyId")
      this.UpdatePartnerBalance();
    else if (e.PropertyName == "DisplayDebitCreditTotal")
      this.RaisePropertyChanged<PartnerBalanceResult>((Expression<Func<PartnerBalanceResult>>) (() => this.PartnerBalanceResult));
    else if (e.PropertyName == "InvoiceType")
    {
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.CanSelectSource));
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.CanCreateAggregatedStockOrder));
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.CanShowNewPrices));
    }
    else if (e.PropertyName == "OfficeId")
      this.UpdateFacilityFilters();
    else if (e.PropertyName == "StockPriceGroup")
      this.StockSearcher.PriceGroup = this.Details.StockPriceGroup;
    this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.CanSelectSource));
  }

  private void UpdateFacilityFilters()
  {
    this.Warehouses.Filter = (Func<Warehouse, bool>) (x =>
    {
      if (!(x.OfficeId == this.Details?.OfficeId))
        return false;
      return !x.IsDisabled || x.Id == this.Details?.WarehouseId;
    });
    this.Depositories.Filter = (Func<Depository, bool>) (x =>
    {
      if (!(x.OfficeId == this.Details?.OfficeId))
        return false;
      return !x.IsDisabled || x.Id == this.Details?.DepositoryId;
    });
  }

  private async void UpdatePartnerBalance()
  {
    InvoiceDetailsViewModel detailsViewModel = this;
    if (!string.IsNullOrEmpty(detailsViewModel.Details?.PartnerId) && !string.IsNullOrEmpty(detailsViewModel.Details?.OfficeId) && detailsViewModel.Details?.CurrencyConvertions != null)
    {
      PartnerBalanceResult balanceToDateAsync = await detailsViewModel._partnerBalancesRepository.GetBalanceToDateAsync(detailsViewModel.Details.OfficeId, detailsViewModel.Details.PartnerId, detailsViewModel.Details.Date, detailsViewModel.Details.Id);
      CurrencyConvertion currencyConvertion = detailsViewModel.CurrencyConverter(detailsViewModel.Details.DisplayCurrencyId);
      detailsViewModel.PartnerBalanceToDate = new PartnerBalanceResult()
      {
        Balance = balanceToDateAsync.Balance / currencyConvertion.Multiplier * currencyConvertion.Divider
      };
    }
    else
      detailsViewModel.PartnerBalanceToDate = (PartnerBalanceResult) null;
  }

  protected override async Task<bool> OnSaveAsync()
  {
    InvoiceDetailsViewModel detailsViewModel1 = this;
    try
    {
      if (!string.IsNullOrEmpty(detailsViewModel1.Details.PartnerId))
      {
        if (detailsViewModel1.Details.IsDebitCredit)
        {
          // ISSUE: reference to a compiler-generated method
          Partner partner = detailsViewModel1.Partners.List.Single<Partner>(new Func<Partner, bool>(detailsViewModel1.\u003COnSaveAsync\u003Eb__39_0));
          Decimal? creditLimit = partner.CreditLimit;
          if (creditLimit.HasValue)
          {
            creditLimit = partner.CreditLimit;
            Decimal credit = detailsViewModel1.PartnerBalanceResult.Credit;
            if (creditLimit.GetValueOrDefault() < credit & creditLimit.HasValue)
            {
              IUserInteractionService interactionService = detailsViewModel1.UserInteractionService;
              string caption = detailsViewModel1["Partner credit limit reached", Array.Empty<object>()];
              InvoiceDetailsViewModel detailsViewModel2 = detailsViewModel1;
              string textName = $"{{0}} partner has a credit limit at: {{1:#,##0.00}}{Environment.NewLine}Are you sure you want to continue?";
              object[] objArray = new object[2]
              {
                (object) partner.Fullname,
                null
              };
              creditLimit = partner.CreditLimit;
              objArray[1] = (object) creditLimit.Value;
              string message = detailsViewModel2[textName, objArray];
              if (!interactionService.ShowMessage(caption, message, UserInteractionType.YesNo).GetValueOrDefault())
                return false;
            }
          }
        }
      }
    }
    catch (Exception ex)
    {
      detailsViewModel1.UserInteractionService.ShowExceptionMessage(ex);
    }
    // ISSUE: reference to a compiler-generated method
    if (!await detailsViewModel1.\u003C\u003En__3())
      return false;
    IPrintingService printingService = detailsViewModel1._printingService;
    Invoice details = detailsViewModel1.Details;
    PartnerBalanceResult partnerBalanceToDate = detailsViewModel1.PartnerBalanceToDate;
    Decimal balance = partnerBalanceToDate != null ? partnerBalanceToDate.Balance : 0M;
    await printingService.PrintInvoice(details, balance);
    return true;
  }

  public ICommand SelectedLineAlternativeCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectedLineAlternativeCommandAsync), (Func<bool>) (() => !this.IsBusy && this.CanEditSelectedLine));
    }
  }

  protected virtual async Task OnSelectedLineAlternativeCommandAsync()
  {
    InvoiceDetailsViewModel detailsViewModel = this;
    string stockId = await detailsViewModel.NavigationService.Navigate<SelectStockAlternativeViewModel, Tuple<string, string>, string>(new Tuple<string, string>(detailsViewModel.SelectedLine.StockId, detailsViewModel.Details.WarehouseId));
    if (string.IsNullOrEmpty(stockId) || detailsViewModel.SelectedLine.StockId == stockId)
      return;
    Stock stocksCacheAsync = await detailsViewModel.GetFromStocksCacheAsync(stockId);
    detailsViewModel.SelectedLine.StockId = stocksCacheAsync.Id;
    detailsViewModel.SelectedLine.UnitId = stocksCacheAsync.UnitId;
    CurrencyConvertion currencyConvertion = detailsViewModel.Details.CurrencyConverter(stocksCacheAsync.CurrencyId);
    detailsViewModel.SelectedLine.Price = detailsViewModel.Details.GetDisplayAmount(stocksCacheAsync.Price * currencyConvertion.Multiplier / currencyConvertion.Divider);
    detailsViewModel.SelectedLine.CurrencyId = detailsViewModel.Details.DisplayCurrencyId;
  }

  public ICommand UpdatePaymentCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnUpdatePaymentCommandAsync), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess));
    }
  }

  private async Task OnUpdatePaymentCommandAsync()
  {
    InvoiceDetailsViewModel detailsViewModel = this;
    try
    {
      IMvxNavigationService navigationService = detailsViewModel.NavigationService;
      IpdParams ipdParams1 = new IpdParams();
      ipdParams1.SubTotal = detailsViewModel.Details.DisplayTotal;
      ipdParams1.DiscountsTotal = detailsViewModel.Details.DisplayDiscountsTotal;
      ipdParams1.PaymentsTotal = detailsViewModel.Details.DisplayPaymentsTotal;
      ipdParams1.ChangesTotal = detailsViewModel.Details.DisplayChangesTotal;
      ipdParams1.CanDebitCredit = detailsViewModel.Details.CanDebitCredit;
      ipdParams1.DebitCreditLeftAmount = detailsViewModel.Details.DebitCreditLeftAmount;
      CancellationToken cancellationToken = new CancellationToken();
      IpdParams ipdParams2 = await navigationService.Navigate<InvoicePaymentDialogViewModel, IpdParams, IpdParams>(ipdParams1, cancellationToken: cancellationToken);
      if (ipdParams2 == null)
        return;
      if (detailsViewModel.Details.DisplayDiscountsTotal != ipdParams2.DiscountsTotal)
      {
        detailsViewModel.Details.Discounts.Clear();
        if (ipdParams2.DiscountsTotal > 0M)
          detailsViewModel.Details.Discounts.Add(new InvoiceDiscount()
          {
            Amount = ipdParams2.DiscountsTotal,
            Type = InvoiceDiscountType.Flat
          });
      }
      if (detailsViewModel.Details.DisplayPaymentsTotal != ipdParams2.PaymentsTotal)
      {
        detailsViewModel.Details.Payments.Clear();
        if (ipdParams2.PaymentsTotal > 0M)
          detailsViewModel.Details.Payments.Add(new InvoicePayment()
          {
            Amount = ipdParams2.PaymentsTotal,
            CurrencyId = detailsViewModel.Details.DisplayCurrencyId
          });
      }
      if (detailsViewModel.Details.DisplayChangesTotal != ipdParams2.ChangesTotal)
      {
        detailsViewModel.Details.Changes.Clear();
        if (ipdParams2.ChangesTotal > 0M)
          detailsViewModel.Details.Changes.Add(new InvoicePayment()
          {
            Amount = ipdParams2.ChangesTotal,
            CurrencyId = detailsViewModel.Details.DisplayCurrencyId
          });
      }
      detailsViewModel.Details.DebitCreditLeftAmount = ipdParams2.DebitCreditLeftAmount;
    }
    catch (Exception ex)
    {
      detailsViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
  }

  public ICommand RemovePartnerCommand
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.OnRemovePartnerCommand), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess));
    }
  }

  private void OnRemovePartnerCommand()
  {
    this.Details.DebitCreditLeftAmount = false;
    this.Details.PartnerId = (string) null;
  }

  public ICommand SelectPartnerCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectPartnerCommandAsync), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess));
    }
  }

  private async Task OnSelectPartnerCommandAsync()
  {
    InvoiceDetailsViewModel detailsViewModel = this;
    Invoice invoice = detailsViewModel.Details;
    invoice.PartnerId = await detailsViewModel.NavigationService.Navigate<ListViewModel<Partner>, string, string>(detailsViewModel.Details.PartnerId ?? Guid.Empty.ToString());
    invoice = (Invoice) null;
  }

  public ICommand SelectDepositoryCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectDepositoryCommandAsync), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess));
    }
  }

  private async Task OnSelectDepositoryCommandAsync()
  {
    InvoiceDetailsViewModel detailsViewModel = this;
    Invoice invoice = detailsViewModel.Details;
    invoice.DepositoryId = await detailsViewModel.NavigationService.Navigate<ListViewModel<Depository>, string, string>(detailsViewModel.Details.DepositoryId ?? Guid.Empty.ToString());
    invoice = (Invoice) null;
  }

  public bool CanSelectSource
  {
    get
    {
      return this.HasSaveAccess && this.Details != null && this.Details.InvoiceType == InvoiceType.SalesReturn;
    }
  }

  public ICommand SelectSource
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectSourceAsync), (Func<bool>) (() => !this.IsBusy && this.CanSelectSource));
    }
  }

  private async Task OnSelectSourceAsync()
  {
    InvoiceDetailsViewModel detailsViewModel = this;
    IEnumerable<StockAction> stockActions = await detailsViewModel.NavigationService.Navigate<SelectSourceInvoiceViewModel, IEnumerable<StockAction>>();
    CurrencyConvertion displayCurrencyConvertion;
    if (stockActions == null)
    {
      displayCurrencyConvertion = (CurrencyConvertion) null;
    }
    else
    {
      displayCurrencyConvertion = detailsViewModel.CurrencyConverter(detailsViewModel.Details.DisplayCurrencyId);
      foreach (StockAction source in stockActions)
      {
        Stock stocksCacheAsync = await detailsViewModel.GetFromStocksCacheAsync(source.ActionStockId);
        Decimal num = source.ActionPrice / displayCurrencyConvertion.Multiplier * displayCurrencyConvertion.Divider;
        string displayCurrencyId = detailsViewModel.Details.DisplayCurrencyId;
        InvoiceLine newLine = detailsViewModel.CreateNewLine(stocksCacheAsync, new Decimal?(source.ActionExpense), stocksCacheAsync.UnitId, new Decimal?(num), displayCurrencyId);
        newLine.SourceId = source.ActionId;
        detailsViewModel.Details.Lines.Add(newLine);
        detailsViewModel.SelectedLine = newLine;
      }
      displayCurrencyConvertion = (CurrencyConvertion) null;
    }
  }

  public ICommand PrintCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnPrintCommandAsync), (Func<bool>) (() => !this.IsBusy && !this.IsDirty));
    }
  }

  protected virtual async Task OnPrintCommandAsync()
  {
    InvoiceDetailsViewModel detailsViewModel = this;
    IPrintingService printingService = detailsViewModel._printingService;
    Invoice details = detailsViewModel.Details;
    PartnerBalanceResult partnerBalanceToDate = detailsViewModel.PartnerBalanceToDate;
    Decimal balance = partnerBalanceToDate != null ? partnerBalanceToDate.Balance : 0M;
    await printingService.PrintInvoice(details, balance, true);
  }

  public bool CanCreateAggregatedStockOrder
  {
    get => this.Details != null && this.Details.InvoiceType == InvoiceType.Purchase;
  }

  public ICommand ToAggregatedStockOrderCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnToAggregatedStockOrderCommandAsync), (Func<bool>) (() => !this.IsBusy && !this.IsDirty && this.CanCreateAggregatedStockOrder));
    }
  }

  protected virtual Task OnToAggregatedStockOrderCommandAsync()
  {
    return this.NavigationService.Navigate<AggregatedStockOrderDetailsViewModel, AggregatedStockOrderDetailsViewModel.Params>(new AggregatedStockOrderDetailsViewModel.Params()
    {
      WarehouseId = this.Details.WarehouseId,
      StockIds = this.Details.Lines.Select<InvoiceLine, string>((Func<InvoiceLine, string>) (x => x.StockId))
    });
  }

  public bool CanShowNewPrices
  {
    get => this.Details != null && this.Details.InvoiceType == InvoiceType.Purchase;
  }

  public ICommand ShowNewPricesCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnShowNewPricesCommandAsync), (Func<bool>) (() => !this.IsBusy && !this.IsDirty && this.CanShowNewPrices));
    }
  }

  private Task OnShowNewPricesCommandAsync()
  {
    return this.NavigationService.Navigate<StockRepriceDialogViewModel, IEnumerable<StockRepriceRequest>>(this.Details.Lines.Select<InvoiceLine, StockRepriceRequest>((Func<InvoiceLine, StockRepriceRequest>) (x => new StockRepriceRequest()
    {
      StockId = x.StockId,
      ReferencePrice = x.Price,
      ReferencePriceCurrencyId = x.CurrencyId
    })));
  }

  public ICommand SelectOfficeCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectOfficeAsync), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess));
    }
  }

  private async Task OnSelectOfficeAsync()
  {
    InvoiceDetailsViewModel detailsViewModel = this;
    Invoice invoice = detailsViewModel.Details;
    invoice.OfficeId = await detailsViewModel.NavigationService.Navigate<ListViewModel<Office>, string, string>(detailsViewModel.Details.OfficeId ?? Guid.Empty.ToString());
    invoice = (Invoice) null;
  }
}
