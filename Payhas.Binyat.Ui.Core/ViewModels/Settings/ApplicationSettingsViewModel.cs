// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Settings.ApplicationSettingsViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Common.Settings;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.StockManagement.Services;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using Payhas.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Settings;

public class ApplicationSettingsViewModel : DialogViewModel
{
  private readonly IConfigurator _configurator;
  private readonly IStocksRepository _stocksRepository;
  private AppSettings _config;
  private string[] _priceGroupNames;

  public ApplicationSettingsViewModel(
    IConfigurator configurator,
    IMvxMessenger messenger,
    Reference<Office> officeReference,
    IStocksRepository stocksRepository,
    Reference<Currency> currenciesReference,
    Reference<Warehouse> warehouseReference,
    Reference<Depository> depositoryReference,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._configurator = configurator;
    this.Offices = officeReference;
    this.Currencies = currenciesReference;
    this.Warehouses = warehouseReference;
    this.Depositories = depositoryReference;
    this.Languages = new List<ListHelper<string>>()
    {
      new ListHelper<string>("en-US", "English"),
      new ListHelper<string>("ru-RU", "Русский"),
      new ListHelper<string>("tk-TM", "Türkmençe")
    };
    this._stocksRepository = stocksRepository;
  }

  public virtual AppSettings Config
  {
    get => this._config;
    set => this.SetProperty<AppSettings>(ref this._config, value, nameof (Config));
  }

  public Reference<Office> Offices { get; }

  public Reference<Currency> Currencies { get; }

  public Reference<Warehouse> Warehouses { get; set; }

  public Reference<Depository> Depositories { get; set; }

  public List<ListHelper<string>> Languages { get; set; }

  public virtual string[] PriceGroupNames
  {
    get => this._priceGroupNames;
    set => this.SetProperty<string[]>(ref this._priceGroupNames, value, nameof (PriceGroupNames));
  }

  protected async Task LoadFacetsAsync()
  {
    this.PriceGroupNames = (await this._stocksRepository.GetFacets("PriceGroupNames"))["PriceGroupNames"].Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (x => x.Key)).ToArray<string>();
  }

  protected override Task PreLoad()
  {
    return Task.WhenAll(base.PreLoad(), this.LoadFacetsAsync(), this.Offices.Initialize(), this.Currencies.Initialize(), this.Warehouses.Initialize(), this.Depositories.Initialize());
  }

  protected override async Task OnLoad()
  {
    this.Config = await this._configurator.GetConfigAsync<AppSettings>();
  }

  protected override async Task PostLoad()
  {
    await base.PostLoad();
    this.Offices.Filter = (Func<Office, bool>) (x => !x.IsDisabled);
    this.Currencies.Filter = (Func<Currency, bool>) (x => !x.IsDisabled);
    this.Warehouses.Filter = (Func<Warehouse, bool>) (x => !x.IsDisabled);
    this.Depositories.Filter = (Func<Depository, bool>) (x => !x.IsDisabled);
  }

  public ICommand SaveCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSaveAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private async Task OnSaveAsync()
  {
    ApplicationSettingsViewModel settingsViewModel = this;
    settingsViewModel.IsBusy = true;
    try
    {
      await settingsViewModel._configurator.SetConfigAsync<AppSettings>(settingsViewModel.Config);
      int num = await settingsViewModel.OnCloseAsync() ? 1 : 0;
    }
    catch (Exception ex)
    {
      settingsViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    settingsViewModel.IsBusy = false;
  }
}
