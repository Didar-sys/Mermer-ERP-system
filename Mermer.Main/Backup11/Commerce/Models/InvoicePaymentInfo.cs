// Decompiled with JetBrains decompiler
// Type: Mermer.Commerce.Models.InvoicePaymentInfo
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.CRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Mermer.Commerce.Models;

public class InvoicePaymentInfo
{
  public string Id { get; set; }

  public string Code { get; set; }

  public DateTime Date { get; set; }

  public DateTime DueDate { get; set; }

  public string UserId { get; set; }

  public string UserName { get; set; }

  public bool IsCompleted { get; set; }

  public bool IsDisabled { get; set; }

  public string PartnerId { get; set; }

  public string OfficeId { get; set; }

  public string WarehouseId { get; set; }

  public string DepositoryId { get; set; }

  public InvoiceType InvoiceType { get; set; }

  public bool IsPartnerDebit { get; set; }

  public Decimal Total { get; private set; }

  public Decimal PaymentsTotal { get; private set; }

  public DateTime LastPaymentDate { get; private set; }

  public bool IsPayed => this.Total == this.PaymentsTotal;

  public bool IsOverDue
  {
    get
    {
      return this.IsPayed ? this.DueDate.AddDays(1.0).Date < this.LastPaymentDate : this.DueDate.AddDays(1.0).Date < DateTime.Now;
    }
  }

  public bool IsOverDueSoon
  {
    get
    {
      if (this.IsPayed || this.IsOverDue)
        return false;
      DateTime dateTime = this.DueDate;
      dateTime = dateTime.Date;
      return dateTime.AddDays(-3.0) <= DateTime.Today;
    }
  }

  public void UpdatePaymentInfo(PartnerActionInfo[] partnerActions)
  {
    PartnerActionInfo partnerActionInfo1 = ((IEnumerable<PartnerActionInfo>) partnerActions).SingleOrDefault<PartnerActionInfo>((Func<PartnerActionInfo, bool>) (x => x.TransactionId == this.Id));
    if (partnerActionInfo1 == null)
      return;
    this.PaymentsTotal = 0M;
    this.Total = Math.Round(partnerActionInfo1.ActionCredit, 2);
    this.LastPaymentDate = this.Date;
    Decimal num = ((IEnumerable<PartnerActionInfo>) partnerActions).Where<PartnerActionInfo>((Func<PartnerActionInfo, bool>) (x => x.TransactionDate < this.Date)).Sum<PartnerActionInfo>((Func<PartnerActionInfo, Decimal>) (x => Math.Round(x.ActionEffect, 2)));
    if (Math.Round(num + partnerActionInfo1.ActionEffect, 2) >= 0M)
    {
      this.PaymentsTotal = this.Total;
    }
    else
    {
      if (num > 0M)
      {
        this.PaymentsTotal += num;
        num = 0M;
      }
      foreach (PartnerActionInfo partnerActionInfo2 in ((IEnumerable<PartnerActionInfo>) partnerActions).Where<PartnerActionInfo>((Func<PartnerActionInfo, bool>) (x => x.TransactionDate >= this.Date && x.ActionDebit > 0M)))
      {
        if (num < 0M)
        {
          num += Math.Round(partnerActionInfo2.ActionDebit, 2);
          if (num > 0M)
          {
            this.PaymentsTotal += num;
            this.LastPaymentDate = partnerActionInfo2.TransactionDate;
          }
        }
        else
        {
          this.PaymentsTotal += Math.Round(partnerActionInfo2.ActionDebit, 2);
          this.LastPaymentDate = partnerActionInfo2.TransactionDate;
        }
        if (!(this.PaymentsTotal < this.Total))
        {
          this.PaymentsTotal = this.Total;
          break;
        }
      }
    }
  }
}
