// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.StockManagement.Services.StockActionsRepository
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase;
using Couchbase.Core;
using Couchbase.Views;
using Microsoft.CSharp.RuntimeBinder;
using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Commerce.Models;
using Mermer.Core.Couch.Common;
using Mermer.CRM.Models;
using Mermer.Enterprise.Models;
using Mermer.FundsManagement.Models;
using Mermer.FundsManagement.Models.Extenders;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Models.Extenders;
using Mermer.StockManagement.Services;
using Mermer.Warehousing.Models;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.StockManagement.Services;

public class StockActionsRepository : CouchView, IStockActionsRepository
{
  private readonly ILoginService _loginService;
  private readonly IAuthorizationService _authService;
  private readonly IReadOnlyListAuthorizer<StockActionWithData> _authorizer;
  private readonly IStocksRepository _stocksRepository;
  private readonly IStockBalancesRepository _balancesRepository;
  private readonly IReadOnlyRepository<Partner> _partnersRepository;
  private readonly IReadOnlyRepository<Currency> _currenciesRepository;
  private readonly IReadOnlyRepository<Warehouse> _warehousesRepository;

  public StockActionsRepository(
    ICouchCluster cluster,
    ILoginService loginService,
    IAuthorizationService authService,
    IReadOnlyListAuthorizer<StockActionWithData> authorizer,
    IStocksRepository stocksRepository,
    IStockBalancesRepository balancesRepository,
    IReadOnlyRepository<Partner> partnersRepository,
    IReadOnlyRepository<Currency> currenciesRepository,
    IReadOnlyRepository<Warehouse> warehousesRepository)
    : base(cluster)
  {
    this._loginService = loginService;
    this._authService = authService;
    this._authorizer = authorizer;
    this._stocksRepository = stocksRepository;
    this._balancesRepository = balancesRepository;
    this._partnersRepository = partnersRepository;
    this._currenciesRepository = currenciesRepository;
    this._warehousesRepository = warehousesRepository;
  }

  public async Task<int> CountAsync(
    DateTime? startDate,
    DateTime? endDate,
    string stockId,
    params string[] warehouseIds)
  {
    return (await this.GetRecordsAsync<int>(startDate, endDate, warehouseIds, stockId, true)).Sum();
  }

  public async Task<IEnumerable<StockActionWithData>> GetAsync(
    DateTime? startDate,
    DateTime? endDate,
    string stockId,
    params string[] warehouseIds)
  {
    List<StockActionWithData> list = (await this.GetActionsAsync<StockActionWithData>(startDate, endDate, stockId, warehouseIds)).ToList<StockActionWithData>();
    Dictionary<string, string> relatedPartners = new Dictionary<string, string>();
    string[] array1 = list.Select<StockActionWithData, string>((Func<StockActionWithData, string>) (x => x.ActionRelatedPartnerId)).Where<string>((Func<string, bool>) (x => !string.IsNullOrEmpty(x))).Distinct<string>().ToArray<string>();
    if (((IEnumerable<string>) array1).Any<string>())
      relatedPartners = (await this._partnersRepository.GetAsync(array1)).Where<Partner>((Func<Partner, bool>) (x => x != null)).ToDictionary<Partner, string, string>((Func<Partner, string>) (x => x.Id), (Func<Partner, string>) (x => x.Name));
    Dictionary<string, string> relatedWarehouses = new Dictionary<string, string>();
    string[] array2 = list.Select<StockActionWithData, string>((Func<StockActionWithData, string>) (x => x.ActionRelatedWarehouseId)).Where<string>((Func<string, bool>) (x => !string.IsNullOrEmpty(x))).Distinct<string>().ToArray<string>();
    if (((IEnumerable<string>) array2).Any<string>())
      relatedWarehouses = (await this._warehousesRepository.GetAsync(array2)).ToDictionary<Warehouse, string, string>((Func<Warehouse, string>) (x => x.Id), (Func<Warehouse, string>) (x => x.Name));
    string[] array3 = list.Select<StockActionWithData, string>((Func<StockActionWithData, string>) (x => x.ActionStockId)).Distinct<string>().ToArray<string>();
    if (((IEnumerable<string>) array3).Any<string>())
    {
      Dictionary<string, Stock> relatedStocks = (await this._stocksRepository.GetListAsync(array3)).ToDictionary<Stock, string, Stock>((Func<Stock, string>) (x => x.Id), (Func<Stock, Stock>) (x => x));
      Dictionary<string, Currency> dictionary = (await this._currenciesRepository.GetAsync()).ToDictionary<Currency, string, Currency>((Func<Currency, string>) (x => x.Id), (Func<Currency, Currency>) (x => x));
      Currency currency = dictionary.Values.Single<Currency>((Func<Currency, bool>) (x => x.IsDefault));
      foreach (StockActionWithData stockActionWithData in list)
      {
        Stock stock = relatedStocks[stockActionWithData.ActionStockId];
        stockActionWithData.StockCode = stock.Code;
        stockActionWithData.StockName = stock.Name;
        stockActionWithData.StockType = stock.Type;
        stockActionWithData.StockGroup = stock.Group;
        stockActionWithData.StockTags = stock.Tags;
        StockPrice price = stock.GetPrice(new DateTime?(stockActionWithData.TransactionDate));
        CurrencyRate rate = dictionary[price.CurrencyId].GetRate(new DateTime?(stockActionWithData.TransactionDate));
        stockActionWithData.RecommendedPrice = Math.Round(price.Price * rate.Multiplier / rate.Divider, currency.Decimals);
        if (!string.IsNullOrEmpty(stockActionWithData.ActionRelatedPartnerId) && relatedPartners.ContainsKey(stockActionWithData.ActionRelatedPartnerId))
          stockActionWithData.ActionRelatedObjectName = relatedPartners[stockActionWithData.ActionRelatedPartnerId];
        if (!string.IsNullOrEmpty(stockActionWithData.ActionRelatedWarehouseId) && relatedWarehouses.ContainsKey(stockActionWithData.ActionRelatedWarehouseId))
          stockActionWithData.ActionRelatedObjectName = relatedWarehouses[stockActionWithData.ActionRelatedWarehouseId];
      }
      relatedStocks = (Dictionary<string, Stock>) null;
    }
    IEnumerable<StockActionWithData> async = (IEnumerable<StockActionWithData>) list;
    list = (List<StockActionWithData>) null;
    relatedPartners = (Dictionary<string, string>) null;
    relatedWarehouses = (Dictionary<string, string>) null;
    return async;
  }

  private async Task<IEnumerable<T>> GetActionsAsync<T>(
    DateTime? startDate,
    DateTime? endDate,
    string stockId,
    params string[] warehouseIds)
    where T : StockAction
  {
    int dateIndexOwn = stockId == null ? 3 : 4;
    int stockIndexOwn = stockId == null ? 4 : 3;
    int dateIndexAll = stockId == null ? 2 : 3;
    int stockIndexAll = stockId == null ? 3 : 2;
    return (IEnumerable<T>) (await this.GetRecordsAsync<T>(startDate, endDate, warehouseIds, stockId, projectorAll: (Func<ViewRow<object>, T>) (x =>
    {
      T instance = Activator.CreateInstance<T>();
      // ISSUE: variable of a boxed type
      __Boxed<T> local1 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__1 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__1 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target1 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__1.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p1 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__1;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__0 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "tId", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj1 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__0.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__0, x.Value);
      string str1 = target1((CallSite) p1, obj1);
      local1.TransactionId = str1;
      // ISSUE: variable of a boxed type
      __Boxed<T> local2 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__3 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__3 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target2 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__3.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p3 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__3;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__2 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__2 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "tCode", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj2 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__2.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__2, x.Value);
      string str2 = target2((CallSite) p3, obj2);
      local2.TransactionCode = str2;
      // ISSUE: variable of a boxed type
      __Boxed<T> local3 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__5 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__5 = CallSite<Func<CallSite, object, DateTime>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (DateTime), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, DateTime> target3 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__5.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, DateTime>> p5 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__5;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__4 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__4 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj3 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__4.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__4, x.Key, dateIndexAll);
      DateTime dateTime = target3((CallSite) p5, obj3);
      local3.TransactionDate = dateTime;
      // ISSUE: variable of a boxed type
      __Boxed<T> local4 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__7 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__7 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target4 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__7.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p7 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__7;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__6 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__6 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj4 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__6.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__6, x.Key, 0);
      string str3 = target4((CallSite) p7, obj4);
      local4.TransactionType = str3;
      // ISSUE: variable of a boxed type
      __Boxed<T> local5 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__9 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__9 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target5 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__9.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p9 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__9;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__8 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__8 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj5 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__8.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__8, x.Key, 4);
      string str4 = target5((CallSite) p9, obj5);
      local5.TransactionUserId = str4;
      // ISSUE: variable of a boxed type
      __Boxed<T> local6 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__11 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__11 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target6 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__11.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p11 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__11;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__10 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__10 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "tUserName", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj6 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__10.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__10, x.Value);
      string str5 = target6((CallSite) p11, obj6);
      local6.TransactionUserName = str5;
      // ISSUE: variable of a boxed type
      __Boxed<T> local7 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__13 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__13 = CallSite<Func<CallSite, object, bool>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (bool), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, bool> target7 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__13.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, bool>> p13 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__13;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__12 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__12 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "tIsCash", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj7 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__12.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__12, x.Value) ?? (object) false;
      int num1 = target7((CallSite) p13, obj7) ? 1 : 0;
      local7.TransactionIsCash = num1 != 0;
      // ISSUE: variable of a boxed type
      __Boxed<T> local8 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__15 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__15 = CallSite<Func<CallSite, object, bool>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (bool), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, bool> target8 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__15.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, bool>> p15 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__15;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__14 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__14 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "tIsCompleted", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj8 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__14.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__14, x.Value);
      int num2 = target8((CallSite) p15, obj8) ? 1 : 0;
      local8.TransactionIsCompleted = num2 != 0;
      // ISSUE: variable of a boxed type
      __Boxed<T> local9 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__17 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__17 = CallSite<Func<CallSite, object, bool>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (bool), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, bool> target9 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__17.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, bool>> p17 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__17;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__16 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__16 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "tIsDisabled", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj9 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__16.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__16, x.Value);
      int num3 = target9((CallSite) p17, obj9) ? 1 : 0;
      local9.TransactionIsDisabled = num3 != 0;
      // ISSUE: variable of a boxed type
      __Boxed<T> local10 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__19 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__19 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target10 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__19.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p19 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__19;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__18 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__18 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "tGroup", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj10 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__18.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__18, x.Value);
      string str6 = target10((CallSite) p19, obj10);
      local10.TransactionGroup = str6;
      // ISSUE: variable of a boxed type
      __Boxed<T> local11 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__22 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__22 = CallSite<Func<CallSite, object, IEnumerable<string>>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (IEnumerable<string>), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, IEnumerable<string>> target11 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__22.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, IEnumerable<string>>> p22 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__22;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__20 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__20 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "tTags", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj11 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__20.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__20, x.Value);
      object obj12;
      if (obj11 == null)
      {
        obj12 = (object) null;
      }
      else
      {
        // ISSUE: reference to a compiler-generated field
        if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__21 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__21 = CallSite<Func<CallSite, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToObject", (IEnumerable<Type>) new Type[1]
          {
            typeof (List<string>)
          }, typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        obj12 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__21.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__21, obj11);
      }
      IEnumerable<string> strings = target11((CallSite) p22, obj12);
      local11.TransactionTags = strings;
      // ISSUE: variable of a boxed type
      __Boxed<T> local12 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__24 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__24 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target12 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__24.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p24 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__24;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__23 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__23 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "aId", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj13 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__23.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__23, x.Value);
      string str7 = target12((CallSite) p24, obj13);
      local12.ActionId = str7;
      // ISSUE: variable of a boxed type
      __Boxed<T> local13 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__26 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__26 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target13 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__26.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p26 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__26;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__25 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__25 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "aSourceId", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj14 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__25.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__25, x.Value);
      string str8 = target13((CallSite) p26, obj14);
      local13.ActionSourceId = str8;
      // ISSUE: variable of a boxed type
      __Boxed<T> local14 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__28 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__28 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target14 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__28.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p28 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__28;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__27 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__27 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj15 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__27.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__27, x.Key, 1);
      string str9 = target14((CallSite) p28, obj15);
      local14.ActionWarehouseId = str9;
      // ISSUE: variable of a boxed type
      __Boxed<T> local15 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__30 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__30 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target15 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__30.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p30 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__30;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__29 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__29 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj16 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__29.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__29, x.Key, stockIndexAll);
      string str10 = target15((CallSite) p30, obj16);
      local15.ActionStockId = str10;
      // ISSUE: variable of a boxed type
      __Boxed<T> local16 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__32 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__32 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target16 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__32.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p32 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__32;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__31 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__31 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "aRPId", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj17 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__31.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__31, x.Value);
      string str11 = target16((CallSite) p32, obj17);
      local16.ActionRelatedPartnerId = str11;
      // ISSUE: variable of a boxed type
      __Boxed<T> local17 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__34 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__34 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target17 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__34.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p34 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__34;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__33 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__33 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "aRWId", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj18 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__33.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__33, x.Value);
      string str12 = target17((CallSite) p34, obj18);
      local17.ActionRelatedWarehouseId = str12;
      // ISSUE: variable of a boxed type
      __Boxed<T> local18 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__36 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__36 = CallSite<Func<CallSite, object, Decimal>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (Decimal), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, Decimal> target18 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__36.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, Decimal>> p36 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__36;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__35 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__35 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "aPrice", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj19 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__35.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__35, x.Value);
      Decimal num4 = target18((CallSite) p36, obj19);
      local18.ActionPrice = num4;
      // ISSUE: variable of a boxed type
      __Boxed<T> local19 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__38 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__38 = CallSite<Func<CallSite, object, Decimal>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (Decimal), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, Decimal> target19 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__38.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, Decimal>> p38 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__38;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__37 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__37 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "aIncome", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj20 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__37.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__37, x.Value);
      Decimal num5 = target19((CallSite) p38, obj20);
      local19.ActionIncome = num5;
      // ISSUE: variable of a boxed type
      __Boxed<T> local20 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__40 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__40 = CallSite<Func<CallSite, object, Decimal>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (Decimal), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, Decimal> target20 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__40.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, Decimal>> p40 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__40;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__39 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__39 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "aExpense", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj21 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__39.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__39, x.Value);
      Decimal num6 = target20((CallSite) p40, obj21);
      local20.ActionExpense = num6;
      // ISSUE: variable of a boxed type
      __Boxed<T> local21 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__42 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__42 = CallSite<Func<CallSite, object, Decimal>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (Decimal), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, Decimal> target21 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__42.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, Decimal>> p42 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__42;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__41 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__41 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "aDiscount", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj22 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__41.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__41, x.Value);
      Decimal num7 = target21((CallSite) p42, obj22);
      local21.ActionDiscount = num7;
      // ISSUE: variable of a boxed type
      __Boxed<T> local22 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__44 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__44 = CallSite<Func<CallSite, object, Decimal>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (Decimal), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, Decimal> target22 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__44.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, Decimal>> p44 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__44;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__43 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__43 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "aOverhead", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj23 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__43.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__43, x.Value);
      Decimal num8 = target22((CallSite) p44, obj23);
      local22.ActionOverhead = num8;
      return instance;
    }), projectorOwn: (Func<ViewRow<object>, T>) (x =>
    {
      T instance = Activator.CreateInstance<T>();
      // ISSUE: variable of a boxed type
      __Boxed<T> local23 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__46 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__46 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target23 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__46.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p46 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__46;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__45 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__45 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "tId", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj24 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__45.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__45, x.Value);
      string str13 = target23((CallSite) p46, obj24);
      local23.TransactionId = str13;
      // ISSUE: variable of a boxed type
      __Boxed<T> local24 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__48 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__48 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target24 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__48.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p48 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__48;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__47 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__47 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "tCode", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj25 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__47.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__47, x.Value);
      string str14 = target24((CallSite) p48, obj25);
      local24.TransactionCode = str14;
      // ISSUE: variable of a boxed type
      __Boxed<T> local25 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__50 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__50 = CallSite<Func<CallSite, object, DateTime>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (DateTime), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, DateTime> target25 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__50.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, DateTime>> p50 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__50;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__49 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__49 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj26 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__49.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__49, x.Key, dateIndexOwn);
      DateTime dateTime = target25((CallSite) p50, obj26);
      local25.TransactionDate = dateTime;
      // ISSUE: variable of a boxed type
      __Boxed<T> local26 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__52 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__52 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target26 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__52.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p52 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__52;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__51 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__51 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj27 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__51.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__51, x.Key, 0);
      string str15 = target26((CallSite) p52, obj27);
      local26.TransactionType = str15;
      // ISSUE: variable of a boxed type
      __Boxed<T> local27 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__54 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__54 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target27 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__54.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p54 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__54;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__53 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__53 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj28 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__53.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__53, x.Key, 1);
      string str16 = target27((CallSite) p54, obj28);
      local27.TransactionUserId = str16;
      // ISSUE: variable of a boxed type
      __Boxed<T> local28 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__56 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__56 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target28 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__56.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p56 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__56;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__55 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__55 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "tUserName", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj29 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__55.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__55, x.Value);
      string str17 = target28((CallSite) p56, obj29);
      local28.TransactionUserName = str17;
      // ISSUE: variable of a boxed type
      __Boxed<T> local29 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__58 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__58 = CallSite<Func<CallSite, object, bool>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (bool), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, bool> target29 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__58.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, bool>> p58 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__58;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__57 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__57 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "tIsCash", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj30 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__57.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__57, x.Value) ?? (object) false;
      int num9 = target29((CallSite) p58, obj30) ? 1 : 0;
      local29.TransactionIsCash = num9 != 0;
      // ISSUE: variable of a boxed type
      __Boxed<T> local30 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__60 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__60 = CallSite<Func<CallSite, object, bool>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (bool), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, bool> target30 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__60.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, bool>> p60 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__60;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__59 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__59 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "tIsCompleted", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj31 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__59.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__59, x.Value);
      int num10 = target30((CallSite) p60, obj31) ? 1 : 0;
      local30.TransactionIsCompleted = num10 != 0;
      // ISSUE: variable of a boxed type
      __Boxed<T> local31 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__62 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__62 = CallSite<Func<CallSite, object, bool>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (bool), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, bool> target31 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__62.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, bool>> p62 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__62;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__61 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__61 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "tIsDisabled", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj32 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__61.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__61, x.Value);
      int num11 = target31((CallSite) p62, obj32) ? 1 : 0;
      local31.TransactionIsDisabled = num11 != 0;
      // ISSUE: variable of a boxed type
      __Boxed<T> local32 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__64 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__64 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target32 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__64.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p64 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__64;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__63 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__63 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "tGroup", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj33 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__63.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__63, x.Value);
      string str18 = target32((CallSite) p64, obj33);
      local32.TransactionGroup = str18;
      // ISSUE: variable of a boxed type
      __Boxed<T> local33 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__66 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__66 = CallSite<Func<CallSite, object, IEnumerable<string>>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (IEnumerable<string>), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, IEnumerable<string>> target33 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__66.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, IEnumerable<string>>> p66 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__66;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__65 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__65 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "tTags", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj34 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__65.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__65, x.Value);
      IEnumerable<string> strings = target33((CallSite) p66, obj34);
      local33.TransactionTags = strings;
      // ISSUE: variable of a boxed type
      __Boxed<T> local34 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__68 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__68 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target34 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__68.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p68 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__68;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__67 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__67 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "aId", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj35 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__67.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__67, x.Value);
      string str19 = target34((CallSite) p68, obj35);
      local34.ActionId = str19;
      // ISSUE: variable of a boxed type
      __Boxed<T> local35 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__70 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__70 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target35 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__70.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p70 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__70;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__69 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__69 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "aSourceId", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj36 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__69.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__69, x.Value);
      string str20 = target35((CallSite) p70, obj36);
      local35.ActionSourceId = str20;
      // ISSUE: variable of a boxed type
      __Boxed<T> local36 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__72 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__72 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target36 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__72.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p72 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__72;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__71 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__71 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj37 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__71.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__71, x.Key, 2);
      string str21 = target36((CallSite) p72, obj37);
      local36.ActionWarehouseId = str21;
      // ISSUE: variable of a boxed type
      __Boxed<T> local37 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__74 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__74 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target37 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__74.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p74 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__74;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__73 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__73 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj38 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__73.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__73, x.Key, stockIndexOwn);
      string str22 = target37((CallSite) p74, obj38);
      local37.ActionStockId = str22;
      // ISSUE: variable of a boxed type
      __Boxed<T> local38 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__76 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__76 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target38 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__76.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p76 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__76;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__75 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__75 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "aRPId", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj39 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__75.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__75, x.Value);
      string str23 = target38((CallSite) p76, obj39);
      local38.ActionRelatedPartnerId = str23;
      // ISSUE: variable of a boxed type
      __Boxed<T> local39 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__78 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__78 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target39 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__78.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p78 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__78;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__77 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__77 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "aRWId", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj40 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__77.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__77, x.Value);
      string str24 = target39((CallSite) p78, obj40);
      local39.ActionRelatedWarehouseId = str24;
      // ISSUE: variable of a boxed type
      __Boxed<T> local40 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__80 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__80 = CallSite<Func<CallSite, object, Decimal>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (Decimal), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, Decimal> target40 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__80.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, Decimal>> p80 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__80;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__79 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__79 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "aPrice", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj41 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__79.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__79, x.Value);
      Decimal num12 = target40((CallSite) p80, obj41);
      local40.ActionPrice = num12;
      // ISSUE: variable of a boxed type
      __Boxed<T> local41 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__82 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__82 = CallSite<Func<CallSite, object, Decimal>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (Decimal), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, Decimal> target41 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__82.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, Decimal>> p82 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__82;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__81 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__81 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "aIncome", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj42 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__81.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__81, x.Value);
      Decimal num13 = target41((CallSite) p82, obj42);
      local41.ActionIncome = num13;
      // ISSUE: variable of a boxed type
      __Boxed<T> local42 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__84 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__84 = CallSite<Func<CallSite, object, Decimal>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (Decimal), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, Decimal> target42 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__84.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, Decimal>> p84 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__84;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__83 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__83 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "aExpense", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj43 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__83.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__83, x.Value);
      Decimal num14 = target42((CallSite) p84, obj43);
      local42.ActionExpense = num14;
      // ISSUE: variable of a boxed type
      __Boxed<T> local43 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__86 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__86 = CallSite<Func<CallSite, object, Decimal>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (Decimal), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, Decimal> target43 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__86.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, Decimal>> p86 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__86;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__85 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__85 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "aDiscount", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj44 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__85.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__85, x.Value);
      Decimal num15 = target43((CallSite) p86, obj44);
      local43.ActionDiscount = num15;
      // ISSUE: variable of a boxed type
      __Boxed<T> local44 = (object) instance;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__88 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__88 = CallSite<Func<CallSite, object, Decimal>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (Decimal), typeof (StockActionsRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, Decimal> target44 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__88.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, Decimal>> p88 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__88;
      // ISSUE: reference to a compiler-generated field
      if (StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__87 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__87 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "aOverhead", typeof (StockActionsRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj45 = StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__87.Target((CallSite) StockActionsRepository.\u003C\u003Eo__11<T>.\u003C\u003Ep__87, x.Value);
      Decimal num16 = target44((CallSite) p88, obj45);
      local44.ActionOverhead = num16;
      return instance;
    }))).ToList<T>();
  }

  private async Task<IEnumerable<T>> GetRecordsAsync<T>(
    DateTime? startDate,
    DateTime? endDate,
    string[] warehouseIds,
    string stockId,
    bool reduce = false,
    Func<ViewRow<object>, T> projectorAll = null,
    Func<ViewRow<object>, T> projectorOwn = null)
  {
    StockActionsRepository actionsRepository = this;
    actionsRepository._authorizer.Authorize();
    if (!((IEnumerable<string>) warehouseIds).Any<string>())
      throw new ArgumentNullException(nameof (warehouseIds));
    Enum[] array1 = Enum.GetValues(typeof (InvoiceType)).Cast<Enum>().Union<Enum>(Enum.GetValues(typeof (StockSlipType)).Cast<Enum>()).ToArray<Enum>();
    List<string> ownActions;
    List<string> allActions;
    if (actionsRepository._loginService.Session.IsAdmin)
    {
      allActions = ((IEnumerable<Enum>) array1).Select<Enum, string>((Func<Enum, string>) (x => x.ToString())).ToList<string>();
      allActions.Add("StockTransferSource");
      allActions.Add("StockTransferDestination");
      ownActions = new List<string>();
    }
    else
    {
      IEnumerable<string> readableAccountIds = actionsRepository._authService.GetAccessableAccounts(AccountAccessLevel.Read);
      warehouseIds = ((IEnumerable<string>) warehouseIds).Where<string>((Func<string, bool>) (x => readableAccountIds.Contains<string>(x))).ToArray<string>();
      allActions = actionsRepository._authService.FilterAvailableActions((Enum) TransactionAccessLevel.ReadAll, array1).ToList<string>();
      ownActions = actionsRepository._authService.FilterAvailableActions((Enum) TransactionAccessLevel.ReadOwn, array1).Where<string>((Func<string, bool>) (x => !allActions.Contains(x))).ToList<string>();
      if (actionsRepository._authService.TryAuthorizeAction((Enum) TransactionActions.StockTransfers, (Enum) TransactionAccessLevel.ReadAll))
      {
        allActions.Add("StockTransferSource");
        allActions.Add("StockTransferDestination");
      }
      else if (actionsRepository._authService.TryAuthorizeAction((Enum) TransactionActions.StockTransfers, (Enum) TransactionAccessLevel.ReadOwn))
      {
        ownActions.Add("StockTransferSource");
        ownActions.Add("StockTransferDestination");
      }
    }
    string userId = actionsRepository._loginService.Session.UserId;
    List<T> list = new List<T>();
    List<T> objList;
    if (stockId != null)
    {
      Tuple<object, object>[] array2 = allActions.SelectMany<string, Tuple<object, object>>((Func<string, IEnumerable<Tuple<object, object>>>) (actionType => ((IEnumerable<string>) warehouseIds).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (accountId => new Tuple<object, object>((object) new string[4]
      {
        actionType,
        accountId,
        stockId,
        startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : "0"
      }, (object) new string[4]
      {
        actionType,
        accountId,
        stockId,
        endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : "zzz"
      }))))).ToArray<Tuple<object, object>>();
      if (((IEnumerable<Tuple<object, object>>) array2).Any<Tuple<object, object>>())
      {
        objList = list;
        IEnumerable<T> recordsAsync;
        if (projectorAll == null)
          recordsAsync = await actionsRepository.GetRecordsAsync<T>("stock-management", "stock-actions-by-warehouse-and-id-all", array2, reduce);
        else
          recordsAsync = await actionsRepository.GetRecordsAsync<object, T>("stock-management", "stock-actions-by-warehouse-and-id-all", array2, reduce, projector: projectorAll);
        objList.AddRange(recordsAsync);
        objList = (List<T>) null;
      }
      Tuple<object, object>[] array3 = ownActions.SelectMany<string, Tuple<object, object>>((Func<string, IEnumerable<Tuple<object, object>>>) (actionType => ((IEnumerable<string>) warehouseIds).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (accountId => new Tuple<object, object>((object) new string[5]
      {
        actionType,
        userId,
        accountId,
        stockId,
        startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : "0"
      }, (object) new string[5]
      {
        actionType,
        userId,
        accountId,
        stockId,
        endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : "zzz"
      }))))).ToArray<Tuple<object, object>>();
      if (((IEnumerable<Tuple<object, object>>) array3).Any<Tuple<object, object>>())
      {
        objList = list;
        IEnumerable<T> recordsAsync;
        if (projectorOwn == null)
          recordsAsync = await actionsRepository.GetRecordsAsync<T>("stock-management", "stock-actions-by-warehouse-and-id", array3, reduce);
        else
          recordsAsync = await actionsRepository.GetRecordsAsync<object, T>("stock-management", "stock-actions-by-warehouse-and-id", array3, reduce, projector: projectorOwn);
        objList.AddRange(recordsAsync);
        objList = (List<T>) null;
      }
    }
    else
    {
      Tuple<object, object>[] array4 = allActions.SelectMany<string, Tuple<object, object>>((Func<string, IEnumerable<Tuple<object, object>>>) (actionType => ((IEnumerable<string>) warehouseIds).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (accountId => new Tuple<object, object>((object) new string[3]
      {
        actionType,
        accountId,
        startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : "0"
      }, (object) new string[3]
      {
        actionType,
        accountId,
        endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : "zzz"
      }))))).ToArray<Tuple<object, object>>();
      if (((IEnumerable<Tuple<object, object>>) array4).Any<Tuple<object, object>>())
      {
        objList = list;
        IEnumerable<T> recordsAsync;
        if (projectorAll == null)
          recordsAsync = await actionsRepository.GetRecordsAsync<T>("stock-management", "stock-actions-by-warehouse-all", array4, reduce);
        else
          recordsAsync = await actionsRepository.GetRecordsAsync<object, T>("stock-management", "stock-actions-by-warehouse-all", array4, reduce, projector: projectorAll);
        objList.AddRange(recordsAsync);
        objList = (List<T>) null;
      }
      Tuple<object, object>[] array5 = ownActions.SelectMany<string, Tuple<object, object>>((Func<string, IEnumerable<Tuple<object, object>>>) (actionType => ((IEnumerable<string>) warehouseIds).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (accountId => new Tuple<object, object>((object) new string[4]
      {
        actionType,
        userId,
        accountId,
        startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : "0"
      }, (object) new string[4]
      {
        actionType,
        userId,
        accountId,
        endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : "zzz"
      }))))).ToArray<Tuple<object, object>>();
      if (((IEnumerable<Tuple<object, object>>) array5).Any<Tuple<object, object>>())
      {
        objList = list;
        IEnumerable<T> recordsAsync;
        if (projectorOwn == null)
          recordsAsync = await actionsRepository.GetRecordsAsync<T>("stock-management", "stock-actions-by-warehouse", array5, reduce);
        else
          recordsAsync = await actionsRepository.GetRecordsAsync<object, T>("stock-management", "stock-actions-by-warehouse", array5, reduce, projector: projectorOwn);
        objList.AddRange(recordsAsync);
        objList = (List<T>) null;
      }
    }
    IEnumerable<T> recordsAsync1 = (IEnumerable<T>) list;
    ownActions = (List<string>) null;
    list = (List<T>) null;
    return recordsAsync1;
  }

  public async Task<StockTracking> TrackByLineIdAsync(string lineId)
  {
    return (await this.TrackActionsAsync(lineId: lineId)).SingleOrDefault<StockTracking>();
  }

  public Task<IEnumerable<StockTracking>> TrackByTransactionIdAsync(string transactionId)
  {
    return this.TrackActionsAsync(transactionId, incomeOnly: true);
  }

  protected async Task<IEnumerable<StockTracking>> TrackActionsAsync(
    string transactionId = null,
    string lineId = null,
    string lineSourceId = null,
    Decimal? expensable = null,
    bool incomeOnly = false)
  {
    StockActionsRepository actionsRepository = this;
    List<StockTracking> tracking = new List<StockTracking>();
    List<StockActionsRepository.StockActionSimple> actions = new List<StockActionsRepository.StockActionSimple>();
    Dictionary<string, Stock> stocks;
    using (IBucket bucket = actionsRepository.Cluster.OpenDefaultBucket())
    {
      IViewQuery query;
      if (!string.IsNullOrEmpty(transactionId))
        query = new ViewQuery().From("stock-management-reporting", "stock-tracking-by-transactionId").Key((object) transactionId);
      else if (!string.IsNullOrEmpty(lineId))
        query = new ViewQuery().From("stock-management-reporting", "stock-tracking-by-lineId").Key((object) lineId);
      else
        query = !string.IsNullOrEmpty(lineSourceId) ? new ViewQuery().From("stock-management-reporting", "stock-tracking-by-lineSourceId").Key((object) lineSourceId) : throw new ArgumentNullException();
      IViewResult<StockActionsRepository.StockActionSimple> viewResult = await bucket.QueryAsync<StockActionsRepository.StockActionSimple>((IViewQueryable) query);
      if (!viewResult.Success)
        throw viewResult.Exception ?? new Exception(viewResult.Message);
      actions.AddRange(!incomeOnly ? viewResult.Values : viewResult.Values.Where<StockActionsRepository.StockActionSimple>((Func<StockActionsRepository.StockActionSimple, bool>) (x => x.Income > x.Expense)));
      if (!actions.Any<StockActionsRepository.StockActionSimple>())
        return (IEnumerable<StockTracking>) tracking;
      stocks = ((IEnumerable<IDocumentResult<Stock>>) await bucket.GetDocumentsAsync<Stock>((IEnumerable<string>) actions.Select<StockActionsRepository.StockActionSimple, string>((Func<StockActionsRepository.StockActionSimple, string>) (x => x.StockId)).Distinct<string>().ToArray<string>())).Select<IDocumentResult<Stock>, Stock>((Func<IDocumentResult<Stock>, Stock>) (x => x.Content)).ToDictionary<Stock, string, Stock>((Func<Stock, string>) (x => x.Id), (Func<Stock, Stock>) (x => x));
    }
    foreach (StockActionsRepository.StockActionSimple stockActionSimple in actions)
    {
      StockActionsRepository.StockActionSimple action = stockActionSimple;
      Stock stock = stocks[action.StockId];
      Decimal income = action.Income - action.Expense;
      Decimal sellable = expensable ?? income;
      if (income > sellable)
        income = sellable;
      Decimal prevBalance = (await actionsRepository._balancesRepository.GetAsync(action.StockId, action.Date.AddSeconds(-1.0), action.WarehouseId)).Sum<StockBalance>((Func<StockBalance, Decimal>) (x => x.Balance));
      sellable += prevBalance;
      Decimal directExpenses = await actionsRepository.CountAndDetectReturns(tracking, sellable, action.WarehouseId, action.StockId, action.Date, (Func<StockAction, bool>) (x => x.ActionSourceId == action.Id));
      sellable -= directExpenses;
      Decimal num1 = 0M;
      if (sellable > 0M)
        num1 = await actionsRepository.CountAndDetectReturns(tracking, sellable, action.WarehouseId, action.StockId, action.Date.AddTicks(1L), (Func<StockAction, bool>) (x => x.ActionSourceId == null), prevBalance > 0M ? prevBalance : 0M);
      Decimal num2 = directExpenses + num1 - prevBalance;
      if (num2 < 0M)
        num2 = 0M;
      else if (num2 > income)
        num2 = income;
      tracking.Add(new StockTracking()
      {
        StockId = action.StockId,
        StockCode = stock.Code,
        StockName = stock.Name,
        WarehouseId = action.WarehouseId,
        Income = income,
        Expense = num2
      });
      stock = (Stock) null;
    }
    return (IEnumerable<StockTracking>) tracking;
  }

  private async Task<Decimal> CountAndDetectReturns(
    List<StockTracking> tracking,
    Decimal expensable,
    string warehouseId,
    string stockId,
    DateTime date,
    Func<StockAction, bool> filter,
    Decimal prevBalance = 0M)
  {
    StockActionsRepository actionsRepository = this;
    Decimal expenses = 0M;
    StockAction[] stockActionArray = (await actionsRepository.GetActionsAsync<StockAction>(new DateTime?(date), new DateTime?(), stockId, warehouseId)).Where<StockAction>((Func<StockAction, bool>) (x => x.ActionExpense > x.ActionIncome)).Where<StockAction>(filter).ToArray<StockAction>();
    for (int index = 0; index < stockActionArray.Length; ++index)
    {
      StockAction expenseAction = stockActionArray[index];
      Decimal expenseActionQuantity = expenseAction.ActionExpense - expenseAction.ActionIncome;
      if (prevBalance > 0M)
      {
        if (expenseActionQuantity <= prevBalance)
        {
          prevBalance -= expenseActionQuantity;
          expensable -= expenseActionQuantity;
          continue;
        }
        expenseActionQuantity -= prevBalance;
        expensable -= prevBalance;
        prevBalance = 0M;
      }
      foreach (StockTracking stockTracking in await actionsRepository.TrackActionsAsync(lineSourceId: expenseAction.ActionId, expensable: new Decimal?(expensable)))
      {
        if (stockTracking.WarehouseId == expenseAction.ActionWarehouseId)
          expenseActionQuantity -= stockTracking.Left;
        else
          tracking.Add(stockTracking);
      }
      if (expenseActionQuantity > expensable)
        expenseActionQuantity = expensable;
      expenses += expenseActionQuantity;
      expensable -= expenseActionQuantity;
      if (!(expensable == 0M))
        expenseAction = (StockAction) null;
      else
        break;
    }
    stockActionArray = (StockAction[]) null;
    return expenses;
  }

  internal class StockActionSimple
  {
    public string Id { get; set; }

    public DateTime Date { get; set; }

    public string WarehouseId { get; set; }

    public string StockId { get; set; }

    public Decimal Income { get; set; }

    public Decimal Expense { get; set; }
  }
}
