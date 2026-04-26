// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Reports.Models.StockTransferReport
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Reports.Models;

public class StockTransferReport : StockTransactionReport<StockTransferReportLine>
{
  public string DestinationWarehouse { get; set; }

  public Decimal ReceivedTotal
  {
    get
    {
      IEnumerable<StockTransferReportLine> lines = this.Lines;
      return lines == null ? 0M : lines.Sum<StockTransferReportLine>((Func<StockTransferReportLine, Decimal>) (x => x.ReceivedTotal));
    }
  }

  public bool IsConflicted
  {
    get
    {
      IEnumerable<StockTransferReportLine> lines = this.Lines;
      return lines != null && lines.Any<StockTransferReportLine>((Func<StockTransferReportLine, bool>) (x => x.IsConflicted));
    }
  }
}
