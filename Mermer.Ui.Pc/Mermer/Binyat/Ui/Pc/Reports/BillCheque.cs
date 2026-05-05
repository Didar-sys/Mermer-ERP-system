// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Reports.BillCheque
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using DevExpress.DataAccess.ObjectBinding;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.BarCode;
using DevExpress.XtraReports.UI;
using Mermer.Ui.Pc.Reports.Models;
using System.ComponentModel;
using System.Drawing;

#nullable disable
namespace Mermer.Ui.Pc.Reports;

public class BillCheque : XtraReport
{
  private IContainer components;
  private DetailBand Detail;
  private TopMarginBand TopMargin;
  private BottomMarginBand BottomMargin;
  private XRLabel xrLabel1;
  private ObjectDataSource objectDataSource1;
  private ReportHeaderBand ReportHeader;
  private XRControlStyle TypeHeaderStyle;
  private XRCheckBox xrCheckBox2;
  private XRCheckBox xrCheckBox1;
  private XRLabel xrLabel2;
  private XRBarCode xrBarCode1;
  private XRControlStyle PropertyStyle;
  private DetailReportBand DetailReport;
  private DetailBand Detail1;
  private GroupHeaderBand GroupHeader1;
  private XRTable xrTable1;
  private XRTableRow xrTableRow1;
  private XRTableCell xrTableCell1;
  private XRTableCell xrTableCell3;
  private XRTable xrTable2;
  private XRTableRow xrTableRow2;
  private XRTableCell xrTableCell7;
  private XRTableCell xrTableCell10;
  private XRTableCell xrTableCell12;
  private XRTableCell xrTableCell6;
  private XRControlStyle LineHeaderStyle;
  private XRControlStyle LineStyle;
  private ReportFooterBand ReportFooter;
  private XRControlStyle PropertyHeaderStyle;
  private XRLabel xrLabel19;
  private XRLabel xrLabel27;
  private XRLabel xrLabel26;
  private XRLabel xrLabel25;
  private XRLabel xrLabel24;
  private XRLabel xrLabel15;
  private XRLabel xrLabel9;
  private XRLabel xrLabel20;
  private XRLabel xrLabel21;
  private XRLabel xrLabel22;
  private XRLine xrLine3;
  private XRLabel xrLabel23;
  private XRLabel xrLabel29;
  private XRLabel xrLabel3;

  public BillCheque() => this.InitializeComponent();

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    this.components = (IContainer) new System.ComponentModel.Container();
    ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (BillCheque));
    EAN13Generator eaN13Generator = new EAN13Generator();
    this.Detail = new DetailBand();
    this.TopMargin = new TopMarginBand();
    this.BottomMargin = new BottomMarginBand();
    this.xrLabel1 = new XRLabel();
    this.ReportHeader = new ReportHeaderBand();
    this.xrLabel19 = new XRLabel();
    this.xrCheckBox2 = new XRCheckBox();
    this.xrCheckBox1 = new XRCheckBox();
    this.xrLabel2 = new XRLabel();
    this.xrBarCode1 = new XRBarCode();
    this.TypeHeaderStyle = new XRControlStyle();
    this.PropertyStyle = new XRControlStyle();
    this.DetailReport = new DetailReportBand();
    this.Detail1 = new DetailBand();
    this.xrTable2 = new XRTable();
    this.xrTableRow2 = new XRTableRow();
    this.xrTableCell7 = new XRTableCell();
    this.xrTableCell10 = new XRTableCell();
    this.xrTableCell12 = new XRTableCell();
    this.GroupHeader1 = new GroupHeaderBand();
    this.xrTable1 = new XRTable();
    this.xrTableRow1 = new XRTableRow();
    this.xrTableCell1 = new XRTableCell();
    this.xrTableCell3 = new XRTableCell();
    this.xrTableCell6 = new XRTableCell();
    this.objectDataSource1 = new ObjectDataSource(this.components);
    this.LineHeaderStyle = new XRControlStyle();
    this.LineStyle = new XRControlStyle();
    this.ReportFooter = new ReportFooterBand();
    this.xrLabel15 = new XRLabel();
    this.xrLabel29 = new XRLabel();
    this.xrLabel27 = new XRLabel();
    this.xrLabel26 = new XRLabel();
    this.xrLabel25 = new XRLabel();
    this.xrLabel24 = new XRLabel();
    this.xrLabel9 = new XRLabel();
    this.xrLabel20 = new XRLabel();
    this.xrLabel21 = new XRLabel();
    this.xrLabel22 = new XRLabel();
    this.xrLine3 = new XRLine();
    this.xrLabel23 = new XRLabel();
    this.PropertyHeaderStyle = new XRControlStyle();
    this.xrLabel3 = new XRLabel();
    this.xrTable2.BeginInit();
    this.xrTable1.BeginInit();
    this.objectDataSource1.BeginInit();
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
    this.xrLabel1.StylePriority.UseFont = false;
    this.xrLabel1.StylePriority.UseTextAlignment = false;
    this.ReportHeader.Controls.AddRange(new XRControl[7]
    {
      (XRControl) this.xrLabel3,
      (XRControl) this.xrLabel19,
      (XRControl) this.xrCheckBox2,
      (XRControl) this.xrCheckBox1,
      (XRControl) this.xrLabel2,
      (XRControl) this.xrLabel1,
      (XRControl) this.xrBarCode1
    });
    componentResourceManager.ApplyResources((object) this.ReportHeader, "ReportHeader");
    this.ReportHeader.Name = "ReportHeader";
    this.xrLabel19.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Depository]")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel19, "xrLabel19");
    this.xrLabel19.Name = "xrLabel19";
    this.xrLabel19.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel19.StyleName = "PropertyStyle";
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
    this.xrBarCode1.Padding = new PaddingInfo(10, 10, 5, 0, 100f);
    this.xrBarCode1.StylePriority.UsePadding = false;
    this.xrBarCode1.StylePriority.UseTextAlignment = false;
    this.xrBarCode1.Symbology = (BarCodeGeneratorBase) eaN13Generator;
    this.TypeHeaderStyle.BorderDashStyle = BorderDashStyle.DashDotDot;
    this.TypeHeaderStyle.Borders = BorderSide.Bottom;
    this.TypeHeaderStyle.Font = new Font("Microsoft Sans Serif", 24f, FontStyle.Bold, GraphicsUnit.Point, (byte) 0);
    this.TypeHeaderStyle.ForeColor = Color.Teal;
    this.TypeHeaderStyle.Name = "TypeHeaderStyle";
    this.TypeHeaderStyle.TextAlignment = TextAlignment.MiddleLeft;
    this.PropertyStyle.Name = "PropertyStyle";
    this.PropertyStyle.TextAlignment = TextAlignment.MiddleLeft;
    this.DetailReport.Bands.AddRange(new Band[2]
    {
      (Band) this.Detail1,
      (Band) this.GroupHeader1
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
    this.xrTableRow2.Cells.AddRange(new XRTableCell[3]
    {
      this.xrTableCell7,
      this.xrTableCell10,
      this.xrTableCell12
    });
    this.xrTableRow2.Name = "xrTableRow2";
    componentResourceManager.ApplyResources((object) this.xrTableRow2, "xrTableRow2");
    this.xrTableCell7.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
    this.xrTableCell7.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Amount]")
    });
    this.xrTableCell7.Name = "xrTableCell7";
    this.xrTableCell7.Padding = new PaddingInfo(0, 5, 0, 0, 100f);
    this.xrTableCell7.StyleName = "LineStyle";
    this.xrTableCell7.StylePriority.UseBorders = false;
    this.xrTableCell7.StylePriority.UsePadding = false;
    this.xrTableCell7.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell7, "xrTableCell7");
    this.xrTableCell10.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
    this.xrTableCell10.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Currency]")
    });
    this.xrTableCell10.Name = "xrTableCell10";
    this.xrTableCell10.Padding = new PaddingInfo(0, 5, 0, 0, 100f);
    this.xrTableCell10.StyleName = "LineStyle";
    this.xrTableCell10.StylePriority.UseBorders = false;
    this.xrTableCell10.StylePriority.UsePadding = false;
    this.xrTableCell10.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell10, "xrTableCell10");
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
    this.GroupHeader1.Controls.AddRange(new XRControl[1]
    {
      (XRControl) this.xrTable1
    });
    componentResourceManager.ApplyResources((object) this.GroupHeader1, "GroupHeader1");
    this.GroupHeader1.Name = "GroupHeader1";
    componentResourceManager.ApplyResources((object) this.xrTable1, "xrTable1");
    this.xrTable1.Name = "xrTable1";
    this.xrTable1.Rows.AddRange(new XRTableRow[1]
    {
      this.xrTableRow1
    });
    this.xrTableRow1.Cells.AddRange(new XRTableCell[3]
    {
      this.xrTableCell1,
      this.xrTableCell3,
      this.xrTableCell6
    });
    this.xrTableRow1.Name = "xrTableRow1";
    componentResourceManager.ApplyResources((object) this.xrTableRow1, "xrTableRow1");
    this.xrTableCell1.Name = "xrTableCell1";
    this.xrTableCell1.StyleName = "LineHeaderStyle";
    componentResourceManager.ApplyResources((object) this.xrTableCell1, "xrTableCell1");
    this.xrTableCell3.Name = "xrTableCell3";
    this.xrTableCell3.Padding = new PaddingInfo(0, 5, 0, 0, 100f);
    this.xrTableCell3.StyleName = "LineHeaderStyle";
    this.xrTableCell3.StylePriority.UsePadding = false;
    this.xrTableCell3.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell3, "xrTableCell3");
    this.xrTableCell6.Name = "xrTableCell6";
    this.xrTableCell6.Padding = new PaddingInfo(0, 5, 0, 0, 100f);
    this.xrTableCell6.StyleName = "LineHeaderStyle";
    this.xrTableCell6.StylePriority.UsePadding = false;
    this.xrTableCell6.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrTableCell6, "xrTableCell6");
    this.objectDataSource1.DataSource = (object) typeof (BillReport);
    this.objectDataSource1.Name = "objectDataSource1";
    this.LineHeaderStyle.Borders = BorderSide.All;
    this.LineHeaderStyle.Font = new Font("Microsoft Sans Serif", 8f, FontStyle.Bold, GraphicsUnit.Point, (byte) 0);
    this.LineHeaderStyle.Name = "LineHeaderStyle";
    this.LineHeaderStyle.Padding = new PaddingInfo(5, 0, 0, 0, 100f);
    this.LineHeaderStyle.TextAlignment = TextAlignment.MiddleLeft;
    this.LineStyle.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
    this.LineStyle.Name = "LineStyle";
    this.LineStyle.Padding = new PaddingInfo(5, 0, 0, 0, 100f);
    this.LineStyle.TextAlignment = TextAlignment.MiddleLeft;
    this.ReportFooter.Controls.AddRange(new XRControl[12]
    {
      (XRControl) this.xrLabel15,
      (XRControl) this.xrLabel29,
      (XRControl) this.xrLabel27,
      (XRControl) this.xrLabel26,
      (XRControl) this.xrLabel25,
      (XRControl) this.xrLabel24,
      (XRControl) this.xrLabel9,
      (XRControl) this.xrLabel20,
      (XRControl) this.xrLabel21,
      (XRControl) this.xrLabel22,
      (XRControl) this.xrLine3,
      (XRControl) this.xrLabel23
    });
    componentResourceManager.ApplyResources((object) this.ReportFooter, "ReportFooter");
    this.ReportFooter.Name = "ReportFooter";
    this.xrLabel15.ExpressionBindings.AddRange(new ExpressionBinding[2]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Partner]"),
      new ExpressionBinding("BeforePrint", "Visible", "Iif(IsNullOrEmpty([Partner]), False, ?)")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel15, "xrLabel15");
    this.xrLabel15.Name = "xrLabel15";
    this.xrLabel15.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel15.StyleName = "PropertyStyle";
    this.xrLabel29.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[Total]")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel29, "xrLabel29");
    this.xrLabel29.Name = "xrLabel29";
    this.xrLabel29.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel29.StyleName = "PropertyStyle";
    this.xrLabel29.StylePriority.UseTextAlignment = false;
    this.xrLabel27.ExpressionBindings.AddRange(new ExpressionBinding[2]
    {
      new ExpressionBinding("BeforePrint", "Text", "[PartnerNextBalance]"),
      new ExpressionBinding("BeforePrint", "Visible", "Iif(IsNullOrEmpty([Partner]), False, ?)")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel27, "xrLabel27");
    this.xrLabel27.Name = "xrLabel27";
    this.xrLabel27.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel27.StyleName = "PropertyStyle";
    this.xrLabel27.StylePriority.UseTextAlignment = false;
    this.xrLabel26.ExpressionBindings.AddRange(new ExpressionBinding[2]
    {
      new ExpressionBinding("BeforePrint", "Text", "[PartnerCreditEffect]"),
      new ExpressionBinding("BeforePrint", "Visible", "Iif(IsNullOrEmpty([Partner]), False, ?)")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel26, "xrLabel26");
    this.xrLabel26.Name = "xrLabel26";
    this.xrLabel26.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel26.StyleName = "PropertyStyle";
    this.xrLabel26.StylePriority.UseTextAlignment = false;
    this.xrLabel25.ExpressionBindings.AddRange(new ExpressionBinding[2]
    {
      new ExpressionBinding("BeforePrint", "Text", "[PartnerDebitEffect]"),
      new ExpressionBinding("BeforePrint", "Visible", "Iif(IsNullOrEmpty([Partner]), False, ?)")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel25, "xrLabel25");
    this.xrLabel25.Name = "xrLabel25";
    this.xrLabel25.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel25.StyleName = "PropertyStyle";
    this.xrLabel25.StylePriority.UseTextAlignment = false;
    this.xrLabel24.ExpressionBindings.AddRange(new ExpressionBinding[2]
    {
      new ExpressionBinding("BeforePrint", "Text", "[PartnerPrevBalance]"),
      new ExpressionBinding("BeforePrint", "Visible", "Iif(IsNullOrEmpty([Partner]), False, ?)")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel24, "xrLabel24");
    this.xrLabel24.Name = "xrLabel24";
    this.xrLabel24.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel24.StyleName = "PropertyStyle";
    this.xrLabel24.StylePriority.UseTextAlignment = false;
    componentResourceManager.ApplyResources((object) this.xrLabel9, "xrLabel9");
    this.xrLabel9.Name = "xrLabel9";
    this.xrLabel9.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel9.StyleName = "PropertyHeaderStyle";
    this.xrLabel9.StylePriority.UseTextAlignment = false;
    this.xrLabel20.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Visible", "Iif(IsNullOrEmpty([Partner]), False, ?)")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel20, "xrLabel20");
    this.xrLabel20.Name = "xrLabel20";
    this.xrLabel20.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel20.StyleName = "PropertyHeaderStyle";
    this.xrLabel20.StylePriority.UseTextAlignment = false;
    this.xrLabel21.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Visible", "Iif(IsNullOrEmpty([Partner]), False, ?)")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel21, "xrLabel21");
    this.xrLabel21.Name = "xrLabel21";
    this.xrLabel21.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel21.StyleName = "PropertyHeaderStyle";
    this.xrLabel21.StylePriority.UseTextAlignment = false;
    this.xrLabel22.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Visible", "Iif(IsNullOrEmpty([Partner]), False, ?)")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel22, "xrLabel22");
    this.xrLabel22.Name = "xrLabel22";
    this.xrLabel22.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel22.StyleName = "PropertyHeaderStyle";
    this.xrLabel22.StylePriority.UseTextAlignment = false;
    this.xrLine3.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Visible", "Iif(IsNullOrEmpty([Partner]), False, ?)")
    });
    componentResourceManager.ApplyResources((object) this.xrLine3, "xrLine3");
    this.xrLine3.Name = "xrLine3";
    this.xrLabel23.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Visible", "Iif(IsNullOrEmpty([Partner]), False, ?)")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel23, "xrLabel23");
    this.xrLabel23.Name = "xrLabel23";
    this.xrLabel23.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
    this.xrLabel23.StyleName = "PropertyHeaderStyle";
    this.xrLabel23.StylePriority.UseTextAlignment = false;
    this.PropertyHeaderStyle.Font = new Font("Microsoft Sans Serif", 9f, FontStyle.Bold, GraphicsUnit.Point, (byte) 0);
    this.PropertyHeaderStyle.Name = "PropertyHeaderStyle";
    this.PropertyHeaderStyle.TextAlignment = TextAlignment.MiddleLeft;
    this.xrLabel3.ExpressionBindings.AddRange(new ExpressionBinding[1]
    {
      new ExpressionBinding("BeforePrint", "Text", "[UserName]")
    });
    componentResourceManager.ApplyResources((object) this.xrLabel3, "xrLabel3");
    this.xrLabel3.Multiline = true;
    this.xrLabel3.Name = "xrLabel3";
    this.xrLabel3.Padding = new PaddingInfo(2, 2, 0, 0, 96f);
    this.xrLabel3.StyleName = "PropertyStyle";
    this.Bands.AddRange(new Band[6]
    {
      (Band) this.Detail,
      (Band) this.TopMargin,
      (Band) this.BottomMargin,
      (Band) this.ReportHeader,
      (Band) this.DetailReport,
      (Band) this.ReportFooter
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
    this.xrTable1.EndInit();
    this.objectDataSource1.EndInit();
    this.EndInit();
  }
}
