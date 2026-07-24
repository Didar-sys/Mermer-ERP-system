using Couchbase;
using Couchbase.Core;
using Couchbase.Views;
using FluentValidation;
using Mermer.Authorization.Services;
using Mermer.Commerce.Models;
using Mermer.Common.Services;
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Core.Couch.Common;
using Mermer.Data.Authorizers;
using Mermer.Data.Patcher;
using Mermer.Data.Storage;
using Mermer.FundsManagement.Models;
using Mermer.FundsManagement.Models.Extenders;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Models.Extenders;
using Mermer.StockManagement.Services;
using Mermer.Transactions.Models;
using Mermer.Warehousing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mermer.Core.Couch.StockManagement.Services;

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
        _currenciesRepository = currenciesRepository;
    }

    public override Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
    {
        return GetFacetsFromView("stock-management", "stock-facets", fields);
    }

    public async Task<IEnumerable<Stock>> GetListAsync(params string[] stockIds)
    {
        using (IBucket bucket = Cluster.OpenDefaultBucket())
        {
            var docs = await bucket.GetDocumentsAsync<Stock>(stockIds);
            return docs.Select(x => x.Content);
        }
    }

    public async Task<IEnumerable<StockInfo>> GetInfoAsync(params string[] stockIds)
    {
        using (IBucket bucket = Cluster.OpenDefaultBucket())
        {
            IViewQuery query = new ViewQuery().From("stock-management", "stock-info").Stale(StaleState.UpdateAfter);
            if (stockIds != null && stockIds.Any())
                query = query.Keys(stockIds);

            var viewResult = await bucket.QueryAsync<StockInfo>(query);
            return viewResult.Exception == null ? viewResult.Values : throw viewResult.Exception;
        }
    }

    public async Task<IEnumerable<StockInfo>> GetInfoAsync(string additionalPriceCurrencyId, string additionalPriceGroup)
    {
        if (string.IsNullOrEmpty(additionalPriceCurrencyId) && string.IsNullOrEmpty(additionalPriceGroup))
        {
            return await GetInfoAsync(Array.Empty<string>());
        }

        Dictionary<string, Currency> currencies = null;
        if (!string.IsNullOrEmpty(additionalPriceCurrencyId))
        {
            var currencyList = await _currenciesRepository.GetAsync();
            // Безопасно формируем словарь валют
            currencies = currencyList?.Where(x => x != null && x.Id != null).ToDictionary(x => x.Id, x => x)
                         ?? new Dictionary<string, Currency>();
        }

        var stocks = await GetAsync(Array.Empty<System.Linq.Expressions.Expression<Func<Stock, bool>>>());
        if (stocks == null) return Enumerable.Empty<StockInfo>();

        return stocks.Select(stock =>
        {
            if (stock == null) return null;

            // 1. Безопасно получаем цену. Если она null – ставим ноль по умолчанию
            var price = stock.GetPrice(null, additionalPriceGroup);
            if (price == null)
            {
                return new StockInfo
                {
                    Id = stock.Id,
                    Code = stock.Code,
                    Name = stock.Name,
                    ShortName = stock.ShortName,
                    Unit = stock.Unit,
                    Price = stock.Price,
                    CurrencyId = stock.CurrencyId,
                    AdditionalPrice = 0, // Нету ценны — выводим 0
                    AdditionalPriceCurrencyId = additionalPriceCurrencyId,
                    Type = stock.Type,
                    Group = stock.Group,
                    Tags = stock.Tags,
                    Barcodes = stock.Barcodes,
                    IsDisabled = stock.IsDisabled
                };
            }

            decimal num = price.Price;
            string str2 = price.CurrencyId;

            // 2. Безопасный перерасчет курсов валют
            if (!string.IsNullOrEmpty(additionalPriceCurrencyId) && price.CurrencyId != additionalPriceCurrencyId && currencies != null)
            {
                // Проверяем, существуют ли обе валюты в базе данных во избежание KeyNotFoundException
                if (price.CurrencyId != null &&
                    currencies.TryGetValue(price.CurrencyId, out var currency1) &&
                    currencies.TryGetValue(additionalPriceCurrencyId, out var currency2))
                {
                    var rate1 = currency1?.GetRate();
                    var rate2 = currency2?.GetRate();

                    // Защита от деления на ноль и null-рейтов
                    if (rate1 != null && rate2 != null && rate1.Divider != 0 && rate2.Multiplier != 0)
                    {
                        num = price.Price * rate1.Multiplier / rate1.Divider / rate2.Multiplier * rate2.Divider;
                        str2 = additionalPriceCurrencyId;
                    }
                }
            }

            return new StockInfo
            {
                Id = stock.Id,
                Code = stock.Code,
                Name = stock.Name,
                ShortName = stock.ShortName,
                Unit = stock.Unit,
                Price = stock.Price,
                CurrencyId = stock.CurrencyId,
                AdditionalPrice = num,
                AdditionalPriceCurrencyId = str2,
                Type = stock.Type,
                Group = stock.Group,
                Tags = stock.Tags,
                Barcodes = stock.Barcodes,
                IsDisabled = stock.IsDisabled
            };
        }).Where(x => x != null).ToList(); // .ToList() сразу материализует список в безопасном месте
    }

    public async Task MergeAsync(string mainStockId, string[] mergeStockIds, bool disableMergedItems)
    {
        using (IBucket bucket = Cluster.OpenDefaultBucket())
        {
            var strArray1 = new[] { "Invoice", "StockSlip", "StockTransfer" };
            var queryString = $"SELECT meta().id, `{Cluster.DefaultBucket}`.docType FROM `{Cluster.DefaultBucket}` WHERE meta().id == `{Cluster.DefaultBucket}`.id AND `{Cluster.DefaultBucket}`.docType IN ['{string.Join("', '", strArray1)}'] AND ANY i IN `{Cluster.DefaultBucket}`.lines SATISFIES i.stockId IN ['{string.Join("', '", mergeStockIds)}'] END";

            var transactions = (await bucket.QueryAsync<TransactionInfo>(queryString)).ToArray();
            var stock = await GetAsync(mainStockId);

            await UpdateUsages<Invoice, InvoiceLine>(bucket, transactions, mainStockId, mergeStockIds, stock);
            await UpdateUsages<StockSlip, StockSlipLine>(bucket, transactions, mainStockId, mergeStockIds, stock);
            await UpdateUsages<StockTransfer, StockTransferLine>(bucket, transactions, mainStockId, mergeStockIds, stock);

            if (!disableMergedItems) return;

            var barcodes = stock.Barcodes?.ToList() ?? new List<string>();

            foreach (var mergedItem in await GetListAsync(mergeStockIds))
            {
                mergedItem.IsDisabled = true;
                await UpdateAsync(mergedItem);
                barcodes.Add(mergedItem.Code);
                if (mergedItem.Barcodes != null && mergedItem.Barcodes.Any())
                    barcodes.AddRange(mergedItem.Barcodes);
            }

            stock.Barcodes = barcodes;
            await UpdateAsync(stock);
        }
    }

    private async Task UpdateUsages<T, TLine>(IBucket bucket, TransactionInfo[] transactionInfos, string mainStockId, string[] mergeStockIds, Stock stock = null)
        where T : StockTransaction<TLine>
        where TLine : StockTransactionLine
    {
        var docIds = transactionInfos.Where(x => x.DocType == typeof(T).Name).Select(x => x.Id);
        var transactionDocs = (await bucket.GetDocumentsAsync<T>(docIds)).Select(x => x.Document as IDocument<T>).ToList();

        foreach (var doc in transactionDocs)
        {
            var updated = await UpdateStockUsage<T, TLine>(doc.Content, mainStockId, mergeStockIds, stock);
            await LocalChangesRepositoryService.StorePatchesAsync(new[] { updated.patch }, bucket);
            doc.Content = updated.transaction;
        }

        await bucket.UpsertAsync(transactionDocs);
    }

    private async Task<(T transaction, CouchPatch patch)> UpdateStockUsage<T, TLine>(T transaction, string mainStockId, string[] mergeStockIds, Stock stock = null)
        where T : StockTransaction<TLine>
        where TLine : StockTransactionLine
    {
        var patch = new CouchPatch
        {
            Id = transaction.Id,
            Action = PatchAction.Update,
            SubListPatches = new Dictionary<string, List<Patch>>
            {
                { "Lines", new List<Patch>() },
                { "StockUnitConvertions", new List<Patch>() }
            },
            DocType = transaction.GetType().Name,
            Author = LoginService.Session.Username
        };

        if (stock == null)
            stock = await GetAsync(mainStockId);

        if (transaction.StockUnitConvertions.All(x => x.StockId != stock.Id || x.UnitId != stock.UnitId))
        {
            var source = new StockUnitConvertion
            {
                StockId = stock.Id,
                UnitId = stock.UnitId,
                Multiplier = 1M,
                Divider = 1M
            };
            patch.SubListPatches["StockUnitConvertions"].Add(Patcher.CreatePatch(source, null));
            transaction.StockUnitConvertions.Add(source);
        }

        foreach (var line in transaction.Lines)
        {
            if (mergeStockIds.Contains(line.StockId))
            {
                decimal actionQuantity = line.ActionQuantity;
                decimal num = line.Price;
                if (line.Quantity != actionQuantity)
                    num = line.Price * line.Quantity / actionQuantity;

                line.StockId = stock.Id;
                line.UnitId = stock.UnitId;
                line.Quantity = actionQuantity;
                line.Price = num;

                patch.SubListPatches["Lines"].Add(new Patch
                {
                    Id = line.Id,
                    Action = PatchAction.Update,
                    PropertyPatches = new Dictionary<string, object>
                    {
                        { "StockId", stock.Id },
                        { "UnitId", stock.UnitId },
                        { "Quantity", actionQuantity },
                        { "Price", num }
                    }
                });
            }
        }
        return (transaction, patch);
    }

    internal class TransactionInfo
    {
        public string Id { get; set; }
        public string DocType { get; set; }
    }
}