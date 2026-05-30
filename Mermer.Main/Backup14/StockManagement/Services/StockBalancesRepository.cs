// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.StockManagement.Services.StockBalancesRepository
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase.Views;
using Microsoft.CSharp.RuntimeBinder;
using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Commerce.Models;
using Mermer.Core.Couch.Common;
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

public class StockBalancesRepository : CouchView, IStockBalancesRepository
{
  private readonly ILoginService _loginService;
  private readonly IStocksRepository _stocksRepository;
  private readonly IRepository<Currency> _currenciesRepository;
  private readonly IRepository<Warehouse> _warehousesRepository;
  private readonly IAuthorizationService _authorizationService;
  private readonly IReadOnlyListAuthorizer<StockBalanceWithData> _authorizer;

  public StockBalancesRepository(
    ICouchCluster cluster,
    ILoginService loginService,
    IStocksRepository stocksRepository,
    IRepository<Currency> currenciesRepository,
    IRepository<Warehouse> warehousesRepository,
    IAuthorizationService authorizationService,
    IReadOnlyListAuthorizer<StockBalanceWithData> authorizer)
    : base(cluster)
  {
    this._loginService = loginService;
    this._stocksRepository = stocksRepository;
    this._currenciesRepository = currenciesRepository;
    this._warehousesRepository = warehousesRepository;
    this._authorizationService = authorizationService;
    this._authorizer = authorizer;
  }

  public async Task<IEnumerable<StockBalance>> GetAsync(
    string stockId,
    DateTime date,
    params string[] warehouses)
  {
    StockBalancesRepository balancesRepository = this;
    balancesRepository._authorizer.Authorize();
    if (balancesRepository._loginService.Session.IsAdmin)
    {
      if (warehouses == null || !((IEnumerable<string>) warehouses).Any<string>())
        warehouses = (await balancesRepository._warehousesRepository.GetAsync()).Select<Warehouse, string>((Func<Warehouse, string>) (x => x.Id)).ToArray<string>();
    }
    else
    {
      string[] accounts = balancesRepository._authorizationService.GetAccessableAccounts(AccountAccessLevel.Read).ToArray<string>();
      warehouses = ((IEnumerable<string>) warehouses).Where<string>((Func<string, bool>) (x => ((IEnumerable<string>) accounts).Contains<string>(x))).ToArray<string>();
    }
    if (!((IEnumerable<string>) warehouses).Any<string>())
      return (IEnumerable<StockBalance>) Array.Empty<StockBalance>();
    if (!string.IsNullOrEmpty(stockId))
    {
      Tuple<object, object>[] array = ((IEnumerable<string>) warehouses).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (accountId => new Tuple<object, object>((object) new string[3]
      {
        accountId,
        stockId,
        "0"
      }, (object) new string[3]
      {
        accountId,
        stockId,
        date.ToString("o")
      }))).ToArray<Tuple<object, object>>();
      return await balancesRepository.GetRecordsAsync<StockBalance>("stock-management", "stock-balances-by-warehouse-and-id", array, true, 2, (Func<ViewRow<StockBalance>, StockBalance>) (x =>
      {
        StockBalance async = x.Value;
        StockBalance stockBalance1 = async;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__1 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__1 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target1 = StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__1.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p1 = StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__1;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__0 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj1 = StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__0.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__0, x.Key, 0);
        string str1 = target1((CallSite) p1, obj1);
        stockBalance1.WarehouseId = str1;
        StockBalance stockBalance2 = async;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__3 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__3 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target2 = StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__3.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p3 = StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__3;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__2 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__2 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj2 = StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__2.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__2, x.Key, 1);
        string str2 = target2((CallSite) p3, obj2);
        stockBalance2.StockId = str2;
        return async;
      }));
    }
    Tuple<object, object>[] array1 = ((IEnumerable<string>) warehouses).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (accountId => new Tuple<object, object>((object) new string[2]
    {
      accountId,
      "0"
    }, (object) new string[2]
    {
      accountId,
      date.ToString("o")
    }))).ToArray<Tuple<object, object>>();
    return (await balancesRepository.GetRecordsAsync<StockBalance>("stock-management", "stock-balances-by-warehouse", array1, true, 3, (Func<ViewRow<StockBalance>, StockBalance>) (x =>
    {
      StockBalance async = x.Value;
      StockBalance stockBalance3 = async;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__5 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__5 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target3 = StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__5.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p5 = StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__5;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__4 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__4 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj3 = StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__4.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__4, x.Key, 0);
      string str3 = target3((CallSite) p5, obj3);
      stockBalance3.WarehouseId = str3;
      StockBalance stockBalance4 = async;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__7 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__7 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target4 = StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__7.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p7 = StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__7;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__6 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__6 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj4 = StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__6.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__7.\u003C\u003Ep__6, x.Key, 2);
      string str4 = target4((CallSite) p7, obj4);
      stockBalance4.StockId = str4;
      return async;
    }))).GroupBy(x => new
    {
      WarehouseId = x.WarehouseId,
      StockId = x.StockId
    }).Select<IGrouping<\u003C\u003Ef__AnonymousType4<string, string>, StockBalance>, StockBalance>(g => new StockBalance()
    {
      WarehouseId = g.Key.WarehouseId,
      StockId = g.Key.StockId,
      Income = g.Sum<StockBalance>((Func<StockBalance, Decimal>) (x => x.Income)),
      Expense = g.Sum<StockBalance>((Func<StockBalance, Decimal>) (x => x.Expense))
    });
  }

  public async Task<IEnumerable<StockBalanceWithCodeAndName>> GetAsync(
    string warehouseId,
    string[] stockIds,
    string excludedTransactionId)
  {
    StockBalancesRepository balancesRepository = this;
    balancesRepository._authorizer.Authorize();
    if (!balancesRepository._loginService.Session.IsAdmin && !((IEnumerable<string>) balancesRepository._authorizationService.GetAccessableAccounts(AccountAccessLevel.Read).ToArray<string>()).Contains<string>(warehouseId))
      return (IEnumerable<StockBalanceWithCodeAndName>) Array.Empty<StockBalanceWithCodeAndName>();
    Tuple<object, object>[] array = ((IEnumerable<string>) Enum.GetValues(typeof (InvoiceType)).Cast<Enum>().Concat<Enum>(Enum.GetValues(typeof (StockSlipType)).Cast<Enum>()).Select<Enum, string>((Func<Enum, string>) (x => x.ToString())).Concat<string>((IEnumerable<string>) new string[2]
    {
      "StockTransferSource",
      "StockTransferDestination"
    }).ToArray<string>()).SelectMany<string, Tuple<object, object>>((Func<string, IEnumerable<Tuple<object, object>>>) (actionType => ((IEnumerable<string>) stockIds).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (stockId => new Tuple<object, object>((object) new string[4]
    {
      actionType,
      warehouseId,
      stockId,
      DateTime.MinValue.ToString("o")
    }, (object) new string[4]
    {
      actionType,
      warehouseId,
      stockId,
      DateTime.MaxValue.ToString("o")
    }))))).ToArray<Tuple<object, object>>();
    IEnumerable<StockBalancesRepository.StockActionTempData> stockActions = await balancesRepository.GetRecordsAsync<object, StockBalancesRepository.StockActionTempData>("stock-management", "stock-actions-by-warehouse-and-id-all", array, projector: (Func<ViewRow<object>, StockBalancesRepository.StockActionTempData>) (x =>
    {
      StockBalancesRepository.StockActionTempData async = new StockBalancesRepository.StockActionTempData();
      StockBalancesRepository.StockActionTempData stockActionTempData1 = async;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__1 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__1 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof (string), typeof (StockBalancesRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target1 = StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__1.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p1 = StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__1;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__0 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj1 = StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__0.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__0, x.Key, 2);
      string str1 = target1((CallSite) p1, obj1);
      stockActionTempData1.StockId = str1;
      StockBalancesRepository.StockActionTempData stockActionTempData2 = async;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__3 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__3 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof (string), typeof (StockBalancesRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target2 = StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__3.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p3 = StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__3;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__2 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__2 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "tId", typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj2 = StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__2.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__2, x.Value);
      string str2 = target2((CallSite) p3, obj2);
      stockActionTempData2.TransactionId = str2;
      StockBalancesRepository.StockActionTempData stockActionTempData3 = async;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__5 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__5 = CallSite<Func<CallSite, object, bool>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (bool), typeof (StockBalancesRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, bool> target3 = StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__5.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, bool>> p5 = StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__5;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__4 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__4 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "tIsDisabled", typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj3 = StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__4.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__4, x.Value);
      int num1 = target3((CallSite) p5, obj3) ? 1 : 0;
      stockActionTempData3.TransactionIsDisabled = num1 != 0;
      StockBalancesRepository.StockActionTempData stockActionTempData4 = async;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__7 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__7 = CallSite<Func<CallSite, object, bool>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (bool), typeof (StockBalancesRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, bool> target4 = StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__7.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, bool>> p7 = StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__7;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__6 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__6 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "tIsCompleted", typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj4 = StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__6.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__6, x.Value);
      int num2 = target4((CallSite) p7, obj4) ? 1 : 0;
      stockActionTempData4.TransactionIsCompleted = num2 != 0;
      StockBalancesRepository.StockActionTempData stockActionTempData5 = async;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__9 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__9 = CallSite<Func<CallSite, object, Decimal>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (Decimal), typeof (StockBalancesRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, Decimal> target5 = StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__9.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, Decimal>> p9 = StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__9;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__8 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__8 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "aIncome", typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj5 = StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__8.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__8, x.Value);
      Decimal num3 = target5((CallSite) p9, obj5);
      stockActionTempData5.ActionIncome = num3;
      StockBalancesRepository.StockActionTempData stockActionTempData6 = async;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__11 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__11 = CallSite<Func<CallSite, object, Decimal>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (Decimal), typeof (StockBalancesRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, Decimal> target6 = StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__11.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, Decimal>> p11 = StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__11;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__10 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__10 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "aExpense", typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj6 = StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__10.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__9.\u003C\u003Ep__10, x.Value);
      Decimal num4 = target6((CallSite) p11, obj6);
      stockActionTempData6.ActionExpense = num4;
      return async;
    }));
    return (await balancesRepository._stocksRepository.GetListAsync(stockIds)).ToList<Stock>().GroupJoin<Stock, StockBalancesRepository.StockActionTempData, string, StockBalanceWithCodeAndName>(stockActions.Where<StockBalancesRepository.StockActionTempData>((Func<StockBalancesRepository.StockActionTempData, bool>) (x => x.TransactionId != excludedTransactionId && x.TransactionIsCompleted && !x.TransactionIsDisabled)), (Func<Stock, string>) (x => x.Id), (Func<StockBalancesRepository.StockActionTempData, string>) (x => x.StockId), (Func<Stock, IEnumerable<StockBalancesRepository.StockActionTempData>, StockBalanceWithCodeAndName>) ((stock, g) =>
    {
      return new StockBalanceWithCodeAndName()
      {
        StockId = stock.Id,
        StockCode = stock.Code,
        StockName = stock.Name,
        WarehouseId = warehouseId,
        Income = g.Sum<StockBalancesRepository.StockActionTempData>((Func<StockBalancesRepository.StockActionTempData, Decimal>) (x => x.ActionIncome)),
        Expense = g.Sum<StockBalancesRepository.StockActionTempData>((Func<StockBalancesRepository.StockActionTempData, Decimal>) (x => x.ActionExpense))
      };
    }));
  }

  public Task<IEnumerable<StockBalance>> GetAsync(
    string warehouseId,
    string[] stockIds,
    DateTime? date = null)
  {
    if (string.IsNullOrEmpty(warehouseId))
      throw new ArgumentNullException(nameof (warehouseId));
    return this.GetAsync(new string[1]{ warehouseId }, stockIds, date);
  }

  public async Task<IEnumerable<StockBalance>> GetAsync(
    string[] warehouseIds,
    string[] stockIds,
    DateTime? date = null)
  {
    StockBalancesRepository balancesRepository = this;
    balancesRepository._authorizer.Authorize();
    if (!((IEnumerable<string>) warehouseIds).Any<string>())
      throw new ArgumentNullException(nameof (warehouseIds));
    if (!((IEnumerable<string>) stockIds).Any<string>())
      throw new ArgumentNullException(nameof (stockIds));
    if (!balancesRepository._loginService.Session.IsAdmin)
    {
      string[] accounts = balancesRepository._authorizationService.GetAccessableAccounts(AccountAccessLevel.Read).ToArray<string>();
      warehouseIds = !((IEnumerable<string>) warehouseIds).Any<string>() || ((IEnumerable<string>) warehouseIds).Any<string>(new Func<string, bool>(string.IsNullOrEmpty)) ? accounts : ((IEnumerable<string>) warehouseIds).Where<string>((Func<string, bool>) (x => ((IEnumerable<string>) accounts).Contains<string>(x))).ToArray<string>();
      if (!((IEnumerable<string>) warehouseIds).Any<string>())
        return (IEnumerable<StockBalance>) Array.Empty<StockBalance>();
    }
    Tuple<object, object>[] array = ((IEnumerable<string>) warehouseIds).SelectMany<string, Tuple<object, object>>((Func<string, IEnumerable<Tuple<object, object>>>) (accountId => ((IEnumerable<string>) stockIds).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (stockId => new Tuple<object, object>((object) new string[3]
    {
      accountId,
      stockId,
      "0"
    }, (object) new string[3]
    {
      accountId,
      stockId,
      date.HasValue ? date.Value.ToString("o") : "zzz"
    }))))).ToArray<Tuple<object, object>>();
    return await balancesRepository.GetRecordsAsync<StockBalance>("stock-management", "stock-balances-by-warehouse-and-id", array, true, 2, (Func<ViewRow<StockBalance>, StockBalance>) (x =>
    {
      StockBalance async = x.Value;
      StockBalance stockBalance1 = async;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__11.\u003C\u003Ep__1 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__11.\u003C\u003Ep__1 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target1 = StockBalancesRepository.\u003C\u003Eo__11.\u003C\u003Ep__1.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p1 = StockBalancesRepository.\u003C\u003Eo__11.\u003C\u003Ep__1;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__11.\u003C\u003Ep__0 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__11.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj1 = StockBalancesRepository.\u003C\u003Eo__11.\u003C\u003Ep__0.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__11.\u003C\u003Ep__0, x.Key, 0);
      string str1 = target1((CallSite) p1, obj1);
      stockBalance1.WarehouseId = str1;
      StockBalance stockBalance2 = async;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__11.\u003C\u003Ep__3 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__11.\u003C\u003Ep__3 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target2 = StockBalancesRepository.\u003C\u003Eo__11.\u003C\u003Ep__3.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p3 = StockBalancesRepository.\u003C\u003Eo__11.\u003C\u003Ep__3;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__11.\u003C\u003Ep__2 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__11.\u003C\u003Ep__2 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj2 = StockBalancesRepository.\u003C\u003Eo__11.\u003C\u003Ep__2.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__11.\u003C\u003Ep__2, x.Key, 1);
      string str2 = target2((CallSite) p3, obj2);
      stockBalance2.StockId = str2;
      return async;
    }));
  }

  public Task<IEnumerable<StockBalance>> GetAsync(
    string warehouseId,
    (string stockId, DateTime? balanceDate)[] stockBalanceDates)
  {
    return !string.IsNullOrEmpty(warehouseId) ? this.GetAsync(new string[1]
    {
      warehouseId
    }, stockBalanceDates) : throw new ArgumentNullException(nameof (warehouseId));
  }

  public async Task<IEnumerable<StockBalance>> GetAsync(
    string[] warehouseIds,
    (string stockId, DateTime? balanceDate)[] stockBalanceDates)
  {
    StockBalancesRepository balancesRepository = this;
    balancesRepository._authorizer.Authorize();
    if (!((IEnumerable<string>) warehouseIds).Any<string>())
      throw new ArgumentNullException(nameof (warehouseIds));
    if (!((IEnumerable<(string, DateTime?)>) stockBalanceDates).Any<(string, DateTime?)>())
      throw new ArgumentNullException(nameof (stockBalanceDates));
    if (!balancesRepository._loginService.Session.IsAdmin)
    {
      string[] accounts = balancesRepository._authorizationService.GetAccessableAccounts(AccountAccessLevel.Read).ToArray<string>();
      warehouseIds = !((IEnumerable<string>) warehouseIds).Any<string>() || ((IEnumerable<string>) warehouseIds).Any<string>(new Func<string, bool>(string.IsNullOrEmpty)) ? accounts : ((IEnumerable<string>) warehouseIds).Where<string>((Func<string, bool>) (x => ((IEnumerable<string>) accounts).Contains<string>(x))).ToArray<string>();
      if (!((IEnumerable<string>) warehouseIds).Any<string>())
        return (IEnumerable<StockBalance>) Array.Empty<StockBalance>();
    }
    Tuple<object, object>[] array = ((IEnumerable<string>) warehouseIds).SelectMany<string, Tuple<object, object>>((Func<string, IEnumerable<Tuple<object, object>>>) (accountId => ((IEnumerable<(string, DateTime?)>) stockBalanceDates).Select<(string, DateTime?), Tuple<object, object>>((Func<(string, DateTime?), Tuple<object, object>>) (x => new Tuple<object, object>((object) new string[3]
    {
      accountId,
      x.stockId,
      "0"
    }, (object) new string[3]
    {
      accountId,
      x.stockId,
      x.balanceDate.HasValue ? x.balanceDate.Value.ToString("o") : "zzz"
    }))))).ToArray<Tuple<object, object>>();
    return await balancesRepository.GetRecordsAsync<StockBalance>("stock-management", "stock-balances-by-warehouse-and-id", array, true, 2, (Func<ViewRow<StockBalance>, StockBalance>) (x =>
    {
      StockBalance async = x.Value;
      StockBalance stockBalance1 = async;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__13.\u003C\u003Ep__1 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__13.\u003C\u003Ep__1 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target1 = StockBalancesRepository.\u003C\u003Eo__13.\u003C\u003Ep__1.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p1 = StockBalancesRepository.\u003C\u003Eo__13.\u003C\u003Ep__1;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__13.\u003C\u003Ep__0 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__13.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj1 = StockBalancesRepository.\u003C\u003Eo__13.\u003C\u003Ep__0.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__13.\u003C\u003Ep__0, x.Key, 0);
      string str1 = target1((CallSite) p1, obj1);
      stockBalance1.WarehouseId = str1;
      StockBalance stockBalance2 = async;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__13.\u003C\u003Ep__3 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__13.\u003C\u003Ep__3 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target2 = StockBalancesRepository.\u003C\u003Eo__13.\u003C\u003Ep__3.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p3 = StockBalancesRepository.\u003C\u003Eo__13.\u003C\u003Ep__3;
      // ISSUE: reference to a compiler-generated field
      if (StockBalancesRepository.\u003C\u003Eo__13.\u003C\u003Ep__2 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockBalancesRepository.\u003C\u003Eo__13.\u003C\u003Ep__2 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj2 = StockBalancesRepository.\u003C\u003Eo__13.\u003C\u003Ep__2.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__13.\u003C\u003Ep__2, x.Key, 1);
      string str2 = target2((CallSite) p3, obj2);
      stockBalance2.StockId = str2;
      return async;
    }));
  }

  public async Task<IEnumerable<StockBalanceByTypeWithBalanceAndData>> GetByTypeAsync(
    string[] warehouseIds,
    string stockId,
    DateTime dateFrom,
    DateTime dateTill,
    bool aggregate)
  {
    StockBalancesRepository balancesRepository = this;
    balancesRepository._authorizer.Authorize();
    if (dateFrom >= dateTill)
      throw new ArgumentException("From date should be lower than or equal to till date");
    if (!balancesRepository._loginService.Session.IsAdmin)
    {
      List<string> accounts = balancesRepository._authorizationService.GetAccessableAccounts(AccountAccessLevel.Read).ToList<string>();
      warehouseIds = ((IEnumerable<string>) warehouseIds).Where<string>((Func<string, bool>) (x => accounts.Contains(x))).ToArray<string>();
    }
    if (!((IEnumerable<string>) warehouseIds).Any<string>())
      return (IEnumerable<StockBalanceByTypeWithBalanceAndData>) Array.Empty<StockBalanceByTypeWithBalanceAndData>();
    List<StockBalance> startingBalances;
    List<StockBalanceByType> changingBalances;
    if (!string.IsNullOrEmpty(stockId))
    {
      Tuple<object, object>[] array1 = ((IEnumerable<string>) warehouseIds).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (accountId => new Tuple<object, object>((object) new string[3]
      {
        accountId,
        stockId,
        "0"
      }, (object) new string[3]
      {
        accountId,
        stockId,
        dateFrom.ToString("yyyy-MM-dd")
      }))).ToArray<Tuple<object, object>>();
      startingBalances = (await balancesRepository.GetRecordsAsync<StockBalance>("stock-management", "stock-balances-by-warehouse-and-id", array1, true, 2, (Func<ViewRow<StockBalance>, StockBalance>) (x =>
      {
        StockBalance byTypeAsync = x.Value;
        StockBalance stockBalance1 = byTypeAsync;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__1 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__1 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target1 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__1.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p1 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__1;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__0 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj1 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__0.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__0, x.Key, 0);
        string str1 = target1((CallSite) p1, obj1);
        stockBalance1.WarehouseId = str1;
        StockBalance stockBalance2 = byTypeAsync;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__3 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__3 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target2 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__3.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p3 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__3;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__2 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__2 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj2 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__2.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__2, x.Key, 1);
        string str2 = target2((CallSite) p3, obj2);
        stockBalance2.StockId = str2;
        return byTypeAsync;
      }))).ToList<StockBalance>();
      Tuple<object, object>[] array2 = ((IEnumerable<string>) warehouseIds).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (accountId => new Tuple<object, object>((object) new string[3]
      {
        accountId,
        stockId,
        dateFrom.ToString("yyyy-MM-dd")
      }, (object) new string[3]
      {
        accountId,
        stockId,
        dateTill.ToString("yyyy-MM-dd")
      }))).ToArray<Tuple<object, object>>();
      changingBalances = (await balancesRepository.GetRecordsAsync<StockBalanceByType>("stock-management", "stock-balances-by-warehouse-and-id", array2, true, 2, (Func<ViewRow<StockBalanceByType>, StockBalanceByType>) (x =>
      {
        StockBalanceByType byTypeAsync = x.Value;
        StockBalanceByType stockBalanceByType1 = byTypeAsync;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__5 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__5 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target3 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__5.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p5 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__5;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__4 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__4 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj3 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__4.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__4, x.Key, 0);
        string str3 = target3((CallSite) p5, obj3);
        stockBalanceByType1.WarehouseId = str3;
        StockBalanceByType stockBalanceByType2 = byTypeAsync;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__7 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__7 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target4 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__7.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p7 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__7;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__6 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__6 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj4 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__6.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__6, x.Key, 1);
        string str4 = target4((CallSite) p7, obj4);
        stockBalanceByType2.StockId = str4;
        return byTypeAsync;
      }))).ToList<StockBalanceByType>();
    }
    else
    {
      Tuple<object, object>[] array3 = ((IEnumerable<string>) warehouseIds).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (accountId => new Tuple<object, object>((object) new string[2]
      {
        accountId,
        "0"
      }, (object) new string[2]
      {
        accountId,
        dateFrom.ToString("yyyy-MM-dd")
      }))).ToArray<Tuple<object, object>>();
      startingBalances = (await balancesRepository.GetRecordsAsync<StockBalance>("stock-management", "stock-balances-by-warehouse", array3, true, 3, (Func<ViewRow<StockBalance>, StockBalance>) (x =>
      {
        StockBalance byTypeAsync = x.Value;
        StockBalance stockBalance3 = byTypeAsync;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__9 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__9 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target5 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__9.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p9 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__9;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__8 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__8 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj5 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__8.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__8, x.Key, 0);
        string str5 = target5((CallSite) p9, obj5);
        stockBalance3.WarehouseId = str5;
        StockBalance stockBalance4 = byTypeAsync;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__11 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__11 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target6 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__11.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p11 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__11;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__10 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__10 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj6 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__10.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__10, x.Key, 2);
        string str6 = target6((CallSite) p11, obj6);
        stockBalance4.StockId = str6;
        return byTypeAsync;
      }))).ToList<StockBalance>();
      Tuple<object, object>[] array4 = ((IEnumerable<string>) warehouseIds).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (accountId => new Tuple<object, object>((object) new string[2]
      {
        accountId,
        dateFrom.ToString("yyyy-MM-dd")
      }, (object) new string[2]
      {
        accountId,
        dateTill.ToString("yyyy-MM-dd")
      }))).ToArray<Tuple<object, object>>();
      changingBalances = (await balancesRepository.GetRecordsAsync<StockBalanceByType>("stock-management", "stock-balances-by-warehouse", array4, true, 3, (Func<ViewRow<StockBalanceByType>, StockBalanceByType>) (x =>
      {
        StockBalanceByType byTypeAsync = x.Value;
        StockBalanceByType stockBalanceByType3 = byTypeAsync;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__13 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__13 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target7 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__13.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p13 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__13;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__12 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__12 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj7 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__12.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__12, x.Key, 0);
        string str7 = target7((CallSite) p13, obj7);
        stockBalanceByType3.WarehouseId = str7;
        StockBalanceByType stockBalanceByType4 = byTypeAsync;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__15 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__15 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target8 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__15.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p15 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__15;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__14 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__14 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj8 = StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__14.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__14.\u003C\u003Ep__14, x.Key, 2);
        string str8 = target8((CallSite) p15, obj8);
        stockBalanceByType4.StockId = str8;
        return byTypeAsync;
      }))).ToList<StockBalanceByType>();
    }
    string[] array = startingBalances.Select<StockBalance, string>((Func<StockBalance, string>) (x => x.StockId)).Union<string>(changingBalances.Select<StockBalanceByType, string>((Func<StockBalanceByType, string>) (x => x.StockId))).Distinct<string>().ToArray<string>();
    if (!((IEnumerable<string>) array).Any<string>())
      return (IEnumerable<StockBalanceByTypeWithBalanceAndData>) Array.Empty<StockBalanceByTypeWithBalanceAndData>();
    IEnumerable<StockInfo> stocks = await balancesRepository._stocksRepository.GetInfoAsync(array);
    return !aggregate ? startingBalances.Select(x => new
    {
      WarehouseId = x.WarehouseId,
      StockId = x.StockId
    }).Union(changingBalances.Select(x => new
    {
      WarehouseId = x.WarehouseId,
      StockId = x.StockId
    })).Distinct().Select(x => new
    {
      item = x,
      stock = stocks.Single<StockInfo>((Func<StockInfo, bool>) (y => y.Id == x.StockId)),
      startingBalances = startingBalances.Where<StockBalance>((Func<StockBalance, bool>) (z => z.WarehouseId == x.WarehouseId && z.StockId == x.StockId)),
      changingBalances = changingBalances.Where<StockBalanceByType>((Func<StockBalanceByType, bool>) (z => z.WarehouseId == x.WarehouseId && z.StockId == x.StockId))
    }).Select(x =>
    {
      return new StockBalanceByTypeWithBalanceAndData()
      {
        WarehouseId = x.item.WarehouseId,
        StockId = x.item.StockId,
        StockCode = x.stock.Code,
        StockName = x.stock.Name,
        StockShortName = x.stock.ShortName,
        StockUnit = x.stock.Unit,
        StockPrice = x.stock.Price,
        StockCurrencyId = x.stock.CurrencyId,
        StockType = x.stock.Type,
        StockGroup = x.stock.Group,
        StockTags = x.stock.Tags,
        StartingBalance = x.startingBalances.Sum<StockBalance>((Func<StockBalance, Decimal>) (z => z.Balance)),
        Income = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.Income)),
        Expense = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.Expense)),
        StockOpening = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.StockOpening)),
        StockSpoilage = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.StockSpoilage)),
        StockUsage = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.StockUsage)),
        RevisionExceed = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.RevisionExceed)),
        RevisionDeficit = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.RevisionDeficit)),
        StockTransferSource = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.StockTransferSource)),
        StockTransferDestination = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.StockTransferDestination)),
        Sales = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.Sales)),
        SalesReturn = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.SalesReturn)),
        Purchase = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.Purchase)),
        PurchaseReturn = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.PurchaseReturn))
      };
    }) : stocks.Select(x => new
    {
      stock = x,
      startingBalances = startingBalances.Where<StockBalance>((Func<StockBalance, bool>) (z => z.StockId == x.Id)),
      changingBalances = changingBalances.Where<StockBalanceByType>((Func<StockBalanceByType, bool>) (z => z.StockId == x.Id))
    }).Select(x =>
    {
      return new StockBalanceByTypeWithBalanceAndData()
      {
        WarehouseId = (string) null,
        StockId = x.stock.Id,
        StockCode = x.stock.Code,
        StockName = x.stock.Name,
        StockShortName = x.stock.ShortName,
        StockUnit = x.stock.Unit,
        StockPrice = x.stock.Price,
        StockCurrencyId = x.stock.CurrencyId,
        StockType = x.stock.Type,
        StockGroup = x.stock.Group,
        StockTags = x.stock.Tags,
        StartingBalance = x.startingBalances.Sum<StockBalance>((Func<StockBalance, Decimal>) (z => z.Balance)),
        Income = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.Income)),
        Expense = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.Expense)),
        StockOpening = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.StockOpening)),
        StockSpoilage = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.StockSpoilage)),
        StockUsage = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.StockUsage)),
        RevisionExceed = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.RevisionExceed)),
        RevisionDeficit = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.RevisionDeficit)),
        StockTransferSource = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.StockTransferSource)),
        StockTransferDestination = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.StockTransferDestination)),
        Sales = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.Sales)),
        SalesReturn = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.SalesReturn)),
        Purchase = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.Purchase)),
        PurchaseReturn = x.changingBalances.Sum<StockBalanceByType>((Func<StockBalanceByType, Decimal>) (z => z.PurchaseReturn))
      };
    });
  }

  public async Task<IEnumerable<StockBalanceByWarehouses>> GetByDateAndWarehousesAsync(
    DateTime date,
    IEnumerable<string> warehouseIds,
    string displayCurrencyId,
    IEnumerable<string> stockIds = null)
  {
    StockBalancesRepository balancesRepository = this;
    balancesRepository._authorizer.Authorize();
    if (!(warehouseIds is string[] strArray1))
      strArray1 = warehouseIds.ToArray<string>();
    string[] source1 = strArray1;
    if (!balancesRepository._loginService.Session.IsAdmin)
    {
      string[] accounts = balancesRepository._authorizationService.GetAccessableAccounts(AccountAccessLevel.Read).ToArray<string>();
      source1 = ((IEnumerable<string>) source1).Where<string>((Func<string, bool>) (x => ((IEnumerable<string>) accounts).Contains<string>(x))).ToArray<string>();
    }
    if (!((IEnumerable<string>) source1).Any<string>())
      return (IEnumerable<StockBalanceByWarehouses>) Array.Empty<StockBalanceByWarehouses>();
    if (!(stockIds is string[] strArray2))
    {
      IEnumerable<string> source2 = stockIds;
      strArray2 = source2 != null ? source2.ToArray<string>() : (string[]) null;
    }
    string[] stockIdsArray = strArray2;
    IEnumerable<StockBalance> recordsAsync;
    if (stockIdsArray != null)
    {
      if (!((IEnumerable<string>) stockIdsArray).Any<string>())
        return (IEnumerable<StockBalanceByWarehouses>) Array.Empty<StockBalanceByWarehouses>();
      Tuple<object, object>[] array = ((IEnumerable<string>) source1).SelectMany<string, Tuple<object, object>>((Func<string, IEnumerable<Tuple<object, object>>>) (accountId => ((IEnumerable<string>) stockIdsArray).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (stockId => new Tuple<object, object>((object) new string[3]
      {
        accountId,
        stockId,
        "0"
      }, (object) new string[3]
      {
        accountId,
        stockId,
        date.ToString("o")
      }))))).ToArray<Tuple<object, object>>();
      recordsAsync = await balancesRepository.GetRecordsAsync<StockBalance>("stock-management", "stock-balances-by-warehouse-and-id", array, true, 2, (Func<ViewRow<StockBalance>, StockBalance>) (x =>
      {
        StockBalance andWarehousesAsync = x.Value;
        StockBalance stockBalance1 = andWarehousesAsync;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__1 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__1 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target1 = StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__1.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p1 = StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__1;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__0 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj1 = StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__0.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__0, x.Key, 0);
        string str1 = target1((CallSite) p1, obj1);
        stockBalance1.WarehouseId = str1;
        StockBalance stockBalance2 = andWarehousesAsync;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__3 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__3 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target2 = StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__3.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p3 = StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__3;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__2 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__2 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj2 = StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__2.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__2, x.Key, 1);
        string str2 = target2((CallSite) p3, obj2);
        stockBalance2.StockId = str2;
        return andWarehousesAsync;
      }));
    }
    else
    {
      Tuple<object, object>[] array = ((IEnumerable<string>) source1).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (accountId => new Tuple<object, object>((object) new string[2]
      {
        accountId,
        "0"
      }, (object) new string[2]
      {
        accountId,
        date.ToString("o")
      }))).ToArray<Tuple<object, object>>();
      recordsAsync = await balancesRepository.GetRecordsAsync<StockBalance>("stock-management", "stock-balances-by-warehouse", array, true, 3, (Func<ViewRow<StockBalance>, StockBalance>) (x =>
      {
        StockBalance andWarehousesAsync = x.Value;
        StockBalance stockBalance3 = andWarehousesAsync;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__5 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__5 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target3 = StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__5.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p5 = StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__5;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__4 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__4 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj3 = StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__4.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__4, x.Key, 0);
        string str3 = target3((CallSite) p5, obj3);
        stockBalance3.WarehouseId = str3;
        StockBalance stockBalance4 = andWarehousesAsync;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__7 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__7 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target4 = StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__7.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p7 = StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__7;
        // ISSUE: reference to a compiler-generated field
        if (StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__6 == null)
        {
          // ISSUE: reference to a compiler-generated field
          StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__6 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj4 = StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__6.Target((CallSite) StockBalancesRepository.\u003C\u003Eo__15.\u003C\u003Ep__6, x.Key, 2);
        string str4 = target4((CallSite) p7, obj4);
        stockBalance4.StockId = str4;
        return andWarehousesAsync;
      }));
    }
    \u003C\u003Ef__AnonymousType8<string, string, Decimal>[] blanacesByStockAndWarehouse = recordsAsync.GroupBy(x => new
    {
      StockId = x.StockId,
      WarehouseId = x.WarehouseId
    }).Select(g => new
    {
      StockId = g.Key.StockId,
      WarehouseId = g.Key.WarehouseId,
      Balance = g.Sum<StockBalance>((Func<StockBalance, Decimal>) (x => x.Balance))
    }).ToArray();
    Stock[] array1;
    if (stockIdsArray != null)
      array1 = (await balancesRepository._stocksRepository.GetAsync(stockIdsArray)).ToArray<Stock>();
    else
      array1 = (await balancesRepository._stocksRepository.GetAsync()).ToArray<Stock>();
    Stock[] stocks = array1;
    Currency[] array2 = (await balancesRepository._currenciesRepository.GetAsync()).ToArray<Currency>();
    Currency currency = ((IEnumerable<Currency>) array2).Single<Currency>((Func<Currency, bool>) (x => x.Id == displayCurrencyId));
    CurrencyRate displayCurrencyRate = currency.GetRate(new DateTime?(date));
    int displayCurrencyDecimals = currency.Decimals;
    return ((IEnumerable<Stock>) stocks).Select(x => new
    {
      stock = x,
      price = x.GetPrice(new DateTime?(date))
    }).Join((IEnumerable<Currency>) array2, x => x.price.CurrencyId, (Func<Currency, string>) (x => x.Id), (x, c) => new
    {
      stock = x.stock,
      price = x.price,
      currencyRate = c.GetRate(new DateTime?(date))
    }).GroupJoin(blanacesByStockAndWarehouse, x => x.stock.Id, x => x.StockId, (x, b) => new StockBalanceByWarehouses()
    {
      StockId = x.stock.Id,
      StockCode = x.stock.Code,
      StockName = x.stock.Name,
      StockUnit = x.stock.Unit,
      StockPrice = Math.Round(x.price.Price * x.currencyRate.Multiplier / x.currencyRate.Divider / displayCurrencyRate.Multiplier * displayCurrencyRate.Divider, displayCurrencyDecimals),
      StockPriceCurrencyId = displayCurrencyId,
      StockGroup = x.stock.Group,
      StockType = x.stock.Type,
      StockTags = x.stock.Tags == null ? "" : string.Join(" ", x.stock.Tags),
      Balances = b.ToDictionary(y => y.WarehouseId, y => y.Balance)
    });
  }

  private class StockActionTempData
  {
    public string StockId { get; set; }

    public string TransactionId { get; set; }

    public bool TransactionIsDisabled { get; set; }

    public bool TransactionIsCompleted { get; set; }

    public Decimal ActionIncome { get; set; }

    public Decimal ActionExpense { get; set; }
  }
}
