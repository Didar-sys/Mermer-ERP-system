// Decompiled with JetBrains decompiler
// Type: Mermer.Commerce.Models.Validators.BillValidator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using Mermer.Transactions.Models;
using Mermer.Transactions.Models.Validators;
using Mermer.Data;
using System.Linq;

namespace Mermer.Commerce.Models.Validators;

public class BillValidator : TransactionValidator<Bill>
{
    public BillValidator(
      IValidator<BillLine> lineValidator,
      IValidator<CurrencyConvertion> rateValidator)
    {
        // Це звичайні рядки, тут залишається RuleFor
        RuleFor(x => x.PartnerId).NotEmpty();
        RuleFor(x => x.DepositoryId).NotEmpty();

        // Перевірка, що список ліній не порожній
        RuleFor(x => x.Lines)
            .Must(x => x != null && x.Any())
            .WithLocalizationMessageKey("{PropertyName} can not be empty");

        // А ОСЬ ТУТ ми застосовуємо RuleForEach для валідації кожного елемента списку
        RuleForEach(x => x.Lines).SetValidator(lineValidator);

        // Додаткове правило для колекції
        RuleFor(x => x.Lines)
            .Must((model, list) => list == null || list.Where(x => !string.IsNullOrEmpty(x.CurrencyId)).All(x =>
            {
                var convertions = model.CurrencyConvertions;
                return convertions != null && convertions.Any(z => z.CurrencyId == x.CurrencyId);
            }))
            .WithLocalizationMessageKey("Not all currencies in {PropertyName} convertable");

        // Застосовуємо RuleForEach для колекції конвертацій
        RuleForEach(x => x.CurrencyConvertions).SetValidator(rateValidator);

        // Додаткове правило для конвертацій
        RuleFor(x => x.CurrencyConvertions)
            .Must(list => list == null || list.GroupBy(i => i.CurrencyId).All(g => g.Count() == 1))
            .WithLocalizationMessageKey("Some convertions in {PropertyName} apear more than once");
    }
}
