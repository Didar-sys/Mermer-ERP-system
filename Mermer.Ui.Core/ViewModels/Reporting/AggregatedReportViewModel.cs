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
    public object SelectedItem { get; set; }
    public System.Windows.Input.ICommand SelectOrViewDetailsCommand { get; set; }

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
            var selectedOfficeIds = this.SelectedOfficeIds;
            return selectedOfficeIds != null
                ? selectedOfficeIds.Where(x => x != null).Select(x => x.ToString()).ToArray()
                : Array.Empty<string>();
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
        if (!_loaded && (SelectedOfficeIds == null || !SelectedOfficeIds.Any()))
        {
            var config = await _configurator.GetConfigAsync<AppSettings>();
            if (config != null && !string.IsNullOrEmpty(config.DefaultOfficeId))
            {
                SelectedOfficeIds = new List<object> { config.DefaultOfficeId };
            }
            else
            {
                SelectedOfficeIds = new List<object>(); // Инициализируем пустым, если офиса нет
            }
        }

        _loaded = true;

        await Task.WhenAll(
            base.PreLoad(),
            Offices.Initialize()
        );
    }

    protected override async Task OnLoad()
    {
        try
        {
            // Безопасный вызов репозитория
            var safeOfficeIds = this.OfficeIds ?? Array.Empty<string>();
            var fetchedReport = await this._repository.GetAsync(safeOfficeIds, this.DateFilterFrom, this.DateFilterTillInclusive);

            // Если база вернула null, создаем пустой каркас отчета, чтобы XAML не упал
            this.Report = fetchedReport ?? new AggregatedReport();

            this.SubCaption = $"{this.DateFilterFrom:MMM d} - {this.DateFilterTill:MMM d}";
        }
        catch (Exception ex)
        {
            // Если ошибка произойдет глубоко в базе, мы ее перехватим и не дадим крашнуть окно
            this.Report = new AggregatedReport();
            this.UserInteractionService.ShowExceptionMessage(ex);
        }
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
        // Якщо самого звіту взагалі немає - тоді точно нічого друкувати
        if (Report == null) return Task.CompletedTask;

        // БЕЗПЕЧНО збираємо касу (якщо null - створюємо порожній)
        var safeFundsReport = Report.FundsReport == null ? new FundsBalanceAggregated() : new FundsBalanceAggregated
        {
            Income = Report.FundsReport.Income,
            Expense = Report.FundsReport.Expense,
            StartingBalance = Report.FundsReport.StartingBalance,
            Lines = Report.FundsReport.Lines?.Select(x => new FundsBalanceAggregatedLine
            {
                Income = x.Income,
                Expense = x.Expense,
                // FirstOrDefault не видасть помилку, якщо типу немає в словнику
                Type = Types?.List?.FirstOrDefault(i => i.Value == x.Type)?.Text ?? x.Type
            }) ?? Enumerable.Empty<FundsBalanceAggregatedLine>()
        };

        // БЕЗПЕЧНО збираємо склади
        var safeStocksReport = Report.StocksReport == null ? new StockBalanceAggregated() : new StockBalanceAggregated
        {
            Income = Report.StocksReport.Income,
            Expense = Report.StocksReport.Expense,
            StartingBalance = Report.StocksReport.StartingBalance,
            Lines = Report.StocksReport.Lines?.Select(x => new StockBalanceAggregatedLine
            {
                Income = x.Income,
                Expense = x.Expense,
                Type = Types?.List?.FirstOrDefault(i => i.Value == x.Type)?.Text ?? x.Type
            }) ?? Enumerable.Empty<StockBalanceAggregatedLine>()
        };

        // БЕЗПЕЧНО збираємо партнерів
        var safePartnersReport = Report.PartnersReport == null ? new PartnerBalanceAggregated() : new PartnerBalanceAggregated
        {
            Debit = Report.PartnersReport.Debit,
            Credit = Report.PartnersReport.Credit,
            StartingBalance = Report.PartnersReport.StartingBalance,
            Lines = Report.PartnersReport.Lines?.Select(x => new PartnerBalanceAggregatedLine
            {
                Debit = x.Debit,
                Credit = x.Credit,
                Type = Types?.List?.FirstOrDefault(i => i.Value == x.Type)?.Text ?? x.Type
            }) ?? Enumerable.Empty<PartnerBalanceAggregatedLine>()
        };

        // Безпечно формуємо список офісів
        var selectedOffices = Offices?.List?.Where(x => OfficeIds != null && OfficeIds.Contains(x.Id)).Select(x => x.Name).ToArray() ?? Array.Empty<string>();

        // Відправляємо зібрані безпечні блоки на друк
        return _printingService.PrintAggregatedReport(new AggregatedReport
        {
            FundsReport = safeFundsReport,
            StocksReport = safeStocksReport,
            PartnersReport = safePartnersReport
        }, DateFilterFrom, DateFilterTill, selectedOffices);
    }
}
