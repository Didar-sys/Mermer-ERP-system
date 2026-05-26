// Decompiled with JetBrains decompiler
// Type: Mermer.Commerce.Models.Bill
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Transactions.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Commerce.Models;

public class Bill : FundsTransaction<BillLine>
{
  private string _officeId;
  private string _partnerId;
  private BillType _billType;
  private Decimal _total;

  public Bill()
  {
    this.AutoRaisePropertyChanged(new Dictionary<string, string[]>()
    {
      {
        nameof (ActionTotal),
        new string[1]{ nameof (ActionDebitCreditTotal) }
      },
      {
        nameof (ActionDebitCreditTotal),
        new string[3]
        {
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
        nameof (ActionCreditTotal),
        new string[1]{ nameof (DisplayCreditTotal) }
      }
    });
  }

  public virtual string OfficeId
  {
    get => this._officeId;
    set => this.SetProperty<string>(ref this._officeId, value, nameof (OfficeId));
  }

  public virtual string PartnerId
  {
    get => this._partnerId;
    set => this.SetProperty<string>(ref this._partnerId, value, nameof (PartnerId));
  }

  public virtual BillType BillType
  {
    get => this._billType;
    set
    {
      this.SetProperty<BillType>(ref this._billType, value, nameof (BillType));
      this.RaisePropertyChanged<string>((Expression<Func<string>>) (() => this.Type));
    }
  }

  public override string Type => this.BillType.ToString();

  public Decimal ActionDebitCreditTotal
  {
    get => !this.IsPartnerDebit ? -this.ActionTotal : this.ActionTotal;
  }

  public Decimal ActionDebitTotal
  {
    get => !(this.ActionDebitCreditTotal > 0M) ? 0M : this.ActionDebitCreditTotal;
  }

  public Decimal ActionCreditTotal
  {
    get => !(this.ActionDebitCreditTotal < 0M) ? 0M : this.ActionDebitCreditTotal * -1M;
  }

  public Decimal DisplayDebitCreditTotal => this.GetDisplayAmount(this.ActionDebitCreditTotal);

  public Decimal DisplayDebitTotal => this.GetDisplayAmount(this.ActionDebitTotal);

  public Decimal DisplayCreditTotal => this.GetDisplayAmount(this.ActionCreditTotal);

  public override bool IsFundsIncome
  {
    get
    {
      switch (this.BillType)
      {
        case BillType.Collection:
          return true;
        case BillType.Payment:
          return false;
        default:
          throw new ArgumentOutOfRangeException("BillType");
      }
    }
  }

  public bool IsPartnerDebit
  {
    get
    {
      switch (this.BillType)
      {
        case BillType.Collection:
          return true;
        case BillType.Payment:
          return false;
        default:
          throw new ArgumentOutOfRangeException("BillType");
      }
    }
  }

  public override Decimal ActionTotal
  {
    get
    {
      Decimal actionTotal = base.ActionTotal;
      return !(actionTotal != 0M) ? this._total : actionTotal;
    }
  }

  public Decimal Total
  {
    get => !(this.ActionTotal != 0M) ? this._total : this.ActionTotal;
    set => this._total = value;
  }
}
