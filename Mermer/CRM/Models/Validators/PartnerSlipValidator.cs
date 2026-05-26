using FluentValidation;
using Mermer.Transactions.Models;
using Mermer.Transactions.Models.Validators;
using System.Linq;

namespace Mermer.CRM.Models.Validators;

public class PartnerSlipValidator : TransactionValidator<PartnerSlip>
{
    public PartnerSlipValidator(
      IValidator<PartnerSlipLine> lineValidator,
      IValidator<CurrencyConvertion> currencyConvertionValidator)
    {
        RuleFor(x => x.OfficeId).NotEmpty();

        // 1. Перевірка, що колекція ліній не порожня
        RuleFor(x => x.Lines)
            .Must(x => x != null && x.Any())
            .WithLocalizationMessageKey("{PropertyName} can not be empty");

        // 2. Застосовуємо валідатор для кожної лінії
        RuleForEach(x => x.Lines).SetValidator(lineValidator);

        // 3. Збираємо унікальні валюти з ліній і перевіряємо їх у конвертаціях
        RuleFor(x => x.Lines)
            .Must((model, list) =>
            {
                if (list == null) return true;

                var uniqueCurrencies = list.Select(x => x.DebitCurrencyId)
                                       .Union(list.Select(x => x.CreditCurrencyId))
                                       .Where(x => !string.IsNullOrEmpty(x))
                                       .Distinct();

                return uniqueCurrencies.All(x => model.CurrencyConvertions != null &&
                                             model.CurrencyConvertions.Any(z => z.CurrencyId == x));
            })
            .WithLocalizationMessageKey("Not all currencies in {PropertyName} convertable");

        // 4. Валідація колекції конвертацій валют
        RuleForEach(x => x.CurrencyConvertions).SetValidator(currencyConvertionValidator);

        RuleFor(x => x.CurrencyConvertions)
            .Must(list => list == null || list.GroupBy(i => i.CurrencyId).All(g => g.Count() == 1))
            .WithLocalizationMessageKey("Some convertions in {PropertyName} apear more than once");
    }
}