// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Reports.Models.Mappers.TransactionReportLineMapper`2
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using Payhas.Binyat.Transactions.Models;
using Payhas.Binyat.Ui.Pc.Reports.Helpers;
using System;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Reports.Models.Mappers;

public abstract class TransactionReportLineMapper<TLine, TReportLine>
  where TLine : TransactionLine
  where TReportLine : TransactionReportLine
{
  protected readonly NameHelper NameHelper;

  public TransactionReportLineMapper(NameHelper nameHelper) => this.NameHelper = nameHelper;

  public virtual async Task<TReportLine> Map(TLine source)
  {
    TReportLine destination = Activator.CreateInstance<TReportLine>();
    destination.Total = source.DisplayTotal;
    TReportLine reportLine = destination;
    reportLine.Currency = await this.NameHelper.GetCurrencyName(source.CurrencyId);
    reportLine = default (TReportLine);
    TReportLine reportLine1 = destination;
    destination = default (TReportLine);
    return reportLine1;
  }
}
