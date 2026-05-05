// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Reports.Models.InvoiceReport
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using System;

#nullable disable
namespace Mermer.Ui.Pc.Reports.Models;

public class InvoiceReport : StockTransactionReport<InvoiceReportLine>
{
  public DateTime DueDate { get; set; }

  public string Depository { get; set; }

  public bool IsCash { get; set; }

  public Decimal DiscountsTotal { get; set; }

  public Decimal GrandTotal => this.Total - this.DiscountsTotal;

  public Decimal PaymentsTotal { get; set; }

  public Decimal ChangesTotal { get; set; }

  public string Partner { get; set; }

  public Decimal PartnerPrevBalance { get; set; }

  public Decimal PartnerDebitEffect { get; set; }

  public Decimal PartnerCreditEffect { get; set; }

  public Decimal PartnerNextBalance
  {
    get => this.PartnerPrevBalance + this.PartnerDebitEffect - this.PartnerCreditEffect;
  }
}
