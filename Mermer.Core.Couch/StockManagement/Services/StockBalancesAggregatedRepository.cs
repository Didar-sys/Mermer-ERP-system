// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.StockManagement.Services.StockBalancesAggregatedRepository
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase.Views;
using Microsoft.CSharp.RuntimeBinder;
using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Core.Couch.Common;
using Mermer.FundsManagement.Models;
using Mermer.FundsManagement.Models.Extenders;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Models.Extenders;
using Mermer.StockManagement.Services;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.StockManagement.Services;

public class StockBalancesAggregatedRepository : CouchView, IStockBalancesAggregatedRepository
{
  private readonly ILoginService _loginService;
  private readonly IStocksRepository _stocksRepository;
  private readonly IRepository<Currency> _currenciesRepository;
  private readonly IAuthorizationService _authorizationService;
  private readonly IStockRepriceEffectsRepository _repriceEffectsRepository;
  private readonly IReadOnlyListAuthorizer<StockBalanceWithData> _authorizer;

  public StockBalancesAggregatedRepository(
    ICouchCluster cluster,
    ILoginService loginService,
    IStocksRepository stocksRepository,
    IRepository<Currency> currenciesRepository,
    IAuthorizationService authorizationService,
    IStockRepriceEffectsRepository repriceEffectsRepository,
    IReadOnlyListAuthorizer<StockBalanceWithData> authorizer)
    : base(cluster)
  {
    this._loginService = loginService;
    this._stocksRepository = stocksRepository;
    this._currenciesRepository = currenciesRepository;
    this._authorizationService = authorizationService;
    this._repriceEffectsRepository = repriceEffectsRepository;
    this._authorizer = authorizer;
  }

  public async Task<StockBalanceAggregated> GetByTypeAggregatedAsync(
    string[] warehouseIds,
    DateTime dateFrom,
    DateTime dateTill)
  {
    StockBalancesAggregatedRepository aggregatedRepository = this;
    aggregatedRepository._authorizer.Authorize();
    if (!aggregatedRepository._loginService.Session.IsAdmin)
    {
      List<string> accounts = aggregatedRepository._authorizationService.GetAccessableAccounts(AccountAccessLevel.Read).ToList<string>();
      warehouseIds = ((IEnumerable<string>) warehouseIds).Where<string>((Func<string, bool>) (x => accounts.Contains(x))).ToArray<string>();
    }
    if (!((IEnumerable<string>) warehouseIds).Any<string>())
      return new StockBalanceAggregated(true);
    Tuple<object, object>[] array1 = ((IEnumerable<string>) warehouseIds).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (accountId => new Tuple<object, object>((object) new string[2]
    {
      accountId,
      "0"
    }, (object) new string[2]
    {
      accountId,
      dateFrom.ToString("yyyy-MM-dd")
    }))).ToArray<Tuple<object, object>>();
    List<StockBalance> startingBalances = (await aggregatedRepository.GetRecordsAsync<StockBalance>("stock-management", "stock-balances-by-warehouse", array1, true, 3, (Func<ViewRow<StockBalance>, StockBalance>) (x =>
    {
      StockBalance typeAggregatedAsync = x.Value;
      StockBalance stockBalance1 = typeAggregatedAsync;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__1 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__1 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesAggregatedRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target1 = StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__1.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p1 = StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__1;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__0 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesAggregatedRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj1 = StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__0.Target((CallSite) StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__0, x.Key, 0);
      string str1 = target1((CallSite) p1, obj1);
      stockBalance1.WarehouseId = str1;
      StockBalance stockBalance2 = typeAggregatedAsync;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__3 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__3 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesAggregatedRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target2 = StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__3.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p3 = StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__3;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__2 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__2 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesAggregatedRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj2 = StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__2.Target((CallSite) StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__2, x.Key, 2);
      string str2 = target2((CallSite) p3, obj2);
      stockBalance2.StockId = str2;
      return typeAggregatedAsync;
    }))).ToList<StockBalance>();
    Tuple<object, object>[] array2 = ((IEnumerable<string>) warehouseIds).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (accountId => new Tuple<object, object>((object) new string[2]
    {
      accountId,
      dateFrom.ToString("yyyy-MM-dd")
    }, (object) new string[2]
    {
      accountId,
      dateTill.ToString("yyyy-MM-dd")
    }))).ToArray<Tuple<object, object>>();
    List<StockBalancesAggregatedRepository.StockBalanceByTypeWithDate> changingBalances = (await aggregatedRepository.GetRecordsAsync<StockBalancesAggregatedRepository.StockBalanceByTypeWithDate>("stock-management", "stock-balances-by-warehouse", array2, true, 3, (Func<ViewRow<StockBalancesAggregatedRepository.StockBalanceByTypeWithDate>, StockBalancesAggregatedRepository.StockBalanceByTypeWithDate>) (x =>
    {
      StockBalancesAggregatedRepository.StockBalanceByTypeWithDate typeAggregatedAsync = x.Value;
      StockBalancesAggregatedRepository.StockBalanceByTypeWithDate balanceByTypeWithDate1 = typeAggregatedAsync;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__5 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__5 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesAggregatedRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target3 = StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__5.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p5 = StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__5;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__4 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__4 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesAggregatedRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj3 = StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__4.Target((CallSite) StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__4, x.Key, 0);
      string str3 = target3((CallSite) p5, obj3);
      balanceByTypeWithDate1.WarehouseId = str3;
      StockBalancesAggregatedRepository.StockBalanceByTypeWithDate balanceByTypeWithDate2 = typeAggregatedAsync;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__7 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__7 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesAggregatedRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target4 = StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__7.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p7 = StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__7;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__6 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__6 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesAggregatedRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj4 = StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__6.Target((CallSite) StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__6, x.Key, 2);
      string str4 = target4((CallSite) p7, obj4);
      balanceByTypeWithDate2.StockId = str4;
      StockBalancesAggregatedRepository.StockBalanceByTypeWithDate balanceByTypeWithDate3 = typeAggregatedAsync;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__9 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__9 = CallSite<Func<CallSite, object, DateTime>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (DateTime), typeof (StockBalancesAggregatedRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, DateTime> target5 = StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__9.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, DateTime>> p9 = StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__9;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__8 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__8 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesAggregatedRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj5 = StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__8.Target((CallSite) StockBalancesAggregatedRepository.\u003C\u003Eo__7.\u003C\u003Ep__8, x.Key, 1);
      DateTime dateTime = target5((CallSite) p9, obj5);
      balanceByTypeWithDate3.Date = dateTime;
      return typeAggregatedAsync;
    }))).ToList<StockBalancesAggregatedRepository.StockBalanceByTypeWithDate>();
    string[] array3 = startingBalances.Select<StockBalance, string>((Func<StockBalance, string>) (x => x.StockId)).Union<string>(changingBalances.Select<StockBalancesAggregatedRepository.StockBalanceByTypeWithDate, string>((Func<StockBalancesAggregatedRepository.StockBalanceByTypeWithDate, string>) (x => x.StockId))).Distinct<string>().ToArray<string>();
    if (!((IEnumerable<string>) array3).Any<string>())
      return new StockBalanceAggregated(true);
    Stock[] stocks = (await aggregatedRepository._stocksRepository.GetListAsync(array3)).ToArray<Stock>();
    Dictionary<string, Currency> currencies = (await aggregatedRepository._currenciesRepository.GetAsync()).ToDictionary<Currency, string, Currency>((Func<Currency, string>) (x => x.Id), (Func<Currency, Currency>) (x => x));
    IEnumerable<Decimal> starting = startingBalances.Join<StockBalance, Stock, string, Decimal>((IEnumerable<Stock>) stocks, (Func<StockBalance, string>) (x => x.StockId), (Func<Stock, string>) (i => i.Id), (Func<StockBalance, Stock, Decimal>) ((x, i) =>
    {
      StockPrice price = i.GetPrice(new DateTime?(dateFrom));
      CurrencyRate rate = currencies[price.CurrencyId].GetRate(new DateTime?(dateFrom));
      return x.Balance * price.Price * rate.Multiplier / rate.Divider;
    }));
    List<StockBalanceAggregated> changing = changingBalances.Join<StockBalancesAggregatedRepository.StockBalanceByTypeWithDate, Stock, string, StockBalanceAggregated>((IEnumerable<Stock>) stocks, (Func<StockBalancesAggregatedRepository.StockBalanceByTypeWithDate, string>) (x => x.StockId), (Func<Stock, string>) (i => i.Id), (Func<StockBalancesAggregatedRepository.StockBalanceByTypeWithDate, Stock, StockBalanceAggregated>) ((x, i) =>
    {
      StockPrice price = i.GetPrice(new DateTime?(x.Date));
      CurrencyRate rate = currencies[price.CurrencyId].GetRate(new DateTime?(x.Date));
      Decimal num = price.Price * rate.Multiplier / rate.Divider;
      return new StockBalanceAggregated()
      {
        Income = x.Income * num,
        Expense = x.Expense * num,
        Lines = (IEnumerable<StockBalanceAggregatedLine>) new StockBalanceAggregatedLine[12]
        {
          new StockBalanceAggregatedLine("StockOpening", x.StockOpening * num),
          new StockBalanceAggregatedLine("StockSpoilage", x.StockSpoilage * num),
          new StockBalanceAggregatedLine("StockUsage", x.StockUsage * num),
          new StockBalanceAggregatedLine("RevisionExceed", x.RevisionExceed * num),
          new StockBalanceAggregatedLine("RevisionDeficit", x.RevisionDeficit * num),
          new StockBalanceAggregatedLine("StockTransferSource", x.StockTransferSource * num),
          new StockBalanceAggregatedLine("StockTransferDestination", x.StockTransferDestination * num),
          new StockBalanceAggregatedLine("Sales", x.Sales * num),
          new StockBalanceAggregatedLine("SalesReturn", x.SalesReturn * num),
          new StockBalanceAggregatedLine("Purchase", x.Purchase * num),
          new StockBalanceAggregatedLine("PurchaseReturn", x.PurchaseReturn * num),
          new StockBalanceAggregatedLine("Repricing", 0M)
        }
      };
    })).ToList<StockBalanceAggregated>();
    StockBalanceAggregatedLine[] balanceAggregatedLineArray;
    try
    {
      balanceAggregatedLineArray = (await aggregatedRepository._repriceEffectsRepository.GetAsync(dateFrom, dateTill, warehouseIds)).Select<StockRepriceEffect, StockBalanceAggregatedLine>((Func<StockRepriceEffect, StockBalanceAggregatedLine>) (x => new StockBalanceAggregatedLine("Repricing", x.BalanceEffect))).ToArray<StockBalanceAggregatedLine>();
    }
    catch (Exception ex)
    {
      balanceAggregatedLineArray = Array.Empty<StockBalanceAggregatedLine>();
    }
    return new StockBalanceAggregated()
    {
      StartingBalance = starting.Sum(),
      Income = changing.Sum<StockBalanceAggregated>((Func<StockBalanceAggregated, Decimal>) (x => x.Income)) + ((IEnumerable<StockBalanceAggregatedLine>) balanceAggregatedLineArray).Sum<StockBalanceAggregatedLine>((Func<StockBalanceAggregatedLine, Decimal>) (x => x.Income)),
      Expense = changing.Sum<StockBalanceAggregated>((Func<StockBalanceAggregated, Decimal>) (x => x.Expense)) + ((IEnumerable<StockBalanceAggregatedLine>) balanceAggregatedLineArray).Sum<StockBalanceAggregatedLine>((Func<StockBalanceAggregatedLine, Decimal>) (x => x.Expense)),
      Lines = changing.SelectMany<StockBalanceAggregated, StockBalanceAggregatedLine>((Func<StockBalanceAggregated, IEnumerable<StockBalanceAggregatedLine>>) (x => x.Lines)).Union<StockBalanceAggregatedLine>((IEnumerable<StockBalanceAggregatedLine>) balanceAggregatedLineArray).GroupBy<StockBalanceAggregatedLine, string>((Func<StockBalanceAggregatedLine, string>) (x => x.Type)).Select<IGrouping<string, StockBalanceAggregatedLine>, StockBalanceAggregatedLine>((Func<IGrouping<string, StockBalanceAggregatedLine>, StockBalanceAggregatedLine>) (g => new StockBalanceAggregatedLine()
      {
        Type = g.Key,
        Income = g.Sum<StockBalanceAggregatedLine>((Func<StockBalanceAggregatedLine, Decimal>) (x => x.Income)),
        Expense = g.Sum<StockBalanceAggregatedLine>((Func<StockBalanceAggregatedLine, Decimal>) (x => x.Expense))
      }))
    };
  }

  private class StockBalanceByTypeWithDate : StockBalanceByType
  {
    public DateTime Date { get; set; }
  }
}
