// Decompiled with JetBrains decompiler
// Type: Mermer.Transactions.Models.Validators.StockTransactionValidator`2
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using FluentValidation.Validators;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;
using Mermer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

#nullable disable
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
    this.RuleFor<string>((Expression<Func<T, string>>) (x => x.WarehouseId)).NotEmpty<T, string>();
    this.RuleFor<WatchedObservableCollection<TLine>>((Expression<Func<T, WatchedObservableCollection<TLine>>>) (x => x.Lines)).Must<T, WatchedObservableCollection<TLine>>((Func<T, WatchedObservableCollection<TLine>, bool>) ((model, list) => list == null || list.All<TLine>((Func<TLine, bool>) (x =>
    {
      WatchedObservableCollection<StockUnitConvertion> stockUnitConvertions = model.StockUnitConvertions;
      return stockUnitConvertions != null && stockUnitConvertions.Any<StockUnitConvertion>((Func<StockUnitConvertion, bool>) (z => z.StockId == x.StockId && z.UnitId == x.UnitId));
    })))).WithLocalizationMessageKey<T, WatchedObservableCollection<TLine>>("Not all stock units in {PropertyName} convertable");
    this.RuleFor<WatchedObservableCollection<TLine>>((Expression<Func<T, WatchedObservableCollection<TLine>>>) (x => x.Lines)).MustAsync<T, WatchedObservableCollection<TLine>>((Func<T, WatchedObservableCollection<TLine>, PropertyValidatorContext, CancellationToken, Task<bool>>) (async (model, list, context, cancellationToken) =>
    {
      if (model.IsStockIncome || !model.IsCompleted || !context.ParentContext.RootContextData.ContainsKey("AllowNegativeBalance") || (bool) context.ParentContext.RootContextData["AllowNegativeBalance"])
        return true;
      string[] array = list.Select<TLine, string>((Func<TLine, string>) (x => x.StockId)).Distinct<string>().ToArray<string>();
      IEnumerable<StockBalanceWithCodeAndName> async = await stockBalancesRepository.GetAsync(model.WarehouseId, array, model.Id);
      if (cancellationToken.IsCancellationRequested)
        return false;
      IEnumerable<string> strings = list.GroupBy<TLine, string>((Func<TLine, string>) (l => l.StockId)).Select(g => new
      {
        StockId = g.Key,
        ActionQuantity = g.Sum<TLine>((Func<TLine, Decimal>) (l => l.ActionQuantity))
      }).Join(async, l => l.StockId, (Func<StockBalanceWithCodeAndName, string>) (b => b.StockId), (l, b) => new
      {
        ActionQuantity = l.ActionQuantity,
        Balance = b.Balance,
        StockCode = b.StockCode,
        StockName = b.StockName
      }).Where(x => x.ActionQuantity > x.Balance).Select(x => $"{x.StockCode} | {x.StockName}");
      if (!strings.Any<string>())
        return true;
      context.MessageFormatter.AppendArgument("LowBalanceStocks", (object) (Environment.NewLine + string.Join(Environment.NewLine, strings)));
      return false;
    })).WithLocalizationMessageKey<T, WatchedObservableCollection<TLine>>("Balance is lower than used quantities for: {LowBalanceStocks}");
    ((IRuleBuilder<T, IEnumerable<StockTransactionOverhead>>) this.RuleFor<WatchedObservableCollection<StockTransactionOverhead>>((Expression<Func<T, WatchedObservableCollection<StockTransactionOverhead>>>) (x => x.Overheads))).SetCollectionValidator<T, StockTransactionOverhead>(overheadValidator).Must<T, IEnumerable<StockTransactionOverhead>>((Func<T, IEnumerable<StockTransactionOverhead>, bool>) ((model, list) => list == null || list.Where<StockTransactionOverhead>((Func<StockTransactionOverhead, bool>) (x => !string.IsNullOrEmpty(x.CurrencyId))).All<StockTransactionOverhead>((Func<StockTransactionOverhead, bool>) (x =>
    {
      WatchedObservableCollection<CurrencyConvertion> currencyConvertions = model.CurrencyConvertions;
      return currencyConvertions != null && currencyConvertions.Any<CurrencyConvertion>((Func<CurrencyConvertion, bool>) (z => z.CurrencyId == x.CurrencyId));
    })))).WithLocalizationMessageKey<T, IEnumerable<StockTransactionOverhead>>("Not all currencies in {PropertyName} convertable");
    ((IRuleBuilder<T, IEnumerable<StockUnitConvertion>>) this.RuleFor<WatchedObservableCollection<StockUnitConvertion>>((Expression<Func<T, WatchedObservableCollection<StockUnitConvertion>>>) (x => x.StockUnitConvertions))).SetCollectionValidator<T, StockUnitConvertion>(stockUnitConvertionValidator).Must<T, IEnumerable<StockUnitConvertion>>((Func<IEnumerable<StockUnitConvertion>, bool>) (list => list == null || list.GroupBy(i => new
    {
      StockId = i.StockId,
      UnitId = i.UnitId
    }).All<IGrouping<\u003C\u003Ef__AnonymousType2<string, string>, StockUnitConvertion>>(g => g.Count<StockUnitConvertion>() == 1))).WithLocalizationMessageKey<T, IEnumerable<StockUnitConvertion>>("Some convertions in {PropertyName} apear more than once");
  }
}
