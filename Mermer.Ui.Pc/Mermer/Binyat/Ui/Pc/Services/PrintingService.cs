// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Services.PrintingService
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using DevExpress.DataAccess.ObjectBinding;
using DevExpress.Xpf.Printing;
using DevExpress.XtraReports;
using DevExpress.XtraReports.UI;
using MvvmCross.Core.Navigation;
using Mermer.Commerce.Models;
using Mermer.Common.Services;
using Mermer.Common.Settings;
using Mermer.Finance.Spending.Models;
using Mermer.Reporting.Models;
using Mermer.Ui.Core.Services;
using Mermer.Ui.Core.ViewModels.Common;
using Mermer.Ui.Core.ViewModels.Warehousing;
using Mermer.Ui.Pc.Reports;
using Mermer.Ui.Pc.Reports.Models;
using Mermer.Ui.Pc.Reports.Models.Mappers;
using Mermer.Warehousing.Models;
using Mermer.Warehousing.Ordering.Models;
using Mermer.Mvvm.Services;
using Mermer.Services;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

#nullable disable
namespace Mermer.Ui.Pc.Services;

public class PrintingService : IPrintingService
{
  private readonly IConfigurator _configurator;
  private readonly BillReportMapper _billMapper;
  private readonly InvoiceReportMapper _invoiceMapper;
  private readonly StockSlipReportMapper _stockSlipMapper;
  private readonly StockOrderReportMapper _stockOrderMapper;
  private readonly ExpenseSlipReportMapper _expenseSlipMapper;
  private readonly StockTransferReportMapper _stockTransferMapper;
  private readonly ILocalizationService _localizationService;
  private readonly IMvxNavigationService _navigationService;
  private readonly IUserInteractionService _userInteractionService;
  private readonly IReportLayoutStorageService _reportLayoutStorageService;

  public PrintingService(
    IConfigurator configurator,
    BillReportMapper billMapper,
    InvoiceReportMapper invoiceMapper,
    StockSlipReportMapper stockSlipMapper,
    StockOrderReportMapper stockOrderMapper,
    ExpenseSlipReportMapper expenseSlipMapper,
    StockTransferReportMapper stockTransferMapper,
    ILocalizationService localizationService,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService,
    IReportLayoutStorageService reportLayoutStorageService)
  {
    this._configurator = configurator;
    this._billMapper = billMapper;
    this._invoiceMapper = invoiceMapper;
    this._stockSlipMapper = stockSlipMapper;
    this._stockOrderMapper = stockOrderMapper;
    this._expenseSlipMapper = expenseSlipMapper;
    this._stockTransferMapper = stockTransferMapper;
    this._localizationService = localizationService;
    this._navigationService = navigationService;
    this._userInteractionService = userInteractionService;
    this._reportLayoutStorageService = reportLayoutStorageService;
  }

  public IEnumerable<string> GetPrinterNames()
  {
    foreach (object installedPrinter in PrinterSettings.InstalledPrinters)
      yield return installedPrinter.ToString();
  }

  private Task SetPrintingPreferences(Type itemType, PrintingPreferences preferences)
  {
    return this._configurator.SetConfigAsync<PrintingPreferences>(preferences, itemType.Name);
  }

  private Task<PrintingPreferences> GetPrintingPreferences(Type itemType)
  {
    return this._configurator.GetConfigAsync<PrintingPreferences>(itemType.Name);
  }

  private async Task<PrintKind?> GetPrintKind(Type itemType, bool force = false)
  {
    PrintingPreferences preferences = await this.GetPrintingPreferences(itemType);
    if (!force && (preferences == null || !preferences.PrintOnSave))
      return new PrintKind?();
    if (!force && preferences.PrintOnSave && preferences.PrintKind.HasValue)
      return preferences.PrintKind;
    PrintingPreferencesRequest preferencesRequest = new PrintingPreferencesRequest();
    preferencesRequest.PrintKind = (PrintKind?) preferences?.PrintKind;
    PrintingPreferences printingPreferences = preferences;
    preferencesRequest.PrintOnSave = printingPreferences != null && printingPreferences.PrintOnSave;
    preferencesRequest.AvailablePrintKinds = this.GetAvailablePrintKinds(itemType);
    PrintingPreferencesResult result = await this._navigationService.Navigate<PrintKindDialogViewModel, PrintingPreferencesRequest, PrintingPreferencesResult>(preferencesRequest);
    if (result == null)
      return new PrintKind?();
    preferences = new PrintingPreferences()
    {
      PrintKind = !result.SetAsDefaultPrintKind ? (PrintKind?) preferences?.PrintKind : result.PrintKind,
      PrintOnSave = result.PrintOnSave
    };
    await this.SetPrintingPreferences(itemType, preferences);
    return result.PrintKind;
  }

  private PrintKind GetAvailablePrintKinds(Type itemType)
  {
    switch (itemType.Name)
    {
      case "Bill":
      case "Invoice":
        return PrintKind.Cheque | PrintKind.Split | PrintKind.Standard | PrintKind.Preview;
      case "StockSlip":
      case "StockTransfer":
      case "StockOrder":
      case "ExpenseSlip":
        return PrintKind.Split | PrintKind.Standard | PrintKind.Preview;
      default:
        throw new ArgumentException(nameof (itemType));
    }
  }

  public async Task PrintStockSlip(StockSlip item, bool force = false)
  {
    try
    {
      PrintKind? kind = await this.GetPrintKind(item.GetType(), force);
      if (!kind.HasValue)
        return;
      StockSlipReport stockSlipReport = await this._stockSlipMapper.Map(item, this._localizationService.GetText(item.Type));
      using (XtraReport reportTemplate = await this.GetReportTemplate<StockSlipStandard>((object) new ObjectDataSource()
      {
        DataSource = (object) stockSlipReport
      }))
        this.PrintA4(reportTemplate, kind.Value);
      kind = new PrintKind?();
    }
    catch (Exception ex)
    {
      this._userInteractionService.ShowExceptionMessage(ex);
    }
  }

  public async Task PrintStockTransfer(StockTransfer item, bool force = false)
  {
    try
    {
      PrintKind? kind = await this.GetPrintKind(item.GetType(), force);
      if (!kind.HasValue)
        return;
      StockTransferPrintingType? printingType = await this._navigationService.Navigate<StockTransferPrintTypeDialogViewModel, StockTransferPrintingType?>();
      if (printingType.HasValue)
      {
        StockTransferReport stockTransferReport = await this._stockTransferMapper.Map(item, this._localizationService.GetText(item.Type));
        ObjectDataSource source = new ObjectDataSource()
        {
          DataSource = (object) stockTransferReport
        };
        if (printingType.HasValue)
        {
          XtraReport reportTemplate;
          switch (printingType.GetValueOrDefault())
          {
            case StockTransferPrintingType.Both:
              reportTemplate = await this.GetReportTemplate<StockTransferStandard>((object) source);
              break;
            case StockTransferPrintingType.SentOnly:
              reportTemplate = await this.GetReportTemplate<StockTransferStandardSent>((object) source);
              break;
            case StockTransferPrintingType.ReceivedOnly:
              reportTemplate = await this.GetReportTemplate<StockTransferStandardReceived>((object) source);
              break;
            default:
              goto label_11;
          }
          using (reportTemplate)
            this.PrintA4(reportTemplate, kind.Value);
          kind = new PrintKind?();
          printingType = new StockTransferPrintingType?();
          return;
        }
label_11:
        throw new Exception("Unknown printing type");
      }
    }
    catch (Exception ex)
    {
      this._userInteractionService.ShowExceptionMessage(ex);
    }
  }

  public async Task PrintStockOrder(StockOrder item, bool force = false)
  {
    try
    {
      PrintKind? kind = await this.GetPrintKind(item.GetType(), force);
      if (!kind.HasValue)
        return;
      StockOrderReport stockOrderReport = await this._stockOrderMapper.Map(item, this._localizationService.GetText(item.Type));
      using (XtraReport reportTemplate = await this.GetReportTemplate<StockOrderStandard>((object) new ObjectDataSource()
      {
        DataSource = (object) stockOrderReport
      }))
        this.PrintA4(reportTemplate, kind.Value);
      kind = new PrintKind?();
    }
    catch (Exception ex)
    {
      this._userInteractionService.ShowExceptionMessage(ex);
    }
  }

  public async Task PrintExpenseSlip(ExpenseSlip item, bool force = false)
  {
    try
    {
      PrintKind? kind = await this.GetPrintKind(item.GetType(), force);
      if (!kind.HasValue)
        return;
      ExpenseSlipReport expenseSlipReport = await this._expenseSlipMapper.Map(item, this._localizationService.GetText(item.Type));
      using (XtraReport reportTemplate = await this.GetReportTemplate<ExpenseSlipStandard>((object) new ObjectDataSource()
      {
        DataSource = (object) expenseSlipReport
      }))
        this.PrintA4(reportTemplate, kind.Value);
      kind = new PrintKind?();
    }
    catch (Exception ex)
    {
      this._userInteractionService.ShowExceptionMessage(ex);
    }
  }

  public async Task PrintBill(Bill item, Decimal partnerPrevBalance, bool force = false)
  {
    try
    {
      PrintKind? kind = await this.GetPrintKind(item.GetType(), force);
      if (!kind.HasValue)
        return;
      this._billMapper.SetPartnerPrevBalance(partnerPrevBalance);
      BillReport billReport = await this._billMapper.Map(item, this._localizationService.GetText(item.Type));
      ObjectDataSource source = new ObjectDataSource()
      {
        DataSource = (object) billReport
      };
      if (kind.Value == PrintKind.Cheque)
      {
        using (XtraReport reportTemplate = await this.GetReportTemplate<BillCheque>((object) source))
          this.PrintCheque(reportTemplate);
      }
      else
      {
        using (XtraReport reportTemplate = await this.GetReportTemplate<BillStandard>((object) source))
          this.PrintA4(reportTemplate, kind.Value);
      }
      kind = new PrintKind?();
    }
    catch (Exception ex)
    {
      this._userInteractionService.ShowExceptionMessage(ex);
    }
  }

  public async Task PrintInvoice(Invoice item, Decimal partnerPrevBalance, bool force = false)
  {
    try
    {
      PrintKind? kind = await this.GetPrintKind(item.GetType(), force);
      if (!kind.HasValue)
        return;
      this._invoiceMapper.SetPartnerPrevBalance(partnerPrevBalance);
      InvoiceReport invoiceReport = await this._invoiceMapper.Map(item, this._localizationService.GetText(item.Type));
      ObjectDataSource source = new ObjectDataSource()
      {
        DataSource = (object) invoiceReport
      };
      if (kind.Value == PrintKind.Cheque)
      {
        using (XtraReport reportTemplate = await this.GetReportTemplate<InvoiceCheque>((object) source))
          this.PrintCheque(reportTemplate);
      }
      else
      {
        using (XtraReport reportTemplate = await this.GetReportTemplate<InvoiceStandard>((object) source))
          this.PrintA4(reportTemplate, kind.Value);
      }
      kind = new PrintKind?();
    }
    catch (Exception ex)
    {
      this._userInteractionService.ShowExceptionMessage(ex);
    }
  }

  public async Task PrintAggregatedReport(
    AggregatedReport data,
    DateTime from,
    DateTime till,
    string[] offices)
  {
    ObjectDataSource objectDataSource1 = new ObjectDataSource()
    {
      DataSource = (object) data.FundsReport
    };
    ObjectDataSource objectDataSource2 = new ObjectDataSource()
    {
      DataSource = (object) data.StocksReport
    };
    ObjectDataSource objectDataSource3 = new ObjectDataSource()
    {
      DataSource = (object) data.PartnersReport
    };
    using (AggregatedReportAll aggregatedReportAll = new AggregatedReportAll()
    {
      DateFrom = from,
      DateTill = till,
      Offices = offices
    })
    {
      AggregatedReportFunds aggregatedReportFunds1 = new AggregatedReportFunds();
      aggregatedReportFunds1.DataSource = (object) objectDataSource1;
      using (AggregatedReportFunds aggregatedReportFunds2 = aggregatedReportFunds1)
      {
        AggregatedReportStocks aggregatedReportStocks1 = new AggregatedReportStocks();
        aggregatedReportStocks1.DataSource = (object) objectDataSource2;
        using (AggregatedReportStocks aggregatedReportStocks2 = aggregatedReportStocks1)
        {
          AggregatedReportPartners aggregatedReportPartners1 = new AggregatedReportPartners();
          aggregatedReportPartners1.DataSource = (object) objectDataSource3;
          using (AggregatedReportPartners aggregatedReportPartners2 = aggregatedReportPartners1)
          {
            aggregatedReportAll.FundsReport = (XtraReport) aggregatedReportFunds2;
            aggregatedReportAll.StocksReport = (XtraReport) aggregatedReportStocks2;
            aggregatedReportAll.PartnersReport = (XtraReport) aggregatedReportPartners2;
            aggregatedReportAll.PaperKind = PaperKind.A4Rotated;
            aggregatedReportAll.CreateDocument();
            aggregatedReportAll.PrintingSystem.Document.AutoFitToPagesWidth = 1;
            PrintHelper.ShowPrintPreviewDialog((Window) MainWindow.Instance, (IReport) aggregatedReportAll);
          }
        }
      }
    }
  }

  public async Task PrintBarcodes(string title, string barcode, string price, int copiesCount = 1)
  {
    if (copiesCount < 1)
      copiesCount = 1;
    if (string.IsNullOrEmpty(barcode) || string.IsNullOrEmpty(title))
      throw new InvalidOperationException("Barcode and Title must be specified to print barcode tag");
    BarcodeConfig config1 = this._configurator.GetConfig<BarcodeConfig>();
    Barcodes report = new Barcodes(title, barcode, price, config1.RowsCount, config1.Spaceing, config1.LeftMargin, config1.RightMargin, config1.PrintPrice, config1.TagWidth, config1.TagHeight, config1.Orientation == 0 ? BarcodeAlignment.Horizontal : BarcodeAlignment.Vertical, new BarcodeMargin(config1.TagMarginLeft, config1.TagMarginTop, config1.TagMarginRight, config1.TagMarginBottom));
    report.CreateDocument();
    PrinterConfig config2 = this._configurator.GetConfig<PrinterConfig>();
    report.PrinterName = config2.BarcodePrinterName;
    int num = 0;
    while (num < copiesCount && PrintingService.Print((XtraReport) report))
      ++num;
  }

  private void PrintA4(XtraReport template, PrintKind kind)
  {
    XtraReport report;
    if (kind == PrintKind.Standard || kind == PrintKind.Preview)
    {
      report = (XtraReport) new ReportStandard()
      {
        ReportContent = template
      };
    }
    else
    {
      SplitReport splitReport = new SplitReport();
      splitReport.LeftReport = template;
      splitReport.RightReport = template;
      splitReport.Landscape = true;
      report = (XtraReport) splitReport;
    }
    report.PaperKind = PaperKind.A4;
    report.CreateDocument();
    report.PrintingSystem.Document.AutoFitToPagesWidth = 1;
    if (kind == PrintKind.Preview)
    {
      PrintHelper.ShowPrintPreviewDialog((Window) MainWindow.Instance, (IReport) report);
    }
    else
    {
      PrinterConfig config = this._configurator.GetConfig<PrinterConfig>();
      report.PrinterName = config.StandardPrinterName;
      PrintingService.Print(report);
    }
  }

  private void PrintCheque(XtraReport template)
  {
    ReportCheque report = new ReportCheque();
    report.ReportContent = template;
    report.PageWidth = 290;
    report.CreateDocument();
    report.PrinterName = this._configurator.GetConfig<PrinterConfig>().ChequePrinterName;
    PrintingService.Print((XtraReport) report);
  }

  private static bool Print(XtraReport report)
  {
    PrintQueue queue = (PrintQueue) null;
    if (!string.IsNullOrEmpty(report.PrinterName))
    {
      foreach (PrintQueue printQueue in new LocalPrintServer().GetPrintQueues())
      {
        if (!(printQueue.Name != report.PrinterName))
        {
          queue = printQueue;
          break;
        }
      }
    }
    if (queue == null)
    {
      PrintHelper.ShowPrintPreviewDialog((Window) MainWindow.Instance, (IReport) report);
      return false;
    }
    PrintHelper.PrintDirect((IReport) report, queue);
    return true;
  }

  private async Task<XtraReport> GetReportTemplate<TReport>(object source = null) where TReport : XtraReport
  {
    string async = await this._reportLayoutStorageService.GetAsync(typeof (TReport).Name);
    XtraReport reportTemplate;
    if (!string.IsNullOrEmpty(async))
    {
      using (MemoryStream memoryStream = new MemoryStream())
      {
        using (StreamWriter streamWriter = new StreamWriter((Stream) memoryStream, Encoding.UTF8))
        {
          await streamWriter.WriteAsync(async);
          await streamWriter.FlushAsync();
          memoryStream.Seek(0L, SeekOrigin.Begin);
          reportTemplate = XtraReport.FromStream((Stream) memoryStream);
        }
      }
    }
    else
      reportTemplate = (XtraReport) Activator.CreateInstance<TReport>();
    reportTemplate.DataSource = source;
    return reportTemplate;
  }
}
