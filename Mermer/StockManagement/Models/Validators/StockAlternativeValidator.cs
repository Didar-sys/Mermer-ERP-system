using FluentValidation;
using Mermer.Common.Models.Validators;
using System.Linq;

namespace Mermer.StockManagement.Models.Validators;

public class StockAlternativeValidator : AbstractModelValidator<StockAlternative>
{
    public StockAlternativeValidator(IValidator<StockAlternativeLine> valueValidator)
    {
        RuleFor(x => x.Name).NotEmpty();

        // Перевірка на заповненість колекції альтернатив
        RuleFor(x => x.Lines)
            .Must(x => x != null && x.Any())
            .WithLocalizationMessageKey("{PropertyName} must be specified");

        // Валідуємо кожну лінію окремо
        RuleForEach(x => x.Lines).SetValidator(valueValidator);
    }
}