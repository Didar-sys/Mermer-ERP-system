// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.StockManagement.Services.IStockActionsRepository
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.StockManagement.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.StockManagement.Services;

public interface IStockActionsRepository
{
  Task<int> CountAsync(
    DateTime? startDate,
    DateTime? endDate,
    string stockId,
    params string[] warehouseIds);

  Task<IEnumerable<StockActionWithData>> GetAsync(
    DateTime? startDate,
    DateTime? endDate,
    string stockId,
    params string[] warehouseIds);

  Task<StockTracking> TrackByLineIdAsync(string lineId);

  Task<IEnumerable<StockTracking>> TrackByTransactionIdAsync(string transactionId);
}
