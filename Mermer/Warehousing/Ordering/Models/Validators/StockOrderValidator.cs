using FluentValidation;
using Mermer.Transactions.Models;
using Mermer.Transactions.Models.Validators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mermer.Warehousing.Ordering.Models.Validators;

public class StockOrderValidator : TransactionValidator<StockOrder>
{
    public StockOrderValidator(
      StockOrderLineValidator lineValidator,
      IValidator<StockUnitConvertion> stockUnitConvertionValidator)
    {
        RuleFor(x => x.WarehouseId).NotEmpty();

        RuleFor(x => x.Lines)
            .Must(x => x != null && x.Any())
            .WithLocalizationMessageKey("{PropertyName} can not be empty");

        // Перейшли на RuleForEach
        RuleForEach(x => x.Lines).SetValidator(lineValidator);

        RuleFor(x => x.Lines)
            .Must((model, list) => list == null || list.All(x =>
            {
                var convertions = model.StockUnitConvertions;
                return convertions != null && convertions.Any(z => z.StockId == x.StockId && z.UnitId == x.UnitId);
            }))
            .WithLocalizationMessageKey("Not all stock units in {PropertyName} convertable");

        // Перейшли на RuleForEach і прибрали зламаний LINQ
        RuleForEach(x => x.StockUnitConvertions).SetValidator(stockUnitConvertionValidator);

        RuleFor(x => x.StockUnitConvertions)
            .Must(list => list == null || list.GroupBy(i => new { i.StockId, i.UnitId }).All(g => g.Count() == 1))
            .WithLocalizationMessageKey("Some convertions in {PropertyName} apear more than once");
    }
}