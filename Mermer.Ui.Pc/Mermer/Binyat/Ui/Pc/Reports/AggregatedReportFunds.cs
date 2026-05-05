// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Reports.AggregatedReportFunds
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using DevExpress.DataAccess.ObjectBinding;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Mermer.FundsManagement.Models;
using System.ComponentModel;
using System.Drawing;

#nullable disable
namespace Mermer.Ui.Pc.Reports;

public class AggregatedReportFunds : XtraReport
{
  private IContainer components;
  private DetailBand Detail;
  private TopMarginBand TopMargin;
  private BottomMarginBand BottomMargin;
  private ObjectDataSource objectDataSource1;
  private ReportHeaderBand reportHeaderBand1;
  private XRLabel xrLabel1;
  private DetailReportBand detailReportBand1;
  private GroupHeaderBand groupHeaderBand1;
  private XRTable xrTable2;
  private XRTableRow xrTableRow3;
  private XRTableCell xrTableCell11;
  private XRTableCell xrTableCell12;
  private XRTableCell xrTableCell13;
  private XRTableCell xrTableCell14;
  private DetailBand detailBand1;
  private XRTable xrTable3;
  private XRTableRow xrTableRow4;
  private XRTableCell xrTableCell15;
  private XRTableCell xrTableCell16;
  private XRTableCell xrTableCell17;
  private XRTableCell xrTableCell18;
  private XRControlStyle Title;
  private XRControlStyle DetailCaption1;
  private XRControlStyle DetailData1;
  private XRControlStyle DetailCaption3;
  private XRControlStyle DetailData3;
  private XRControlStyle DetailData3_Odd;
  private XRControlStyle DetailCaptionBackground3;
  private XRControlStyle PageInfo;
  private ReportFooterBand ReportFooter;
  private XRLabel xrLabel2;
  private XRControlStyle TotalsLabel;
  private XRLabel xrLabel7;
  private XRLabel xrLabel6;
  private XRLabel xrLabel5;
  private XRLabel xrLabel4;
  private XRLabel xrLabel3;
  private XRControlStyle TotalsValue;

  public AggregatedReportFunds() => this.InitializeComponent();

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    this.components = (IContainer) new System.ComponentModel.Container();
    ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (AggregatedReportFunds));
    this.Detail = new DetailBand();
    this.TopMargin = new TopMarginBand();
    this.BottomMargin = new BottomMarginBand();
    this.objectDataSource1 = new ObjectDataSource(this.components);
    this.reportHeaderBand1 = new ReportHeaderBand();
    this.xrLabel1 = new XRLabel();
    this.detailReportBand1 = new DetailReportBand();
    this.groupHeaderBand1 = new GroupHeaderBand();
    this.xrTable2 = new XRTable();
    this.xrTableRow3 = new XRTableRow();
    this.xrTableCell11 = new XRTableCell();
    this.xrTableCell12 = new XRTableCell();
    this.xrTableCell13 = new XRTableCell();
    this.xrTableCell14 = new XRTableCell();
    this.detailBand1 = new DetailBand();
    this.xrTable3 = new XRTable();
    this.xrTableRow4 = new XRTableRow();
    this.xrTableCell15 = new XRTableCell();
    this.xrTableCell16 = new XRTableCell();
    this.xrTableCell17 = new XRTableCell();
    this.xrTableCell18 = new XRTableCell();
    this.Title = new XRControlStyle();
    this.DetailCaption1 = new XRControlStyle();
    this.DetailData1 = new XRControlStyle();
    this.DetailCaption3 = new XRControlStyle();
    this.DetailData3 = new XRControlStyle();
    this.DetailData3_Odd = new XRControlStyle();
    this.DetailCaptionBackground3 = new XRControlStyle();
    this.PageInfo = new XRControlStyle();
    this.ReportFooter = new ReportFooterBand();
    this.xrLabel7 = new XRLabel();
    this.xrLabel6 = new XRLabel();
    this.xrLabel5 = new XRLabel();
    this.xrLabel4 = new XRLabel();
    this.xrLabel3 = new XRLabel();
    this.xrLabel2 = new XRLabel();
    this.TotalsLabel = new XRControlStyle();
    this.TotalsValue = new XRControlStyle();
    this.objectDataSource1.BeginInit();
    this.xrTable2.BeginInit();
    this.xrTable3.BeginInit();
    this.BeginInit();
    componentResourceManager.ApplyResources((object) this.Detail, "Detail");
    this.Detail.KeepTogether = true;
    this.Detail.Name = "Detail";
    this.Detail.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
    componentResourceManager.ApplyResources((object) this.TopMargin, "TopMargin");
    this.TopMargin.Name = "TopMargin";
    this.TopMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
    componentResourceManager.ApplyResources((object) this.BottomMargin, "BottomMargin");
    this.BottomMargin.Name = "BottomMargin";
    this.BottomMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
    this.objectDataSource1.DataSource = (object) typeof (FundsBalanceAggregated);
    this.objectDataSource1.Name = "objectDataSource1";
    this.reportHeaderBand1.Controls.AddRange(new XRControl[1]
    {
      (XRControl) this.xrLabel1
    });
    componentResourceManager.ApplyResources((object) this.reportHeaderBand1, "reportHeaderBand1");
    this.reportHeaderBand1.Name = "reportHeaderBand1";
    componentResourceManager.ApplyResources((object) this.xrLabel1, "xrLabel1");
    this.xrLabel1.Name = "xrLabel1";
    this.xrLabel1.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel1.StyleName = "Title";
    this.detailReportBand1.Bands.AddRange(new Band[2]
    {
      (Band) this.groupHeaderBand1,
      (Band) this.detailBand1
    });
    this.detailReportBand1.DataMember = "Lines";
    this.detailReportBand1.DataSource = (object) this.objectDataSource1;
    this.detailReportBand1.Level = 0;
    this.detailReportBand1.Name = "detailReportBand1";
    this.groupHeaderBand1.Controls.AddRange(new XRControl[1]
    {
      (XRControl) this.xrTable2
    });
    this.groupHeaderBand1.GroupUnion = GroupUnion.WithFirstDetail;
    componentResourceManager.ApplyResources((object) this.groupHeaderBand1, "groupHeaderBand1");
    this.groupHeaderBand1.Name = "groupHeaderBand1";
    componentResourceManager.ApplyResources((object) this.xrTable2, "xrTable2");
    this.xrTable2.Name = "xrTable2";
    this.xrTable2.Rows.AddRange(new XRTableRow[1]
    {
      this.xrTableRow3
    });
    this.xrTableRow3.Cells.AddRange(new XRTableCell[4]
    {
      this.xrTableCell11,
      this.xrTableCell12,
      this.xrTableCell13,
      this.xrTableCell14
    });
    this.xrTableRow3.Name = "xrTableRow3";
    componentResourceManager.ApplyResources((object) this.xrTableRow3, "xrTableRow3");
    this.xrTableCell11.Name = "xrTableCell11";
    this.xrTableCell11.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrTableCell11.StyleName = "DetailCaption3";
    componentResourceManager.ApplyResources((object) this.xrTableCell11, "xrTableCell11");
    this.xrTableCell12.Name = "xrTableCell12";
    this.xrTableCell12.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrTableCell12.StyleName = "DetailCaption3";
    this.xrTableCell12.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell12, "xrTableCell12");
    this.xrTableCell13.Name = "xrTableCell13";
    this.xrTableCell13.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrTableCell13.StyleName = "DetailCaption3";
    this.xrTableCell13.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell13, "xrTableCell13");
    this.xrTableCell14.Name = "xrTableCell14";
    this.xrTableCell14.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrTableCell14.StyleName = "DetailCaption3";
    this.xrTableCell14.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell14, "xrTableCell14");
    this.detailBand1.Controls.AddRange(new XRControl[1]
    {
      (XRControl) this.xrTable3
    });
    componentResourceManager.ApplyResources((object) this.detailBand1, "detailBand1");
    this.detailBand1.Name = "detailBand1";
    componentResourceManager.ApplyResources((object) this.xrTable3, "xrTable3");
    this.xrTable3.Name = "xrTable3";
    this.xrTable3.OddStyleName = "DetailData3_Odd";
    this.xrTable3.Rows.AddRange(new XRTableRow[1]
    {
      this.xrTableRow4
    });
    this.xrTableRow4.Cells.AddRange(new XRTableCell[4]
    {
      this.xrTableCell15,
      this.xrTableCell16,
      this.xrTableCell17,
      this.xrTableCell18
    });
    this.xrTableRow4.Name = "xrTableRow4";
    componentResourceManager.ApplyResources((object) this.xrTableRow4, "xrTableRow4");
    this.xrTableCell15.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Type]")
    });
    this.xrTableCell15.Name = "xrTableCell15";
    this.xrTableCell15.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrTableCell15.StyleName = "DetailData3";
    componentResourceManager.ApplyResources((object) this.xrTableCell15, "xrTableCell15");
    this.xrTableCell16.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Income]")
    });
    this.xrTableCell16.Name = "xrTableCell16";
    this.xrTableCell16.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrTableCell16.StyleName = "DetailData3";
    this.xrTableCell16.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell16, "xrTableCell16");
    this.xrTableCell17.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Expense]")
    });
    this.xrTableCell17.Name = "xrTableCell17";
    this.xrTableCell17.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrTableCell17.StyleName = "DetailData3";
    this.xrTableCell17.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell17, "xrTableCell17");
    this.xrTableCell18.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Effect]")
    });
    this.xrTableCell18.Name = "xrTableCell18";
    this.xrTableCell18.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrTableCell18.StyleName = "DetailData3";
    this.xrTableCell18.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell18, "xrTableCell18");
    this.Title.BackColor = Color.Transparent;
    this.Title.BorderColor = Color.Black;
    this.Title.Borders = BorderSide.None;
    this.Title.BorderWidth = 1f;
    this.Title.Font = new Font("Tahoma", 14f);
    this.Title.ForeColor = Color.FromArgb(75, 75, 75);
    this.Title.Name = "Title";
    this.DetailCaption1.BackColor = Color.FromArgb(75, 75, 75);
    this.DetailCaption1.BorderColor = Color.White;
    this.DetailCaption1.Borders = BorderSide.Left;
    this.DetailCaption1.BorderWidth = 2f;
    this.DetailCaption1.Font = new Font("Tahoma", 8f, FontStyle.Bold);
    this.DetailCaption1.ForeColor = Color.White;
    this.DetailCaption1.Name = "DetailCaption1";
    this.DetailCaption1.Padding = new PaddingInfo(6, 6, 0, 0, 100f);
    this.DetailCaption1.TextAlignment = TextAlignment.MiddleLeft;
    this.DetailData1.BackColor = Color.Transparent;
    this.DetailData1.BorderColor = Color.Transparent;
    this.DetailData1.Borders = BorderSide.Left;
    this.DetailData1.BorderWidth = 2f;
    this.DetailData1.Font = new Font("Tahoma", 8f);
    this.DetailData1.ForeColor = Color.Black;
    this.DetailData1.Name = "DetailData1";
    this.DetailData1.Padding = new PaddingInfo(6, 6, 0, 0, 100f);
    this.DetailData1.TextAlignment = TextAlignment.MiddleLeft;
    this.DetailCaption3.BackColor = Color.Transparent;
    this.DetailCaption3.BorderColor = Color.Transparent;
    this.DetailCaption3.Borders = BorderSide.None;
    this.DetailCaption3.Font = new Font("Tahoma", 8f, FontStyle.Bold);
    this.DetailCaption3.ForeColor = Color.FromArgb(75, 75, 75);
    this.DetailCaption3.Name = "DetailCaption3";
    this.DetailCaption3.Padding = new PaddingInfo(6, 6, 0, 0, 100f);
    this.DetailCaption3.TextAlignment = TextAlignment.MiddleLeft;
    this.DetailData3.Font = new Font("Tahoma", 8f);
    this.DetailData3.ForeColor = Color.Black;
    this.DetailData3.Name = "DetailData3";
    this.DetailData3.Padding = new PaddingInfo(6, 6, 0, 0, 100f);
    this.DetailData3.TextAlignment = TextAlignment.MiddleLeft;
    this.DetailData3_Odd.BackColor = Color.FromArgb(231, 231, 231);
    this.DetailData3_Odd.BorderColor = Color.Transparent;
    this.DetailData3_Odd.Borders = BorderSide.None;
    this.DetailData3_Odd.BorderWidth = 1f;
    this.DetailData3_Odd.Font = new Font("Tahoma", 8f);
    this.DetailData3_Odd.ForeColor = Color.Black;
    this.DetailData3_Odd.Name = "DetailData3_Odd";
    this.DetailData3_Odd.Padding = new PaddingInfo(6, 6, 0, 0, 100f);
    this.DetailData3_Odd.TextAlignment = TextAlignment.MiddleLeft;
    this.DetailCaptionBackground3.BackColor = Color.Transparent;
    this.DetailCaptionBackground3.BorderColor = Color.FromArgb(206, 206, 206);
    this.DetailCaptionBackground3.Borders = BorderSide.Top;
    this.DetailCaptionBackground3.BorderWidth = 2f;
    this.DetailCaptionBackground3.Name = "DetailCaptionBackground3";
    this.PageInfo.Font = new Font("Tahoma", 8f, FontStyle.Bold);
    this.PageInfo.ForeColor = Color.FromArgb(75, 75, 75);
    this.PageInfo.Name = "PageInfo";
    this.PageInfo.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.ReportFooter.Controls.AddRange(new XRControl[6]
    {
      (XRControl) this.xrLabel7,
      (XRControl) this.xrLabel6,
      (XRControl) this.xrLabel5,
      (XRControl) this.xrLabel4,
      (XRControl) this.xrLabel3,
      (XRControl) this.xrLabel2
    });
    componentResourceManager.ApplyResources((object) this.ReportFooter, "ReportFooter");
    this.ReportFooter.Name = "ReportFooter";
    this.ReportFooter.PrintAtBottom = true;
    componentResourceManager.ApplyResources((object) this.xrLabel7, "xrLabel7");
    this.xrLabel7.Name = "xrLabel7";
    this.xrLabel7.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel7.StyleName = "TotalsLabel";
    componentResourceManager.ApplyResources((object) this.xrLabel6, "xrLabel6");
    this.xrLabel6.Name = "xrLabel6";
    this.xrLabel6.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel6.StyleName = "TotalsLabel";
    this.xrLabel5.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[ResultingBalance]")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel5, "xrLabel5");
    this.xrLabel5.Name = "xrLabel5";
    this.xrLabel5.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel5.StyleName = "TotalsValue";
    this.xrLabel4.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[EffectedBalance]")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel4, "xrLabel4");
    this.xrLabel4.Name = "xrLabel4";
    this.xrLabel4.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel4.StyleName = "TotalsValue";
    this.xrLabel3.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[StartingBalance]")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel3, "xrLabel3");
    this.xrLabel3.Name = "xrLabel3";
    this.xrLabel3.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel3.StyleName = "TotalsValue";
    componentResourceManager.ApplyResources((object) this.xrLabel2, "xrLabel2");
    this.xrLabel2.Name = "xrLabel2";
    this.xrLabel2.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel2.StyleName = "TotalsLabel";
    this.TotalsLabel.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
    this.TotalsLabel.Name = "TotalsLabel";
    this.TotalsLabel.TextAlignment = TextAlignment.MiddleRight;
    this.TotalsValue.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
    this.TotalsValue.Name = "TotalsValue";
    this.TotalsValue.Padding = new PaddingInfo(0, 5, 0, 0, 100f);
    this.TotalsValue.TextAlignment = TextAlignment.MiddleRight;
    this.Bands.AddRange(new Band[6]
    {
      (Band) this.Detail,
      (Band) this.TopMargin,
      (Band) this.BottomMargin,
      (Band) this.reportHeaderBand1,
      (Band) this.detailReportBand1,
      (Band) this.ReportFooter
    });
    this.ComponentStorage.AddRange(new IComponent[1]
    {
      (IComponent) this.objectDataSource1
    });
    this.DataSource = (object) this.objectDataSource1;
    componentResourceManager.ApplyResources((object) this, "$this");
    this.StyleSheet.AddRange(new XRControlStyle[10]
    {
      this.Title,
      this.DetailCaption1,
      this.DetailData1,
      this.DetailCaption3,
      this.DetailData3,
      this.DetailData3_Odd,
      this.DetailCaptionBackground3,
      this.PageInfo,
      this.TotalsLabel,
      this.TotalsValue
    });
    this.Version = "17.2";
    this.objectDataSource1.EndInit();
    this.xrTable2.EndInit();
    this.xrTable3.EndInit();
    this.EndInit();
  }
}
