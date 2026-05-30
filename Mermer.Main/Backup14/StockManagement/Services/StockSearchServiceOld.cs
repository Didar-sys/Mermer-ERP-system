// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.StockManagement.Services.StockSearchServiceOld
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

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
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

#nullable disable
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
    this._cluster = cluster;
    this._balancesRepository = balancesRepository;
    this._transliterationService = transliterationService;
  }

  public async Task<IEnumerable<StockSearchResult>> Search(
    string text,
    string warehouseId,
    string priceGroup,
    string currencyId = null,
    CancellationToken cancellationToken = default (CancellationToken))
  {
    text = text.ToLower();
    text = text.Replace("\\", "\\\\").Replace("+", "\\+").Replace("-", "\\-").Replace("=", "\\=").Replace("&", "\\&").Replace("|", "\\|").Replace("<", "\\<").Replace(">", "\\>").Replace("!", "\\!").Replace("(", "\\(").Replace(")", "\\)").Replace("{", "\\{").Replace("}", "\\}").Replace("[", "\\[").Replace("]", "\\]").Replace("^", "\\^").Replace("\"", "\\\"").Replace("~", "\\~").Replace("*", "\\*").Replace("?", "\\?").Replace(":", "\\:").Replace("/", "\\/");
    string[] terms = (await this._transliterationService.Parse(text)).ToArray<string>();
    if (!((IEnumerable<string>) terms).Any<string>())
      return (IEnumerable<StockSearchResult>) Array.Empty<StockSearchResult>();
    this.CheckIfCanceled(cancellationToken);
    using (IBucket bucket = this._cluster.OpenDefaultBucket())
    {
      ISearchQueryResult searchQueryResult = await bucket.QueryAsync(new SearchQuery()
      {
        Index = "stock-search",
        Query = (IFtsQuery) new DisjunctionQuery(new FtsQueryBase[7]
        {
          (FtsQueryBase) new QueryStringQuery($"Code:{text}^10"),
          (FtsQueryBase) new QueryStringQuery($"Barcodes:{text}^10"),
          (FtsQueryBase) new QueryStringQuery(this.GetSearchQueryText("Code", terms, 3.0)),
          (FtsQueryBase) new QueryStringQuery(this.GetSearchQueryText("Name", terms, 4.0)),
          (FtsQueryBase) new QueryStringQuery(this.GetSearchQueryText("Name", terms, 5.0, WildCharPosition.End)),
          (FtsQueryBase) new QueryStringQuery(this.GetSearchQueryText("ShortName", terms, 6.0)),
          (FtsQueryBase) new QueryStringQuery(this.GetSearchQueryText("Tags", terms, 6.0))
        }),
        SearchParams = new SearchParams().Limit(32 /*0x20*/)
      });
      if (!searchQueryResult.Success)
        throw searchQueryResult.Exception ?? new Exception(searchQueryResult.Message);
      this.CheckIfCanceled(cancellationToken);
      string[] stockIds = searchQueryResult.Hits.OrderByDescending<ISearchQueryRow, double>((Func<ISearchQueryRow, double>) (x => x.Score)).Select<ISearchQueryRow, string>((Func<ISearchQueryRow, string>) (x => x.Id)).ToArray<string>();
      if (!((IEnumerable<string>) stockIds).Any<string>())
        return (IEnumerable<StockSearchResult>) Array.Empty<StockSearchResult>();
      BucketContext bucketContext = new BucketContext(bucket);
      ParameterExpression parameterExpression1;
      ParameterExpression parameterExpression2;
      // ISSUE: method reference
      // ISSUE: method reference
      // ISSUE: method reference
      // ISSUE: method reference
      // ISSUE: method reference
      // ISSUE: method reference
      // ISSUE: method reference
      // ISSUE: method reference
      // ISSUE: method reference
      // ISSUE: method reference
      List<StockSearchResult> result = (await bucketContext.Query<Stock>().UseKeys<Stock>((IEnumerable<string>) stockIds).Join<Stock, Currency, string, StockSearchResult>((IEnumerable<Currency>) bucketContext.Query<Currency>().Where<Currency>((Expression<Func<Currency, bool>>) (x => x.DocType == "Currency")), (Expression<Func<Stock, string>>) (x => x.CurrencyId), (Expression<Func<Currency, string>>) (x => N1QlFunctions.Key(x)), Expression.Lambda<Func<Stock, Currency, StockSearchResult>>((Expression) Expression.MemberInit(Expression.New(typeof (StockSearchResult)), (MemberBinding) Expression.Bind((MethodInfo) MethodBase.GetMethodFromHandle(__methodref (StockSearchResult.set_Id)), ))))); // Unable to render the statement
      this.CheckIfCanceled(cancellationToken);
      StockBalance[] source;
      try
      {
        if (string.IsNullOrEmpty(warehouseId))
          throw new Exception();
        source = (await this._balancesRepository.GetAsync(warehouseId, stockIds)).ToArray<StockBalance>();
      }
      catch (Exception ex)
      {
        source = Array.Empty<StockBalance>();
      }
      this.CheckIfCanceled(cancellationToken);
      foreach (StockSearchResult stockSearchResult in result)
      {
        StockSearchResult item = stockSearchResult;
        item.CodeHtml = this.FormatText(item.Code, terms);
        item.NameHtml = this.FormatText(item.Name, terms);
        item.Balance = ((IEnumerable<StockBalance>) source).Where<StockBalance>((Func<StockBalance, bool>) (x => x.StockId == item.Id)).Sum<StockBalance>((Func<StockBalance, Decimal>) (x => x.Balance));
      }
      return (IEnumerable<StockSearchResult>) result;
    }
  }

  private string GetSearchQueryText(
    string field,
    string[] terms,
    double boost = 1.0,
    WildCharPosition wildCharPosition = WildCharPosition.Both)
  {
    string str1 = wildCharPosition.HasFlag((Enum) WildCharPosition.Start) ? "*" : "";
    string str2 = wildCharPosition.HasFlag((Enum) WildCharPosition.End) ? "*" : "";
    string separator = $"{str2}^{boost} {field}:{str1}";
    return $"{field}:{str1}{string.Join(separator, terms)}{str2}^{boost}";
  }

  private string FormatText(string text, string[] searchWords)
  {
    if (((IEnumerable<string>) searchWords).Contains<string>("b") || ((IEnumerable<string>) searchWords).Contains<string>("B"))
      text = text.ToUpper().Replace("B", "<B>B</B>");
    foreach (string str in ((IEnumerable<string>) searchWords).Where<string>((Func<string, bool>) (w => w != "b" && w != "B")))
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
