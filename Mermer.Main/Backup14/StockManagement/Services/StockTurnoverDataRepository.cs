// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.StockManagement.Services.StockTurnoverDataRepository
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase.Views;
using Microsoft.CSharp.RuntimeBinder;
using Mermer.Core.Couch.Common;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.StockManagement.Services;

public class StockTurnoverDataRepository : CouchView, IStockTurnoverDataRepository
{
  private readonly IStocksRepository _stocksRepository;

  public StockTurnoverDataRepository(ICouchCluster cluster, IStocksRepository stocksRepository)
    : base(cluster)
  {
    this._stocksRepository = stocksRepository;
  }

  public async Task<IEnumerable<StockTurnoverData>> GetAsync(string warehouseId = null)
  {
    StockTurnoverDataRepository turnoverDataRepository = this;
    Tuple<object, object>[] tupleArray;
    if (string.IsNullOrEmpty(warehouseId))
      tupleArray = (Tuple<object, object>[]) null;
    else
      tupleArray = new Tuple<object, object>[1]
      {
        new Tuple<object, object>((object) new string[2]
        {
          warehouseId,
          "0"
        }, (object) new string[2]{ warehouseId, "zzz" })
      };
    Tuple<object, object>[] startEndKeys = tupleArray;
    StockTurnoverData[] list = (await turnoverDataRepository.GetRecordsAsync<StockTurnoverData>("stock-management-reporting", "stock-turnovers", startEndKeys, true, 2, (Func<ViewRow<StockTurnoverData>, StockTurnoverData>) (x =>
    {
      StockTurnoverData async = x.Value;
      StockTurnoverData stockTurnoverData1 = async;
      // ISSUE: reference to a compiler-generated field
      if (StockTurnoverDataRepository.\u003C\u003Eo__2.\u003C\u003Ep__1 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockTurnoverDataRepository.\u003C\u003Eo__2.\u003C\u003Ep__1 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockTurnoverDataRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target1 = StockTurnoverDataRepository.\u003C\u003Eo__2.\u003C\u003Ep__1.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p1 = StockTurnoverDataRepository.\u003C\u003Eo__2.\u003C\u003Ep__1;
      // ISSUE: reference to a compiler-generated field
      if (StockTurnoverDataRepository.\u003C\u003Eo__2.\u003C\u003Ep__0 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockTurnoverDataRepository.\u003C\u003Eo__2.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockTurnoverDataRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj1 = StockTurnoverDataRepository.\u003C\u003Eo__2.\u003C\u003Ep__0.Target((CallSite) StockTurnoverDataRepository.\u003C\u003Eo__2.\u003C\u003Ep__0, x.Key, 0);
      string str1 = target1((CallSite) p1, obj1);
      stockTurnoverData1.WarehouseId = str1;
      StockTurnoverData stockTurnoverData2 = async;
      // ISSUE: reference to a compiler-generated field
      if (StockTurnoverDataRepository.\u003C\u003Eo__2.\u003C\u003Ep__3 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockTurnoverDataRepository.\u003C\u003Eo__2.\u003C\u003Ep__3 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (StockTurnoverDataRepository)));
      }
      // ISSUE: reference to a compiler-generated field
      Func<CallSite, object, string> target2 = StockTurnoverDataRepository.\u003C\u003Eo__2.\u003C\u003Ep__3.Target;
      // ISSUE: reference to a compiler-generated field
      CallSite<Func<CallSite, object, string>> p3 = StockTurnoverDataRepository.\u003C\u003Eo__2.\u003C\u003Ep__3;
      // ISSUE: reference to a compiler-generated field
      if (StockTurnoverDataRepository.\u003C\u003Eo__2.\u003C\u003Ep__2 == null)
      {
        // ISSUE: reference to a compiler-generated field
        StockTurnoverDataRepository.\u003C\u003Eo__2.\u003C\u003Ep__2 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (StockTurnoverDataRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
        {
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
          CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
        }));
      }
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      object obj2 = StockTurnoverDataRepository.\u003C\u003Eo__2.\u003C\u003Ep__2.Target((CallSite) StockTurnoverDataRepository.\u003C\u003Eo__2.\u003C\u003Ep__2, x.Key, 1);
      string str2 = target2((CallSite) p3, obj2);
      stockTurnoverData2.StockId = str2;
      return async;
    }))).ToArray<StockTurnoverData>();
    string[] array = ((IEnumerable<StockTurnoverData>) list).Select<StockTurnoverData, string>((Func<StockTurnoverData, string>) (x => x.StockId)).Distinct<string>().ToArray<string>();
    Dictionary<string, Stock> stocks = (await turnoverDataRepository._stocksRepository.GetListAsync(array)).ToDictionary<Stock, string, Stock>((Func<Stock, string>) (x => x.Id), (Func<Stock, Stock>) (x => x));
    IEnumerable<StockTurnoverData> async1 = ((IEnumerable<StockTurnoverData>) list).Select<StockTurnoverData, StockTurnoverData>((Func<StockTurnoverData, StockTurnoverData>) (x =>
    {
      Stock stock = stocks[x.StockId];
      x.StockCode = stock.Code;
      x.StockName = stock.Name;
      x.StockGroup = stock.Group;
      x.StockType = stock.Type;
      x.StockTags = stock.Tags;
      return x;
    }));
    list = (StockTurnoverData[]) null;
    return async1;
  }
}
