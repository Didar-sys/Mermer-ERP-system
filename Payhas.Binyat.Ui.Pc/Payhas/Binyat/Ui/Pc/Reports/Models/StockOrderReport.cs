// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Reports.Models.StockOrderReport
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using System;
using System.Collections.Generic;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Reports.Models;

public class StockOrderReport
{
  public string Code { get; set; }

  public DateTime Date { get; set; }

  public string Type { get; set; }

  public string Warehouse { get; set; }

  public string UserName { get; set; }

  public bool IsCompleted { get; set; }

  public bool IsDisabled { get; set; }

  public IEnumerable<StockOrderReportLine> Lines { get; set; }
}
