// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Reports.Models.Mappers.TransactionReportMapper`4
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using Mermer.Transactions.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Pc.Reports.Models.Mappers;

public abstract class TransactionReportMapper<T, TLine, TReport, TReportLine>
  where T : Transaction<TLine>
  where TLine : TransactionLine
  where TReport : TransactionReport<TReportLine>
  where TReportLine : TransactionReportLine
{
  private readonly TransactionReportLineMapper<TLine, TReportLine> _lineMapper;

  public TransactionReportMapper(
    TransactionReportLineMapper<TLine, TReportLine> lineMapper)
  {
    this._lineMapper = lineMapper;
  }

  public virtual async Task<TReport> Map(T source, string localizedType)
  {
    TReport destination = Activator.CreateInstance<TReport>();
    destination.Code = source.Code;
    destination.Date = source.Date;
    destination.Type = localizedType;
    destination.UserName = source.UserName;
    destination.IsCompleted = source.IsCompleted;
    destination.IsDisabled = source.IsDisabled;
    List<TReportLine> lines = new List<TReportLine>();
    for (int i = 0; i < source.Lines.Count; ++i)
    {
      TReportLine reportLine = await this._lineMapper.Map(source.Lines[i]);
      reportLine.RowNo = i + 1;
      lines.Add(reportLine);
    }
    destination.Lines = (IEnumerable<TReportLine>) lines;
    TReport report = destination;
    destination = default (TReport);
    lines = (List<TReportLine>) null;
    return report;
  }
}
