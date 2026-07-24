// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Warehousing.StockSlipDetailsViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using Mermer.Authorization.Services;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Enterprise.Models;
using Mermer.FundsManagement.Models;
using Mermer.Mvvm.Services;
using Mermer.Services;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Models.Extenders;
using Mermer.StockManagement.Services;
using Mermer.Transactions.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.Services;
using Mermer.Ui.Core.ViewModels.Common;
using Mermer.Ui.Core.ViewModels.Transactions;
using Mermer.Warehousing.Models;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Warehousing;

public class StockSlipDetailsViewModel : 
  StockTransactionDetailsViewModel<StockSlip, StockSlipLine, StockSlipType>,
  IMvxViewModel<StockSlipType>,
  IMvxViewModel
{
  private StockSlipType _newSlipType = StockSlipType.StockUsage;
  private readonly IPrintingService _printingService;

  public StockSlipDetailsViewModel(
    CopyCreate copyCreate,
    IConfigurator configurator,
    ILoginService loginService,
    StockSearcher stockSearcher,
    Reference<Currency> currencies,
    Reference<Warehouse> warehouses,
    IPrintingService printingService,
    IRepository<StockSlip> repository,
    IListAuthorizer<StockSlip> authorizer,
    IStocksRepository stocksRepository,
    IMvxNavigationService navigationService,
    ITransactionCodeGenerationService codegentor,
    IUserInteractionService userInteractionService)
    : base(copyCreate, repository, authorizer, configurator, loginService, stockSearcher, currencies, warehouses, stocksRepository, navigationService, codegentor, userInteractionService)
  {
    this._printingService = printingService;
  }

  public void Prepare(StockSlipType parameter) => this._newSlipType = parameter;

    protected override async Task PostLoad()
    {
        await base.PostLoad();
        if (!string.IsNullOrEmpty(ItemId))
            return;
        Details.SlipType = _newSlipType;
    }

    protected override async Task<bool> OnSaveAsync()
    {
        try
        {
            
            if (string.IsNullOrEmpty(Details.WarehouseId))
            {
                throw new Exception(this["Field '{0}' is required", this["Warehouse"]]);
            }

  
            if (Details.Lines == null || !Details.Lines.Any())
            {
                throw new Exception(this["Document cannot be empty"]);
            }

            // Валидация каждой строки накладной
            foreach (var line in Details.Lines)
            {
                
                if (line.Quantity <= 0)
                    throw new Exception(this["Quantity must be greater than zero"]);

                
                if (Details.IsPriceEditable && line.Price < 0)
                    throw new Exception(this["Price cannot be negative"]);

                if (string.IsNullOrEmpty(line.CurrencyId))
                    throw new Exception(this["Field '{0}' is required", this["Currency"]]);
            }
        }
        catch (Exception ex)
        {
           
            UserInteractionService.ShowExceptionMessage(ex);
            return false;
        }

        
        _newSlipType = Details.SlipType;
        if (!await base.OnSaveAsync())
            return false;

        await _printingService.PrintStockSlip(Details);
        return true;
    }

    protected override void Details_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        base.Details_PropertyChanged(sender, e);

        if (e.PropertyName == "DisplayCurrencyId")
        {
            var newCurrencyId = this.Details.DisplayCurrencyId;

            // 1. ТРЕБОВАНИЕ ЛЕОНА: Обновляем валюту в поисковике товаров
            if (StockSearcher != null && !string.IsNullOrEmpty(newCurrencyId))
            {
                // Используем Reflection для надежности, если свойство скрыто
                var prop = StockSearcher.GetType().GetProperty("CurrencyId");
                if (prop != null)
                {
                    prop.SetValue(StockSearcher, newCurrencyId);
                }
            }

            // 2. ЛЕЧИМ РАССИНХРОН: Делаем микро-паузу для загрузки курсов
            System.Threading.Tasks.Task.Run(async () =>
            {
                // Ждем 150 миллисекунд, пока ядро ERP подтянет CurrencyConvertions
                await System.Threading.Tasks.Task.Delay(150);

                // Возвращаемся в главный поток интерфейса
                InvokeOnMainThread(() =>
                {
                    if (this.Details.Lines != null)
                    {
                        foreach (var line in this.Details.Lines)
                        {
                            // Обновляем каждую ячейку
                            line.RaisePropertyChanged("DisplayTotal");
                        }
                    }

                    // Принудительно обновляем итог документа
                    this.Details.RaisePropertyChanged("DisplayTotal");

                    // Эта магическая строка заставляет DevExpress GridControl мгновенно пересчитать TotalSummary внизу экрана!
                    this.Details.RaisePropertyChanged("Lines");
                });
            });
        }

        // Существующая логика для расчета цен остается без изменений
        if (!(e.PropertyName == "IsPriceEditable") || this.Details.IsPriceEditable)
            return;

        foreach (StockSlipLine line in (System.Collections.ObjectModel.Collection<StockSlipLine>)this.Details.Lines)
        {
            Stock fromStocksCache = this.GetFromStocksCache(line.StockId);
            Decimal num = line.ActionQuantity / line.Quantity;
            DateTime? date = new DateTime?(this.Details.Date);
            StockPrice price = fromStocksCache.GetPrice(date);
            line.Price = price.Price * num;
            line.CurrencyId = price.CurrencyId;
        }
    }

    protected override async Task OnSelectedLineEditAsync()
  {
    StockSlipDetailsViewModel detailsViewModel = this;
    Stock stocksCacheAsync = await detailsViewModel.GetFromStocksCacheAsync(detailsViewModel.SelectedLine.StockId);
    IMvxNavigationService navigationService = detailsViewModel.NavigationService;
    StockSlipDetailsLineEditViewModel.Params @params = new StockSlipDetailsLineEditViewModel.Params();
    @params.StockCode = stocksCacheAsync.Code;
    @params.StockName = stocksCacheAsync.Name;
    @params.Quantity = detailsViewModel.SelectedLine.Quantity;
    @params.UnitId = detailsViewModel.SelectedLine.UnitId;
    @params.Units = (IEnumerable<StockUnit>) stocksCacheAsync.Units;
    @params.Price = detailsViewModel.SelectedLine.Price;
    @params.CurrencyId = detailsViewModel.SelectedLine.CurrencyId;
    @params.Currencies = detailsViewModel.Currencies.List;
    @params.IsPriceEditable = detailsViewModel.Details.IsPriceEditable;
    CancellationToken cancellationToken = new CancellationToken();
    StockTransactionDetailsLineEditViewModel.Result result = await navigationService.Navigate<StockSlipDetailsLineEditViewModel, StockSlipDetailsLineEditViewModel.Params, StockTransactionDetailsLineEditViewModel.Result>(@params, cancellationToken: cancellationToken);
    if (result == null)
      return;
    detailsViewModel.SelectedLine.Quantity = result.Quantity;
    detailsViewModel.SelectedLine.UnitId = result.UnitId;
    if (!detailsViewModel.Details.IsPriceEditable)
      return;
    detailsViewModel.SelectedLine.Price = result.Price;
    detailsViewModel.SelectedLine.CurrencyId = result.CurrencyId;
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
    StockSlipDetailsViewModel detailsViewModel = this;
    await detailsViewModel._printingService.PrintStockSlip(detailsViewModel.Details, true);
  }
}
