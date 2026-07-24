// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Reporting.Services.RevenueReportsRepository
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Mermer.Reporting.Models;
using Mermer.Reporting.Services;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Reporting.Services;

public class RevenueReportsRepository : IRevenueReportsRepository
{
  private readonly IStockActionsRepository _stockActionsRepository;

  public RevenueReportsRepository(IStockActionsRepository stockActionsRepository)
  {
    this._stockActionsRepository = stockActionsRepository;
  }

  public async Task<IEnumerable<RevenueReport>> GetAsync(
    string[] warehouseIds,
    DateTime dateFrom,
    DateTime dateTill)
  {
    IEnumerable<StockActionWithData> async1 = await this._stockActionsRepository.GetAsync(new DateTime?(dateFrom), new DateTime?(dateTill), (string) null, warehouseIds);
    List<RevenueReport> revenues = new List<RevenueReport>();
    foreach (StockActionWithData x in async1.Where<StockActionWithData>((Func<StockActionWithData, bool>) (x =>
    {
      if (!x.TransactionIsCompleted || x.TransactionIsDisabled)
        return false;
      return x.TransactionType == "Sales" || x.TransactionType == "SalesReturn";
    })))
    {
      (Decimal, Decimal) costsAsync = await this.GetCostsAsync((StockAction) x);
      revenues.Add(new RevenueReport()
      {
        Date = x.TransactionDate,
        WarehouseId = x.ActionWarehouseId,
        StockId = x.ActionStockId,
        StockCode = x.StockCode,
        StockName = x.StockName,
        StockType = x.StockType,
        StockGroup = x.StockGroup,
        StockTags = x.StockTags,
        Quantity = -x.ActionEffect,
        InitialCosts = costsAsync.Item1,
        OverheadsCosts = x.ActionOverhead + costsAsync.Item2,
        RecommendedPrice = x.RecommendedPrice,
        ActualPrice = x.ActionPrice
      });
    }
    IEnumerable<RevenueReport> async2 = (IEnumerable<RevenueReport>) revenues;
    revenues = (List<RevenueReport>) null;
    return async2;
  }

  private Task<(Decimal initial, Decimal overheads)> GetCostsAsync(
    StockAction action,
    Decimal? quantity = null)
  {
    return this.GetCostsAsync(action.TransactionDate, action.ActionWarehouseId, action.ActionStockId, quantity ?? -action.ActionEffect, action.ActionSourceId);
  }

  private async Task<(Decimal initial, Decimal overheads)> GetCostsAsync(
    DateTime date,
    string warehouseId,
    string stockId,
    Decimal quantity,
    string sourceId = null)
  {
    Decimal initial = 0M;
    Decimal overheads = 0M;
    StockActionWithData[] array1 = (await this._stockActionsRepository.GetAsync(new DateTime?(DateTime.MinValue), new DateTime?(date.AddDays(1.0)), stockId, warehouseId)).Where<StockActionWithData>((Func<StockActionWithData, bool>) (x => x.TransactionDate < date && x.TransactionIsCompleted && !x.TransactionIsDisabled)).OrderByDescending<StockActionWithData, DateTime>((Func<StockActionWithData, DateTime>) (x => x.TransactionDate)).ToArray<StockActionWithData>();
    if (!string.IsNullOrEmpty(sourceId))
    {
      StockActionWithData stockActionWithData = ((IEnumerable<StockActionWithData>) array1).SingleOrDefault<StockActionWithData>((Func<StockActionWithData, bool>) (x => x.ActionId == sourceId));
      if (stockActionWithData != null)
      {
        Decimal num = stockActionWithData.ActionIncome > quantity ? quantity : stockActionWithData.ActionIncome;
        initial += num * stockActionWithData.ActionPrice;
        overheads += stockActionWithData.ActionOverhead * (num / stockActionWithData.ActionIncome);
        quantity -= num;
      }
    }
    Decimal balance;
    if (quantity > 0M)
    {
      balance = ((IEnumerable<StockActionWithData>) array1).Where<StockActionWithData>((Func<StockActionWithData, bool>) (x => x.ActionId != sourceId)).Sum<StockActionWithData>((Func<StockActionWithData, Decimal>) (x => x.ActionEffect));
      if (balance >= quantity)
      {
        foreach (StockActionWithData stockActionWithData in ((IEnumerable<StockActionWithData>) array1).Where<StockActionWithData>((Func<StockActionWithData, bool>) (x => x.ActionEffect > 0M && x.ActionId != sourceId)))
        {
          StockActionWithData item = stockActionWithData;
          balance -= item.ActionEffect;
          if (!(balance > quantity))
          {
            Decimal usage = quantity;
            if (balance > 0M)
              usage -= balance;
            if (item.TransactionType == "StockTransferDestination")
            {
              (Decimal, Decimal) costsAsync = await this.GetCostsAsync(item.TransactionDate, item.ActionRelatedWarehouseId, stockId, usage);
              initial += costsAsync.Item1;
              overheads += costsAsync.Item2;
            }
            else if (item.TransactionType == "SalesReturn")
            {
                            // Передаем отрицательный usage, чтобы функция искала прошлые продажи (Sales), а не закупки.
                            // Поскольку функция вернет отрицательную себестоимость, мы используем "-=", чтобы добавить ее в нашу.
                            (Decimal, Decimal) costsAsync = await this.GetCostsAsync((StockAction) item, new Decimal?(-usage));
              initial -= costsAsync.Item1;
              overheads -= costsAsync.Item2;
            }
            else
              initial += usage * item.ActionPrice;
            overheads += item.ActionOverhead * (usage / item.ActionIncome);
            quantity -= usage;
            if (!(quantity == 0M))
              item = (StockActionWithData) null;
            else
              break;
          }
        }
      }
    }
    else
    {
      quantity = -quantity;
      StockActionWithData[] array2 = ((IEnumerable<StockActionWithData>) array1).Where<StockActionWithData>((Func<StockActionWithData, bool>) (x => x.TransactionType == "Sales")).ToArray<StockActionWithData>();
      if (((IEnumerable<StockActionWithData>) array2).Sum<StockActionWithData>((Func<StockActionWithData, Decimal>) (x => x.ActionExpense)) >= quantity)
      {
        StockActionWithData[] stockActionWithDataArray = array2;
        for (int index = 0; index < stockActionWithDataArray.Length; ++index)
        {
          StockActionWithData action = stockActionWithDataArray[index];
          balance = action.ActionExpense < quantity ? action.ActionExpense : quantity;
          (Decimal num1, Decimal num2) = await this.GetCostsAsync((StockAction) action, new Decimal?(balance));
          initial -= num1;
          overheads -= num2;
          quantity -= balance;
          if (quantity == 0M)
            break;
        }
        stockActionWithDataArray = (StockActionWithData[]) null;
      }
    }
    return (initial, overheads);
  }
}
