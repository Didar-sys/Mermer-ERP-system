// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Reporting.RevenueReportViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Mermer.Common.Settings;
using Mermer.Enterprise.Models;
using Mermer.Reporting.Models;
using Mermer.Reporting.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using Mermer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Reporting;

public class RevenueReportViewModel : BaseViewModel
{
  private readonly IConfigurator _configurator;
  private readonly IRevenueReportsRepository _repository;
  private IEnumerable<RevenueReport> _list;
  private System.Collections.Generic.List<object> _selectedWarehouseIds;
  private DateTime _dateFilterFrom = DateTime.Today;
  private DateTime _dateFilterTill = DateTime.Today;
  private bool _loaded;
    public object SelectedItem { get; set; }
    public System.Windows.Input.ICommand SelectOrViewDetailsCommand { get; set; }

    public RevenueReportViewModel(
    IConfigurator configurator,
    Reference<Warehouse> warehouses,
    IRevenueReportsRepository repository,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(navigationService, userInteractionService)
  {
    this._configurator = configurator;
    this._repository = repository;
    this.Warehouses = warehouses;
  }

  public Reference<Warehouse> Warehouses { get; }

  public virtual IEnumerable<RevenueReport> List
  {
    get => this._list;
    set => this.SetProperty<IEnumerable<RevenueReport>>(ref this._list, value, nameof (List));
  }

  public System.Collections.Generic.List<object> SelectedWarehouseIds
  {
    get => this._selectedWarehouseIds;
    set
    {
      if (!this.SetProperty<System.Collections.Generic.List<object>>(ref this._selectedWarehouseIds, value, nameof (SelectedWarehouseIds)))
        return;
      this.RaisePropertyChanged("WarehouseIds");
    }
  }

  public string[] WarehouseIds
  {
    get
    {
      System.Collections.Generic.List<object> selectedWarehouseIds = this.SelectedWarehouseIds;
      return (selectedWarehouseIds != null ? selectedWarehouseIds.Cast<string>().ToArray<string>() : (string[]) null) ?? Array.Empty<string>();
    }
  }

  public DateTime DateFilterFrom
  {
    get => this._dateFilterFrom;
    set => this.SetProperty<DateTime>(ref this._dateFilterFrom, value, nameof (DateFilterFrom));
  }

  public DateTime DateFilterTill
  {
    get => this._dateFilterTill;
    set => this.SetProperty<DateTime>(ref this._dateFilterTill, value, nameof (DateFilterTill));
  }

  public DateTime DateFilterTillInclusive
  {
    get
    {
      DateTime dateTime = this.DateFilterTill;
      dateTime = dateTime.AddDays(1.0);
      return dateTime.Date;
    }
  }

    protected override async Task PreLoad()
    {
        if (!_loaded && !WarehouseIds.Any())
        {
            AppSettings configAsync = await _configurator.GetConfigAsync<AppSettings>();
            SelectedWarehouseIds = new List<object> { configAsync.DefaultWarehouseId };
        }

        _loaded = true;

        await Task.WhenAll(
            base.PreLoad(),
            Warehouses.Initialize()
        );
    }

    protected override async Task OnLoad()
  {
    RevenueReportViewModel revenueReportViewModel = this;
    IEnumerable<RevenueReport> async = await revenueReportViewModel._repository.GetAsync(revenueReportViewModel.WarehouseIds, revenueReportViewModel.DateFilterFrom, revenueReportViewModel.DateFilterTillInclusive);
    revenueReportViewModel.List = async;
    revenueReportViewModel.SubCaption = $"{revenueReportViewModel.DateFilterFrom:MMM d} - {revenueReportViewModel.DateFilterTill:MMM d}";
  }

  public ICommand ReloadCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnReloadAsync), (Func<bool>) (() => !this.IsBusy && this.WarehouseIds.Length != 0));
    }
  }

  public virtual Task OnReloadAsync() => this.Initialize();
}
