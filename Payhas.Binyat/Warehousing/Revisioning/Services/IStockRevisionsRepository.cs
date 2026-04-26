// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Warehousing.Revisioning.Services.IStockRevisionsRepository
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.StockManagement.Models;
using Payhas.Binyat.Warehousing.Revisioning.Models;
using Payhas.Data.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Warehousing.Revisioning.Services;

public interface IStockRevisionsRepository : 
  IRepository<StockRevision>,
  IReadOnlyRepository<StockRevision>
{
  Task<StockRevisionLine> GetLineAsync(string stockRevisionLineId);

  Task<IEnumerable<StockRevisionLine>> GetLinesAsync(string revisionId, params string[] lineIds);

  Task<IEnumerable<StockRevisionLineInfo>> GetLineInfosAsync(
    string revisionId,
    params string[] lineIds);

  Task<IEnumerable<StockRevisionLineInfo>> CalcLineInfosAsync(
    StockRevision revision,
    IEnumerable<StockRevisionLine> lines,
    Func<string[], Task<IEnumerable<Stock>>> stocksGetter,
    Func<string[], Task<IEnumerable<StockBalance>>> stockBalancesGetter,
    Func<(string stockId, DateTime? balanceDate)[], Task<IEnumerable<StockBalance>>> stockBalancesGetterAlt,
    string priceDisplayCurrencyId = null);

  Task StoreLineAsync(StockRevisionLine line);

  Task StoreLinesAsync(string revisionId, IEnumerable<StockRevisionLine> list);

  Task DeleteLineAsync(string stockRevisionLineId);

  Task<StockRevisionCountInfo> GetCountInfoAsync(
    string revisionId,
    string stockId,
    Func<string, DateTime?> countDateGetter = null);

  Task<IEnumerable<StockRevisionCountInfoWithData>> GetCountInfosAsync(
    string revisionId,
    string priceDisplayCurrencyId = null);

  Task<IEnumerable<StockRevisionUncountedInfo>> GetUncountedAsync(string revisionId);
}
