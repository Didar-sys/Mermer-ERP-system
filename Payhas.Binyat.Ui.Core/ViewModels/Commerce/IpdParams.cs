// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Commerce.IpdParams
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using Payhas.Data.Models;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Commerce;

public class IpdParams : BindableObject
{
  private Decimal _subTotal;
  private Decimal _discountsTotal;
  private Decimal _paymentsTotal;
  private Decimal _changesTotal;
  private bool _canDebitCredit;
  private bool _debitCreditLeftAmount;

  public virtual Decimal SubTotal
  {
    get => this._subTotal;
    set
    {
      this.SetProperty<Decimal>(ref this._subTotal, value, nameof (SubTotal));
      this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.GrandTotal));
      this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.LeftTotal));
    }
  }

  public virtual Decimal DiscountsTotal
  {
    get => this._discountsTotal;
    set
    {
      this.SetProperty<Decimal>(ref this._discountsTotal, value, nameof (DiscountsTotal));
      this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.GrandTotal));
      this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.LeftTotal));
    }
  }

  public virtual Decimal GrandTotal => this.SubTotal - this.DiscountsTotal;

  public virtual Decimal PaymentsTotal
  {
    get => this._paymentsTotal;
    set
    {
      this.SetProperty<Decimal>(ref this._paymentsTotal, value, nameof (PaymentsTotal));
      this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.LeftTotal));
      if (!(this.PaymentsTotal > this.GrandTotal) || this.CanDebitCredit)
        return;
      this.ChangesTotal = this.PaymentsTotal - this.GrandTotal;
    }
  }

  public virtual Decimal ChangesTotal
  {
    get => this._changesTotal;
    set
    {
      this.SetProperty<Decimal>(ref this._changesTotal, value, nameof (ChangesTotal));
      this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.LeftTotal));
    }
  }

  public virtual bool CanDebitCredit
  {
    get => this._canDebitCredit;
    set
    {
      this.SetProperty<bool>(ref this._canDebitCredit, value, nameof (CanDebitCredit));
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.DebitCreditLeftAmount));
      this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.LeftTotal));
    }
  }

  public virtual bool DebitCreditLeftAmount
  {
    get => this.CanDebitCredit && this._debitCreditLeftAmount;
    set
    {
      this.SetProperty<bool>(ref this._debitCreditLeftAmount, value, nameof (DebitCreditLeftAmount));
    }
  }

  public Decimal LeftTotal
  {
    get
    {
      return !this.DebitCreditLeftAmount ? this.GrandTotal - (this.PaymentsTotal - this.ChangesTotal) : 0M;
    }
  }
}
