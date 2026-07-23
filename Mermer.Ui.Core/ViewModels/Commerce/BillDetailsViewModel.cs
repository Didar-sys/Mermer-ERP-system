using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Mermer.Authorization.Services;
using Mermer.Commerce.Models;
using Mermer.CRM.Models;
using Mermer.CRM.Services;
using Mermer.Enterprise.Models;
using Mermer.FundsManagement.Models;
using Mermer.Transactions.Models;
using Mermer.Transactions.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.Services;
using Mermer.Ui.Core.ViewModels.Transactions;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using Mermer.Services;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mermer.Ui.Core.ViewModels.Commerce;

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
        _printingService = printingService;
        _partnerBalancesRepository = partnerBalancesRepository;
        Offices = offices;
        Partners = partners;
    }

    public Reference<Office> Offices { get; }
    public Reference<Partner> Partners { get; }

    public virtual PartnerBalanceResult PartnerBalanceToDate
    {
        get => _partnerBalanceToDate;
        set
        {
            SetProperty(ref _partnerBalanceToDate, value);
            RaisePropertyChanged(() => PartnerBalanceResult);
        }
    }

    public virtual PartnerBalanceResult PartnerBalanceResult
    {
        get
        {
            if (PartnerBalanceToDate == null)
                return null;
            return new PartnerBalanceResult
            {
                Balance = PartnerBalanceToDate.Balance + Details.DisplayDebitCreditTotal
            };
        }
    }

    public void Prepare(BillType parameter) => _newSlipType = parameter;

    protected override Task PreLoad()
    {
        return Task.WhenAll(base.PreLoad(), Offices.Initialize(), Partners.Initialize());
    }

    protected override async Task OnLoad()
    {
        await base.OnLoad();
        if (!string.IsNullOrEmpty(ItemId))
            return;
        Details.OfficeId = AppSettings.DefaultOfficeId;
    }

    protected override async Task PostLoad()
    {
        await base.PostLoad();

        if (string.IsNullOrEmpty(ItemId))
            Details.BillType = _newSlipType;

        UpdatePartnerBalance();

        Details.RaisePropertyChanged("DisplayDebitCreditTotal");
        Details.RaisePropertyChanged("DisplayCreditTotal");
        Details.RaisePropertyChanged("DisplayDebitTotal");

        RaisePropertyChanged(() => PartnerBalanceToDate);
        RaisePropertyChanged(() => PartnerBalanceResult);

        Partners.Filter = x => !x.IsDisabled || x.Id == Details?.PartnerId;
        Offices.Filter = x => !x.IsDisabled || x.Id == Details?.OfficeId;

        UpdateFacilityFilters();
    }

    protected override async Task<bool> OnSaveAsync()
    {
        try
        {
            // ПЕРЕВІРКА: Якщо PartnerId порожній, блокуємо збереження
            if (string.IsNullOrEmpty(Details.PartnerId))
            {
                throw new Exception(this["Field '{0}' is required", this["Partner"]]);
            }
        }
        catch (Exception ex)
        {
            // Виводимо повідомлення користувачу та зупиняємо процес збереження
            UserInteractionService.ShowExceptionMessage(ex);
            return false;
        }

        // Якщо перевірку пройдено, виконуємо стандартне збереження
        if (!await base.OnSaveAsync())
            return false;

        decimal balance = PartnerBalanceToDate?.Balance ?? 0M;
        await _printingService.PrintBill(Details, balance);
        return true;
    }

    protected override void Details_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        base.Details_PropertyChanged(sender, e);

        if (e.PropertyName == "Date" || e.PropertyName == "PartnerId" || e.PropertyName == "DepositoryId" || e.PropertyName == "DisplayCurrencyId")
            UpdatePartnerBalance();
        else if (e.PropertyName == "DisplayDebitCreditTotal")
            RaisePropertyChanged(() => PartnerBalanceResult);
        else if (e.PropertyName == "OfficeId")
            UpdateFacilityFilters();
    }

    private void UpdateFacilityFilters()
    {
        Depositories.Filter = x =>
        {
            if (x.OfficeId != Details?.OfficeId) return false;
            return !x.IsDisabled || x.Id == Details?.DepositoryId;
        };
    }

    private async void UpdatePartnerBalance()
    {
        if (!string.IsNullOrEmpty(Details?.PartnerId) && !string.IsNullOrEmpty(Details?.OfficeId) && Details?.CurrencyConvertions != null)
        {
            var balanceToDateAsync = await _partnerBalancesRepository.GetBalanceToDateAsync(Details.OfficeId, Details.PartnerId, Details.Date, Details.Id);
            var currencyConvertion = CurrencyConverter(Details.DisplayCurrencyId);

            PartnerBalanceToDate = new PartnerBalanceResult
            {
                Balance = balanceToDateAsync.Balance / currencyConvertion.Multiplier * currencyConvertion.Divider
            };
        }
        else
        {
            PartnerBalanceToDate = null;
        }
    }

    public ICommand SelectPartnerCommand => new MvxAsyncCommand(OnSelectPartnerCommandAsync, () => !IsBusy && HasSaveAccess);

    private async Task OnSelectPartnerCommandAsync()
    {
        Details.PartnerId = await NavigationService.Navigate<ListViewModel<Partner>, string, string>(Details.PartnerId ?? Guid.Empty.ToString());
    }

    public ICommand PrintCommand => new MvxAsyncCommand(OnPrintCommandAsync, () => !IsBusy && !IsDirty);

    protected virtual async Task OnPrintCommandAsync()
    {
        decimal balance = PartnerBalanceToDate?.Balance ?? 0M;
        await _printingService.PrintBill(Details, balance, true);
    }

    public ICommand SelectOfficeCommand => new MvxAsyncCommand(OnSelectOfficeAsync, () => !IsBusy && HasSaveAccess);

    private async Task OnSelectOfficeAsync()
    {
        Details.OfficeId = await NavigationService.Navigate<ListViewModel<Office>, string, string>(Details.OfficeId ?? Guid.Empty.ToString());
    }
}