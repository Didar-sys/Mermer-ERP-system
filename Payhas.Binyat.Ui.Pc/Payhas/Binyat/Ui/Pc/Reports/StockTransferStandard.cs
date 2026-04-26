// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Reports.StockTransferStandard
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

public class StockTransferStandard : XtraReport
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
  private XRTableCell xrTableCell4;
  private XRTableCell xrTableCell5;
  private XRControlStyle LineHeaderStyle;
  private XRControlStyle LineStyle;
  private ReportFooterBand ReportFooter;
  private PageFooterBand PageFooter;
  private XRPageInfo xrPageInfo2;
  private XRPageInfo xrPageInfo1;
  private XRControlStyle PropertyHeaderStyle;
  private XRCheckBox xrCheckBox3;
  private XRLabel xrLabel9;
  private XRLabel xrLabel8;
  private XRTableCell xrTableCell6;
  private XRTableCell xrTableCell15;
  private XRTableCell xrTableCell16;
  private XRTableCell xrTableCell17;
  private XRTableCell xrTableCell18;
  private XRTableCell xrTableCell12;
  private XRTableCell xrTableCell13;
  private XRTableCell xrTableCell14;
  private XRLabel xrLabel10;
  private XRLabel xrLabel11;
  private XRLabel xrLabel7;
  private XRLabel xrLabel6;
  private XRTableCell xrTableCell20;
  private XRTableCell xrTableCell19;
  private PageHeaderBand PageHeader;
  private XRLabel xrLabel30;
  private XRLabel xrLabel31;

  public StockTransferStandard() => this.InitializeComponent();

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    this.components = (IContainer) new System.ComponentModel.Container();
    ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (StockTransferStandard));
    EAN13Generator eaN13Generator = new EAN13Generator();
    this.Detail = new DetailBand();
    this.TopMargin = new TopMarginBand();
    this.BottomMargin = new BottomMarginBand();
    this.xrLabel1 = new XRLabel();
    this.ReportHeader = new ReportHeaderBand();
    this.xrLabel30 = new XRLabel();
    this.xrLabel31 = new XRLabel();
    this.xrCheckBox3 = new XRCheckBox();
    this.xrLabel9 = new XRLabel();
    this.xrLabel3 = new XRLabel();
    this.xrCheckBox2 = new XRCheckBox();
    this.xrCheckBox1 = new XRCheckBox();
    this.xrLabel2 = new XRLabel();
    this.xrBarCode1 = new XRBarCode();
    this.xrLabel4 = new XRLabel();
    this.xrLabel5 = new XRLabel();
    this.xrLabel8 = new XRLabel();
    this.TypeHeaderStyle = new XRControlStyle();
    this.PropertyStyle = new XRControlStyle();
    this.DetailReport = new DetailReportBand();
    this.Detail1 = new DetailBand();
    this.xrTable2 = new XRTable();
    this.xrTableRow2 = new XRTableRow();
    this.xrTableCell20 = new XRTableCell();
    this.xrTableCell7 = new XRTableCell();
    this.xrTableCell15 = new XRTableCell();
    this.xrTableCell16 = new XRTableCell();
    this.xrTableCell8 = new XRTableCell();
    this.xrTableCell9 = new XRTableCell();
    this.xrTableCell17 = new XRTableCell();
    this.xrTableCell10 = new XRTableCell();
    this.xrTableCell11 = new XRTableCell();
    this.xrTableCell18 = new XRTableCell();
    this.objectDataSource1 = new ObjectDataSource(this.components);
    this.xrTable1 = new XRTable();
    this.xrTableRow1 = new XRTableRow();
    this.xrTableCell19 = new XRTableCell();
    this.xrTableCell1 = new XRTableCell();
    this.xrTableCell6 = new XRTableCell();
    this.xrTableCell12 = new XRTableCell();
    this.xrTableCell2 = new XRTableCell();
    this.xrTableCell4 = new XRTableCell();
    this.xrTableCell13 = new XRTableCell();
    this.xrTableCell3 = new XRTableCell();
    this.xrTableCell5 = new XRTableCell();
    this.xrTableCell14 = new XRTableCell();
    this.LineHeaderStyle = new XRControlStyle();
    this.LineStyle = new XRControlStyle();
    this.ReportFooter = new ReportFooterBand();
    this.xrLabel10 = new XRLabel();
    this.xrLabel11 = new XRLabel();
    this.xrLabel7 = new XRLabel();
    this.xrLabel6 = new XRLabel();
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
    componentResourceManager.ApplyResources((object) this.xrLabel1, "xrLabel1");
    this.xrLabel1.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Type]")
    });
    this.xrLabel1.Name = "xrLabel1";
    this.xrLabel1.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel1.StyleName = "TypeHeaderStyle";
    this.xrLabel1.StylePriority.UseTextAlignment = false;
    this.ReportHeader.Controls.AddRange(new XRControl[13]
    {
      (XRControl) this.xrLabel30,
      (XRControl) this.xrLabel31,
      (XRControl) this.xrCheckBox3,
      (XRControl) this.xrLabel9,
      (XRControl) this.xrLabel3,
      (XRControl) this.xrCheckBox2,
      (XRControl) this.xrCheckBox1,
      (XRControl) this.xrLabel2,
      (XRControl) this.xrLabel1,
      (XRControl) this.xrBarCode1,
      (XRControl) this.xrLabel4,
      (XRControl) this.xrLabel5,
      (XRControl) this.xrLabel8
    });
    componentResourceManager.ApplyResources((object) this.ReportHeader, "ReportHeader");
    this.ReportHeader.Name = "ReportHeader";
    componentResourceManager.ApplyResources((object) this.xrLabel30, "xrLabel30");
    this.xrLabel30.Multiline = true;
    this.xrLabel30.Name = "xrLabel30";
    this.xrLabel30.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel30.StyleName = "PropertyHeaderStyle";
    componentResourceManager.ApplyResources((object) this.xrLabel31, "xrLabel31");
    this.xrLabel31.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[UserName]")
    });
    this.xrLabel31.Multiline = true;
    this.xrLabel31.Name = "xrLabel31";
    this.xrLabel31.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel31.StyleName = "PropertyStyle";
    componentResourceManager.ApplyResources((object) this.xrCheckBox3, "xrCheckBox3");
    this.xrCheckBox3.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "CheckState", "[IsConflicted]")
    });
    this.xrCheckBox3.Name = "xrCheckBox3";
    componentResourceManager.ApplyResources((object) this.xrLabel9, "xrLabel9");
    this.xrLabel9.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[DestinationWarehouse]")
    });
    this.xrLabel9.Name = "xrLabel9";
    this.xrLabel9.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel9.StyleName = "PropertyStyle";
    componentResourceManager.ApplyResources((object) this.xrLabel3, "xrLabel3");
    this.xrLabel3.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Warehouse]")
    });
    this.xrLabel3.Name = "xrLabel3";
    this.xrLabel3.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel3.StyleName = "PropertyStyle";
    componentResourceManager.ApplyResources((object) this.xrCheckBox2, "xrCheckBox2");
    this.xrCheckBox2.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "CheckState", "[IsDisabled]")
    });
    this.xrCheckBox2.Name = "xrCheckBox2";
    this.xrCheckBox2.StyleName = "PropertyStyle";
    componentResourceManager.ApplyResources((object) this.xrCheckBox1, "xrCheckBox1");
    this.xrCheckBox1.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "CheckState", "[IsCompleted]")
    });
    this.xrCheckBox1.Name = "xrCheckBox1";
    this.xrCheckBox1.StyleName = "PropertyStyle";
    componentResourceManager.ApplyResources((object) this.xrLabel2, "xrLabel2");
    this.xrLabel2.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Date]")
    });
    this.xrLabel2.Name = "xrLabel2";
    this.xrLabel2.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel2.StyleName = "PropertyStyle";
    this.xrBarCode1.Alignment = TextAlignment.TopCenter;
    componentResourceManager.ApplyResources((object) this.xrBarCode1, "xrBarCode1");
    this.xrBarCode1.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Code]")
    });
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
    componentResourceManager.ApplyResources((object) this.xrLabel8, "xrLabel8");
    this.xrLabel8.Name = "xrLabel8";
    this.xrLabel8.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel8.StyleName = "PropertyHeaderStyle";
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
    this.xrTableRow2.Cells.AddRange(new XRTableCell[10]
    {
      this.xrTableCell20,
      this.xrTableCell7,
      this.xrTableCell15,
      this.xrTableCell16,
      this.xrTableCell8,
      this.xrTableCell9,
      this.xrTableCell17,
      this.xrTableCell10,
      this.xrTableCell11,
      this.xrTableCell18
    });
    componentResourceManager.ApplyResources((object) this.xrTableRow2, "xrTableRow2");
    this.xrTableRow2.Name = "xrTableRow2";
    this.xrTableCell20.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
    componentResourceManager.ApplyResources((object) this.xrTableCell20, "xrTableCell20");
    this.xrTableCell20.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[RowNo]")
    });
    this.xrTableCell20.Name = "xrTableCell20";
    this.xrTableCell20.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
    this.xrTableCell20.StyleName = "LineStyle";
    this.xrTableCell20.StylePriority.UseBorders = false;
    this.xrTableCell20.StylePriority.UsePadding = false;
    this.xrTableCell20.StylePriority.UseTextAlignment = false;
    this.xrTableCell7.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
    componentResourceManager.ApplyResources((object) this.xrTableCell7, "xrTableCell7");
    this.xrTableCell7.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Stock]")
    });
    this.xrTableCell7.Name = "xrTableCell7";
    this.xrTableCell7.StyleName = "LineStyle";
    this.xrTableCell7.StylePriority.UseBorders = false;
    this.xrTableCell15.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
    componentResourceManager.ApplyResources((object) this.xrTableCell15, "xrTableCell15");
    this.xrTableCell15.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Price]")
    });
    this.xrTableCell15.Name = "xrTableCell15";
    this.xrTableCell15.Padding = new PaddingInfo(0, 5, 0, 0, 100f);
    this.xrTableCell15.StyleName = "LineStyle";
    this.xrTableCell15.StylePriority.UseBorders = false;
    this.xrTableCell15.StylePriority.UsePadding = false;
    this.xrTableCell15.StylePriority.UseTextAlignment = false;
    this.xrTableCell16.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
    componentResourceManager.ApplyResources((object) this.xrTableCell16, "xrTableCell16");
    this.xrTableCell16.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Currency]")
    });
    this.xrTableCell16.Name = "xrTableCell16";
    this.xrTableCell16.Padding = new PaddingInfo(0, 5, 0, 0, 100f);
    this.xrTableCell16.StyleName = "LineStyle";
    this.xrTableCell16.StylePriority.UseBorders = false;
    this.xrTableCell16.StylePriority.UsePadding = false;
    this.xrTableCell16.StylePriority.UseTextAlignment = false;
    this.xrTableCell8.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
    componentResourceManager.ApplyResources((object) this.xrTableCell8, "xrTableCell8");
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
    this.xrTableCell9.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
    componentResourceManager.ApplyResources((object) this.xrTableCell9, "xrTableCell9");
    this.xrTableCell9.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Unit]")
    });
    this.xrTableCell9.Name = "xrTableCell9";
    this.xrTableCell9.StyleName = "LineStyle";
    this.xrTableCell9.StylePriority.UseBorders = false;
    this.xrTableCell17.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
    componentResourceManager.ApplyResources((object) this.xrTableCell17, "xrTableCell17");
    this.xrTableCell17.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Total]")
    });
    this.xrTableCell17.Name = "xrTableCell17";
    this.xrTableCell17.StyleName = "LineStyle";
    this.xrTableCell17.StylePriority.UseBorders = false;
    this.xrTableCell17.StylePriority.UseTextAlignment = false;
    this.xrTableCell10.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
    componentResourceManager.ApplyResources((object) this.xrTableCell10, "xrTableCell10");
    this.xrTableCell10.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[ReceivedQuantity]")
    });
    this.xrTableCell10.Name = "xrTableCell10";
    this.xrTableCell10.Padding = new PaddingInfo(0, 5, 0, 0, 100f);
    this.xrTableCell10.StyleName = "LineStyle";
    this.xrTableCell10.StylePriority.UseBorders = false;
    this.xrTableCell10.StylePriority.UsePadding = false;
    this.xrTableCell10.StylePriority.UseTextAlignment = false;
    this.xrTableCell11.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
    componentResourceManager.ApplyResources((object) this.xrTableCell11, "xrTableCell11");
    this.xrTableCell11.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[ReceivedUnit]")
    });
    this.xrTableCell11.Name = "xrTableCell11";
    this.xrTableCell11.StyleName = "LineStyle";
    this.xrTableCell11.StylePriority.UseBorders = false;
    this.xrTableCell18.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
    componentResourceManager.ApplyResources((object) this.xrTableCell18, "xrTableCell18");
    this.xrTableCell18.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[ReceivedTotal]")
    });
    this.xrTableCell18.Name = "xrTableCell18";
    this.xrTableCell18.StyleName = "LineStyle";
    this.xrTableCell18.StylePriority.UseBorders = false;
    this.xrTableCell18.StylePriority.UseTextAlignment = false;
    this.objectDataSource1.DataSource = (object) typeof (StockTransferReport);
    this.objectDataSource1.Name = "objectDataSource1";
    componentResourceManager.ApplyResources((object) this.xrTable1, "xrTable1");
    this.xrTable1.Name = "xrTable1";
    this.xrTable1.Rows.AddRange(new XRTableRow[1]
    {
      this.xrTableRow1
    });
    this.xrTableRow1.Cells.AddRange(new XRTableCell[10]
    {
      this.xrTableCell19,
      this.xrTableCell1,
      this.xrTableCell6,
      this.xrTableCell12,
      this.xrTableCell2,
      this.xrTableCell4,
      this.xrTableCell13,
      this.xrTableCell3,
      this.xrTableCell5,
      this.xrTableCell14
    });
    componentResourceManager.ApplyResources((object) this.xrTableRow1, "xrTableRow1");
    this.xrTableRow1.Name = "xrTableRow1";
    componentResourceManager.ApplyResources((object) this.xrTableCell19, "xrTableCell19");
    this.xrTableCell19.Name = "xrTableCell19";
    this.xrTableCell19.StyleName = "LineHeaderStyle";
    componentResourceManager.ApplyResources((object) this.xrTableCell1, "xrTableCell1");
    this.xrTableCell1.Name = "xrTableCell1";
    this.xrTableCell1.StyleName = "LineHeaderStyle";
    componentResourceManager.ApplyResources((object) this.xrTableCell6, "xrTableCell6");
    this.xrTableCell6.Name = "xrTableCell6";
    this.xrTableCell6.Padding = new PaddingInfo(0, 5, 0, 0, 100f);
    this.xrTableCell6.StyleName = "LineHeaderStyle";
    this.xrTableCell6.StylePriority.UsePadding = false;
    this.xrTableCell6.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell12, "xrTableCell12");
    this.xrTableCell12.Name = "xrTableCell12";
    this.xrTableCell12.Padding = new PaddingInfo(0, 5, 0, 0, 100f);
    this.xrTableCell12.StyleName = "LineHeaderStyle";
    this.xrTableCell12.StylePriority.UsePadding = false;
    this.xrTableCell12.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell2, "xrTableCell2");
    this.xrTableCell2.Name = "xrTableCell2";
    this.xrTableCell2.Padding = new PaddingInfo(0, 5, 0, 0, 100f);
    this.xrTableCell2.StyleName = "LineHeaderStyle";
    this.xrTableCell2.StylePriority.UsePadding = false;
    this.xrTableCell2.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell4, "xrTableCell4");
    this.xrTableCell4.Name = "xrTableCell4";
    this.xrTableCell4.StyleName = "LineHeaderStyle";
    componentResourceManager.ApplyResources((object) this.xrTableCell13, "xrTableCell13");
    this.xrTableCell13.Name = "xrTableCell13";
    this.xrTableCell13.StyleName = "LineHeaderStyle";
    this.xrTableCell13.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell3, "xrTableCell3");
    this.xrTableCell3.Name = "xrTableCell3";
    this.xrTableCell3.Padding = new PaddingInfo(0, 5, 0, 0, 100f);
    this.xrTableCell3.StyleName = "LineHeaderStyle";
    this.xrTableCell3.StylePriority.UsePadding = false;
    this.xrTableCell3.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell5, "xrTableCell5");
    this.xrTableCell5.Name = "xrTableCell5";
    this.xrTableCell5.StyleName = "LineHeaderStyle";
    componentResourceManager.ApplyResources((object) this.xrTableCell14, "xrTableCell14");
    this.xrTableCell14.Name = "xrTableCell14";
    this.xrTableCell14.StyleName = "LineHeaderStyle";
    this.xrTableCell14.StylePriority.UseTextAlignment = false;
    this.LineHeaderStyle.Borders = BorderSide.All;
    this.LineHeaderStyle.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Bold, GraphicsUnit.Point, (byte) 0);
    this.LineHeaderStyle.Name = "LineHeaderStyle";
    this.LineHeaderStyle.Padding = new PaddingInfo(5, 0, 0, 0, 100f);
    this.LineHeaderStyle.TextAlignment = TextAlignment.MiddleLeft;
    this.LineStyle.Name = "LineStyle";
    this.LineStyle.Padding = new PaddingInfo(5, 0, 0, 0, 100f);
    this.LineStyle.TextAlignment = TextAlignment.MiddleLeft;
    this.ReportFooter.Controls.AddRange(new XRControl[4]
    {
      (XRControl) this.xrLabel10,
      (XRControl) this.xrLabel11,
      (XRControl) this.xrLabel7,
      (XRControl) this.xrLabel6
    });
    componentResourceManager.ApplyResources((object) this.ReportFooter, "ReportFooter");
    this.ReportFooter.Name = "ReportFooter";
    componentResourceManager.ApplyResources((object) this.xrLabel10, "xrLabel10");
    this.xrLabel10.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[ReceivedTotal]")
    });
    this.xrLabel10.Name = "xrLabel10";
    this.xrLabel10.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel10.StyleName = "PropertyStyle";
    componentResourceManager.ApplyResources((object) this.xrLabel11, "xrLabel11");
    this.xrLabel11.Name = "xrLabel11";
    this.xrLabel11.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel11.StyleName = "PropertyHeaderStyle";
    this.xrLabel11.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrLabel7, "xrLabel7");
    this.xrLabel7.Name = "xrLabel7";
    this.xrLabel7.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel7.StyleName = "PropertyHeaderStyle";
    this.xrLabel7.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrLabel6, "xrLabel6");
    this.xrLabel6.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Total]")
    });
    this.xrLabel6.Name = "xrLabel6";
    this.xrLabel6.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel6.StyleName = "PropertyStyle";
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
