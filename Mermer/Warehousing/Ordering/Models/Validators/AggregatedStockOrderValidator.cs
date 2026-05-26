using FluentValidation;
using Mermer.Transactions.Models.Validators;
using System.Linq;

namespace Mermer.Warehousing.Ordering.Models.Validators;

public class AggregatedStockOrderValidator : TransactionValidator<AggregatedStockOrder>
{
    public AggregatedStockOrderValidator(IValidator<AggregatedStockOrderLine> lineValidator)
    {
        RuleFor(x => x.WarehouseId).NotEmpty();

        RuleFor(x => x.Lines)
            .NotEmpty()
            .Must(lines => lines != null && lines.Any())
            .WithLocalizationMessageKey("{PropertyName} should not be empty!");

        RuleForEach(x => x.Lines).SetValidator(lineValidator);
    }
}