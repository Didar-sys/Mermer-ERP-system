// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Transactions.Models.TransactionModel
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Common.Models;
using Payhas.Data.Models;
using System;
using System.Collections.Generic;

#nullable disable
namespace Payhas.Binyat.Transactions.Models;

public class TransactionModel : Model, ITransactionModel, IModel
{
  private DateTime _date;
  private string _code;
  private string _type;
  private string _userId;
  private string _userName;
  private bool _isCompleted;
  private string _group;
  private IEnumerable<string> _tags;
  private string _description;

  public TransactionModel() => this.Date = DateTime.Now;

  public virtual DateTime Date
  {
    get => this._date;
    set => this.SetProperty<DateTime>(ref this._date, value, nameof (Date));
  }

  public virtual string Code
  {
    get => this._code;
    set => this.SetProperty<string>(ref this._code, value, nameof (Code));
  }

  public virtual string Type
  {
    get => this._type;
    set => this.SetProperty<string>(ref this._type, value, nameof (Type));
  }

  public virtual string UserId
  {
    get => this._userId;
    set => this.SetProperty<string>(ref this._userId, value, nameof (UserId));
  }

  public string UserName
  {
    get => this._userName;
    set => this.SetProperty<string>(ref this._userName, value, nameof (UserName));
  }

  public virtual bool IsCompleted
  {
    get => this._isCompleted;
    set => this.SetProperty<bool>(ref this._isCompleted, value, nameof (IsCompleted));
  }

  public virtual string Group
  {
    get => this._group;
    set => this.SetProperty<string>(ref this._group, value, nameof (Group));
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

  public override string ToString() => this.Code;
}
