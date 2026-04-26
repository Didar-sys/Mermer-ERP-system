// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Reports.Barcodes
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using DevExpress.Utils;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.BarCode;
using DevExpress.XtraReports.UI;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Reports;

public class Barcodes : XtraReport
{
  private IContainer components;
  private DetailBand Detail;
  private TopMarginBand TopMargin;
  private BottomMarginBand BottomMargin;

  public Barcodes(
    string title,
    string barcode,
    string price,
    int columnCount,
    int spaceing,
    int leftMargin,
    int rightMargin,
    bool printPrice,
    int tagWidth,
    int tagHeight,
    BarcodeAlignment alignment,
    BarcodeMargin margin)
  {
    this.InitializeComponent();
    if (alignment == BarcodeAlignment.Vertical)
    {
      this.PageWidth = (tagHeight + spaceing) * columnCount - spaceing + leftMargin + rightMargin;
      this.PageHeight = tagWidth + 5;
      this.Margins = new Margins(leftMargin, rightMargin, 0, 0);
    }
    else
    {
      this.PageWidth = (tagWidth + spaceing) * columnCount - spaceing + leftMargin + rightMargin;
      this.PageHeight = tagHeight + 5;
      this.Margins = new Margins(leftMargin, rightMargin, 0, 0);
    }
    this.Detail.Height = this.PageHeight;
    string mainCode;
    BarCodeGeneratorBase codeGeneratorBase;
    if (barcode.Length == 13 && this.IsEan(barcode, out mainCode))
      codeGeneratorBase = (BarCodeGeneratorBase) new EAN13Generator();
    else if (barcode.Length == 8 && this.IsEan(barcode, out mainCode))
    {
      codeGeneratorBase = (BarCodeGeneratorBase) new EAN8Generator();
    }
    else
    {
      codeGeneratorBase = (BarCodeGeneratorBase) new Code128Generator();
      mainCode = barcode;
    }
    for (int index = 0; index < columnCount; ++index)
    {
      bool flag = printPrice && !string.IsNullOrEmpty(price);
      XRLabel xrLabel1 = new XRLabel();
      xrLabel1.Text = title;
      xrLabel1.CanGrow = false;
      xrLabel1.Dpi = 254f;
      xrLabel1.Font = new Font("Times New Roman", 7f);
      xrLabel1.Multiline = true;
      XRLabel child1 = xrLabel1;
      child1.StylePriority.UseFont = false;
      child1.StylePriority.UseTextAlignment = false;
      XRLabel xrLabel2 = new XRLabel();
      xrLabel2.Text = price;
      xrLabel2.CanGrow = false;
      xrLabel2.Dpi = 254f;
      xrLabel2.Font = new Font("Times New Roman", 9f);
      xrLabel2.Multiline = false;
      XRLabel child2 = xrLabel2;
      child2.StylePriority.UseFont = false;
      child2.StylePriority.UseTextAlignment = false;
      XRBarCode xrBarCode = new XRBarCode();
      xrBarCode.Symbology = codeGeneratorBase;
      xrBarCode.Text = mainCode;
      xrBarCode.AutoModule = true;
      xrBarCode.Module = 5.08f;
      xrBarCode.Dpi = 254f;
      xrBarCode.Alignment = TextAlignment.MiddleCenter;
      xrBarCode.TextAlignment = TextAlignment.MiddleCenter;
      XRBarCode child3 = xrBarCode;
      child3.StylePriority.UsePadding = false;
      child3.StylePriority.UseTextAlignment = false;
      if (alignment == BarcodeAlignment.Vertical)
      {
        SizeF sizeF1;
        SizeF sizeF2;
        if (flag)
        {
          sizeF1 = new SizeF((float) tagHeight / 4f, (float) tagWidth);
          sizeF2 = new SizeF((float) tagHeight / 2f, (float) tagWidth);
          child2.Angle = 90f;
          child2.SizeF = sizeF1;
          child2.LocationFloat = new PointFloat((float) (index * (spaceing + tagHeight)) + sizeF1.Width, 0.0f);
          child1.Padding = new PaddingInfo(0, 0, margin.Right, margin.Left, 254f);
          child2.TextAlignment = TextAlignment.MiddleLeft;
        }
        else
        {
          sizeF1 = new SizeF((float) tagHeight / 3f, (float) tagWidth);
          sizeF2 = new SizeF((float) ((double) tagHeight / 3.0 * 2.0), (float) tagWidth);
        }
        child1.Angle = 90f;
        child1.SizeF = sizeF1;
        child1.LocationFloat = new PointFloat((float) (index * (spaceing + tagHeight)), 0.0f);
        child1.Padding = new PaddingInfo(margin.Top, 0, margin.Right, margin.Left, 254f);
        child1.TextAlignment = TextAlignment.MiddleLeft;
        child3.BarCodeOrientation = BarCodeOrientation.RotateLeft;
        child3.SizeF = sizeF2;
        child3.LocationFloat = new PointFloat((float) (index * (spaceing + tagHeight)) + ((float) tagHeight - sizeF2.Width), 0.0f);
        child3.Padding = new PaddingInfo(0, margin.Bottom, margin.Right, margin.Left, 254f);
      }
      else
      {
        SizeF sizeF3;
        SizeF sizeF4;
        if (flag)
        {
          sizeF3 = new SizeF((float) tagWidth, (float) tagHeight / 4f);
          sizeF4 = new SizeF((float) tagWidth, (float) tagHeight / 2f);
          child2.Angle = 0.0f;
          child2.SizeF = sizeF3;
          child2.LocationFloat = new PointFloat((float) (index * (spaceing + tagWidth)), sizeF3.Height);
          child1.Padding = new PaddingInfo(margin.Right, margin.Left, 0, 0, 254f);
          child2.TextAlignment = TextAlignment.TopCenter;
        }
        else
        {
          sizeF3 = new SizeF((float) tagWidth, (float) tagHeight / 3f);
          sizeF4 = new SizeF((float) tagWidth, (float) ((double) tagHeight / 3.0 * 2.0));
        }
        child1.Angle = 0.0f;
        child1.SizeF = sizeF3;
        child1.LocationFloat = new PointFloat((float) (index * (spaceing + tagWidth)), 0.0f);
        child1.Padding = new PaddingInfo(margin.Left, margin.Right, margin.Top, 0, 254f);
        child1.TextAlignment = TextAlignment.TopCenter;
        child3.BarCodeOrientation = BarCodeOrientation.Normal;
        child3.SizeF = sizeF4;
        child3.LocationFloat = new PointFloat((float) (index * (spaceing + tagWidth)), (float) tagHeight - sizeF4.Height);
        child3.Padding = new PaddingInfo(margin.Left, margin.Right, 0, margin.Bottom, 254f);
      }
      this.Detail.Controls.Add((XRControl) child1);
      if (flag)
        this.Detail.Controls.Add((XRControl) child2);
      this.Detail.Controls.Add((XRControl) child3);
    }
  }

  private bool IsEan(string barcode, out string mainCode)
  {
    mainCode = barcode.Substring(0, barcode.Length - 1);
    return barcode.Substring(barcode.Length - 1, 1) == this.CalculateChecksumDigit(mainCode);
  }

  private string CalculateChecksumDigit(string barcode)
  {
    int num1 = 0;
    int num2 = barcode.Length % 2;
    for (int length = barcode.Length; length >= 1; --length)
    {
      int result;
      if (!int.TryParse(barcode.Substring(length - 1, 1), out result))
        return (string) null;
      if (length % 2 == num2)
        num1 += result * 3;
      else
        num1 += result;
    }
    return ((10 - num1 % 10) % 10).ToString();
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
    this.BeginInit();
    this.Detail.Dpi = 254f;
    this.Detail.HeightF = 200f;
    this.Detail.Name = "Detail";
    this.Detail.Padding = new PaddingInfo(0, 0, 0, 0, 254f);
    this.Detail.TextAlignment = TextAlignment.TopLeft;
    this.TopMargin.Dpi = 254f;
    this.TopMargin.HeightF = 0.0f;
    this.TopMargin.Name = "TopMargin";
    this.TopMargin.Padding = new PaddingInfo(0, 0, 0, 0, 254f);
    this.TopMargin.TextAlignment = TextAlignment.TopLeft;
    this.BottomMargin.Dpi = 254f;
    this.BottomMargin.HeightF = 0.0f;
    this.BottomMargin.Name = "BottomMargin";
    this.BottomMargin.Padding = new PaddingInfo(0, 0, 0, 0, 254f);
    this.BottomMargin.TextAlignment = TextAlignment.TopLeft;
    this.Bands.AddRange(new Band[3]
    {
      (Band) this.Detail,
      (Band) this.TopMargin,
      (Band) this.BottomMargin
    });
    this.Dpi = 254f;
    this.Margins = new Margins(0, 0, 0, 0);
    this.PageHeight = 200;
    this.PageWidth = 1150;
    this.PaperKind = PaperKind.Custom;
    this.ReportUnit = ReportUnit.TenthsOfAMillimeter;
    this.SnapGridSize = 25f;
    this.Version = "17.2";
    this.EndInit();
  }
}
