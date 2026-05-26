// Decompiled with JetBrains decompiler
// Type: Mermer.StockManagement.Services.IStockBalancesRepository
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.StockManagement.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.StockManagement.Services;

public interface IStockBalancesRepository
{
  Task<IEnumerable<StockBalance>> GetAsync(
    string stockId,
    DateTime date,
    params string[] warehouses);

  Task<IEnumerable<StockBalance>> GetAsync(string warehouseId, string[] stockIds, DateTime? date = null);

  Task<IEnumerable<StockBalance>> GetAsync(
    string[] warehouseIds,
    string[] stockIds,
    DateTime? date = null);

  Task<IEnumerable<StockBalance>> GetAsync(
    string warehouseId,
    (string stockId, DateTime? balanceDate)[] stockBalanceDates);

  Task<IEnumerable<StockBalance>> GetAsync(
    string[] warehouseIds,
    (string stockId, DateTime? balanceDate)[] stockBalanceDates);

  Task<IEnumerable<StockBalanceWithCodeAndName>> GetAsync(
    string warehouseId,
    string[] stockIds,
    string excludedTransactionId);

  Task<IEnumerable<StockBalanceByTypeWithBalanceAndData>> GetByTypeAsync(
    string[] warehouseIds,
    string stockId,
    DateTime dateFrom,
    DateTime dateTill,
    bool aggregate);

  Task<IEnumerable<StockBalanceByWarehouses>> GetByDateAndWarehousesAsync(
    DateTime date,
    IEnumerable<string> warehouseIds,
    string displayCurrencyId,
    IEnumerable<string> stockIds = null);
}
