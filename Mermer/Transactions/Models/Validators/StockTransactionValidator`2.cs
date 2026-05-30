using FluentValidation;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;
using Mermer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mermer.Transactions.Models.Validators;

public class StockTransactionValidator<T, TLine> : TransactionValidator<T, TLine>
  where T : StockTransaction<TLine>
  where TLine : StockTransactionLine
{
    public StockTransactionValidator(
      IValidator<TLine> lineValidator,
      IValidator<StockUnitConvertion> stockUnitConvertionValidator,
      IValidator<CurrencyConvertion> currencyConvertionValidator,
      IValidator<StockTransactionOverhead> overheadValidator,
      IStockBalancesRepository stockBalancesRepository)
      : base(lineValidator, currencyConvertionValidator)
    {
        RuleFor(x => x.WarehouseId).NotEmpty();

        RuleFor(x => x.Lines)
            .Must((model, list) => list == null || list.All(x =>
            {
                var convertions = model.StockUnitConvertions;
                return convertions != null && convertions.Any(z => z.StockId == x.StockId && z.UnitId == x.UnitId);
            }))
            .WithLocalizationMessageKey("Not all stock units in {PropertyName} convertable");

        // Синтаксис контексту для сумісності з FluentValidation 9.3.0
        RuleFor(x => x.Lines).MustAsync(async (model, list, context, cancellationToken) =>
        {
            // ВИПРАВЛЕНО: Додано ParentContext перед RootContextData
            if (model.IsStockIncome || !model.IsCompleted || !context.ParentContext.RootContextData.ContainsKey("AllowNegativeBalance") || (bool)context.ParentContext.RootContextData["AllowNegativeBalance"])
                return true;

            string[] array = list.Select(x => x.StockId).Distinct().ToArray();
            IEnumerable<StockBalanceWithCodeAndName> balances = await stockBalancesRepository.GetAsync(model.WarehouseId, array, model.Id);

            if (cancellationToken.IsCancellationRequested)
                return false;

            var lowBalances = list.GroupBy(l => l.StockId).Select(g => new
            {
                StockId = g.Key,
                ActionQuantity = g.Sum(l => l.ActionQuantity)
            }).Join(balances, l => l.StockId, b => b.StockId, (l, b) => new
            {
                ActionQuantity = l.ActionQuantity,
                Balance = b.Balance,
                StockCode = b.StockCode,
                StockName = b.StockName
            }).Where(x => x.ActionQuantity > x.Balance).Select(x => $"{x.StockCode} | {x.StockName}");

            if (!lowBalances.Any())
                return true;

            context.MessageFormatter.AppendArgument("LowBalanceStocks", Environment.NewLine + string.Join(Environment.NewLine, lowBalances));
            return false;
        }).WithLocalizationMessageKey("Balance is lower than used quantities for: {LowBalanceStocks}");
    }
}