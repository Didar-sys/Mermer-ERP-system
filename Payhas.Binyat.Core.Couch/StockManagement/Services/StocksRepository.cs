// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Couch.StockManagement.Services.StocksRepository
// Assembly: Payhas.Binyat.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.Couch.dll

using Couchbase;
using Couchbase.Core;
using Couchbase.Views;
using FluentValidation;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Commerce.Models;
using Payhas.Binyat.Common.Services;
using Payhas.Binyat.Core.Couch.Changes;
using Payhas.Binyat.Core.Couch.Changes.Services;
using Payhas.Binyat.Core.Couch.Common;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.FundsManagement.Models.Extenders;
using Payhas.Binyat.StockManagement.Models;
using Payhas.Binyat.StockManagement.Models.Extenders;
using Payhas.Binyat.StockManagement.Services;
using Payhas.Binyat.Transactions.Models;
using Payhas.Binyat.Warehousing.Models;
using Payhas.Data.Authorizers;
using Payhas.Data.Patcher;
using Payhas.Data.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Core.Couch.StockManagement.Services;

public class StocksRepository : 
  CouchRepositoryWithFacet<Stock>,
  IStocksRepository,
  IRepositoryWithFacets<Stock>,
  IRepository<Stock>,
  IReadOnlyRepository<Stock>
{
  private readonly IReadOnlyRepository<Currency> _currenciesRepository;

  public StocksRepository(
    ICouchCluster cluster,
    IValidator<Stock> validator,
    IListAuthorizer<Stock> authorizer,
    IDocumentChangeListener changeListener,
    IReadOnlyRepository<Currency> currenciesRepository,
    ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService,
    IPatcher patcher,
    ILoginService loginService)
    : base(patcher, cluster, validator, loginService, authorizer, changeListener, localChangesRepositoryService)
  {
    this._currenciesRepository = currenciesRepository;
  }

  public override Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
  {
    return this.GetFacetsFromView("stock-management", "stock-facets", fields);
  }

  public async Task<IEnumerable<Stock>> GetListAsync(params string[] stockIds)
  {
    IEnumerable<Stock> listAsync;
    using (IBucket bucket = this.Cluster.OpenDefaultBucket())
      listAsync = ((IEnumerable<IDocumentResult<Stock>>) await bucket.GetDocumentsAsync<Stock>((IEnumerable<string>) stockIds)).Select<IDocumentResult<Stock>, Stock>((Func<IDocumentResult<Stock>, Stock>) (x => x.Content));
    return listAsync;
  }

  public async Task<IEnumerable<StockInfo>> GetInfoAsync(params string[] stockIds)
  {
    IEnumerable<StockInfo> infoAsync;
    using (IBucket bucket = this.Cluster.OpenDefaultBucket())
    {
      IViewQuery query = new ViewQuery().From("stock-management", "stock-info").Stale(StaleState.UpdateAfter);
      if (stockIds != null && ((IEnumerable<string>) stockIds).Any<string>())
        query = query.Keys((IEnumerable) stockIds);
      IViewResult<StockInfo> viewResult = await bucket.QueryAsync<StockInfo>((IViewQueryable) query);
      infoAsync = viewResult.Exception == null ? viewResult.Values : throw viewResult.Exception;
    }
    return infoAsync;
  }

  public async Task<IEnumerable<StockInfo>> GetInfoAsync(
    string additionalPriceCurrencyId,
    string additionalPriceGroup)
  {
    StocksRepository stocksRepository = this;
    if (string.IsNullOrEmpty(additionalPriceCurrencyId) && string.IsNullOrEmpty(additionalPriceGroup))
    {
      // ISSUE: explicit non-virtual call
      return await __nonvirtual (stocksRepository.GetInfoAsync(Array.Empty<string>()));
    }
    Dictionary<string, Currency> currencies = (Dictionary<string, Currency>) null;
    if (!string.IsNullOrEmpty(additionalPriceCurrencyId))
      currencies = (await stocksRepository._currenciesRepository.GetAsync()).ToDictionary<Currency, string, Currency>((Func<Currency, string>) (x => x.Id), (Func<Currency, Currency>) (x => x));
    return (await stocksRepository.GetAsync(Array.Empty<Expression<Func<Stock, bool>>>())).Select<Stock, StockInfo>((Func<Stock, StockInfo>) (x =>
    {
      Stock stock = x;
      string str1 = additionalPriceGroup;
      DateTime? date = new DateTime?();
      string priceGroup = str1;
      StockPrice price = stock.GetPrice(date, priceGroup);
      Decimal num = price.Price;
      string str2 = price.CurrencyId;
      if (!string.IsNullOrEmpty(additionalPriceCurrencyId) && price.CurrencyId != additionalPriceCurrencyId)
      {
        CurrencyRate rate1 = currencies[price.CurrencyId].GetRate();
        CurrencyRate rate2 = currencies[additionalPriceCurrencyId].GetRate();
        num = price.Price * rate1.Multiplier / rate1.Divider / rate2.Multiplier * rate2.Divider;
        str2 = additionalPriceCurrencyId;
      }
      return new StockInfo()
      {
        Id = x.Id,
        Code = x.Code,
        Name = x.Name,
        ShortName = x.ShortName,
        Unit = x.Unit,
        Price = x.Price,
        CurrencyId = x.CurrencyId,
        AdditionalPrice = num,
        AdditionalPriceCurrencyId = str2,
        Type = x.Type,
        Group = x.Group,
        Tags = x.Tags,
        Barcodes = x.Barcodes,
        IsDisabled = x.IsDisabled
      };
    }));
  }

  public async Task MergeAsync(string mainStockId, string[] mergeStockIds, bool disableMergedItems)
  {
    StocksRepository stocksRepository = this;
    using (IBucket bucket = stocksRepository.Cluster.OpenDefaultBucket())
    {
      string[] strArray1 = new string[3]
      {
        "Invoice",
        "StockSlip",
        "StockTransfer"
      };
      string[] strArray2 = new string[15]
      {
        "SELECT meta().id, `",
        stocksRepository.Cluster.DefaultBucket,
        "`.docType FROM `",
        stocksRepository.Cluster.DefaultBucket,
        "` WHERE meta().id == `",
        stocksRepository.Cluster.DefaultBucket,
        "`.id AND `",
        stocksRepository.Cluster.DefaultBucket,
        "`.docType IN ['",
        string.Join("', '", strArray1),
        "'] AND ANY i IN `",
        stocksRepository.Cluster.DefaultBucket,
        "`.lines SATISFIES i.stockId IN ['",
        string.Join("', '", mergeStockIds),
        "'] END"
      };
      StocksRepository.TransactionInfo[] transactions = (await bucket.QueryAsync<StocksRepository.TransactionInfo>(string.Concat(strArray2))).ToArray<StocksRepository.TransactionInfo>();
      Stock stock = await stocksRepository.GetAsync(mainStockId);
      await stocksRepository.UpdateUsages<Invoice, InvoiceLine>(bucket, transactions, mainStockId, mergeStockIds, stock);
      await stocksRepository.UpdateUsages<StockSlip, StockSlipLine>(bucket, transactions, mainStockId, mergeStockIds, stock);
      await stocksRepository.UpdateUsages<StockTransfer, StockTransferLine>(bucket, transactions, mainStockId, mergeStockIds, stock);
      if (!disableMergedItems)
        return;
      IEnumerable<string> barcodes1 = stock.Barcodes;
      List<string> barcodes = (barcodes1 != null ? barcodes1.ToList<string>() : (List<string>) null) ?? new List<string>();
      // ISSUE: explicit non-virtual call
      foreach (Stock mergedItem in await __nonvirtual (stocksRepository.GetListAsync(mergeStockIds)))
      {
        mergedItem.IsDisabled = true;
        await stocksRepository.UpdateAsync(mergedItem);
        barcodes.Add(mergedItem.Code);
        if (mergedItem.Barcodes != null && mergedItem.Barcodes.Any<string>())
          barcodes.AddRange(mergedItem.Barcodes);
      }
      stock.Barcodes = (IEnumerable<string>) barcodes;
      await stocksRepository.UpdateAsync(stock);
      transactions = (StocksRepository.TransactionInfo[]) null;
      stock = (Stock) null;
      barcodes = (List<string>) null;
    }
  }

  private async Task UpdateUsages<T, TLine>(
    IBucket bucket,
    StocksRepository.TransactionInfo[] transactionInfos,
    string mainStockId,
    string[] mergeStockIds,
    Stock stock = null)
    where T : StockTransaction<TLine>
    where TLine : StockTransactionLine
  {
    StocksRepository stocksRepository = this;
    List<IDocument<T>> transactionDocs = ((IEnumerable<IDocumentResult<T>>) await bucket.GetDocumentsAsync<T>(((IEnumerable<StocksRepository.TransactionInfo>) transactionInfos).Where<StocksRepository.TransactionInfo>((Func<StocksRepository.TransactionInfo, bool>) (x => x.DocType == typeof (T).Name)).Select<StocksRepository.TransactionInfo, string>((Func<StocksRepository.TransactionInfo, string>) (x => x.Id)))).Select<IDocumentResult<T>, IDocument<T>>((Func<IDocumentResult<T>, IDocument<T>>) (x => (IDocument<T>) x.Document)).ToList<IDocument<T>>();
    foreach (IDocument<T> doc in transactionDocs)
    {
      (T, CouchPatch) updated = await stocksRepository.UpdateStockUsage<T, TLine>(doc.Content, mainStockId, mergeStockIds, stock);
      await stocksRepository.LocalChangesRepositoryService.StorePatchesAsync((IEnumerable<CouchPatch>) new CouchPatch[1]
      {
        updated.Item2
      }, bucket);
      doc.Content = updated.Item1;
      updated = ();
    }
    IDocumentResult<T>[] documentResultArray = await bucket.UpsertAsync<T>(transactionDocs);
    transactionDocs = (List<IDocument<T>>) null;
  }

  private async Task<(T transaction, CouchPatch patch)> UpdateStockUsage<T, TLine>(
    T transaction,
    string mainStockId,
    string[] mergeStockIds,
    Stock stock = null)
    where T : StockTransaction<TLine>
    where TLine : StockTransactionLine
  {
    StocksRepository stocksRepository = this;
    CouchPatch couchPatch = new CouchPatch();
    couchPatch.Id = ((T) transaction).Id;
    couchPatch.Action = PatchAction.Update;
    couchPatch.SubListPatches = new Dictionary<string, List<Patch>>()
    {
      {
        "Lines",
        new List<Patch>()
      },
      {
        "StockUnitConvertions",
        new List<Patch>()
      }
    };
    couchPatch.DocType = ((T) transaction).GetType().Name;
    couchPatch.Author = stocksRepository.LoginService.Session.Username;
    CouchPatch patch = couchPatch;
    Stock stock1 = stock;
    if (stock1 == null)
      stock1 = await stocksRepository.GetAsync(mainStockId);
    stock = stock1;
    if (((T) transaction).StockUnitConvertions.All<StockUnitConvertion>((Func<StockUnitConvertion, bool>) (x => x.StockId != stock.Id || x.UnitId != stock.UnitId)))
    {
      StockUnitConvertion source = new StockUnitConvertion()
      {
        StockId = stock.Id,
        UnitId = stock.UnitId,
        Multiplier = 1M,
        Divider = 1M
      };
      patch.SubListPatches["StockUnitConvertions"].Add(stocksRepository.Patcher.CreatePatch<StockUnitConvertion>(source, (StockUnitConvertion) null));
      ((T) transaction).StockUnitConvertions.Add(source);
    }
    foreach (TLine line in (Collection<TLine>) ((T) transaction).Lines)
    {
      if (((IEnumerable<string>) mergeStockIds).Contains<string>(line.StockId))
      {
        Decimal actionQuantity = line.ActionQuantity;
        Decimal num = line.Price;
        if (line.Quantity != actionQuantity)
          num = line.Price * line.Quantity / actionQuantity;
        line.StockId = stock.Id;
        line.UnitId = stock.UnitId;
        line.Quantity = actionQuantity;
        line.Price = num;
        patch.SubListPatches["Lines"].Add(new Patch()
        {
          Id = line.Id,
          Action = PatchAction.Update,
          PropertyPatches = new Dictionary<string, object>()
          {
            {
              "StockId",
              (object) stock.Id
            },
            {
              "UnitId",
              (object) stock.UnitId
            },
            {
              "Quantity",
              (object) actionQuantity
            },
            {
              "Price",
              (object) num
            }
          }
        });
      }
    }
    (T, CouchPatch) valueTuple = (transaction, patch);
    patch = (CouchPatch) null;
    return valueTuple;
  }

  internal class TransactionInfo
  {
    public string Id { get; set; }

    public string DocType { get; set; }
  }
}
