// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Reports.Models.TransactionReport`1
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Mermer.Ui.Pc.Reports.Models;

public class TransactionReport<TLine> where TLine : TransactionReportLine
{
  public string Code { get; set; }

  public DateTime Date { get; set; }

  public string Type { get; set; }

  public string UserName { get; set; }

  public bool IsCompleted { get; set; }

  public bool IsDisabled { get; set; }

  public IEnumerable<TLine> Lines { get; set; }

  public Decimal Total
  {
    get
    {
      IEnumerable<TLine> lines = this.Lines;
      return lines == null ? 0M : lines.Sum<TLine>((Func<TLine, Decimal>) (x => x.Total));
    }
  }
}
