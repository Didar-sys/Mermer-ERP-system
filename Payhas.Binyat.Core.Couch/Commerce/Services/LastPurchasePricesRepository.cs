// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Couch.Commerce.Services.LastPurchasePricesRepository
// Assembly: Payhas.Binyat.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.Couch.dll

using Payhas.Binyat.Commerce.Models;
using Payhas.Binyat.Commerce.Services;
using Payhas.Binyat.Core.Couch.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Core.Couch.Commerce.Services;

public class LastPurchasePricesRepository(ICouchCluster cluster) : 
  CouchView(cluster),
  ILastPurchasePricesRepository
{
  public async Task<IEnumerable<LastPurchasePrice>> GetAsync(string warehouseId, string[] stockIds)
  {
    return await this.GetRecordsAsync<LastPurchasePrice>("commerce", "last-purchase-prices", ((IEnumerable<string>) stockIds).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (stockId => new Tuple<object, object>((object) new string[2]
    {
      warehouseId,
      stockId
    }, (object) new string[2]{ warehouseId, stockId }))).ToArray<Tuple<object, object>>(), true, 2, inclusiveEnd: true);
  }
}
