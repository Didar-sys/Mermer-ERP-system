// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.CRM.Models.PartnerSlipLine
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Data.Models;
using System;

#nullable disable
namespace Payhas.Binyat.CRM.Models;

public class PartnerSlipLine : BindableObject
{
  private string _id;
  private string _partnerId;
  private Decimal _debitAmount;
  private string _debitCurrencyId;
  private Decimal _creditAmount;
  private string _creditCurrencyId;

  public PartnerSlipLine() => this.Id = Guid.NewGuid().ToString();

  public string Id
  {
    get => this._id;
    set => this.SetProperty<string>(ref this._id, value, nameof (Id));
  }

  public virtual string PartnerId
  {
    get => this._partnerId;
    set => this.SetProperty<string>(ref this._partnerId, value, nameof (PartnerId));
  }

  public virtual Decimal DebitAmount
  {
    get => this._debitAmount;
    set => this.SetProperty<Decimal>(ref this._debitAmount, value, nameof (DebitAmount));
  }

  public virtual string DebitCurrencyId
  {
    get => this._debitCurrencyId;
    set => this.SetProperty<string>(ref this._debitCurrencyId, value, nameof (DebitCurrencyId));
  }

  public virtual Decimal CreditAmount
  {
    get => this._creditAmount;
    set => this.SetProperty<Decimal>(ref this._creditAmount, value, nameof (CreditAmount));
  }

  public virtual string CreditCurrencyId
  {
    get => this._creditCurrencyId;
    set => this.SetProperty<string>(ref this._creditCurrencyId, value, nameof (CreditCurrencyId));
  }
}
