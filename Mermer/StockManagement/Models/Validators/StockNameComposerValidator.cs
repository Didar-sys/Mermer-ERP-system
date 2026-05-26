using FluentValidation;
using Mermer.Common.Models.Validators;
using System.Linq;

namespace Mermer.StockManagement.Models.Validators;

public class StockNameComposerValidator : AbstractModelValidator<StockNameComposer>
{
    public StockNameComposerValidator(IValidator<StockNameComposerValue> valueValidator)
    {
        RuleFor(x => x.Name).NotEmpty();

        RuleFor(x => x.Values)
            .Must(x => x != null && x.Any())
            .WithLocalizationMessageKey("{PropertyName} must be specified");

        RuleForEach(x => x.Values).SetValidator(valueValidator);
    }
}