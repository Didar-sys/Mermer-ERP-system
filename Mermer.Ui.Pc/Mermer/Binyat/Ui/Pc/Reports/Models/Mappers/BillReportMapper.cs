// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Reports.Models.Mappers.BillReportMapper
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using Mermer.Commerce.Models;
using Mermer.Ui.Pc.Reports.Helpers;
using System;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Pc.Reports.Models.Mappers;

public class BillReportMapper(NameHelper nameHelper, BillReportLineMapper lineMapper) : 
  FundsTransactionReportMapper<Bill, BillLine, BillReport, BillReportLine>(nameHelper, (TransactionReportLineMapper<BillLine, BillReportLine>) lineMapper)
{
  private Decimal _prevBalance;

  public void SetPartnerPrevBalance(Decimal prevBalance) => this._prevBalance = prevBalance;

  public override async Task<BillReport> Map(Bill source, string localizedType)
  {
    BillReportMapper billReportMapper = this;
    // ISSUE: reference to a compiler-generated method
    BillReport destination = await billReportMapper.\u003C\u003En__0(source, localizedType);
    BillReport billReport = destination;
    billReport.Partner = await billReportMapper.NameHelper.GetPartnerName(source.PartnerId);
    billReport = (BillReport) null;
    destination.PartnerPrevBalance = billReportMapper._prevBalance;
    destination.PartnerDebitEffect = source.IsPartnerDebit ? source.DisplayTotal : 0M;
    destination.PartnerCreditEffect = source.IsPartnerDebit ? 0M : source.DisplayTotal;
    BillReport billReport1 = destination;
    destination = (BillReport) null;
    return billReport1;
  }
}
