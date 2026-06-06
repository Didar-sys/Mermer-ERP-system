// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Reporting.AggregatedReportViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Mermer.Common.Settings;
using Mermer.CRM.Models;
using Mermer.Enterprise.Models;
using Mermer.FundsManagement.Models;
using Mermer.Reporting.Models;
using Mermer.Reporting.Services;
using Mermer.StockManagement.Models;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.Services;
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

public class AggregatedReportViewModel : BaseViewModel
{
  private readonly IConfigurator _configurator;
  private readonly IPrintingService _printingService;
  private readonly IAggregatedReportsRepository _repository;
  private AggregatedReport _report;
  private List<object> _selectedOfficeIds;
  private DateTime _dateFilterFrom = DateTime.Today;
  private DateTime _dateFilterTill = DateTime.Today;
  private bool _loaded;

  public AggregatedReportViewModel(
    IConfigurator configurator,
    Reference<Office> offices,
    IPrintingService printingService,
    IAggregatedReportsRepository repository,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(navigationService, userInteractionService)
  {
    this._configurator = configurator;
    this._printingService = printingService;
    this._repository = repository;
    this.Offices = offices;
        this.Types = new LocalizedTransactionTypes("Repricing");
    }

  public Reference<Office> Offices { get; }

  public LocalizedTransactionTypes Types { get; }

  public virtual AggregatedReport Report
  {
    get => this._report;
    set => this.SetProperty<AggregatedReport>(ref this._report, value, nameof (Report));
  }

  public List<object> SelectedOfficeIds
  {
    get => this._selectedOfficeIds;
    set
    {
      if (!this.SetProperty<List<object>>(ref this._selectedOfficeIds, value, nameof (SelectedOfficeIds)))
        return;
      this.RaisePropertyChanged("OfficeIds");
    }
  }

  public string[] OfficeIds
  {
    get
    {
      List<object> selectedOfficeIds = this.SelectedOfficeIds;
      return (selectedOfficeIds != null ? selectedOfficeIds.Cast<string>().ToArray<string>() : (string[]) null) ?? Array.Empty<string>();
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
    if (!_loaded && !OfficeIds.Any())
    {
        var config = await _configurator.GetConfigAsync<AppSettings>();
        SelectedOfficeIds = new List<object> { config.DefaultOfficeId };
    }
    
    _loaded = true;
    
    await Task.WhenAll(
        base.PreLoad(), 
        Offices.Initialize()
    );
}

  protected override async Task OnLoad()
  {
    AggregatedReportViewModel aggregatedReportViewModel = this;
    AggregatedReport async = await aggregatedReportViewModel._repository.GetAsync(aggregatedReportViewModel.OfficeIds, aggregatedReportViewModel.DateFilterFrom, aggregatedReportViewModel.DateFilterTillInclusive);
    aggregatedReportViewModel.Report = async;
    aggregatedReportViewModel.SubCaption = $"{aggregatedReportViewModel.DateFilterFrom:MMM d} - {aggregatedReportViewModel.DateFilterTill:MMM d}";
  }

  public ICommand ReloadCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnReloadAsync), (Func<bool>) (() => !this.IsBusy && this.OfficeIds.Length != 0));
    }
  }

  public virtual Task OnReloadAsync() => this.Initialize();

  public ICommand PrintCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnPrintAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  public virtual Task OnPrintAsync()
  {
    return this._printingService.PrintAggregatedReport(new AggregatedReport()
    {
      FundsReport = new FundsBalanceAggregated()
      {
        Income = this.Report.FundsReport.Income,
        Expense = this.Report.FundsReport.Expense,
        StartingBalance = this.Report.FundsReport.StartingBalance,
        Lines = this.Report.FundsReport.Lines.Select<FundsBalanceAggregatedLine, FundsBalanceAggregatedLine>((Func<FundsBalanceAggregatedLine, FundsBalanceAggregatedLine>) (x => new FundsBalanceAggregatedLine()
        {
          Income = x.Income,
          Expense = x.Expense,
          Type = this.Types.List.Single<ListHelper<string>>((Func<ListHelper<string>, bool>) (i => i.Value == x.Type)).Text
        }))
      },
      StocksReport = new StockBalanceAggregated()
      {
        Income = this.Report.StocksReport.Income,
        Expense = this.Report.StocksReport.Expense,
        StartingBalance = this.Report.StocksReport.StartingBalance,
        Lines = this.Report.StocksReport.Lines.Select<StockBalanceAggregatedLine, StockBalanceAggregatedLine>((Func<StockBalanceAggregatedLine, StockBalanceAggregatedLine>) (x => new StockBalanceAggregatedLine()
        {
          Income = x.Income,
          Expense = x.Expense,
          Type = this.Types.List.Single<ListHelper<string>>((Func<ListHelper<string>, bool>) (i => i.Value == x.Type)).Text
        }))
      },
      PartnersReport = new PartnerBalanceAggregated()
      {
        Debit = this.Report.PartnersReport.Debit,
        Credit = this.Report.PartnersReport.Credit,
        StartingBalance = this.Report.PartnersReport.StartingBalance,
        Lines = this.Report.PartnersReport.Lines.Select<PartnerBalanceAggregatedLine, PartnerBalanceAggregatedLine>((Func<PartnerBalanceAggregatedLine, PartnerBalanceAggregatedLine>) (x => new PartnerBalanceAggregatedLine()
        {
          Debit = x.Debit,
          Credit = x.Credit,
          Type = this.Types.List.Single<ListHelper<string>>((Func<ListHelper<string>, bool>) (i => i.Value == x.Type)).Text
        }))
      }
    }, this.DateFilterFrom, this.DateFilterTill, this.Offices.List.Where<Office>((Func<Office, bool>) (x => ((IEnumerable<string>) this.OfficeIds).Contains<string>(x.Id))).Select<Office, string>((Func<Office, string>) (x => x.Name)).ToArray<string>());
  }
}
