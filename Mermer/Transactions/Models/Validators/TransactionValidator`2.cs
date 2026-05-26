using FluentValidation;
using Mermer.Data;
using System.Linq;

namespace Mermer.Transactions.Models.Validators;

public class TransactionValidator<T, TLine> : TransactionValidator<T>
  where T : Transaction<TLine>
  where TLine : TransactionLine
{
  public TransactionValidator(
    IValidator<TLine> lineValidator,
    IValidator<CurrencyConvertion> currencyConvertionValidator)
  {
    // 1. Валідація ліній (Lines)
    RuleForEach(x => x.Lines).SetValidator(lineValidator);
    RuleFor(x => x.Lines)
        .Must(x => x != null && x.Any())
        .WithLocalizationMessageKey("{PropertyName} can not be empty")
        .Must((model, list) => list == null || list.Where(x => !string.IsNullOrEmpty(x.CurrencyId)).All(x =>
        {
          var convertions = model.CurrencyConvertions;
          return convertions != null && convertions.Any(z => z.CurrencyId == x.CurrencyId);
        }))
        .WithLocalizationMessageKey("Not all currencies in {PropertyName} convertable");

    // 2. Валідація конвертацій валют (CurrencyConvertions)
    RuleForEach(x => x.CurrencyConvertions).SetValidator(currencyConvertionValidator);
    RuleFor(x => x.CurrencyConvertions)
        .Must(list => list == null || list.GroupBy(i => i.CurrencyId).All(g => g.Count() == 1))
        .WithLocalizationMessageKey("Some convertions in {PropertyName} apear more than once");
  }
}