// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Common.Settings.BarcodeConfig
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

#nullable disable
namespace Payhas.Binyat.Common.Settings;

public class BarcodeConfig
{
  public BarcodeConfig()
  {
    this.RowsCount = 3;
    this.Orientation = 0;
    this.Spaceing = 5;
    this.LeftMargin = 15;
    this.RightMargin = 15;
    this.PrintPrice = false;
    this.TagWidth = 335;
    this.TagHeight = 185;
    this.TagMarginLeft = 25;
    this.TagMarginRight = 25;
    this.TagMarginTop = 5;
    this.TagMarginBottom = 5;
  }

  public int RowsCount { get; set; }

  public int Orientation { get; set; }

  public int Spaceing { get; set; }

  public int LeftMargin { get; set; }

  public int RightMargin { get; set; }

  public bool PrintPrice { get; set; }

  public int TagWidth { get; set; }

  public int TagHeight { get; set; }

  public int TagMarginLeft { get; set; }

  public int TagMarginRight { get; set; }

  public int TagMarginTop { get; set; }

  public int TagMarginBottom { get; set; }
}
