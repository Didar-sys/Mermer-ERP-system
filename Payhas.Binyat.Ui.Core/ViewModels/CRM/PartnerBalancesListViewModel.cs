// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.CRM.PartnerBalancesListViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Common.Settings;
using Payhas.Binyat.CRM.Models;
using Payhas.Binyat.CRM.Services;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.FundsManagement.Models.Extenders;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.ViewModels.Common;
using Payhas.Mvvm.Services;
using Payhas.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.CRM;

public class PartnerBalancesListViewModel : 
  ListViewModelBaseWithFilterDate<PartnerBalanceByTypeWithBalance>
{
  private readonly IConfigurator _configurator;
  private readonly IPartnerBalancesRepository _repository;
  private System.Collections.Generic.List<object> _selectedOfficeIds;
  private string _partnerId;
  private string _currencyId;
  private bool _loaded;

  public PartnerBalancesListViewModel(
    IMvxMessenger messenger,
    IConfigurator configurator,
    Reference<Office> offices,
    Reference<Partner> partners,
    Reference<Currency> currencies,
    IPartnerBalancesRepository repository,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._configurator = configurator;
    this._repository = repository;
    this.Offices = offices;
    this.Partners = partners;
    this.Currencies = currencies;
  }

  public System.Collections.Generic.List<object> SelectedOfficeIds
  {
    get => this._selectedOfficeIds;
    set
    {
      if (this._selectedOfficeIds != null && value != null && this._selectedOfficeIds.SequenceEqual<object>((IEnumerable<object>) value) || !this.SetProperty<System.Collections.Generic.List<object>>(ref this._selectedOfficeIds, value, nameof (SelectedOfficeIds)) || this.IsBusy)
        return;
      this.Initialize();
    }
  }

  public string[] OfficeIds
  {
    get
    {
      System.Collections.Generic.List<object> selectedOfficeIds = this.SelectedOfficeIds;
      return (selectedOfficeIds != null ? selectedOfficeIds.Cast<string>().ToArray<string>() : (string[]) null) ?? Array.Empty<string>();
    }
  }

  public virtual string PartnerId
  {
    get => this._partnerId;
    set
    {
      if (!this.SetProperty<string>(ref this._partnerId, value, nameof (PartnerId)) || this.IsBusy)
        return;
      this.Initialize();
    }
  }

  public virtual string CurrencyId
  {
    get => this._currencyId;
    set
    {
      if (!this.SetProperty<string>(ref this._currencyId, value, nameof (CurrencyId)) || this.IsBusy)
        return;
      this.ApplyCustomCurrencyRate();
    }
  }

  public Reference<Office> Offices { get; }

  public Reference<Partner> Partners { get; }

  public Reference<Currency> Currencies { get; }

  protected override async Task PreLoad()
  {
    PartnerBalancesListViewModel balancesListViewModel = this;
    if (!balancesListViewModel._loaded && !((IEnumerable<string>) balancesListViewModel.OfficeIds).Any<string>())
    {
      AppSettings configAsync = await balancesListViewModel._configurator.GetConfigAsync<AppSettings>();
      balancesListViewModel.SelectedOfficeIds = new System.Collections.Generic.List<object>((IEnumerable<object>) new object[1]
      {
        (object) configAsync.DefaultOfficeId
      });
    }
    balancesListViewModel._loaded = true;
    await Task.WhenAll(balancesListViewModel.Offices.Initialize(), balancesListViewModel.Partners.Initialize(), balancesListViewModel.Currencies.Initialize());
    balancesListViewModel.CurrencyId = balancesListViewModel.Currencies.List.FirstOrDefault<Currency>((Func<Currency, bool>) (x => x.IsDefault)).Id;
  }

  protected override async Task<IEnumerable<PartnerBalanceByTypeWithBalance>> GetFilteredListByDateAsync(
    DateTime from,
    DateTime till)
  {
    return this.ApplyCustomCurrencyRate(await this._repository.GetByTypeAsync(from, till, this.PartnerId, this.OfficeIds));
  }

  protected override async Task<IEnumerable<PartnerBalanceByTypeWithBalance>> GetFilteredListAsync(
    ListFilter filter)
  {
    return this.ApplyCustomCurrencyRate(await this._repository.GetByTypeAsync(DateTime.MinValue, DateTime.MaxValue, this.PartnerId, this.OfficeIds));
  }

  private void ApplyCustomCurrencyRate() => this.List = this.ApplyCustomCurrencyRate(this.List);

  private IEnumerable<PartnerBalanceByTypeWithBalance> ApplyCustomCurrencyRate(
    IEnumerable<PartnerBalanceByTypeWithBalance> list)
  {
    Decimal rate = 0M;
    Currency currency = this.Currencies.List.SingleOrDefault<Currency>((Func<Currency, bool>) (x => x.Id == this._currencyId));
    CurrencyRate rate1 = currency != null ? currency.GetRate() : (CurrencyRate) null;
    if (rate1 != null)
      rate = rate1.Divider / rate1.Multiplier;
    return list.Select<PartnerBalanceByTypeWithBalance, PartnerBalanceByTypeWithBalance>((Func<PartnerBalanceByTypeWithBalance, PartnerBalanceByTypeWithBalance>) (item =>
    {
      item.ResultingBalanceInCustomCurrency = item.ResultingBalance * rate;
      return item;
    }));
  }

  public ICommand SelectOrViewDetailsCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectOrViewDetailsAsync), (Func<bool>) (() => !this.IsBusy && this.SelectedItem != null));
    }
  }

  private Task OnSelectOrViewDetailsAsync()
  {
    return this.NavigationService.Navigate<PartnerActionsListViewModel, PartnerActionsFilter>(new PartnerActionsFilter()
    {
      OfficeIds = new string[1]
      {
        this.SelectedItem.OfficeId
      },
      PartnerId = this.SelectedItem.PartnerId,
      DateFrom = this.DateFilterFrom,
      DateTill = this.DateFilterTill
    });
  }

  public ICommand ShowActionsCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnShowActionsAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private Task OnShowActionsAsync()
  {
    return this.NavigationService.Navigate<PartnerActionsListViewModel, PartnerActionsFilter>(new PartnerActionsFilter()
    {
      OfficeIds = this.OfficeIds,
      PartnerId = this.PartnerId,
      DateFrom = this.DateFilterFrom,
      DateTill = this.DateFilterTill
    });
  }

  protected override Expression<Func<PartnerBalanceByTypeWithBalance, bool>> GetDateFilter(
    DateTime from,
    DateTime till)
  {
    throw new NotImplementedException();
  }

  protected override Task<int> CountListAsync(
    params Expression<Func<PartnerBalanceByTypeWithBalance, bool>>[] predicates)
  {
    throw new NotImplementedException();
  }

  protected override Task<IEnumerable<PartnerBalanceByTypeWithBalance>> GetListAsync(
    params Expression<Func<PartnerBalanceByTypeWithBalance, bool>>[] predicates)
  {
    throw new NotImplementedException();
  }
}
