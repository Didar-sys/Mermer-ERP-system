// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Reports.SplitReport
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using DevExpress.Utils;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Reports;

public class SplitReport : XtraReport
{
  private IContainer components;
  private DetailBand Detail;
  private TopMarginBand TopMargin;
  private BottomMarginBand BottomMargin;
  private XRSubreport xrSubreport1;
  private XRSubreport xrSubreport2;
  private XRLine xrLine1;

  public SplitReport() => this.InitializeComponent();

  public XtraReport LeftReport
  {
    get => this.xrSubreport1.ReportSource;
    set => this.xrSubreport1.ReportSource = value;
  }

  public XtraReport RightReport
  {
    get => this.xrSubreport2.ReportSource;
    set => this.xrSubreport2.ReportSource = value;
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    this.Detail = new DetailBand();
    this.xrSubreport1 = new XRSubreport();
    this.xrSubreport2 = new XRSubreport();
    this.xrLine1 = new XRLine();
    this.TopMargin = new TopMarginBand();
    this.BottomMargin = new BottomMarginBand();
    this.BeginInit();
    this.Detail.Controls.AddRange(new XRControl[3]
    {
      (XRControl) this.xrSubreport1,
      (XRControl) this.xrSubreport2,
      (XRControl) this.xrLine1
    });
    this.Detail.HeightF = 700f;
    this.Detail.Name = "Detail";
    this.Detail.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
    this.Detail.TextAlignment = TextAlignment.TopLeft;
    this.xrSubreport1.LocationFloat = new PointFloat(0.0f, 0.0f);
    this.xrSubreport1.Name = "xrSubreport1";
    this.xrSubreport1.SizeF = new SizeF(727f, 700f);
    this.xrSubreport2.LocationFloat = new PointFloat(750f, 0.0f);
    this.xrSubreport2.Name = "xrSubreport2";
    this.xrSubreport2.SizeF = new SizeF(727f, 700f);
    this.xrLine1.AnchorVertical = VerticalAnchorStyles.Both;
    this.xrLine1.BorderDashStyle = BorderDashStyle.Solid;
    this.xrLine1.LineDirection = LineDirection.Vertical;
    this.xrLine1.LineStyle = DashStyle.DashDotDot;
    this.xrLine1.LocationFloat = new PointFloat(727.0001f, 0.0f);
    this.xrLine1.Name = "xrLine1";
    this.xrLine1.SizeF = new SizeF(22.99994f, 700f);
    this.xrLine1.StylePriority.UseBorderDashStyle = false;
    this.TopMargin.HeightF = 50f;
    this.TopMargin.Name = "TopMargin";
    this.TopMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
    this.TopMargin.TextAlignment = TextAlignment.TopLeft;
    this.BottomMargin.HeightF = 50f;
    this.BottomMargin.Name = "BottomMargin";
    this.BottomMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
    this.BottomMargin.TextAlignment = TextAlignment.TopLeft;
    this.Bands.AddRange(new Band[3]
    {
      (Band) this.Detail,
      (Band) this.TopMargin,
      (Band) this.BottomMargin
    });
    this.Margins = new Margins(49, 49, 50, 50);
    this.PageHeight = 827;
    this.PageWidth = 1575;
    this.PaperKind = PaperKind.Custom;
    this.Version = "17.2";
    this.EndInit();
  }
}
