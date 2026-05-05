// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Reports.BarcodeMargin
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

#nullable disable
namespace Mermer.Ui.Pc.Reports;

public class BarcodeMargin
{
  public int Left { get; set; }

  public int Top { get; set; }

  public int Right { get; set; }

  public int Bottom { get; set; }

  public BarcodeMargin(int left, int top, int right, int bottom)
  {
    this.Left = left;
    this.Top = top;
    this.Right = right;
    this.Bottom = bottom;
  }
}
