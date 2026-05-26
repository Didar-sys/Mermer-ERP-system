using FluentValidation;
using Mermer.Transactions.Models;
using Mermer.Transactions.Models.Validators;
using Mermer.Data;
using System.Linq;

namespace Mermer.Finance.Spending.Models.Validators;

public class ExpenseSlipValidator : TransactionValidator<ExpenseSlip>
{
    public ExpenseSlipValidator(
      IValidator<ExpenseSlipLine> lineValidator,
      IValidator<CurrencyConvertion> currencyConvertionValidator)
    {
        RuleFor(x => x.DepositoryId).NotEmpty();

        // Перевірка, що лінії не порожні
        RuleFor(x => x.Lines)
            .Must(x => x != null && x.Any())
            .WithLocalizationMessageKey("{PropertyName} can not be empty");

        // Застосовуємо валідатор до кожної лінії через RuleForEach
        RuleForEach(x => x.Lines).SetValidator(lineValidator);

        RuleFor(x => x.Lines)
            .Must((model, list) => list == null || list.Where(x => !string.IsNullOrEmpty(x.CurrencyId)).All(x =>
            {
                var convertions = model.CurrencyConvertions;
                return convertions != null && convertions.Any(z => z.CurrencyId == x.CurrencyId);
            }))
            .WithLocalizationMessageKey("Not all currencies in {PropertyName} convertable");

        // Валідація колекції конвертацій
        RuleForEach(x => x.CurrencyConvertions).SetValidator(currencyConvertionValidator);

        RuleFor(x => x.CurrencyConvertions)
            .Must(list => list == null || list.GroupBy(i => i.CurrencyId).All(g => g.Count() == 1))
            .WithLocalizationMessageKey("Some convertions in {PropertyName} apear more than once");
    }
}