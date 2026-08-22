// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.StockManagement.StockTurnoverDataListViewModel
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
using Mermer.Data.Tools.Expressions;
using Mermer.Mvvm.Services;
using Mermer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.StockManagement;

public class StockTurnoverDataListViewModel : ListViewModelBaseWithFilter<StockTurnoverData>
{
  protected const string FilterNotSellingString = "Not Selling";
  protected const string FilterBadSellingString = "Bad Selling";
  protected const string FilterNormalSellingString = "Normal Selling";
  protected const string FilterGoodSellingString = "Good Selling";
  private readonly IConfigurator _configurator;
  private readonly IStockTurnoverDataRepository _repository;
  private string _warehouseId;
  private bool _initialized;

    public System.Windows.Input.ICommand SelectOrViewDetailsCommand => new MvvmCross.Core.ViewModels.MvxCommand(() =>
    {
        if (SelectedItem != null)
        {
            // Резерв под предстоящее открытие деталей
        }
    });
    public StockTurnoverDataListViewModel(
      IMvxMessenger messenger,
      IConfigurator configurator,
      Reference<Warehouse> warehouses,
      IStockTurnoverDataRepository repository,
      IMvxNavigationService navigationService,
      IUserInteractionService userInteractionService)
      : base(messenger, navigationService, userInteractionService)
    {
        Warehouses = warehouses;
        _configurator = configurator;
        _repository = repository;

        Filters = new[]
        {
        new ListFilter
        {
            Title = this["Not Selling"],
            Tag = "Not Selling",
            CanLoad = x => !IsBusy,
            Loader = x => LoadByFilterAsync(x),
            Counter = CountByFilterAsync
        },
        new ListFilter
        {
            Title = this["Bad Selling"],
            Tag = "Bad Selling",
            CanLoad = x => !IsBusy,
            Loader = x => LoadByFilterAsync(x),
            Counter = CountByFilterAsync
        },
        new ListFilter
        {
            Title = this["Normal Selling"],
            Tag = "Normal Selling",
            CanLoad = x => !IsBusy,
            Loader = x => LoadByFilterAsync(x),
            Counter = CountByFilterAsync
        },
        new ListFilter
        {
            Title = this["Good Selling"],
            Tag = "Good Selling",
            CanLoad = x => !IsBusy,
            Loader = x => LoadByFilterAsync(x),
            Counter = CountByFilterAsync
        },
        new ListFilter
        {
            Title = this["All Records"],
            Tag = "All", 
            CanLoad = x => !IsBusy,
            Loader = x => LoadByFilterAsync(x),
            Counter = CountByFilterAsync
        }
    };
    }

    public Reference<Warehouse> Warehouses { get; }

  public IEnumerable<StockTurnoverData> AllItems { get; set; }

  public virtual string WarehouseId
  {
    get => this._warehouseId;
    set
    {
      if (!this.SetProperty<string>(ref this._warehouseId, value, nameof (WarehouseId)) || this.IsBusy)
        return;
      this.Initialize();
    }
  }

  protected override Task PreLoad()
  {
    if (!this._initialized)
      this.WarehouseId = this._configurator.GetConfig<AppSettings>()?.DefaultWarehouseId;
    this._initialized = true;
    return (Task) this.PreLoadList().ContinueWith<Task>((Func<Task, Task>) (t => Task.WhenAll(base.PreLoad(), this.Warehouses.Initialize())));
  }

    protected async Task PreLoadList()
    {
        this.AllItems = await this._repository.GetAsync(this.WarehouseId);

        // ИСПРАВЛЕНИЕ: Защита от null
        if (this.AllItems == null)
        {
            this.AllItems = new List<StockTurnoverData>();
        }
    }

    protected override PredicateBuilder<StockTurnoverData> GetPredicateBuilder(ListFilter filter)
  {
    PredicateBuilder<StockTurnoverData> predicateBuilder = base.GetPredicateBuilder(filter);
    if (filter.Tag is string tag)
    {
      switch (tag)
      {
        case "Not Selling":
          predicateBuilder.Add((Expression<Func<StockTurnoverData, bool>>) (x => x.Turnover == 0));
          break;
        case "Bad Selling":
          predicateBuilder.Add((Expression<Func<StockTurnoverData, bool>>) (x => x.Turnover <= 30));
          break;
        case "Normal Selling":
          predicateBuilder.Add((Expression<Func<StockTurnoverData, bool>>) (x => x.Turnover > 30 && x.Turnover < 70));
          break;
        case "Good Selling":
          predicateBuilder.Add((Expression<Func<StockTurnoverData, bool>>) (x => x.Turnover >= 70));
          break;
      }
    }
    return predicateBuilder;
  }

    // Обновите методы подсчета, чтобы они не падали, если AllItems оказался null в процессе работы:
    protected override Task<int> CountListAsync(params Expression<Func<StockTurnoverData, bool>>[] predicates)
    {
        var safeItems = this.AllItems ?? Enumerable.Empty<StockTurnoverData>();
        return Task.Run(() => predicates.Aggregate(safeItems, (current, filter) => current.Where(filter.Compile())).Count());
    }

    protected override Task<IEnumerable<StockTurnoverData>> GetListAsync(params Expression<Func<StockTurnoverData, bool>>[] predicates)
    {
        var safeItems = this.AllItems ?? Enumerable.Empty<StockTurnoverData>();
        return Task.Run(() => predicates.Aggregate(safeItems, (current, filter) => current.Where(filter.Compile())));
    }
}
