using FluentValidation;
using Mermer.Transactions.Models;
using Mermer.Transactions.Models.Validators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mermer.CRM.Models.Validators;

public class PartnerTransferValidator : TransactionValidator<PartnerTransfer>
{
    public PartnerTransferValidator(
      IValidator<PartnerTransferLine> lineValidator,
      IValidator<CurrencyConvertion> currencyConvertionValidator)
    {
        // 1. Перевірка статусу завершення
        RuleFor(x => x.IsCompleted)
            .NotEqual(true)
            .When(x => x.IsConflicted)
            .WithLocalizationMessageKey("Tranfer can not be completed while conflicted");

        // 2. Перевірка, що колекція ліній не порожня
        RuleFor(x => x.Lines)
            .Must(x => x != null && x.Any())
            .WithLocalizationMessageKey("{PropertyName} can not be empty");

        // Застосовуємо валідатор для кожної лінії
        RuleForEach(x => x.Lines).SetValidator(lineValidator);

        // Перевірка конвертації валют для всіх ліній (вичистили марення декомпілятора)
        RuleFor(x => x.Lines)
            .Must((model, list) =>
            {
                if (list == null) return true;

                // Збираємо унікальні валюти дебету та кредиту з усіх ліній
                var uniqueCurrencies = list.Select(x => x.DebitCurrencyId)
                                       .Union(list.Select(x => x.CreditCurrencyId))
                                       .Where(x => !string.IsNullOrEmpty(x))
                                       .Distinct();

                // Перевіряємо, чи всі вони є в списку конвертацій документа
                return uniqueCurrencies.All(x => model.CurrencyConvertions != null &&
                                             model.CurrencyConvertions.Any(z => z.CurrencyId == x));
            })
            .WithLocalizationMessageKey("Not all currencies in {PropertyName} convertable");

        // 3. Валідація колекції конвертацій валют
        RuleForEach(x => x.CurrencyConvertions).SetValidator(currencyConvertionValidator);

        RuleFor(x => x.CurrencyConvertions)
            .Must(list => list == null || list.GroupBy(i => i.CurrencyId).All(g => g.Count() == 1))
            .WithLocalizationMessageKey("Some convertions in {PropertyName} apear more than once");
    }
}