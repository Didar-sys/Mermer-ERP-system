// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.CRM.Models.Partner
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Common.Models;
using System;
using System.Collections.Generic;

#nullable disable
namespace Payhas.Binyat.CRM.Models;

public class Partner : Model
{
  private string _code;
  private string _name;
  private string _phone;
  private string _address;
  private string _group;
  private Decimal? _creditLimit;
  private IEnumerable<string> _tags;
  private string _description;
  private Decimal _rating;
  private string _currencyId;

  public virtual string Code
  {
    get => this._code;
    set => this.SetProperty<string>(ref this._code, value, nameof (Code));
  }

  public virtual string Name
  {
    get => this._name;
    set => this.SetProperty<string>(ref this._name, value, nameof (Name));
  }

  public virtual string Phone
  {
    get => this._phone;
    set => this.SetProperty<string>(ref this._phone, value, nameof (Phone));
  }

  public virtual string Address
  {
    get => this._address;
    set => this.SetProperty<string>(ref this._address, value, nameof (Address));
  }

  public virtual string Group
  {
    get => this._group;
    set => this.SetProperty<string>(ref this._group, value, nameof (Group));
  }

  public Decimal? CreditLimit
  {
    get => this._creditLimit;
    set => this.SetProperty<Decimal?>(ref this._creditLimit, value, nameof (CreditLimit));
  }

  public virtual IEnumerable<string> Tags
  {
    get => this._tags;
    set => this.SetProperty<IEnumerable<string>>(ref this._tags, value, nameof (Tags));
  }

  public virtual string Description
  {
    get => this._description;
    set => this.SetProperty<string>(ref this._description, value, nameof (Description));
  }

  public string Fullname => $"{this.Code} | {this.Name}";

  public override string ToString() => this.Fullname;

  public Decimal Rating
  {
    get => this._rating;
    set => this.SetProperty<Decimal>(ref this._rating, value, nameof (Rating));
  }

  public string CurrencyId
  {
    get => this._currencyId;
    set => this.SetProperty<string>(ref this._currencyId, value, nameof (CurrencyId));
  }
}
