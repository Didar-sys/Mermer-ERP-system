// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Commerce.Models.Invoice
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Transactions.Models;
using Payhas.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Commerce.Models;

public class Invoice : StockTransaction<InvoiceLine>
{
  private DateTime _dueDate;
  private InvoiceType _invoiceType;
  private string _officeId;
  private string _depositoryId;
  private string _partnerId;
  private bool _debitCreditLeftAmount;
  private string _stockPriceGroup;
  private WatchedObservableCollection<InvoiceDiscount> _discounts;
  private WatchedObservableCollection<InvoicePayment> _payments;
  private WatchedObservableCollection<InvoicePayment> _changes;

  public Invoice()
  {
    this.AutoRaisePropertyChanged(new Dictionary<string, string[]>()
    {
      {
        nameof (PartnerId),
        new string[1]{ nameof (CanDebitCredit) }
      },
      {
        nameof (CanDebitCredit),
        new string[1]{ nameof (DebitCreditLeftAmount) }
      },
      {
        "ActionTotal",
        new string[1]{ nameof (ActionGrandTotal) }
      },
      {
        nameof (ActionDiscountsTotal),
        new string[2]
        {
          nameof (DisplayDiscountsTotal),
          nameof (ActionGrandTotal)
        }
      },
      {
        nameof (DisplayDiscountsTotal),
        new string[1]{ nameof (DisplayDiscountsTotalString) }
      },
      {
        nameof (ActionGrandTotal),
        new string[2]
        {
          nameof (DisplayGrandTotal),
          nameof (ActionLeftTotal)
        }
      },
      {
        nameof (DisplayGrandTotal),
        new string[1]{ nameof (DisplayGrandTotalString) }
      },
      {
        nameof (ActionPaymentsTotal),
        new string[2]
        {
          nameof (DisplayPaymentsTotal),
          nameof (ActionLeftTotal)
        }
      },
      {
        nameof (DisplayPaymentsTotal),
        new string[1]{ nameof (DisplayPaymentsTotalString) }
      },
      {
        nameof (ActionChangesTotal),
        new string[2]
        {
          nameof (DisplayChangesTotal),
          nameof (ActionLeftTotal)
        }
      },
      {
        nameof (DisplayChangesTotal),
        new string[1]{ nameof (DisplayChangesTotalString) }
      },
      {
        nameof (ActionLeftTotal),
        new string[3]
        {
          nameof (DisplayLeftTotal),
          nameof (ActionDebitCreditTotal),
          nameof (IsPayed)
        }
      },
      {
        nameof (DisplayLeftTotal),
        new string[1]{ nameof (DisplayLeftTotalString) }
      },
      {
        nameof (ActionDebitCreditTotal),
        new string[5]
        {
          nameof (IsCash),
          nameof (IsDebitCredit),
          nameof (ActionDebitTotal),
          nameof (ActionCreditTotal),
          nameof (DisplayDebitCreditTotal)
        }
      },
      {
        nameof (ActionDebitTotal),
        new string[1]{ nameof (DisplayDebitTotal) }
      },
      {
        nameof (DisplayDebitTotal),
        new string[1]{ nameof (DisplayDebitTotalString) }
      },
      {
        nameof (ActionCreditTotal),
        new string[1]{ nameof (DisplayCreditTotal) }
      },
      {
        nameof (DisplayCreditTotal),
        new string[1]{ nameof (DisplayCreditTotalString) }
      }
    });
    this.PropertyChanged += new PropertyChangedEventHandler(this.InvoicePropertyChanged);
  }

  public virtual DateTime DueDate
  {
    get => this._dueDate;
    set => this.SetProperty<DateTime>(ref this._dueDate, value, nameof (DueDate));
  }

  public virtual InvoiceType InvoiceType
  {
    get => this._invoiceType;
    set => this.SetProperty<InvoiceType>(ref this._invoiceType, value, nameof (InvoiceType));
  }

  public override string Type => this.InvoiceType.ToString();

  public virtual string OfficeId
  {
    get => this._officeId;
    set => this.SetProperty<string>(ref this._officeId, value, nameof (OfficeId));
  }

  public virtual string DepositoryId
  {
    get => this._depositoryId;
    set => this.SetProperty<string>(ref this._depositoryId, value, nameof (DepositoryId));
  }

  public virtual string PartnerId
  {
    get => this._partnerId;
    set => this.SetProperty<string>(ref this._partnerId, value, nameof (PartnerId));
  }

  public virtual bool DebitCreditLeftAmount
  {
    get => this.CanDebitCredit && this._debitCreditLeftAmount;
    set
    {
      this.SetProperty<bool>(ref this._debitCreditLeftAmount, (value ? 1 : 0) != 0, nameof (DebitCreditLeftAmount), "ActionLeftTotal");
    }
  }

  public Decimal ActionDiscountsTotal
  {
    get
    {
      WatchedObservableCollection<InvoiceDiscount> discounts = this.Discounts;
      return discounts == null ? 0M : discounts.Sum<InvoiceDiscount>((Func<InvoiceDiscount, Decimal>) (x => x.ActionAmount));
    }
  }

  public Decimal ActionPaymentsTotal
  {
    get
    {
      WatchedObservableCollection<InvoicePayment> payments = this.Payments;
      return payments == null ? 0M : payments.Sum<InvoicePayment>((Func<InvoicePayment, Decimal>) (x => x.ActionAmount));
    }
  }

  public Decimal ActionChangesTotal
  {
    get
    {
      WatchedObservableCollection<InvoicePayment> changes = this.Changes;
      return changes == null ? 0M : changes.Sum<InvoicePayment>((Func<InvoicePayment, Decimal>) (x => x.ActionAmount));
    }
  }

  public Decimal ActionGrandTotal => this.ActionTotal - this.ActionDiscountsTotal;

  public Decimal ActionDebitCreditTotal
  {
    get
    {
      if (!this.DebitCreditLeftAmount)
        return 0M;
      return !this.IsPartnerDebit ? this.ActionPaymentsTotal - this.ActionChangesTotal - this.ActionGrandTotal : this.ActionGrandTotal - (this.ActionPaymentsTotal - this.ActionChangesTotal);
    }
  }

  public Decimal ActionDebitTotal
  {
    get
    {
      return this.DebitCreditLeftAmount && this.ActionDebitCreditTotal > 0M ? this.ActionDebitCreditTotal : 0M;
    }
  }

  public Decimal ActionCreditTotal
  {
    get
    {
      return this.DebitCreditLeftAmount && this.ActionDebitCreditTotal < 0M ? this.ActionDebitCreditTotal * -1M : 0M;
    }
  }

  public Decimal ActionLeftTotal
  {
    get
    {
      return !this.DebitCreditLeftAmount ? this.ActionGrandTotal - (this.ActionPaymentsTotal - this.ActionChangesTotal) : 0M;
    }
  }

  public Decimal DisplayDiscountsTotal => this.GetDisplayAmount(this.ActionDiscountsTotal);

  public string DisplayDiscountsTotalString
  {
    get => this.GetDisplayAmountString(this.DisplayDiscountsTotal);
  }

  public Decimal DisplayPaymentsTotal => this.GetDisplayAmount(this.ActionPaymentsTotal);

  public string DisplayPaymentsTotalString
  {
    get => this.GetDisplayAmountString(this.DisplayPaymentsTotal);
  }

  public Decimal DisplayChangesTotal => this.GetDisplayAmount(this.ActionChangesTotal);

  public string DisplayChangesTotalString => this.GetDisplayAmountString(this.DisplayChangesTotal);

  public Decimal DisplayGrandTotal => this.GetDisplayAmount(this.ActionGrandTotal);

  public string DisplayGrandTotalString => this.GetDisplayAmountString(this.DisplayGrandTotal);

  public Decimal DisplayDebitCreditTotal => this.GetDisplayAmount(this.ActionDebitCreditTotal);

  public string DisplayDebitCreditTotalString
  {
    get => this.GetDisplayAmountString(this.DisplayDebitCreditTotal);
  }

  public Decimal DisplayDebitTotal => this.GetDisplayAmount(this.ActionDebitTotal);

  public string DisplayDebitTotalString => this.GetDisplayAmountString(this.DisplayDebitTotal);

  public Decimal DisplayCreditTotal => this.GetDisplayAmount(this.ActionCreditTotal);

  public string DisplayCreditTotalString => this.GetDisplayAmountString(this.DisplayCreditTotal);

  public Decimal DisplayLeftTotal => this.GetDisplayAmount(this.ActionLeftTotal);

  public string DisplayLeftTotalString => this.GetDisplayAmountString(this.DisplayLeftTotal);

  public virtual string StockPriceGroup
  {
    get => this._stockPriceGroup;
    set => this.SetProperty<string>(ref this._stockPriceGroup, value, nameof (StockPriceGroup));
  }

  public override bool IsStockIncome
  {
    get
    {
      switch (this.InvoiceType)
      {
        case InvoiceType.Purchase:
        case InvoiceType.SalesReturn:
          return true;
        case InvoiceType.PurchaseReturn:
        case InvoiceType.Sales:
          return false;
        default:
          throw new ArgumentOutOfRangeException("InvoiceType");
      }
    }
  }

  public bool IsFundsIncome
  {
    get
    {
      switch (this.InvoiceType)
      {
        case InvoiceType.Purchase:
        case InvoiceType.SalesReturn:
          return false;
        case InvoiceType.PurchaseReturn:
        case InvoiceType.Sales:
          return true;
        default:
          throw new ArgumentOutOfRangeException("InvoiceType");
      }
    }
  }

  public bool IsPartnerDebit
  {
    get
    {
      switch (this.InvoiceType)
      {
        case InvoiceType.Purchase:
        case InvoiceType.SalesReturn:
          return true;
        case InvoiceType.PurchaseReturn:
        case InvoiceType.Sales:
          return false;
        default:
          throw new ArgumentOutOfRangeException("InvoiceType");
      }
    }
  }

  public bool CanDebitCredit => !string.IsNullOrEmpty(this.PartnerId);

  public bool IsPayed => Math.Round(this.ActionLeftTotal, 2) == 0M;

  public bool IsCash => this.ActionGrandTotal <= this.ActionPaymentsTotal - this.ActionChangesTotal;

  public bool IsDebitCredit => this.ActionDebitTotal > 0M || this.ActionCreditTotal > 0M;

  public virtual WatchedObservableCollection<InvoiceDiscount> Discounts
  {
    get => this._discounts;
    set
    {
      if (this._discounts != null)
      {
        this._discounts.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.Discounts_CollectionChanged);
        foreach (InvoiceDiscount discount in (Collection<InvoiceDiscount>) this._discounts)
        {
          discount.InvoiceTotalRequested -= new InvoiceTotalRequest(this.GetInvoiceTotal);
          discount.PropertyChanged -= new PropertyChangedEventHandler(this.DicountPropertyChanged);
        }
      }
      this.SetProperty<WatchedObservableCollection<InvoiceDiscount>>(ref this._discounts, value, nameof (Discounts));
      if (this._discounts != null)
      {
        foreach (InvoiceDiscount discount in (Collection<InvoiceDiscount>) this._discounts)
        {
          discount.InvoiceTotalRequested += new InvoiceTotalRequest(this.GetInvoiceTotal);
          discount.PropertyChanged += new PropertyChangedEventHandler(this.DicountPropertyChanged);
        }
        this._discounts.CollectionChanged += new NotifyCollectionChangedEventHandler(this.Discounts_CollectionChanged);
      }
      this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.ActionDiscountsTotal));
    }
  }

  public virtual WatchedObservableCollection<InvoicePayment> Payments
  {
    get => this._payments;
    set
    {
      if (this._payments != null)
      {
        this._payments.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.Payments_CollectionChanged);
        foreach (InvoicePayment payment in (Collection<InvoicePayment>) this._payments)
        {
          payment.PropertyChanged -= new PropertyChangedEventHandler(this.PaymentPropertyChanged);
          payment.CurrencyConverterRequested -= new Payhas.Binyat.Transactions.Models.CurrencyConverter(((Transaction<InvoiceLine>) this).CurrencyConverter);
          payment.DisplayCurrencyIdRequested -= new CurrencyId(((Transaction<InvoiceLine>) this).GetDisplayCurrencyId);
          payment.DefaultCurrencyIdRequested -= new CurrencyId(((Transaction<InvoiceLine>) this).GetDefaultCurrencyId);
        }
      }
      this.SetProperty<WatchedObservableCollection<InvoicePayment>>(ref this._payments, value, nameof (Payments));
      if (this._payments != null)
      {
        foreach (InvoicePayment payment in (Collection<InvoicePayment>) this._payments)
        {
          payment.PropertyChanged += new PropertyChangedEventHandler(this.PaymentPropertyChanged);
          payment.CurrencyConverterRequested += new Payhas.Binyat.Transactions.Models.CurrencyConverter(((Transaction<InvoiceLine>) this).CurrencyConverter);
          payment.DisplayCurrencyIdRequested += new CurrencyId(((Transaction<InvoiceLine>) this).GetDisplayCurrencyId);
          payment.DefaultCurrencyIdRequested += new CurrencyId(((Transaction<InvoiceLine>) this).GetDefaultCurrencyId);
          payment.UpdateCurrencyConvertion();
          payment.UpdateDisplayCurrencyId(false);
          payment.UpdateDefaultCurrencyId();
        }
        this._payments.CollectionChanged += new NotifyCollectionChangedEventHandler(this.Payments_CollectionChanged);
      }
      this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.ActionPaymentsTotal));
    }
  }

  public virtual WatchedObservableCollection<InvoicePayment> Changes
  {
    get => this._changes;
    set
    {
      if (this._changes != null)
      {
        this._changes.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.Changes_CollectionChanged);
        foreach (InvoicePayment change in (Collection<InvoicePayment>) this._changes)
        {
          change.PropertyChanged -= new PropertyChangedEventHandler(this.ChangePropertyChanged);
          change.CurrencyConverterRequested -= new Payhas.Binyat.Transactions.Models.CurrencyConverter(((Transaction<InvoiceLine>) this).CurrencyConverter);
          change.DisplayCurrencyIdRequested -= new CurrencyId(((Transaction<InvoiceLine>) this).GetDisplayCurrencyId);
          change.DefaultCurrencyIdRequested -= new CurrencyId(((Transaction<InvoiceLine>) this).GetDefaultCurrencyId);
        }
      }
      this.SetProperty<WatchedObservableCollection<InvoicePayment>>(ref this._changes, value, nameof (Changes));
      if (this._changes != null)
      {
        foreach (InvoicePayment change in (Collection<InvoicePayment>) this._changes)
        {
          change.PropertyChanged += new PropertyChangedEventHandler(this.ChangePropertyChanged);
          change.CurrencyConverterRequested += new Payhas.Binyat.Transactions.Models.CurrencyConverter(((Transaction<InvoiceLine>) this).CurrencyConverter);
          change.DisplayCurrencyIdRequested += new CurrencyId(((Transaction<InvoiceLine>) this).GetDisplayCurrencyId);
          change.DefaultCurrencyIdRequested += new CurrencyId(((Transaction<InvoiceLine>) this).GetDefaultCurrencyId);
          change.UpdateCurrencyConvertion();
          change.UpdateDisplayCurrencyId(false);
          change.UpdateDefaultCurrencyId();
        }
        this._changes.CollectionChanged += new NotifyCollectionChangedEventHandler(this.Changes_CollectionChanged);
      }
      this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.ActionChangesTotal));
    }
  }

  public override void UpdateDisplayCurrencyId(bool raiseChangeEvent = false)
  {
    base.UpdateDisplayCurrencyId(raiseChangeEvent);
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.DisplayDiscountsTotal));
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.DisplayGrandTotal));
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.DisplayPaymentsTotal));
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.DisplayChangesTotal));
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.DisplayLeftTotal));
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.DisplayDebitCreditTotal));
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.DisplayDebitTotal));
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.DisplayCreditTotal));
  }

  private void InvoicePropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    if (!(e.PropertyName == "ActionTotal") || this.Discounts == null)
      return;
    foreach (InvoiceDiscount discount in (Collection<InvoiceDiscount>) this.Discounts)
      discount.UpdateInvoiceTotal();
  }

  private Decimal GetInvoiceTotal() => this.ActionTotal;

  private void Discounts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
  {
    if (e.OldItems != null)
    {
      foreach (InvoiceDiscount invoiceDiscount in e.OldItems.Cast<InvoiceDiscount>())
      {
        invoiceDiscount.InvoiceTotalRequested -= new InvoiceTotalRequest(this.GetInvoiceTotal);
        invoiceDiscount.PropertyChanged -= new PropertyChangedEventHandler(this.DicountPropertyChanged);
      }
    }
    if (e.NewItems != null)
    {
      foreach (InvoiceDiscount invoiceDiscount in e.NewItems.Cast<InvoiceDiscount>())
      {
        invoiceDiscount.InvoiceTotalRequested += new InvoiceTotalRequest(this.GetInvoiceTotal);
        invoiceDiscount.PropertyChanged += new PropertyChangedEventHandler(this.DicountPropertyChanged);
        invoiceDiscount.UpdateInvoiceTotal();
      }
    }
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.ActionDiscountsTotal));
  }

  private void DicountPropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.ActionDiscountsTotal));
  }

  private void Payments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
  {
    if (e.OldItems != null)
    {
      foreach (InvoicePayment invoicePayment in e.OldItems.Cast<InvoicePayment>())
      {
        invoicePayment.PropertyChanged -= new PropertyChangedEventHandler(this.PaymentPropertyChanged);
        invoicePayment.CurrencyConverterRequested -= new Payhas.Binyat.Transactions.Models.CurrencyConverter(((Transaction<InvoiceLine>) this).CurrencyConverter);
        invoicePayment.DisplayCurrencyIdRequested -= new CurrencyId(((Transaction<InvoiceLine>) this).GetDisplayCurrencyId);
        invoicePayment.DefaultCurrencyIdRequested -= new CurrencyId(((Transaction<InvoiceLine>) this).GetDefaultCurrencyId);
      }
    }
    if (e.NewItems != null)
    {
      foreach (InvoicePayment invoicePayment in e.NewItems.Cast<InvoicePayment>())
      {
        invoicePayment.PropertyChanged += new PropertyChangedEventHandler(this.PaymentPropertyChanged);
        invoicePayment.CurrencyConverterRequested += new Payhas.Binyat.Transactions.Models.CurrencyConverter(((Transaction<InvoiceLine>) this).CurrencyConverter);
        invoicePayment.DisplayCurrencyIdRequested += new CurrencyId(((Transaction<InvoiceLine>) this).GetDisplayCurrencyId);
        invoicePayment.DefaultCurrencyIdRequested += new CurrencyId(((Transaction<InvoiceLine>) this).GetDefaultCurrencyId);
        invoicePayment.UpdateCurrencyConvertion();
        invoicePayment.UpdateDisplayCurrencyId(false);
        invoicePayment.UpdateDefaultCurrencyId();
      }
    }
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.ActionPaymentsTotal));
  }

  private void PaymentPropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.ActionPaymentsTotal));
  }

  private void Changes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
  {
    if (e.OldItems != null)
    {
      foreach (InvoicePayment invoicePayment in e.OldItems.Cast<InvoicePayment>())
      {
        invoicePayment.PropertyChanged -= new PropertyChangedEventHandler(this.ChangePropertyChanged);
        invoicePayment.CurrencyConverterRequested -= new Payhas.Binyat.Transactions.Models.CurrencyConverter(((Transaction<InvoiceLine>) this).CurrencyConverter);
        invoicePayment.DisplayCurrencyIdRequested -= new CurrencyId(((Transaction<InvoiceLine>) this).GetDisplayCurrencyId);
        invoicePayment.DefaultCurrencyIdRequested -= new CurrencyId(((Transaction<InvoiceLine>) this).GetDefaultCurrencyId);
      }
    }
    if (e.NewItems != null)
    {
      foreach (InvoicePayment invoicePayment in e.NewItems.Cast<InvoicePayment>())
      {
        invoicePayment.PropertyChanged += new PropertyChangedEventHandler(this.ChangePropertyChanged);
        invoicePayment.CurrencyConverterRequested += new Payhas.Binyat.Transactions.Models.CurrencyConverter(((Transaction<InvoiceLine>) this).CurrencyConverter);
        invoicePayment.DisplayCurrencyIdRequested += new CurrencyId(((Transaction<InvoiceLine>) this).GetDisplayCurrencyId);
        invoicePayment.DefaultCurrencyIdRequested += new CurrencyId(((Transaction<InvoiceLine>) this).GetDefaultCurrencyId);
        invoicePayment.UpdateCurrencyConvertion();
        invoicePayment.UpdateDisplayCurrencyId(false);
        invoicePayment.UpdateDefaultCurrencyId();
      }
    }
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.ActionChangesTotal));
  }

  private void ChangePropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.ActionChangesTotal));
  }
}
