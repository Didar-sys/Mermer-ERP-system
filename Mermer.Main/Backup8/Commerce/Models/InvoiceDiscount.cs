// Decompiled with JetBrains decompiler
// Type: Mermer.Commerce.Models.InvoiceDiscount
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Data.Models;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Commerce.Models;

public class InvoiceDiscount : BindableObject, IRequestInvoiceTotal
{
  private string _id;
  private InvoiceDiscountType _type;
  private Decimal _amount;
  private string _description;

  public InvoiceDiscount() => this.Id = Guid.NewGuid().ToString();

  public string Id
  {
    get => this._id;
    set => this.SetProperty<string>(ref this._id, value, nameof (Id));
  }

  public virtual InvoiceDiscountType Type
  {
    get => this._type;
    set
    {
      this.SetProperty<InvoiceDiscountType>(ref this._type, value, nameof (Type), "ActionAmount");
    }
  }

  public virtual Decimal Amount
  {
    get => this._amount;
    set => this.SetProperty<Decimal>(ref this._amount, value, nameof (Amount), "ActionAmount");
  }

  public virtual string Description
  {
    get => this._description;
    set => this.SetProperty<string>(ref this._description, value, nameof (Description));
  }

  public Decimal ActionAmount
  {
    get
    {
      if (this.Amount == 0M)
        return 0M;
      switch (this.Type)
      {
        case InvoiceDiscountType.Flat:
          return this.Amount;
        case InvoiceDiscountType.Percentage:
          return this.GetInvoiceTotal() * this.Amount / 100M;
        default:
          throw new ArgumentOutOfRangeException(this.Type.ToString());
      }
    }
  }

  public void UpdateInvoiceTotal()
  {
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.ActionAmount));
  }

  public event InvoiceTotalRequest InvoiceTotalRequested;

  protected Decimal GetInvoiceTotal()
  {
    return this.InvoiceTotalRequested == null ? 0M : this.InvoiceTotalRequested();
  }
}
