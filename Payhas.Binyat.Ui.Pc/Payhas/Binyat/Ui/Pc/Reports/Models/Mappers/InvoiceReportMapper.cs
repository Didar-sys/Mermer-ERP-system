// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Reports.Models.Mappers.InvoiceReportMapper
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using Payhas.Binyat.Commerce.Models;
using Payhas.Binyat.Ui.Pc.Reports.Helpers;
using System;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Reports.Models.Mappers;

public class InvoiceReportMapper(NameHelper nameHelper, InvoiceReportLineMapper lineMapper) : 
  StockTransactionReportMapper<Invoice, InvoiceLine, InvoiceReport, InvoiceReportLine>(nameHelper, (TransactionReportLineMapper<InvoiceLine, InvoiceReportLine>) lineMapper)
{
  private Decimal _prevBalance;

  public void SetPartnerPrevBalance(Decimal prevBalance) => this._prevBalance = prevBalance;

  public override async Task<InvoiceReport> Map(Invoice source, string localizedType)
  {
    InvoiceReportMapper invoiceReportMapper = this;
    // ISSUE: reference to a compiler-generated method
    InvoiceReport destination = await invoiceReportMapper.\u003C\u003En__0(source, localizedType);
    destination.DueDate = source.DueDate;
    InvoiceReport invoiceReport = destination;
    invoiceReport.Depository = await invoiceReportMapper.NameHelper.GetDepositoryName(source.DepositoryId);
    invoiceReport = (InvoiceReport) null;
    destination.IsCash = source.IsCash;
    destination.DiscountsTotal = source.DisplayDiscountsTotal;
    destination.PaymentsTotal = source.DisplayPaymentsTotal;
    destination.ChangesTotal = source.DisplayChangesTotal;
    if (!string.IsNullOrEmpty(source.PartnerId))
    {
      invoiceReport = destination;
      invoiceReport.Partner = await invoiceReportMapper.NameHelper.GetPartnerName(source.PartnerId);
      invoiceReport = (InvoiceReport) null;
      destination.PartnerPrevBalance = invoiceReportMapper._prevBalance;
      destination.PartnerDebitEffect = source.DisplayDebitTotal;
      destination.PartnerCreditEffect = source.DisplayCreditTotal;
    }
    InvoiceReport invoiceReport1 = destination;
    destination = (InvoiceReport) null;
    return invoiceReport1;
  }
}
