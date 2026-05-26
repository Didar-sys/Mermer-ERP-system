using FluentValidation;
using Mermer.Common.Models.Validators;
using System.Linq;

namespace Mermer.Warehousing.Ordering.Models.Validators;

public class StockOrderTemplateValidator : AbstractModelValidator<StockOrderTemplate>
{
    public StockOrderTemplateValidator(StockOrderTemplateLineValidator lineValidator)
    {
        RuleFor(x => x.Name).NotEmpty();

        RuleFor(x => x.Lines)
            .Must(x => x != null && x.Any())
            .WithLocalizationMessageKey("{PropertyName} can not be empty");

        RuleForEach(x => x.Lines).SetValidator(lineValidator);
    }
}