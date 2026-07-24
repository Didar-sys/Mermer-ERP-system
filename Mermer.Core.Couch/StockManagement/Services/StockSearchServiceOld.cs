using Couchbase.Core;
using Couchbase.Linq;
using Couchbase.Linq.Extensions;
using Couchbase.Search;
using Couchbase.Search.Queries;
using Couchbase.Search.Queries.Compound;
using Couchbase.Search.Queries.Simple;
using Mermer.Common.Services;
using Mermer.Core.Couch.Common;
using Mermer.FundsManagement.Models;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mermer.Core.Couch.StockManagement.Services;

public class StockSearchServiceOld : IStockSearchService
{
    private readonly ICouchCluster _cluster;
    private readonly IStockBalancesRepository _balancesRepository;
    private readonly ITransliterationService _transliterationService;

    public StockSearchServiceOld(
        ICouchCluster cluster,
        IStockBalancesRepository balancesRepository,
        ITransliterationService transliterationService)
    {
        _cluster = cluster;
        _balancesRepository = balancesRepository;
        _transliterationService = transliterationService;
    }

    public async Task<IEnumerable<StockSearchResult>> Search(string text, string warehouseId, string priceGroup, string currencyId = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        text = text.ToLower();
        text = text.Replace("\\", "\\\\").Replace("+", "\\+").Replace("-", "\\-").Replace("=", "\\=").Replace("&", "\\&").Replace("|", "\\|").Replace("<", "\\<").Replace(">", "\\>").Replace("!", "\\!").Replace("(", "\\(").Replace(")", "\\)").Replace("{", "\\{").Replace("}", "\\}").Replace("[", "\\[").Replace("]", "\\]").Replace("^", "\\^").Replace("\"", "\\\"").Replace("~", "\\~").Replace("*", "\\*").Replace("?", "\\?").Replace(":", "\\:").Replace("/", "\\/");

        var terms = (await _transliterationService.Parse(text)).ToArray();
        if (!terms.Any()) return Array.Empty<StockSearchResult>();

        CheckIfCanceled(cancellationToken);

        using (IBucket bucket = _cluster.OpenDefaultBucket())
        {
            var searchQueryResult = await bucket.QueryAsync(new SearchQuery
            {
                Index = "stock-search",
                Query = new DisjunctionQuery(
                    new QueryStringQuery($"Code:{text}^10"),
                    new QueryStringQuery($"Barcodes:{text}^10"),
                    new QueryStringQuery(GetSearchQueryText("Code", terms, 3.0)),
                    new QueryStringQuery(GetSearchQueryText("Name", terms, 4.0)),
                    new QueryStringQuery(GetSearchQueryText("Name", terms, 5.0, WildCharPosition.End)),
                    new QueryStringQuery(GetSearchQueryText("ShortName", terms, 6.0)),
                    new QueryStringQuery(GetSearchQueryText("Tags", terms, 6.0))
                ),
                SearchParams = new SearchParams().Limit(32)
            });

            if (!searchQueryResult.Success)
                throw searchQueryResult.Exception ?? new Exception(searchQueryResult.Message);

            CheckIfCanceled(cancellationToken);

            var stockIds = searchQueryResult.Hits.OrderByDescending(x => x.Score).Select(x => x.Id).ToArray();
            if (!stockIds.Any()) return Array.Empty<StockSearchResult>();

            var bucketContext = new BucketContext(bucket);

            var result = (await Task.Run(() => bucketContext.Query<Stock>()
                .UseKeys(stockIds)
                .Join(
                    bucketContext.Query<Currency>().Where(x => x.DocType == "Currency"),
                    s => s.CurrencyId,
                    c => N1QlFunctions.Key(c),
                    (s, c) => new StockSearchResult
                    {
                        Id = s.Id,
                        Code = s.Code,
                        Name = s.Name,
                        Unit = s.Unit,
                        Price = s.Price,
                        CurrencyId = s.CurrencyId
                        
                    }
                ).ToList())).ToList();

            CheckIfCanceled(cancellationToken);

            StockBalance[] source;
            try
            {
                if (string.IsNullOrEmpty(warehouseId)) throw new Exception();
                source = (await _balancesRepository.GetAsync(warehouseId, stockIds)).ToArray();
            }
            catch
            {
                source = Array.Empty<StockBalance>();
            }

            CheckIfCanceled(cancellationToken);

            foreach (var item in result)
            {
                item.CodeHtml = FormatText(item.Code, terms);
                item.NameHtml = FormatText(item.Name, terms);
                item.Balance = source.Where(x => x.StockId == item.Id).Sum(x => x.Balance);
            }

            return result;
        }
    }

    private string GetSearchQueryText(string field, string[] terms, double boost = 1.0, WildCharPosition wildCharPosition = WildCharPosition.Both)
    {
        string str1 = wildCharPosition.HasFlag(WildCharPosition.Start) ? "*" : "";
        string str2 = wildCharPosition.HasFlag(WildCharPosition.End) ? "*" : "";
        string separator = $"{str2}^{boost} {field}:{str1}";
        return $"{field}:{str1}{string.Join(separator, terms)}{str2}^{boost}";
    }

    private string FormatText(string text, string[] searchWords)
    {
        if (searchWords.Contains("b") || searchWords.Contains("B"))
            text = text.ToUpper().Replace("B", "<B>B</B>");

        foreach (var str in searchWords.Where(w => w != "b" && w != "B"))
        {
            if (text.ToUpper().Contains(str.ToUpper()))
                text = text.ToUpper().Replace(str.ToUpper(), $"<B>{str.ToUpper()}</B>");
        }

        text = text.ToUpper().Replace("<B>", "<B style=\"background-color:yellow;color:black\">");
        return text;
    }

    private void CheckIfCanceled(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new TaskCanceledException();
    }
}