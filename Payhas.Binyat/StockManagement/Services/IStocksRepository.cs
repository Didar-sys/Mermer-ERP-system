// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.StockManagement.Services.IStocksRepository
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.StockManagement.Models;
using Payhas.Data.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.StockManagement.Services;

public interface IStocksRepository : 
  IRepositoryWithFacets<Stock>,
  IRepository<Stock>,
  IReadOnlyRepository<Stock>
{
  Task<IEnumerable<Stock>> GetListAsync(params string[] stockIds);

  Task<IEnumerable<StockInfo>> GetInfoAsync(params string[] stockIds);

  Task<IEnumerable<StockInfo>> GetInfoAsync(
    string additionalPriceCurrencyId,
    string additionalPriceGroup);

  Task MergeAsync(string mainStockId, string[] mergeStockIds, bool disableMergedItems);
}
