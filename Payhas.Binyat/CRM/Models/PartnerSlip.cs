// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.CRM.Models.PartnerSlip
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Transactions.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.CRM.Models;

public class PartnerSlip : TransactionModel
{
  private string _officeId;
  private PartnerSlipType _slipType;
  private ObservableCollection<PartnerSlipLine> _lines;
  private ObservableCollection<CurrencyConvertion> _currencyConvertions;

  public virtual string OfficeId
  {
    get => this._officeId;
    set => this.SetProperty<string>(ref this._officeId, value, nameof (OfficeId));
  }

  public virtual PartnerSlipType SlipType
  {
    get => this._slipType;
    set
    {
      this.SetProperty<PartnerSlipType>(ref this._slipType, value, nameof (SlipType));
      this.RaisePropertyChanged<string>((Expression<Func<string>>) (() => this.Type));
    }
  }

  public override string Type => this.SlipType.ToString();

  public virtual ObservableCollection<PartnerSlipLine> Lines
  {
    get => this._lines;
    set
    {
      this.SetProperty<ObservableCollection<PartnerSlipLine>>(ref this._lines, value, nameof (Lines));
    }
  }

  public virtual ObservableCollection<CurrencyConvertion> CurrencyConvertions
  {
    get => this._currencyConvertions;
    set
    {
      this.SetProperty<ObservableCollection<CurrencyConvertion>>(ref this._currencyConvertions, value, nameof (CurrencyConvertions));
    }
  }

  public Decimal DebitTotal
  {
    get
    {
      Decimal debitTotal = 0M;
      if (this.Lines != null)
      {
        foreach (PartnerSlipLine line1 in (Collection<PartnerSlipLine>) this.Lines)
        {
          PartnerSlipLine line = line1;
          if (line.DebitAmount > 0M && !string.IsNullOrEmpty(line.DebitCurrencyId))
          {
            ObservableCollection<CurrencyConvertion> currencyConvertions = this.CurrencyConvertions;
            CurrencyConvertion currencyConvertion = currencyConvertions != null ? currencyConvertions.FirstOrDefault<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (x => x.CurrencyId == line.DebitCurrencyId)) : (CurrencyConvertion) null;
            debitTotal += currencyConvertion != null ? line.DebitAmount * currencyConvertion.Multiplier / currencyConvertion.Divider : line.DebitAmount;
          }
        }
      }
      return debitTotal;
    }
  }

  public Decimal CreditTotal
  {
    get
    {
      Decimal creditTotal = 0M;
      if (this.Lines != null)
      {
        foreach (PartnerSlipLine line1 in (Collection<PartnerSlipLine>) this.Lines)
        {
          PartnerSlipLine line = line1;
          if (line.CreditAmount > 0M && !string.IsNullOrEmpty(line.CreditCurrencyId))
          {
            ObservableCollection<CurrencyConvertion> currencyConvertions = this.CurrencyConvertions;
            CurrencyConvertion currencyConvertion = currencyConvertions != null ? currencyConvertions.FirstOrDefault<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (x => x.CurrencyId == line.CreditCurrencyId)) : (CurrencyConvertion) null;
            creditTotal += currencyConvertion != null ? line.CreditAmount * currencyConvertion.Multiplier / currencyConvertion.Divider : line.CreditAmount;
          }
        }
      }
      return creditTotal;
    }
  }
}
