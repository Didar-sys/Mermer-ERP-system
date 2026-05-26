using FluentValidation;
using Mermer.Common.Models.Validators;
using System.Linq;

namespace Mermer.FundsManagement.Models.Validators;

public class CurrencyValidator : AbstractModelValidator<Currency>
{
    public CurrencyValidator(IValidator<CurrencyRate> currencyRateValidator)
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Decimals).GreaterThanOrEqualTo(0);

        // Валідуємо кожен елемент колекції рейдів (Rates)
        RuleForEach(x => x.Rates).SetValidator(currencyRateValidator);

        // Перевірка на наявність елементів у списку
        RuleFor(x => x.Rates)
            .Must(x => x != null && x.Any())
            .WithLocalizationMessageKey("{PropertyName} must be specified");

        // Правило для дефолтної валюти
        RuleFor(x => x.Rates)
            .Must(x => x.Count == 1 && x.Count(z => z.Multiplier == 1M && z.Divider == 1M) == 1)
            .When(x => x.IsDefault)
            .WithLocalizationMessageKey("Default currency must have only one rate with multiplier = 1 & divider = 1");
    }
}