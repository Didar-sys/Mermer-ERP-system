// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Reports.InvoiceStandard
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using DevExpress.DataAccess.ObjectBinding;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.BarCode;
using DevExpress.XtraReports.UI;
using Payhas.Binyat.Ui.Pc.Reports.Models;
using System.ComponentModel;
using System.Drawing;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Reports;

public class InvoiceStandard : XtraReport
{
  private IContainer components;
  private DetailBand Detail;
  private TopMarginBand TopMargin;
  private BottomMarginBand BottomMargin;
  private XRLabel xrLabel1;
  private ObjectDataSource objectDataSource1;
  private ReportHeaderBand ReportHeader;
  private XRControlStyle TypeHeaderStyle;
  private XRLabel xrLabel3;
  private XRCheckBox xrCheckBox2;
  private XRCheckBox xrCheckBox1;
  private XRLabel xrLabel2;
  private XRBarCode xrBarCode1;
  private XRLabel xrLabel4;
  private XRLabel xrLabel5;
  private XRControlStyle PropertyStyle;
  private DetailReportBand DetailReport;
  private DetailBand Detail1;
  private XRTable xrTable1;
  private XRTableRow xrTableRow1;
  private XRTableCell xrTableCell1;
  private XRTableCell xrTableCell2;
  private XRTableCell xrTableCell3;
  private XRTable xrTable2;
  private XRTableRow xrTableRow2;
  private XRTableCell xrTableCell7;
  private XRTableCell xrTableCell8;
  private XRTableCell xrTableCell9;
  private XRTableCell xrTableCell10;
  private XRTableCell xrTableCell11;
  private XRTableCell xrTableCell12;
  private XRTableCell xrTableCell4;
  private XRTableCell xrTableCell5;
  private XRTableCell xrTableCell6;
  private XRControlStyle LineHeaderStyle;
  private XRControlStyle LineStyle;
  private ReportFooterBand ReportFooter;
  private PageFooterBand PageFooter;
  private XRLabel xrLabel6;
  private XRLabel xrLabel7;
  private XRPageInfo xrPageInfo2;
  private XRPageInfo xrPageInfo1;
  private XRControlStyle PropertyHeaderStyle;
  private XRLabel xrLabel19;
  private XRLabel xrLabel18;
  private XRLabel xrLabel27;
  private XRLabel xrLabel26;
  private XRLabel xrLabel25;
  private XRLabel xrLabel24;
  private XRLabel xrLabel17;
  private XRLabel xrLabel15;
  private XRLabel xrLabel14;
  private XRLabel xrLabel16;
  private XRLabel xrLabel13;
  private XRLabel xrLabel12;
  private XRLabel xrLabel8;
  private XRLabel xrLabel9;
  private XRLine xrLine1;
  private XRLabel xrLabel10;
  private XRLine xrLine2;
  private XRLabel xrLabel11;
  private XRLabel xrLabel20;
  private XRLabel xrLabel21;
  private XRLabel xrLabel22;
  private XRLine xrLine3;
  private XRLabel xrLabel23;
  private XRCheckBox xrCheckBox3;
  private XRLabel xrLabel29;
  private XRLabel xrLabel28;
  private XRTableCell xrTableCell14;
  private XRTableCell xrTableCell13;
  private PageHeaderBand PageHeader;
  private XRLabel xrLabel31;
  private XRLabel xrLabel30;

  public InvoiceStandard() => this.InitializeComponent();

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    this.components = (IContainer) new System.ComponentModel.Container();
    ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (InvoiceStandard));
    EAN13Generator eaN13Generator = new EAN13Generator();
    this.Detail = new DetailBand();
    this.TopMargin = new TopMarginBand();
    this.BottomMargin = new BottomMarginBand();
    this.xrLabel1 = new XRLabel();
    this.ReportHeader = new ReportHeaderBand();
    this.xrLabel31 = new XRLabel();
    this.xrLabel30 = new XRLabel();
    this.xrCheckBox3 = new XRCheckBox();
    this.xrLabel19 = new XRLabel();
    this.xrLabel3 = new XRLabel();
    this.xrCheckBox2 = new XRCheckBox();
    this.xrCheckBox1 = new XRCheckBox();
    this.xrLabel2 = new XRLabel();
    this.xrBarCode1 = new XRBarCode();
    this.xrLabel4 = new XRLabel();
    this.xrLabel5 = new XRLabel();
    this.xrLabel18 = new XRLabel();
    this.TypeHeaderStyle = new XRControlStyle();
    this.PropertyStyle = new XRControlStyle();
    this.DetailReport = new DetailReportBand();
    this.Detail1 = new DetailBand();
    this.xrTable2 = new XRTable();
    this.xrTableRow2 = new XRTableRow();
    this.xrTableCell14 = new XRTableCell();
    this.xrTableCell7 = new XRTableCell();
    this.xrTableCell8 = new XRTableCell();
    this.xrTableCell9 = new XRTableCell();
    this.xrTableCell10 = new XRTableCell();
    this.xrTableCell11 = new XRTableCell();
    this.xrTableCell12 = new XRTableCell();
    this.objectDataSource1 = new ObjectDataSource(this.components);
    this.xrTable1 = new XRTable();
    this.xrTableRow1 = new XRTableRow();
    this.xrTableCell13 = new XRTableCell();
    this.xrTableCell1 = new XRTableCell();
    this.xrTableCell2 = new XRTableCell();
    this.xrTableCell4 = new XRTableCell();
    this.xrTableCell3 = new XRTableCell();
    this.xrTableCell5 = new XRTableCell();
    this.xrTableCell6 = new XRTableCell();
    this.LineHeaderStyle = new XRControlStyle();
    this.LineStyle = new XRControlStyle();
    this.ReportFooter = new ReportFooterBand();
    this.xrLabel29 = new XRLabel();
    this.xrLabel28 = new XRLabel();
    this.xrLabel27 = new XRLabel();
    this.xrLabel26 = new XRLabel();
    this.xrLabel25 = new XRLabel();
    this.xrLabel24 = new XRLabel();
    this.xrLabel17 = new XRLabel();
    this.xrLabel15 = new XRLabel();
    this.xrLabel14 = new XRLabel();
    this.xrLabel16 = new XRLabel();
    this.xrLabel13 = new XRLabel();
    this.xrLabel12 = new XRLabel();
    this.xrLabel6 = new XRLabel();
    this.xrLabel7 = new XRLabel();
    this.xrLabel8 = new XRLabel();
    this.xrLabel9 = new XRLabel();
    this.xrLine1 = new XRLine();
    this.xrLabel10 = new XRLabel();
    this.xrLine2 = new XRLine();
    this.xrLabel11 = new XRLabel();
    this.xrLabel20 = new XRLabel();
    this.xrLabel21 = new XRLabel();
    this.xrLabel22 = new XRLabel();
    this.xrLine3 = new XRLine();
    this.xrLabel23 = new XRLabel();
    this.PageFooter = new PageFooterBand();
    this.xrPageInfo2 = new XRPageInfo();
    this.xrPageInfo1 = new XRPageInfo();
    this.PropertyHeaderStyle = new XRControlStyle();
    this.PageHeader = new PageHeaderBand();
    this.xrTable2.BeginInit();
    this.objectDataSource1.BeginInit();
    this.xrTable1.BeginInit();
    this.BeginInit();
    componentResourceManager.ApplyResources((object) this.Detail, "Detail");
    this.Detail.Name = "Detail";
    this.Detail.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
    componentResourceManager.ApplyResources((object) this.TopMargin, "TopMargin");
    this.TopMargin.Name = "TopMargin";
    this.TopMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
    componentResourceManager.ApplyResources((object) this.BottomMargin, "BottomMargin");
    this.BottomMargin.Name = "BottomMargin";
    this.BottomMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
    this.xrLabel1.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Type]")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel1, "xrLabel1");
    this.xrLabel1.Name = "xrLabel1";
    this.xrLabel1.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel1.StyleName = "TypeHeaderStyle";
    this.xrLabel1.StylePriority.UseTextAlignment = false;
    this.ReportHeader.Controls.AddRange(new XRControl[13]
    {
      (XRControl) this.xrLabel31,
      (XRControl) this.xrLabel30,
      (XRControl) this.xrCheckBox3,
      (XRControl) this.xrLabel19,
      (XRControl) this.xrLabel3,
      (XRControl) this.xrCheckBox2,
      (XRControl) this.xrCheckBox1,
      (XRControl) this.xrLabel2,
      (XRControl) this.xrLabel1,
      (XRControl) this.xrBarCode1,
      (XRControl) this.xrLabel4,
      (XRControl) this.xrLabel5,
      (XRControl) this.xrLabel18
    });
    componentResourceManager.ApplyResources((object) this.ReportHeader, "ReportHeader");
    this.ReportHeader.Name = "ReportHeader";
    this.xrLabel31.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[UserName]")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel31, "xrLabel31");
    this.xrLabel31.Multiline = true;
    this.xrLabel31.Name = "xrLabel31";
    this.xrLabel31.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel31.StyleName = "PropertyStyle";
    componentResourceManager.ApplyResources((object) this.xrLabel30, "xrLabel30");
    this.xrLabel30.Multiline = true;
    this.xrLabel30.Name = "xrLabel30";
    this.xrLabel30.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel30.StyleName = "PropertyHeaderStyle";
    this.xrCheckBox3.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "CheckState", "[IsCash]")
    });
    componentResourceManager.ApplyResources((object) this.xrCheckBox3, "xrCheckBox3");
    this.xrCheckBox3.Name = "xrCheckBox3";
    this.xrCheckBox3.StyleName = "PropertyStyle";
    this.xrLabel19.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Depository]")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel19, "xrLabel19");
    this.xrLabel19.Name = "xrLabel19";
    this.xrLabel19.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel19.StyleName = "PropertyStyle";
    this.xrLabel3.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Warehouse]")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel3, "xrLabel3");
    this.xrLabel3.Name = "xrLabel3";
    this.xrLabel3.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel3.StyleName = "PropertyStyle";
    this.xrCheckBox2.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "CheckState", "[IsDisabled]")
    });
    componentResourceManager.ApplyResources((object) this.xrCheckBox2, "xrCheckBox2");
    this.xrCheckBox2.Name = "xrCheckBox2";
    this.xrCheckBox2.StyleName = "PropertyStyle";
    this.xrCheckBox1.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "CheckState", "[IsCompleted]")
    });
    componentResourceManager.ApplyResources((object) this.xrCheckBox1, "xrCheckBox1");
    this.xrCheckBox1.Name = "xrCheckBox1";
    this.xrCheckBox1.StyleName = "PropertyStyle";
    this.xrLabel2.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Date]")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel2, "xrLabel2");
    this.xrLabel2.Name = "xrLabel2";
    this.xrLabel2.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel2.StyleName = "PropertyStyle";
    this.xrBarCode1.Alignment = TextAlignment.TopCenter;
    this.xrBarCode1.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Code]")
    });
    componentResourceManager.ApplyResources((object) this.xrBarCode1, "xrBarCode1");
    this.xrBarCode1.Name = "xrBarCode1";
    this.xrBarCode1.Padding = new PaddingInfo(10, 10, 0, 0, 100f);
    this.xrBarCode1.StylePriority.UseTextAlignment = false;
    this.xrBarCode1.Symbology = (BarCodeGeneratorBase) eaN13Generator;
    componentResourceManager.ApplyResources((object) this.xrLabel4, "xrLabel4");
    this.xrLabel4.Name = "xrLabel4";
    this.xrLabel4.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel4.StyleName = "PropertyHeaderStyle";
    componentResourceManager.ApplyResources((object) this.xrLabel5, "xrLabel5");
    this.xrLabel5.Name = "xrLabel5";
    this.xrLabel5.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel5.StyleName = "PropertyHeaderStyle";
    componentResourceManager.ApplyResources((object) this.xrLabel18, "xrLabel18");
    this.xrLabel18.Name = "xrLabel18";
    this.xrLabel18.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel18.StyleName = "PropertyHeaderStyle";
    this.TypeHeaderStyle.BorderDashStyle = BorderDashStyle.DashDotDot;
    this.TypeHeaderStyle.Borders = BorderSide.Bottom;
    this.TypeHeaderStyle.Font = new Font("Microsoft Sans Serif", 24f, FontStyle.Bold, GraphicsUnit.Point, (byte) 0);
    this.TypeHeaderStyle.ForeColor = Color.Teal;
    this.TypeHeaderStyle.Name = "TypeHeaderStyle";
    this.TypeHeaderStyle.TextAlignment = TextAlignment.MiddleLeft;
    this.PropertyStyle.Name = "PropertyStyle";
    this.PropertyStyle.TextAlignment = TextAlignment.MiddleLeft;
    this.DetailReport.Bands.AddRange(new Band[1]
    {
      (Band) this.Detail1
    });
    this.DetailReport.DataMember = "Lines";
    this.DetailReport.DataSource = (object) this.objectDataSource1;
    this.DetailReport.Level = 0;
    this.DetailReport.Name = "DetailReport";
    this.Detail1.Controls.AddRange(new XRControl[1]
    {
      (XRControl) this.xrTable2
    });
    componentResourceManager.ApplyResources((object) this.Detail1, "Detail1");
    this.Detail1.Name = "Detail1";
    componentResourceManager.ApplyResources((object) this.xrTable2, "xrTable2");
    this.xrTable2.Name = "xrTable2";
    this.xrTable2.Rows.AddRange(new XRTableRow[1]
    {
      this.xrTableRow2
    });
    this.xrTable2.StyleName = "LineStyle";
    this.xrTableRow2.Cells.AddRange(new XRTableCell[7]
    {
      this.xrTableCell14,
      this.xrTableCell7,
      this.xrTableCell8,
      this.xrTableCell9,
      this.xrTableCell10,
      this.xrTableCell11,
      this.xrTableCell12
    });
    this.xrTableRow2.Name = "xrTableRow2";
    componentResourceManager.ApplyResources((object) this.xrTableRow2, "xrTableRow2");
    this.xrTableCell14.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
    this.xrTableCell14.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[RowNo]")
    });
    this.xrTableCell14.Name = "xrTableCell14";
    this.xrTableCell14.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
    this.xrTableCell14.StyleName = "LineStyle";
    this.xrTableCell14.StylePriority.UseBorders = false;
    this.xrTableCell14.StylePriority.UsePadding = false;
    this.xrTableCell14.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell14, "xrTableCell14");
    this.xrTableCell7.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
    this.xrTableCell7.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Stock]")
    });
    this.xrTableCell7.Name = "xrTableCell7";
    this.xrTableCell7.StyleName = "LineStyle";
    this.xrTableCell7.StylePriority.UseBorders = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell7, "xrTableCell7");
    this.xrTableCell8.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
    this.xrTableCell8.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Quantity]")
    });
    this.xrTableCell8.Name = "xrTableCell8";
    this.xrTableCell8.Padding = new PaddingInfo(0, 5, 0, 0, 100f);
    this.xrTableCell8.StyleName = "LineStyle";
    this.xrTableCell8.StylePriority.UseBorders = false;
    this.xrTableCell8.StylePriority.UsePadding = false;
    this.xrTableCell8.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell8, "xrTableCell8");
    this.xrTableCell9.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
    this.xrTableCell9.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Unit]")
    });
    this.xrTableCell9.Name = "xrTableCell9";
    this.xrTableCell9.StyleName = "LineStyle";
    this.xrTableCell9.StylePriority.UseBorders = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell9, "xrTableCell9");
    this.xrTableCell10.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
    this.xrTableCell10.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Price]")
    });
    this.xrTableCell10.Name = "xrTableCell10";
    this.xrTableCell10.Padding = new PaddingInfo(0, 5, 0, 0, 100f);
    this.xrTableCell10.StyleName = "LineStyle";
    this.xrTableCell10.StylePriority.UseBorders = false;
    this.xrTableCell10.StylePriority.UsePadding = false;
    this.xrTableCell10.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell10, "xrTableCell10");
    this.xrTableCell11.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
    this.xrTableCell11.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Currency]")
    });
    this.xrTableCell11.Name = "xrTableCell11";
    this.xrTableCell11.StyleName = "LineStyle";
    this.xrTableCell11.StylePriority.UseBorders = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell11, "xrTableCell11");
    this.xrTableCell12.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
    this.xrTableCell12.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Total]")
    });
    this.xrTableCell12.Name = "xrTableCell12";
    this.xrTableCell12.Padding = new PaddingInfo(0, 5, 0, 0, 100f);
    this.xrTableCell12.StyleName = "LineStyle";
    this.xrTableCell12.StylePriority.UseBorders = false;
    this.xrTableCell12.StylePriority.UsePadding = false;
    this.xrTableCell12.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell12, "xrTableCell12");
    this.objectDataSource1.DataSource = (object) typeof (InvoiceReport);
    this.objectDataSource1.Name = "objectDataSource1";
    componentResourceManager.ApplyResources((object) this.xrTable1, "xrTable1");
    this.xrTable1.Name = "xrTable1";
    this.xrTable1.Rows.AddRange(new XRTableRow[1]
    {
      this.xrTableRow1
    });
    this.xrTableRow1.Cells.AddRange(new XRTableCell[7]
    {
      this.xrTableCell13,
      this.xrTableCell1,
      this.xrTableCell2,
      this.xrTableCell4,
      this.xrTableCell3,
      this.xrTableCell5,
      this.xrTableCell6
    });
    this.xrTableRow1.Name = "xrTableRow1";
    componentResourceManager.ApplyResources((object) this.xrTableRow1, "xrTableRow1");
    this.xrTableCell13.Name = "xrTableCell13";
    this.xrTableCell13.StyleName = "LineHeaderStyle";
    this.xrTableCell13.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell13, "xrTableCell13");
    this.xrTableCell1.Name = "xrTableCell1";
    this.xrTableCell1.StyleName = "LineHeaderStyle";
    componentResourceManager.ApplyResources((object) this.xrTableCell1, "xrTableCell1");
    this.xrTableCell2.Name = "xrTableCell2";
    this.xrTableCell2.Padding = new PaddingInfo(0, 5, 0, 0, 100f);
    this.xrTableCell2.StyleName = "LineHeaderStyle";
    this.xrTableCell2.StylePriority.UsePadding = false;
    this.xrTableCell2.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell2, "xrTableCell2");
    this.xrTableCell4.Name = "xrTableCell4";
    this.xrTableCell4.StyleName = "LineHeaderStyle";
    componentResourceManager.ApplyResources((object) this.xrTableCell4, "xrTableCell4");
    this.xrTableCell3.Name = "xrTableCell3";
    this.xrTableCell3.Padding = new PaddingInfo(0, 5, 0, 0, 100f);
    this.xrTableCell3.StyleName = "LineHeaderStyle";
    this.xrTableCell3.StylePriority.UsePadding = false;
    this.xrTableCell3.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell3, "xrTableCell3");
    this.xrTableCell5.Name = "xrTableCell5";
    this.xrTableCell5.StyleName = "LineHeaderStyle";
    componentResourceManager.ApplyResources((object) this.xrTableCell5, "xrTableCell5");
    this.xrTableCell6.Name = "xrTableCell6";
    this.xrTableCell6.Padding = new PaddingInfo(0, 5, 0, 0, 100f);
    this.xrTableCell6.StyleName = "LineHeaderStyle";
    this.xrTableCell6.StylePriority.UsePadding = false;
    this.xrTableCell6.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell6, "xrTableCell6");
    this.LineHeaderStyle.Borders = BorderSide.All;
    this.LineHeaderStyle.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Bold, GraphicsUnit.Point, (byte) 0);
    this.LineHeaderStyle.Name = "LineHeaderStyle";
    this.LineHeaderStyle.Padding = new PaddingInfo(5, 0, 0, 0, 100f);
    this.LineHeaderStyle.TextAlignment = TextAlignment.MiddleLeft;
    this.LineStyle.Name = "LineStyle";
    this.LineStyle.Padding = new PaddingInfo(5, 0, 0, 0, 100f);
    this.LineStyle.TextAlignment = TextAlignment.MiddleLeft;
    this.ReportFooter.Controls.AddRange(new XRControl[25]
    {
      (XRControl) this.xrLabel29,
      (XRControl) this.xrLabel28,
      (XRControl) this.xrLabel27,
      (XRControl) this.xrLabel26,
      (XRControl) this.xrLabel25,
      (XRControl) this.xrLabel24,
      (XRControl) this.xrLabel17,
      (XRControl) this.xrLabel15,
      (XRControl) this.xrLabel14,
      (XRControl) this.xrLabel16,
      (XRControl) this.xrLabel13,
      (XRControl) this.xrLabel12,
      (XRControl) this.xrLabel6,
      (XRControl) this.xrLabel7,
      (XRControl) this.xrLabel8,
      (XRControl) this.xrLabel9,
      (XRControl) this.xrLine1,
      (XRControl) this.xrLabel10,
      (XRControl) this.xrLine2,
      (XRControl) this.xrLabel11,
      (XRControl) this.xrLabel20,
      (XRControl) this.xrLabel21,
      (XRControl) this.xrLabel22,
      (XRControl) this.xrLine3,
      (XRControl) this.xrLabel23
    });
    componentResourceManager.ApplyResources((object) this.ReportFooter, "ReportFooter");
    this.ReportFooter.Name = "ReportFooter";
    this.xrLabel29.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[GrandTotal]")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel29, "xrLabel29");
    this.xrLabel29.Name = "xrLabel29";
    this.xrLabel29.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel29.StyleName = "PropertyStyle";
    this.xrLabel29.StylePriority.UseTextAlignment = false;
    this.xrLabel28.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[DiscountsTotal]")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel28, "xrLabel28");
    this.xrLabel28.Name = "xrLabel28";
    this.xrLabel28.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel28.StyleName = "PropertyStyle";
    this.xrLabel28.StylePriority.UseTextAlignment = false;
    this.xrLabel27.ExpressionBindings.AddRange(new ExpressionBinding[2]
    {
      new ExpressionBinding("BeforePrint", "Text", "[PartnerNextBalance]"),
      new ExpressionBinding("BeforePrint", "Visible", "Iif(IsNullOrEmpty([Partner]), 'False', ?)")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel27, "xrLabel27");
    this.xrLabel27.Name = "xrLabel27";
    this.xrLabel27.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel27.StyleName = "PropertyStyle";
    this.xrLabel27.StylePriority.UseTextAlignment = false;
    this.xrLabel26.ExpressionBindings.AddRange(new ExpressionBinding[2]
    {
      new ExpressionBinding("BeforePrint", "Text", "[PartnerCreditEffect]"),
      new ExpressionBinding("BeforePrint", "Visible", "Iif(IsNullOrEmpty([Partner]), 'False', ?)")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel26, "xrLabel26");
    this.xrLabel26.Name = "xrLabel26";
    this.xrLabel26.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel26.StyleName = "PropertyStyle";
    this.xrLabel26.StylePriority.UseTextAlignment = false;
    this.xrLabel25.ExpressionBindings.AddRange(new ExpressionBinding[2]
    {
      new ExpressionBinding("BeforePrint", "Text", "[PartnerDebitEffect]"),
      new ExpressionBinding("BeforePrint", "Visible", "Iif(IsNullOrEmpty([Partner]), 'False', ?)")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel25, "xrLabel25");
    this.xrLabel25.Name = "xrLabel25";
    this.xrLabel25.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel25.StyleName = "PropertyStyle";
    this.xrLabel25.StylePriority.UseTextAlignment = false;
    this.xrLabel24.ExpressionBindings.AddRange(new ExpressionBinding[2]
    {
      new ExpressionBinding("BeforePrint", "Text", "[PartnerPrevBalance]"),
      new ExpressionBinding("BeforePrint", "Visible", "Iif(IsNullOrEmpty([Partner]), 'False', ?)")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel24, "xrLabel24");
    this.xrLabel24.Name = "xrLabel24";
    this.xrLabel24.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel24.StyleName = "PropertyStyle";
    this.xrLabel24.StylePriority.UseTextAlignment = false;
    this.xrLabel17.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[DueDate]")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel17, "xrLabel17");
    this.xrLabel17.Name = "xrLabel17";
    this.xrLabel17.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel17.StyleName = "PropertyStyle";
    this.xrLabel15.ExpressionBindings.AddRange(new ExpressionBinding[2]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Partner]"),
      new ExpressionBinding("BeforePrint", "Visible", "Iif(IsNullOrEmpty([Partner]), 'False', ?)")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel15, "xrLabel15");
    this.xrLabel15.Name = "xrLabel15";
    this.xrLabel15.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel15.StyleName = "PropertyStyle";
    this.xrLabel14.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Visible", "Iif(IsNullOrEmpty([Partner]), 'False', ?)")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel14, "xrLabel14");
    this.xrLabel14.Name = "xrLabel14";
    this.xrLabel14.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel14.StyleName = "PropertyHeaderStyle";
    componentResourceManager.ApplyResources((object) this.xrLabel16, "xrLabel16");
    this.xrLabel16.Name = "xrLabel16";
    this.xrLabel16.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel16.StyleName = "PropertyHeaderStyle";
    this.xrLabel13.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[ChangesTotal]")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel13, "xrLabel13");
    this.xrLabel13.Name = "xrLabel13";
    this.xrLabel13.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel13.StyleName = "PropertyStyle";
    this.xrLabel13.StylePriority.UseTextAlignment = false;
    this.xrLabel12.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[PaymentsTotal]")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel12, "xrLabel12");
    this.xrLabel12.Name = "xrLabel12";
    this.xrLabel12.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel12.StyleName = "PropertyStyle";
    this.xrLabel12.StylePriority.UseTextAlignment = false;
    this.xrLabel6.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Total]")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel6, "xrLabel6");
    this.xrLabel6.Name = "xrLabel6";
    this.xrLabel6.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel6.StyleName = "PropertyStyle";
    this.xrLabel6.StylePriority.UsePadding = false;
    this.xrLabel6.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrLabel7, "xrLabel7");
    this.xrLabel7.Name = "xrLabel7";
    this.xrLabel7.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel7.StyleName = "PropertyHeaderStyle";
    this.xrLabel7.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrLabel8, "xrLabel8");
    this.xrLabel8.Name = "xrLabel8";
    this.xrLabel8.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel8.StyleName = "PropertyHeaderStyle";
    this.xrLabel8.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrLabel9, "xrLabel9");
    this.xrLabel9.Name = "xrLabel9";
    this.xrLabel9.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel9.StyleName = "PropertyHeaderStyle";
    this.xrLabel9.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrLine1, "xrLine1");
    this.xrLine1.Name = "xrLine1";
    componentResourceManager.ApplyResources((object) this.xrLabel10, "xrLabel10");
    this.xrLabel10.Name = "xrLabel10";
    this.xrLabel10.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel10.StyleName = "PropertyHeaderStyle";
    this.xrLabel10.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrLine2, "xrLine2");
    this.xrLine2.Name = "xrLine2";
    componentResourceManager.ApplyResources((object) this.xrLabel11, "xrLabel11");
    this.xrLabel11.Name = "xrLabel11";
    this.xrLabel11.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel11.StyleName = "PropertyHeaderStyle";
    this.xrLabel11.StylePriority.UseTextAlignment = false;
    this.xrLabel20.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Visible", "Iif(IsNullOrEmpty([Partner]), 'False', ?)")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel20, "xrLabel20");
    this.xrLabel20.Name = "xrLabel20";
    this.xrLabel20.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel20.StyleName = "PropertyHeaderStyle";
    this.xrLabel20.StylePriority.UseTextAlignment = false;
    this.xrLabel21.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Visible", "Iif(IsNullOrEmpty([Partner]), 'False', ?)")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel21, "xrLabel21");
    this.xrLabel21.Name = "xrLabel21";
    this.xrLabel21.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel21.StyleName = "PropertyHeaderStyle";
    this.xrLabel21.StylePriority.UseTextAlignment = false;
    this.xrLabel22.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Visible", "Iif(IsNullOrEmpty([Partner]), 'False', ?)")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel22, "xrLabel22");
    this.xrLabel22.Name = "xrLabel22";
    this.xrLabel22.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel22.StyleName = "PropertyHeaderStyle";
    this.xrLabel22.StylePriority.UseTextAlignment = false;
    this.xrLine3.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Visible", "Iif(IsNullOrEmpty([Partner]), 'False', ?)")
    });
    componentResourceManager.ApplyResources((object) this.xrLine3, "xrLine3");
    this.xrLine3.Name = "xrLine3";
    this.xrLabel23.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Visible", "Iif(IsNullOrEmpty([Partner]), 'False', ?)")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel23, "xrLabel23");
    this.xrLabel23.Name = "xrLabel23";
    this.xrLabel23.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel23.StyleName = "PropertyHeaderStyle";
    this.xrLabel23.StylePriority.UseTextAlignment = false;
    this.PageFooter.Controls.AddRange(new XRControl[2]
    {
      (XRControl) this.xrPageInfo2,
      (XRControl) this.xrPageInfo1
    });
    componentResourceManager.ApplyResources((object) this.PageFooter, "PageFooter");
    this.PageFooter.Name = "PageFooter";
    componentResourceManager.ApplyResources((object) this.xrPageInfo2, "xrPageInfo2");
    this.xrPageInfo2.Name = "xrPageInfo2";
    this.xrPageInfo2.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrPageInfo2.StylePriority.UseForeColor = false;
    componentResourceManager.ApplyResources((object) this.xrPageInfo1, "xrPageInfo1");
    this.xrPageInfo1.Name = "xrPageInfo1";
    this.xrPageInfo1.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrPageInfo1.PageInfo = PageInfo.DateTime;
    this.xrPageInfo1.StylePriority.UseForeColor = false;
    this.PropertyHeaderStyle.Font = new Font("Microsoft Sans Serif", 9f, FontStyle.Bold, GraphicsUnit.Point, (byte) 0);
    this.PropertyHeaderStyle.Name = "PropertyHeaderStyle";
    this.PropertyHeaderStyle.TextAlignment = TextAlignment.MiddleLeft;
    this.PageHeader.Controls.AddRange(new XRControl[1]
    {
      (XRControl) this.xrTable1
    });
    componentResourceManager.ApplyResources((object) this.PageHeader, "PageHeader");
    this.PageHeader.Name = "PageHeader";
    this.Bands.AddRange(new Band[8]
    {
      (Band) this.Detail,
      (Band) this.TopMargin,
      (Band) this.BottomMargin,
      (Band) this.ReportHeader,
      (Band) this.DetailReport,
      (Band) this.ReportFooter,
      (Band) this.PageFooter,
      (Band) this.PageHeader
    });
    this.ComponentStorage.AddRange(new IComponent[1]
    {
      (IComponent) this.objectDataSource1
    });
    this.DataSource = (object) this.objectDataSource1;
    componentResourceManager.ApplyResources((object) this, "$this");
    this.StyleSheet.AddRange(new XRControlStyle[5]
    {
      this.TypeHeaderStyle,
      this.PropertyStyle,
      this.LineHeaderStyle,
      this.LineStyle,
      this.PropertyHeaderStyle
    });
    this.Version = "18.1";
    this.xrTable2.EndInit();
    this.objectDataSource1.EndInit();
    this.xrTable1.EndInit();
    this.EndInit();
  }
}
