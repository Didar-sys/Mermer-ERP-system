using FluentValidation;
using Mermer.Common.Models.Validators;
using System.Linq;

namespace Mermer.StockManagement.Models.Validators;

public class StockValidator : AbstractModelValidator<Stock>
{
    public StockValidator(
      IValidator<StockUnit> stockUnitValidator,
      IValidator<StockPrice> stockPriceValidator,
      IValidator<StockAdditionalPrice> stockAdditionalPriceValidator)
    {
        RuleFor(x => x.Code).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();

        // 1. Валідація одиниць виміру (Units)
        RuleForEach(x => x.Units).SetValidator(stockUnitValidator);
        RuleFor(x => x.Units)
            .Must(x => x != null && x.Any())
            .WithLocalizationMessageKey("{PropertyName} must be specified")
            .Must(x => x != null && x.Count(z => z.IsDefault) == 1)
            .WithLocalizationMessageKey("{PropertyName} must contain (only) one default unit")
            .Must(x =>
            {
                var defaultUnit = x?.FirstOrDefault(z => z.IsDefault);
                return defaultUnit == null || (defaultUnit.Multiplier == 1M && defaultUnit.Divider == 1M);
            })
            .WithLocalizationMessageKey("Default unit must have multiplier = 1 & divider = 1");

        // 2. Валідація основних цін (Prices)
        RuleForEach(x => x.Prices).SetValidator(stockPriceValidator);
        RuleFor(x => x.Prices)
            .Must(x => x != null && x.Any())
            .WithLocalizationMessageKey("{PropertyName} must be specified");

        // 3. Валідація додаткових цін (AdditionalPrices)
        RuleForEach(x => x.AdditionalPrices).SetValidator(stockAdditionalPriceValidator);
    }
}