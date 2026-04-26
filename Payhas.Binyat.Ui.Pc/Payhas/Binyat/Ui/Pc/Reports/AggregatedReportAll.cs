// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Reports.AggregatedReportAll
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using System;
using System.ComponentModel;
using System.Drawing;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Reports;

public class AggregatedReportAll : XtraReport
{
  private IContainer components;
  private DetailBand Detail;
  private TopMarginBand TopMargin;
  private BottomMarginBand BottomMargin;
  private XRSubreport stocksReport;
  private XRPageInfo xrPageInfo2;
  private XRPageInfo xrPageInfo1;
  private ReportHeaderBand ReportHeader;
  private XRLabel valOffices;
  private XRLabel lblOffices;
  private XRLabel valTill;
  private XRLabel lblTill;
  private XRLabel valFrom;
  private XRLabel lblFrom;
  private XRControlStyle TextLabel;
  private XRControlStyle TextValue;
  private XRSubreport partnersReport;
  private XRSubreport fundsReport;

  public AggregatedReportAll() => this.InitializeComponent();

  public DateTime DateFrom
  {
    set => this.valFrom.Text = value.ToString(": yyyy MMMM dd");
  }

  public DateTime DateTill
  {
    set => this.valTill.Text = value.ToString(": yyyy MMMM dd");
  }

  public string[] Offices
  {
    set => this.valOffices.Text = ": " + string.Join(", ", value);
  }

  public XtraReport FundsReport
  {
    get => this.fundsReport.ReportSource;
    set => this.fundsReport.ReportSource = value;
  }

  public XtraReport StocksReport
  {
    get => this.stocksReport.ReportSource;
    set => this.stocksReport.ReportSource = value;
  }

  public XtraReport PartnersReport
  {
    get => this.partnersReport.ReportSource;
    set => this.partnersReport.ReportSource = value;
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (AggregatedReportAll));
    this.Detail = new DetailBand();
    this.partnersReport = new XRSubreport();
    this.fundsReport = new XRSubreport();
    this.stocksReport = new XRSubreport();
    this.TopMargin = new TopMarginBand();
    this.BottomMargin = new BottomMarginBand();
    this.xrPageInfo2 = new XRPageInfo();
    this.xrPageInfo1 = new XRPageInfo();
    this.ReportHeader = new ReportHeaderBand();
    this.valOffices = new XRLabel();
    this.lblOffices = new XRLabel();
    this.valTill = new XRLabel();
    this.lblTill = new XRLabel();
    this.valFrom = new XRLabel();
    this.lblFrom = new XRLabel();
    this.TextLabel = new XRControlStyle();
    this.TextValue = new XRControlStyle();
    this.BeginInit();
    this.Detail.Controls.AddRange(new XRControl[3]
    {
      (XRControl) this.partnersReport,
      (XRControl) this.fundsReport,
      (XRControl) this.stocksReport
    });
    componentResourceManager.ApplyResources((object) this.Detail, "Detail");
    this.Detail.Name = "Detail";
    this.Detail.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
    componentResourceManager.ApplyResources((object) this.partnersReport, "partnersReport");
    this.partnersReport.Name = "partnersReport";
    componentResourceManager.ApplyResources((object) this.fundsReport, "fundsReport");
    this.fundsReport.Name = "fundsReport";
    componentResourceManager.ApplyResources((object) this.stocksReport, "stocksReport");
    this.stocksReport.Name = "stocksReport";
    componentResourceManager.ApplyResources((object) this.TopMargin, "TopMargin");
    this.TopMargin.Name = "TopMargin";
    this.TopMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
    this.BottomMargin.Controls.AddRange(new XRControl[2]
    {
      (XRControl) this.xrPageInfo2,
      (XRControl) this.xrPageInfo1
    });
    componentResourceManager.ApplyResources((object) this.BottomMargin, "BottomMargin");
    this.BottomMargin.Name = "BottomMargin";
    this.BottomMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
    componentResourceManager.ApplyResources((object) this.xrPageInfo2, "xrPageInfo2");
    this.xrPageInfo2.Name = "xrPageInfo2";
    this.xrPageInfo2.Padding = new PaddingInfo(5, 5, 0, 0, 100f);
    this.xrPageInfo2.PageInfo = PageInfo.DateTime;
    this.xrPageInfo2.StylePriority.UseForeColor = false;
    this.xrPageInfo2.StylePriority.UsePadding = false;
    this.xrPageInfo2.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrPageInfo1, "xrPageInfo1");
    this.xrPageInfo1.Name = "xrPageInfo1";
    this.xrPageInfo1.Padding = new PaddingInfo(5, 5, 0, 0, 100f);
    this.xrPageInfo1.StylePriority.UseForeColor = false;
    this.xrPageInfo1.StylePriority.UsePadding = false;
    this.xrPageInfo1.StylePriority.UseTextAlignment = false;
    this.ReportHeader.Controls.AddRange(new XRControl[6]
    {
      (XRControl) this.valOffices,
      (XRControl) this.lblOffices,
      (XRControl) this.valTill,
      (XRControl) this.lblTill,
      (XRControl) this.valFrom,
      (XRControl) this.lblFrom
    });
    componentResourceManager.ApplyResources((object) this.ReportHeader, "ReportHeader");
    this.ReportHeader.Name = "ReportHeader";
    componentResourceManager.ApplyResources((object) this.valOffices, "valOffices");
    this.valOffices.Name = "valOffices";
    this.valOffices.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.valOffices.StyleName = "TextValue";
    componentResourceManager.ApplyResources((object) this.lblOffices, "lblOffices");
    this.lblOffices.Name = "lblOffices";
    this.lblOffices.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.lblOffices.StyleName = "TextLabel";
    componentResourceManager.ApplyResources((object) this.valTill, "valTill");
    this.valTill.Name = "valTill";
    this.valTill.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.valTill.StyleName = "TextValue";
    componentResourceManager.ApplyResources((object) this.lblTill, "lblTill");
    this.lblTill.Name = "lblTill";
    this.lblTill.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.lblTill.StyleName = "TextLabel";
    componentResourceManager.ApplyResources((object) this.valFrom, "valFrom");
    this.valFrom.Name = "valFrom";
    this.valFrom.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.valFrom.StyleName = "TextValue";
    componentResourceManager.ApplyResources((object) this.lblFrom, "lblFrom");
    this.lblFrom.Name = "lblFrom";
    this.lblFrom.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.lblFrom.StyleName = "TextLabel";
    this.TextLabel.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Bold, GraphicsUnit.Point, (byte) 0);
    this.TextLabel.Name = "TextLabel";
    this.TextLabel.Padding = new PaddingInfo(5, 0, 0, 0, 100f);
    this.TextLabel.TextAlignment = TextAlignment.MiddleLeft;
    this.TextValue.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
    this.TextValue.Name = "TextValue";
    this.TextValue.Padding = new PaddingInfo(5, 0, 0, 0, 100f);
    this.TextValue.TextAlignment = TextAlignment.MiddleLeft;
    this.Bands.AddRange(new Band[4]
    {
      (Band) this.Detail,
      (Band) this.TopMargin,
      (Band) this.BottomMargin,
      (Band) this.ReportHeader
    });
    componentResourceManager.ApplyResources((object) this, "$this");
    this.StyleSheet.AddRange(new XRControlStyle[2]
    {
      this.TextLabel,
      this.TextValue
    });
    this.Version = "17.2";
    this.EndInit();
  }
}
