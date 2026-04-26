// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Reports.Models.Mappers.FundsTransactionReportLineMapper`2
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using Payhas.Binyat.Transactions.Models;
using Payhas.Binyat.Ui.Pc.Reports.Helpers;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Reports.Models.Mappers;

public abstract class FundsTransactionReportLineMapper<TLine, TReportLine> : 
  TransactionReportLineMapper<TLine, TReportLine>
  where TLine : FundsTransactionLine
  where TReportLine : FundsTransactionReportLine
{
  public FundsTransactionReportLineMapper(NameHelper nameHelper)
    : base(nameHelper)
  {
  }

  public override async Task<TReportLine> Map(TLine source)
  {
    TReportLine reportLine = await base.Map(source);
    reportLine.Amount = source.Amount;
    return reportLine;
  }
}
