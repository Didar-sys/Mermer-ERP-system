// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.CRM.Models.PartnerTransfer
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Transactions.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

#nullable disable
namespace Payhas.Binyat.CRM.Models;

public class PartnerTransfer : TransactionModel
{
  private ObservableCollection<PartnerTransferLine> _lines;
  private ObservableCollection<CurrencyConvertion> _currencyConvertions;

  public override string Type => nameof (PartnerTransfer);

  public virtual ObservableCollection<PartnerTransferLine> Lines
  {
    get => this._lines;
    set
    {
      this.SetProperty<ObservableCollection<PartnerTransferLine>>(ref this._lines, value, nameof (Lines), "IsConflicted");
    }
  }

  public virtual ObservableCollection<CurrencyConvertion> CurrencyConvertions
  {
    get => this._currencyConvertions;
    set
    {
      this.SetProperty<ObservableCollection<CurrencyConvertion>>(ref this._currencyConvertions, value, nameof (CurrencyConvertions), "IsConflicted");
    }
  }

  public IEnumerable<string> OfficeIds
  {
    get
    {
      ObservableCollection<PartnerTransferLine> lines = this.Lines;
      return lines == null ? (IEnumerable<string>) null : lines.Select<PartnerTransferLine, string>((Func<PartnerTransferLine, string>) (x => x.OfficeId)).Distinct<string>();
    }
  }

  public IEnumerable<string> PartnerIds
  {
    get
    {
      ObservableCollection<PartnerTransferLine> lines = this.Lines;
      return lines == null ? (IEnumerable<string>) null : lines.Select<PartnerTransferLine, string>((Func<PartnerTransferLine, string>) (x => x.PartnerId)).Distinct<string>();
    }
  }

  public virtual bool IsConflicted
  {
    get
    {
      if (this.Lines == null)
        return false;
      Decimal num1 = 0M;
      Decimal num2 = 0M;
      foreach (PartnerTransferLine line1 in (Collection<PartnerTransferLine>) this.Lines)
      {
        PartnerTransferLine line = line1;
        if (line.DebitAmount > 0M && !string.IsNullOrEmpty(line.DebitCurrencyId))
        {
          ObservableCollection<CurrencyConvertion> currencyConvertions = this.CurrencyConvertions;
          CurrencyConvertion currencyConvertion = currencyConvertions != null ? currencyConvertions.FirstOrDefault<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (x => x.CurrencyId == line.DebitCurrencyId)) : (CurrencyConvertion) null;
          if (currencyConvertion != null)
            num1 += line.DebitAmount * currencyConvertion.Multiplier / currencyConvertion.Divider;
        }
        if (line.CreditAmount > 0M && !string.IsNullOrEmpty(line.CreditCurrencyId))
        {
          ObservableCollection<CurrencyConvertion> currencyConvertions = this.CurrencyConvertions;
          CurrencyConvertion currencyConvertion = currencyConvertions != null ? currencyConvertions.FirstOrDefault<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (x => x.CurrencyId == line.CreditCurrencyId)) : (CurrencyConvertion) null;
          if (currencyConvertion != null)
            num2 += line.CreditAmount * currencyConvertion.Multiplier / currencyConvertion.Divider;
        }
      }
      return num1 != num2;
    }
  }

  public Decimal TotalDebit
  {
    get
    {
      Decimal totalDebit = 0M;
      if (this.Lines != null)
      {
        foreach (PartnerTransferLine line1 in (Collection<PartnerTransferLine>) this.Lines)
        {
          PartnerTransferLine line = line1;
          if (line.DebitAmount > 0M && !string.IsNullOrEmpty(line.DebitCurrencyId))
          {
            ObservableCollection<CurrencyConvertion> currencyConvertions = this.CurrencyConvertions;
            CurrencyConvertion currencyConvertion = currencyConvertions != null ? currencyConvertions.FirstOrDefault<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (x => x.CurrencyId == line.DebitCurrencyId)) : (CurrencyConvertion) null;
            if (currencyConvertion != null)
              totalDebit += line.DebitAmount * currencyConvertion.Multiplier / currencyConvertion.Divider;
          }
        }
      }
      return totalDebit;
    }
  }

  public Decimal TotalCredit
  {
    get
    {
      Decimal totalCredit = 0M;
      if (this.Lines != null)
      {
        foreach (PartnerTransferLine line1 in (Collection<PartnerTransferLine>) this.Lines)
        {
          PartnerTransferLine line = line1;
          if (line.CreditAmount > 0M && !string.IsNullOrEmpty(line.CreditCurrencyId))
          {
            ObservableCollection<CurrencyConvertion> currencyConvertions = this.CurrencyConvertions;
            CurrencyConvertion currencyConvertion = currencyConvertions != null ? currencyConvertions.FirstOrDefault<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (x => x.CurrencyId == line.CreditCurrencyId)) : (CurrencyConvertion) null;
            if (currencyConvertion != null)
              totalCredit += line.CreditAmount * currencyConvertion.Multiplier / currencyConvertion.Divider;
          }
        }
      }
      return totalCredit;
    }
  }
}
