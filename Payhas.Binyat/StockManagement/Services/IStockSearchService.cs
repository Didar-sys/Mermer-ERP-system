// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.StockManagement.Services.IStockSearchService
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.StockManagement.Services;

public interface IStockSearchService
{
  Task<IEnumerable<StockSearchResult>> Search(
    string text,
    string warehouseId,
    string priceGroup = null,
    string currencyId = null,
    CancellationToken cancellationToken = default (CancellationToken));
}
