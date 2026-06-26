// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.StockManagement.StockRepriceEffectsListViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Mermer.Common.Settings;
using Mermer.Enterprise.Models;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.ViewModels.Common;
using Mermer.Mvvm.Services;
using Mermer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.StockManagement;

public class StockRepriceEffectsListViewModel : ListViewModelBaseWithFilterDate<StockRepriceEffect>
{
    private readonly IConfigurator _configurator;
    private readonly IStockRepriceEffectsRepository _repository;
    private IEnumerable<ListHelper<StockPriceChangeReason, string>> _priceChangeReasons;
    private System.Collections.Generic.List<object> _selectedWarehouseIds;
    private bool _initialized;

    public StockRepriceEffectsListViewModel(
      IMvxMessenger messenger,
      IConfigurator configurator,
      Reference<Warehouse> warehouses,
      IStockRepriceEffectsRepository repository,
      IMvxNavigationService navigationService,
      IUserInteractionService userInteractionService)
      : base(messenger, navigationService, userInteractionService)
    {
        this._configurator = configurator;
        this._repository = repository;
        this.Warehouses = warehouses;
        this.PriceChangeReasons = (IEnumerable<ListHelper<StockPriceChangeReason, string>>)new ListHelper<StockPriceChangeReason, string>[2]
        {
      new ListHelper<StockPriceChangeReason, string>(StockPriceChangeReason.PriceChanged, this["PriceChange", Array.Empty<object>()]),
      new ListHelper<StockPriceChangeReason, string>(StockPriceChangeReason.RateChanged, this["RateChange", Array.Empty<object>()])
        };
    }

    public IEnumerable<ListHelper<StockPriceChangeReason, string>> PriceChangeReasons
    {
        get => this._priceChangeReasons;
        set
        {
            this.SetProperty<IEnumerable<ListHelper<StockPriceChangeReason, string>>>(ref this._priceChangeReasons, value, nameof(PriceChangeReasons));
        }
    }

    public Reference<Warehouse> Warehouses { get; }

    public System.Collections.Generic.List<object> SelectedWarehouseIds
    {
        get => this._selectedWarehouseIds;
        set
        {
            if (this._selectedWarehouseIds != null && value != null && this._selectedWarehouseIds.SequenceEqual<object>((IEnumerable<object>)value) || !this.SetProperty<System.Collections.Generic.List<object>>(ref this._selectedWarehouseIds, value, nameof(SelectedWarehouseIds)) || this.IsBusy)
                return;
            this.Initialize();
        }
    }


    public System.Windows.Input.ICommand SelectOrViewDetailsCommand => new MvvmCross.Core.ViewModels.MvxCommand(() =>
    {
        if (SelectedItem != null)
        {
            // Резерв під деталі
        }
    });

    public string[] WarehouseIds
    {
        get
        {
            var selectedWarehouseIds = this.SelectedWarehouseIds;

            // Якщо список пустий (користувач прибрав усі галочки), 
            // повертаємо null, щоб база відключила фільтр по складах і віддала ВСЕ.
            if (selectedWarehouseIds == null || selectedWarehouseIds.Count == 0)
            {
                return null;
            }

            return selectedWarehouseIds.Select(x => x?.ToString()).ToArray();
        }
    }

    protected override Task PreLoad()
    {
        if (!this._initialized)
        {
            this.SelectedWarehouseIds = new System.Collections.Generic.List<object>((IEnumerable<object>)new object[1]
            {
        (object) this._configurator.GetConfig<AppSettings>().DefaultWarehouseId
            });
            this._initialized = true;
        }
        return Task.WhenAll(base.PreLoad(), this.Warehouses.Initialize());
    }

    protected override Task<int> CountFilteredListAsync(ListFilter filter)
    {
        return this._repository.CountAsync(DateTime.MinValue, DateTime.MaxValue);
    }

    protected override Task<int> CountFilteredListByDateAsync(DateTime from, DateTime till)
    {
        return this._repository.CountAsync(from, till);
    }

    protected override Task<IEnumerable<StockRepriceEffect>> GetFilteredListAsync(ListFilter filter)
    {
        return this._repository.GetAsync(DateTime.MinValue, DateTime.MaxValue, this.WarehouseIds);
    }

    protected override Task<IEnumerable<StockRepriceEffect>> GetFilteredListByDateAsync(
      DateTime from,
      DateTime till)
    {
        return this._repository.GetAsync(from, till, this.WarehouseIds);
    }

    protected override Task<int> CountListAsync(params Expression<Func<StockRepriceEffect, bool>>[] predicates)
    {
        // Рахуємо всі записи за весь час
        return this._repository.CountAsync(DateTime.MinValue, DateTime.MaxValue);
    }

    protected override Task<IEnumerable<StockRepriceEffect>> GetListAsync(params Expression<Func<StockRepriceEffect, bool>>[] predicates)
    {
        // Дістаємо всі записи за весь час з урахуванням фільтру складів
        return this._repository.GetAsync(DateTime.MinValue, DateTime.MaxValue, this.WarehouseIds);
    }

    protected override Expression<Func<StockRepriceEffect, bool>> GetDateFilter(DateTime from, DateTime till)
    {
        // Повертаємо умову: Дата зміни повинна бути між From та Till
        return effect => effect.ChangeDate >= from && effect.ChangeDate <= till;
    }
}