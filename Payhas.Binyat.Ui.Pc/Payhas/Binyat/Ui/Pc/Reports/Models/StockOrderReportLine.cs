// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Reports.Models.StockOrderReportLine
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using System;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Reports.Models;

public class StockOrderReportLine
{
  public int RowNo { get; set; }

  public string Stock { get; set; }

  public Decimal Quantity { get; set; }

  public string Unit { get; set; }
}
