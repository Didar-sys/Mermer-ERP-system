// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Couch.Warehousing.Ordering.Services.StockOrderActionsRepository
// Assembly: Payhas.Binyat.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.Couch.dll

using Payhas.Binyat.Core.Couch.Common;
using Payhas.Binyat.Warehousing.Ordering.Models;
using Payhas.Binyat.Warehousing.Ordering.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Core.Couch.Warehousing.Ordering.Services;

public class StockOrderActionsRepository(ICouchCluster cluster) : 
  CouchView(cluster),
  IStockOrderActionsRepository
{
  public Task<IEnumerable<StockOrderAction>> GetAsync(string stockId)
  {
    return this.GetRecordsAsync<StockOrderAction>("warehousing-ordering", "stock-orders", new object[1]
    {
      (object) stockId
    });
  }
}
