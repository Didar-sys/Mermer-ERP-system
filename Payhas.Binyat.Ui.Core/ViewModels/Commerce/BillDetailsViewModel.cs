// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Commerce.BillDetailsViewModel
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
using Payhas.Binyat.Transactions.Models;
using Payhas.Binyat.Transactions.Services;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.Services;
using Payhas.Binyat.Ui.Core.ViewModels.Transactions;
using Payhas.Data.Authorizers;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using Payhas.Services;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Commerce;

public class BillDetailsViewModel : 
  FundsTransactionDetailsViewModel<Bill, BillLine, BillType>,
  IMvxViewModel<BillType>,
  IMvxViewModel
{
  private readonly IPrintingService _printingService;
  private readonly IPartnerBalancesRepository _partnerBalancesRepository;
  private BillType _newSlipType;
  private PartnerBalanceResult _partnerBalanceToDate;

  public BillDetailsViewModel(
    IRepository<Bill> repository,
    IListAuthorizer<Bill> authorizer,
    IConfigurator configurator,
    ILoginService loginService,
    Reference<Office> offices,
    Reference<Partner> partners,
    Reference<Currency> currencies,
    IPrintingService printingService,
    Reference<Depository> depositories,
    IMvxNavigationService navigationService,
    ITransactionCodeGenerationService codegentor,
    IUserInteractionService userInteractionService,
    IPartnerBalancesRepository partnerBalancesRepository)
    : base(repository, authorizer, configurator, loginService, currencies, depositories, navigationService, codegentor, userInteractionService)
  {
    this._printingService = printingService;
    this._partnerBalancesRepository = partnerBalancesRepository;
    this.Offices = offices;
    this.Partners = partners;
  }

  public Reference<Office> Offices { get; }

  public Reference<Partner> Partners { get; }

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

  public void Prepare(BillType parameter) => this._newSlipType = parameter;

  protected override Task PreLoad()
  {
    return Task.WhenAll(base.PreLoad(), this.Offices.Initialize(), this.Partners.Initialize());
  }

  protected override async Task OnLoad()
  {
    BillDetailsViewModel detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__0();
    if (!string.IsNullOrEmpty(detailsViewModel.ItemId))
      return;
    detailsViewModel.Details.OfficeId = detailsViewModel.AppSettings.DefaultOfficeId;
  }

  protected override async Task PostLoad()
  {
    BillDetailsViewModel detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__1();
    if (string.IsNullOrEmpty(detailsViewModel.ItemId))
      detailsViewModel.Details.BillType = detailsViewModel._newSlipType;
    detailsViewModel.UpdatePartnerBalance();
    detailsViewModel.Details.RaisePropertyChanged("DisplayDebitCreditTotal");
    detailsViewModel.Details.RaisePropertyChanged("DisplayCreditTotal");
    detailsViewModel.Details.RaisePropertyChanged("DisplayDebitTotal");
    // ISSUE: explicit non-virtual call
    __nonvirtual (detailsViewModel.RaisePropertyChanged<PartnerBalanceResult>((Expression<Func<PartnerBalanceResult>>) (() => detailsViewModel.PartnerBalanceToDate)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (detailsViewModel.RaisePropertyChanged<PartnerBalanceResult>((Expression<Func<PartnerBalanceResult>>) (() => detailsViewModel.PartnerBalanceResult)));
    // ISSUE: reference to a compiler-generated method
    detailsViewModel.Partners.Filter = new Func<Partner, bool>(detailsViewModel.\u003CPostLoad\u003Eb__19_2);
    // ISSUE: reference to a compiler-generated method
    detailsViewModel.Offices.Filter = new Func<Office, bool>(detailsViewModel.\u003CPostLoad\u003Eb__19_3);
    detailsViewModel.UpdateFacilityFilters();
  }

  protected override async Task<bool> OnSaveAsync()
  {
    BillDetailsViewModel detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    if (!await detailsViewModel.\u003C\u003En__2())
      return false;
    IPrintingService printingService = detailsViewModel._printingService;
    Bill details = detailsViewModel.Details;
    PartnerBalanceResult partnerBalanceToDate = detailsViewModel.PartnerBalanceToDate;
    Decimal balance = partnerBalanceToDate != null ? partnerBalanceToDate.Balance : 0M;
    await printingService.PrintBill(details, balance);
    return true;
  }

  protected override void Details_PropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    base.Details_PropertyChanged(sender, e);
    if (e.PropertyName == "Date" || e.PropertyName == "PartnerId" || e.PropertyName == "DepositoryId" || e.PropertyName == "DisplayCurrencyId")
      this.UpdatePartnerBalance();
    else if (e.PropertyName == "DisplayDebitCreditTotal")
    {
      this.RaisePropertyChanged<PartnerBalanceResult>((Expression<Func<PartnerBalanceResult>>) (() => this.PartnerBalanceResult));
    }
    else
    {
      if (!(e.PropertyName == "OfficeId"))
        return;
      this.UpdateFacilityFilters();
    }
  }

  private void UpdateFacilityFilters()
  {
    this.Depositories.Filter = (Func<Depository, bool>) (x =>
    {
      if (!(x.OfficeId == this.Details?.OfficeId))
        return false;
      return !x.IsDisabled || x.Id == this.Details?.DepositoryId;
    });
  }

  private async void UpdatePartnerBalance()
  {
    BillDetailsViewModel detailsViewModel = this;
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

  public ICommand SelectPartnerCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectPartnerCommandAsync), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess));
    }
  }

  private async Task OnSelectPartnerCommandAsync()
  {
    BillDetailsViewModel detailsViewModel = this;
    Bill bill = detailsViewModel.Details;
    bill.PartnerId = await detailsViewModel.NavigationService.Navigate<ListViewModel<Partner>, string, string>(detailsViewModel.Details.PartnerId ?? Guid.Empty.ToString());
    bill = (Bill) null;
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
    BillDetailsViewModel detailsViewModel = this;
    IPrintingService printingService = detailsViewModel._printingService;
    Bill details = detailsViewModel.Details;
    PartnerBalanceResult partnerBalanceToDate = detailsViewModel.PartnerBalanceToDate;
    Decimal balance = partnerBalanceToDate != null ? partnerBalanceToDate.Balance : 0M;
    await printingService.PrintBill(details, balance, true);
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
    BillDetailsViewModel detailsViewModel = this;
    Bill bill = detailsViewModel.Details;
    bill.OfficeId = await detailsViewModel.NavigationService.Navigate<ListViewModel<Office>, string, string>(detailsViewModel.Details.OfficeId ?? Guid.Empty.ToString());
    bill = (Bill) null;
  }
}
