// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Reports.Models.Mappers.FundsTransactionReportMapper`4
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using Payhas.Binyat.Transactions.Models;
using Payhas.Binyat.Ui.Pc.Reports.Helpers;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Reports.Models.Mappers;

public abstract class FundsTransactionReportMapper<T, TLine, TReport, TReportLine> : 
  TransactionReportMapper<T, TLine, TReport, TReportLine>
  where T : FundsTransaction<TLine>
  where TLine : FundsTransactionLine
  where TReport : FundsTransactionReport<TReportLine>
  where TReportLine : FundsTransactionReportLine
{
  protected readonly NameHelper NameHelper;

  public FundsTransactionReportMapper(
    NameHelper nameHelper,
    TransactionReportLineMapper<TLine, TReportLine> lineMapper)
    : base(lineMapper)
  {
    this.NameHelper = nameHelper;
  }

  public override async Task<TReport> Map(T source, string localizedType)
  {
    TReport destination = await base.Map(source, localizedType);
    TReport report = destination;
    report.Depository = await this.NameHelper.GetDepositoryName(source.DepositoryId);
    report = default (TReport);
    TReport report1 = destination;
    destination = default (TReport);
    return report1;
  }
}
