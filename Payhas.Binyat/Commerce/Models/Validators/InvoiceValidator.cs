// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Commerce.Models.Validators.InvoiceValidator
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using Payhas.Binyat.StockManagement.Services;
using Payhas.Binyat.Transactions.Models;
using Payhas.Binyat.Transactions.Models.Validators;
using Payhas.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Commerce.Models.Validators;

public class InvoiceValidator : StockTransactionValidator<Invoice, InvoiceLine>
{
  public InvoiceValidator(
    IValidator<InvoiceLine> lineValidator,
    IValidator<InvoicePayment> paymentValidator,
    IValidator<StockTransactionOverhead> overheadValidator,
    IValidator<InvoiceDiscount> discountValidator,
    IValidator<StockUnitConvertion> stockUnitConvertionValidator,
    IValidator<CurrencyConvertion> currencyConvertionValidator,
    IStockBalancesRepository stockBalancesRepository)
    : base(lineValidator, stockUnitConvertionValidator, currencyConvertionValidator, overheadValidator, stockBalancesRepository)
  {
    this.RuleFor<DateTime>((Expression<Func<Invoice, DateTime>>) (x => x.DueDate)).NotEmpty<Invoice, DateTime>().Must<Invoice, DateTime>((Func<Invoice, DateTime, bool>) ((x, val) => x.DueDate >= x.Date)).WithLocalizationMessageKey<Invoice, DateTime>("Due date must be equal or greater than invoice date");
    this.RuleFor<string>((Expression<Func<Invoice, string>>) (x => x.DepositoryId)).NotEmpty<Invoice, string>();
    this.RuleFor<string>((Expression<Func<Invoice, string>>) (x => x.PartnerId)).NotEmpty<Invoice, string>().When<Invoice, string>((Func<Invoice, bool>) (x => x.DebitCreditLeftAmount)).WithLocalizationMessageKey<Invoice, string>("Partner must be selected to effect balance");
    this.RuleFor<bool>((Expression<Func<Invoice, bool>>) (x => x.DebitCreditLeftAmount)).Equal<Invoice, bool>(true).When<Invoice, bool>((Func<Invoice, bool>) (x => Math.Round(x.ActionGrandTotal, 2) != Math.Round(x.ActionPaymentsTotal - x.ActionChangesTotal, 2))).WithLocalizationMessageKey<Invoice, bool>("Invoice must be fully payed or must effect balance");
    ((IRuleBuilder<Invoice, IEnumerable<InvoiceDiscount>>) this.RuleFor<WatchedObservableCollection<InvoiceDiscount>>((Expression<Func<Invoice, WatchedObservableCollection<InvoiceDiscount>>>) (x => x.Discounts))).SetCollectionValidator<Invoice, InvoiceDiscount>(discountValidator);
    ((IRuleBuilder<Invoice, IEnumerable<InvoicePayment>>) this.RuleFor<WatchedObservableCollection<InvoicePayment>>((Expression<Func<Invoice, WatchedObservableCollection<InvoicePayment>>>) (x => x.Payments))).SetCollectionValidator<Invoice, InvoicePayment>(paymentValidator).Must<Invoice, IEnumerable<InvoicePayment>>((Func<Invoice, IEnumerable<InvoicePayment>, bool>) ((model, list) => list == null || list.Where<InvoicePayment>((Func<InvoicePayment, bool>) (x => !string.IsNullOrEmpty(x.CurrencyId))).All<InvoicePayment>((Func<InvoicePayment, bool>) (x =>
    {
      WatchedObservableCollection<CurrencyConvertion> currencyConvertions = model.CurrencyConvertions;
      return currencyConvertions != null && currencyConvertions.Any<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (z => z.CurrencyId == x.CurrencyId));
    })))).WithLocalizationMessageKey<Invoice, IEnumerable<InvoicePayment>>("Not all currencies in {PropertyName} convertable");
    ((IRuleBuilder<Invoice, IEnumerable<InvoicePayment>>) this.RuleFor<WatchedObservableCollection<InvoicePayment>>((Expression<Func<Invoice, WatchedObservableCollection<InvoicePayment>>>) (x => x.Changes))).SetCollectionValidator<Invoice, InvoicePayment>(paymentValidator).Must<Invoice, IEnumerable<InvoicePayment>>((Func<Invoice, IEnumerable<InvoicePayment>, bool>) ((model, list) => list == null || list.Where<InvoicePayment>((Func<InvoicePayment, bool>) (x => !string.IsNullOrEmpty(x.CurrencyId))).All<InvoicePayment>((Func<InvoicePayment, bool>) (x =>
    {
      WatchedObservableCollection<CurrencyConvertion> currencyConvertions = model.CurrencyConvertions;
      return currencyConvertions != null && currencyConvertions.Any<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (z => z.CurrencyId == x.CurrencyId));
    })))).WithLocalizationMessageKey<Invoice, IEnumerable<InvoicePayment>>("Not all currencies in {PropertyName} convertable");
  }
}
