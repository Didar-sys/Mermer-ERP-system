using FluentValidation;
using Mermer.StockManagement.Services;
using Mermer.Transactions.Models;
using Mermer.Transactions.Models.Validators;
using Mermer.Data;
using System;
using System.Linq;

namespace Mermer.Commerce.Models.Validators;

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
        // 1. Базові перевірки дат та ідентифікаторів
        RuleFor(x => x.DueDate)
            .NotEmpty()
            .Must((x, dueDate) => dueDate >= x.Date)
            .WithLocalizationMessageKey("Due date must be equal or greater than invoice date");

        RuleFor(x => x.DepositoryId).NotEmpty();

        RuleFor(x => x.PartnerId)
            .NotEmpty()
            .When(x => x.DebitCreditLeftAmount)
            .WithLocalizationMessageKey("Partner must be selected to effect balance");

        // 2. Бізнес-правило для повної оплати або балансу
        RuleFor(x => x.DebitCreditLeftAmount)
            .Equal(true)
            .When(x => Math.Round(x.ActionGrandTotal, 2) != Math.Round(x.ActionPaymentsTotal - x.ActionChangesTotal, 2))
            .WithLocalizationMessageKey("Invoice must be fully payed or must effect balance");

        // 3. Перевірка знижок (Discounts) через новий RuleForEach
        RuleForEach(x => x.Discounts).SetValidator(discountValidator);

        // 4. Перевірка платежів (Payments)
        RuleForEach(x => x.Payments).SetValidator(paymentValidator);
        RuleFor(x => x.Payments)
            .Must((model, list) => list == null || list.Where(x => !string.IsNullOrEmpty(x.CurrencyId)).All(x =>
            {
                var convertions = model.CurrencyConvertions;
                return convertions != null && convertions.Any(z => z.CurrencyId == x.CurrencyId);
            }))
            .WithLocalizationMessageKey("Not all currencies in {PropertyName} convertable");

        // 5. Перевірка решти/змін (Changes)
        RuleForEach(x => x.Changes).SetValidator(paymentValidator);
        RuleFor(x => x.Changes)
            .Must((model, list) => list == null || list.Where(x => !string.IsNullOrEmpty(x.CurrencyId)).All(x =>
            {
                var convertions = model.CurrencyConvertions;
                return convertions != null && convertions.Any(z => z.CurrencyId == x.CurrencyId);
            }))
            .WithLocalizationMessageKey("Not all currencies in {PropertyName} convertable");
    }
}