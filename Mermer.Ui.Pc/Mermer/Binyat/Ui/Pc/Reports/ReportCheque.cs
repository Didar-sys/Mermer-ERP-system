// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Reports.ReportCheque
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using DevExpress.Utils;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;

#nullable disable
namespace Mermer.Ui.Pc.Reports;

public class ReportCheque : XtraReport
{
  private IContainer components;
  private DetailBand Detail;
  private TopMarginBand TopMargin;
  private BottomMarginBand BottomMargin;
  private XRSubreport xrSubreport1;

  public ReportCheque() => this.InitializeComponent();

  public XtraReport ReportContent
  {
    get => this.xrSubreport1.ReportSource;
    set => this.xrSubreport1.ReportSource = value;
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
    this.TopMargin = new TopMarginBand();
    this.BottomMargin = new BottomMarginBand();
    this.xrSubreport1 = new XRSubreport();
    this.BeginInit();
    this.Detail.Controls.AddRange(new XRControl[1]
    {
      (XRControl) this.xrSubreport1
    });
    this.Detail.HeightF = 100f;
    this.Detail.Name = "Detail";
    this.Detail.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
    this.Detail.TextAlignment = TextAlignment.TopLeft;
    this.TopMargin.HeightF = 0.0f;
    this.TopMargin.Name = "TopMargin";
    this.TopMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
    this.TopMargin.TextAlignment = TextAlignment.TopLeft;
    this.BottomMargin.HeightF = 25f;
    this.BottomMargin.Name = "BottomMargin";
    this.BottomMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
    this.BottomMargin.TextAlignment = TextAlignment.TopLeft;
    this.xrSubreport1.LocationFloat = new PointFloat(0.0f, 0.0f);
    this.xrSubreport1.Name = "xrSubreport1";
    this.xrSubreport1.SizeF = new SizeF(300f, 100f);
    this.Bands.AddRange(new Band[3]
    {
      (Band) this.Detail,
      (Band) this.TopMargin,
      (Band) this.BottomMargin
    });
    this.Margins = new Margins(0, 0, 0, 25);
    this.PageWidth = 300;
    this.PaperKind = PaperKind.Custom;
    this.RollPaper = true;
    this.Version = "17.2";
    this.EndInit();
  }
}
