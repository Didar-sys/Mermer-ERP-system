// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Couch.Warehousing.Revisioning.Services.StockRevisionsRepository
// Assembly: Payhas.Binyat.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.Couch.dll

using Couchbase;
using Couchbase.Core;
using Couchbase.Linq;
using Couchbase.Linq.Extensions;
using Couchbase.N1QL;
using FluentValidation;
using Payhas.Binyat.Authorization.Enums;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Common.Services;
using Payhas.Binyat.Common.Settings;
using Payhas.Binyat.Core.Couch.Changes;
using Payhas.Binyat.Core.Couch.Changes.Services;
using Payhas.Binyat.Core.Couch.Common;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.FundsManagement.Models.Extenders;
using Payhas.Binyat.StockManagement.Models;
using Payhas.Binyat.StockManagement.Models.Extenders;
using Payhas.Binyat.StockManagement.Services;
using Payhas.Binyat.Warehousing.Revisioning.Models;
using Payhas.Binyat.Warehousing.Revisioning.Services;
using Payhas.Data.Authorizers;
using Payhas.Data.Patcher;
using Payhas.Data.Storage;
using Payhas.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Core.Couch.Warehousing.Revisioning.Services;

public class StockRevisionsRepository : 
  CouchRepositoryWithFacet<StockRevision>,
  IStockRevisionsRepository,
  IRepository<StockRevision>,
  IReadOnlyRepository<StockRevision>
{
  private readonly AppSettings _settings;
  private readonly IConfigurator _configurator;
  private readonly IValidator<StockRevisionLine> _lineValidator;
  private readonly IStockBalancesRepository _balancesRepository;
  private readonly IReadOnlyRepository<Currency> _currenciesRepository;

  public StockRevisionsRepository(
    IPatcher patcher,
    ICouchCluster cluster,
    ILoginService loginService,
    IConfigurator configurator,
    IValidator<StockRevision> validator,
    IValidator<StockRevisionLine> lineValidator,
    IListAuthorizer<StockRevision> authorizer,
    IStockBalancesRepository balancesRepository,
    IReadOnlyRepository<Currency> currenciesRepository,
    IDocumentChangeListener changeListener,
    ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService)
    : base(patcher, cluster, validator, loginService, authorizer, changeListener, localChangesRepositoryService)
  {
    this._configurator = configurator;
    this._lineValidator = lineValidator;
    this._balancesRepository = balancesRepository;
    this._currenciesRepository = currenciesRepository;
    this._settings = this._configurator.GetConfig<AppSettings>();
  }

  public override Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
  {
    return this.GetFacetsFromView("transaction", "facets", fields);
  }

  public async Task<StockRevisionLine> GetLineAsync(string stockRevisionLineId)
  {
    StockRevisionsRepository revisionsRepository = this;
    StockRevisionLine content;
    using (IBucket bucket = revisionsRepository.Cluster.OpenDefaultBucket())
    {
      IDocumentResult<StockRevisionLine> line = await bucket.GetDocumentAsync<StockRevisionLine>(stockRevisionLineId);
      IDocumentResult<StockRevision> documentAsync = await bucket.GetDocumentAsync<StockRevision>(line.Content.StockRevisionId);
      revisionsRepository.Authorizer.AuthorizeRead(documentAsync.Content);
      content = line.Content;
    }
    return content;
  }

  public async Task<IEnumerable<StockRevisionLine>> GetLinesAsync(
    string revisionId,
    params string[] lineIds)
  {
    StockRevisionsRepository revisionsRepository = this;
    IEnumerable<StockRevisionLine> linesAsync;
    using (IBucket bucket = revisionsRepository.Cluster.OpenDefaultBucket())
    {
      BucketContext context = new BucketContext(bucket);
      linesAsync = await revisionsRepository.GetLinesAsync(revisionId, lineIds, (IBucketContext) context);
    }
    return linesAsync;
  }

  private Task<IEnumerable<StockRevisionLine>> GetLinesAsync(
    string revisionId,
    string[] lineIds,
    IBucketContext context)
  {
    IQueryable<StockRevisionLine> queryable = context.Query<StockRevisionLine>().Where<StockRevisionLine>((Expression<Func<StockRevisionLine, bool>>) (x => x.DocType == "StockRevisionLine" && x.StockRevisionId == revisionId && x.Id == N1QlFunctions.Key(x)));
    if (((IEnumerable<string>) lineIds).Any<string>())
      queryable.UseKeys<StockRevisionLine>((IEnumerable<string>) lineIds);
    return queryable.ExecuteAsync<StockRevisionLine>();
  }

  public async Task<IEnumerable<StockRevisionLineInfo>> GetLineInfosAsync(
    string revisionId,
    params string[] lineIds)
  {
    StockRevisionsRepository revisionsRepository = this;
    IEnumerable<StockRevisionLineInfo> lineInfosAsync;
    using (IBucket bucket = revisionsRepository.Cluster.OpenDefaultBucket())
    {
      BucketContext context = new BucketContext(bucket);
      StockRevision revision = (await bucket.GetDocumentAsync<StockRevision>(revisionId)).Content;
      revisionsRepository.Authorizer.AuthorizeRead(revision);
      IEnumerable<StockRevisionLine> linesAsync = await revisionsRepository.GetLinesAsync(revisionId, lineIds, (IBucketContext) context);
      // ISSUE: explicit non-virtual call
      lineInfosAsync = await __nonvirtual (revisionsRepository.CalcLineInfosAsync(revision, linesAsync, (Func<string[], Task<IEnumerable<Stock>>>) (stockIds => context.Query<Stock>().Where<Stock>((Expression<Func<Stock, bool>>) (x => x.DocType == "Stock")).UseKeys<Stock>((IEnumerable<string>) stockIds).ExecuteAsync<Stock>()), (Func<string[], Task<IEnumerable<StockBalance>>>) (stockIds => this._balancesRepository.GetAsync(revision.WarehouseId, stockIds, revision.FinishDate)), (Func<(string, DateTime?)[], Task<IEnumerable<StockBalance>>>) (stockBalanceDates => this._balancesRepository.GetAsync(revision.WarehouseId, stockBalanceDates)), (string) null));
    }
    return lineInfosAsync;
  }

  public async Task<IEnumerable<StockRevisionLineInfo>> CalcLineInfosAsync(
    StockRevision revision,
    IEnumerable<StockRevisionLine> lines,
    Func<string[], Task<IEnumerable<Stock>>> stocksGetter,
    Func<string[], Task<IEnumerable<StockBalance>>> stockBalancesGetter,
    Func<(string stockId, DateTime? balanceDate)[], Task<IEnumerable<StockBalance>>> stockBalancesGetterAlt,
    string priceDisplayCurrencyId = null)
  {
    if (!(lines is StockRevisionLine[] stockRevisionLineArray))
      stockRevisionLineArray = lines.ToArray<StockRevisionLine>();
    StockRevisionLine[] revisionLines = stockRevisionLineArray;
    if (!((IEnumerable<StockRevisionLine>) revisionLines).Any<StockRevisionLine>())
      return (IEnumerable<StockRevisionLineInfo>) Array.Empty<StockRevisionLineInfo>();
    Dictionary<string, IEnumerable<\u003C\u003Ef__AnonymousType16<string, string, Decimal>>> countsByStocks = ((IEnumerable<StockRevisionLine>) revisionLines).GroupBy<StockRevisionLine, string>((Func<StockRevisionLine, string>) (x => x.StockId)).ToDictionary<IGrouping<string, StockRevisionLine>, string, IEnumerable<\u003C\u003Ef__AnonymousType16<string, string, Decimal>>>((Func<IGrouping<string, StockRevisionLine>, string>) (g => g.Key), g => g.Select(x => new
    {
      Id = x.Id,
      UnitId = x.UnitId,
      Quantity = x.Quantity
    }));
    string[] stockIds = ((IEnumerable<StockRevisionLine>) revisionLines).Select<StockRevisionLine, string>((Func<StockRevisionLine, string>) (x => x.StockId)).Distinct<string>().ToArray<string>();
    Dictionary<string, Stock> stocks = (await stocksGetter(stockIds)).ToDictionary<Stock, string, Stock>((Func<Stock, string>) (x => x.Id), (Func<Stock, Stock>) (x => x));
    IEnumerable<StockBalance> source;
    if (!this._settings.FreezeStockBlanaceOnRevision)
      source = await stockBalancesGetter(stockIds);
    else
      source = await stockBalancesGetterAlt(lines.GroupBy<StockRevisionLine, string>((Func<StockRevisionLine, string>) (x => x.StockId)).Select<IGrouping<string, StockRevisionLine>, (string, DateTime?)>((Func<IGrouping<string, StockRevisionLine>, (string, DateTime?)>) (g => (g.Key, new DateTime?(g.Min<StockRevisionLine, DateTime>((Func<StockRevisionLine, DateTime>) (x => x.Date)))))).ToArray<(string, DateTime?)>());
    Dictionary<string, Decimal> balancesSummed = source.GroupBy<StockBalance, string>((Func<StockBalance, string>) (x => x.StockId)).ToDictionary<IGrouping<string, StockBalance>, string, Decimal>((Func<IGrouping<string, StockBalance>, string>) (g => g.Key), (Func<IGrouping<string, StockBalance>, Decimal>) (g => Math.Round(g.Sum<StockBalance>((Func<StockBalance, Decimal>) (i => i.Balance)), 2)));
    Dictionary<string, Currency> currencies = (await this._currenciesRepository.GetAsync()).ToDictionary<Currency, string, Currency>((Func<Currency, string>) (x => x.Id), (Func<Currency, Currency>) (x => x));
    return ((IEnumerable<StockRevisionLine>) revisionLines).Select<StockRevisionLine, StockRevisionLineInfo>((Func<StockRevisionLine, StockRevisionLineInfo>) (x =>
    {
      Stock stock = stocks[x.StockId];
      Decimal d;
      string key;
      if (this._settings.AllowStockPriceChangeOnRevision)
      {
        Decimal? price = x.Price;
        if (price.HasValue && !string.IsNullOrEmpty(x.CurrencyId))
        {
          price = x.Price;
          d = price.Value;
          key = x.CurrencyId;
          goto label_4;
        }
      }
      StockPrice price1 = stock.GetPrice(revision.FinishDate);
      d = price1.Price;
      key = price1.CurrencyId;
label_4:
      if (!string.IsNullOrEmpty(priceDisplayCurrencyId) && key != priceDisplayCurrencyId)
      {
        CurrencyRate rate1 = currencies[key].GetRate(revision.FinishDate);
        CurrencyRate rate2 = currencies[priceDisplayCurrencyId].GetRate(revision.FinishDate);
        d = d * rate1.Multiplier / rate1.Divider / rate2.Multiplier * rate2.Divider;
        key = priceDisplayCurrencyId;
      }
      Decimal num1 = Math.Round(d, currencies[key].Decimals);
      List<\u003C\u003Ef__AnonymousType17<string, string, string, Decimal>> list = countsByStocks[x.StockId].Select(i =>
      {
        StockUnit stockUnit3 = stock.Units.SingleOrDefault<StockUnit>((Func<StockUnit, bool>) (j => j.Id == i.UnitId));
        if (stockUnit3 == null)
          stockUnit3 = new StockUnit()
          {
            Multiplier = 0M,
            Divider = 1M
          };
        StockUnit stockUnit4 = stockUnit3;
        return new
        {
          Id = i.Id,
          UnitId = i.UnitId,
          UnitName = stockUnit4.Name,
          Total = Math.Round(i.Quantity * stockUnit4.Multiplier / stockUnit4.Divider, 2)
        };
      }).Distinct().ToList();
      var data = list.Single(i => i.Id == x.Id);
      Decimal num2 = list.Sum(i => i.Total);
      return new StockRevisionLineInfo()
      {
        StockRevisionId = x.StockRevisionId,
        StockRevisionLineId = x.Id,
        UserId = x.UserId,
        UserName = x.UserName,
        StockId = x.StockId,
        StockCode = stock.Code,
        StockName = stock.Name,
        StockPrice = num1,
        StockPriceCurrencyId = key,
        Date = x.Date,
        Quantity = x.Quantity,
        UnitId = x.UnitId,
        Unit = data.UnitName,
        CurrentCounted = data.Total,
        TotalCounted = num2,
        TotalComputed = balancesSummed.ContainsKey(x.StockId) ? balancesSummed[x.StockId] : 0M
      };
    }));
  }

  public async Task StoreLineAsync(StockRevisionLine line)
  {
    StockRevisionsRepository revisionsRepository = this;
    using (IBucket bucket1 = revisionsRepository.Cluster.OpenDefaultBucket())
    {
      revisionsRepository._lineValidator.Validate(line);
      IDocumentResult<StockRevision> documentAsync1 = await bucket1.GetDocumentAsync<StockRevision>(line.StockRevisionId);
      revisionsRepository.AuthorizeUpdate(line, documentAsync1.Content);
      IDocumentResult<StockRevisionLine> documentAsync2 = await bucket1.GetDocumentAsync<StockRevisionLine>(line.Id);
      Patch patch = revisionsRepository.Patcher.CreatePatch<StockRevisionLine>(line, documentAsync2.Content);
      if (patch == null)
        return;
      ICouchLocalChangesRepositoryService<CouchPatch> repositoryService = revisionsRepository.LocalChangesRepositoryService;
      CouchPatch[] patches = new CouchPatch[1];
      CouchPatch couchPatch = new CouchPatch();
      couchPatch.Id = patch.Id;
      couchPatch.Action = patch.Action;
      couchPatch.PropertyPatches = patch.PropertyPatches;
      couchPatch.SubListPatches = patch.SubListPatches;
      couchPatch.DocType = typeof (StockRevisionLine).Name;
      couchPatch.Author = revisionsRepository.LoginService.Session.Username;
      patches[0] = couchPatch;
      IBucket bucket2 = bucket1;
      await repositoryService.StorePatchesAsync((IEnumerable<CouchPatch>) patches, bucket2);
      IDocumentResult<StockRevisionLine> documentResult = await bucket1.UpsertAsync<StockRevisionLine>((IDocument<StockRevisionLine>) new Document<StockRevisionLine>()
      {
        Id = line.Id,
        Content = line
      });
    }
    revisionsRepository.ChangeListener.Touch();
  }

  public async Task StoreLinesAsync(string revisionId, IEnumerable<StockRevisionLine> list)
  {
    StockRevisionsRepository revisionsRepository = this;
    if (!(list is StockRevisionLine[] stockRevisionLineArray1))
      stockRevisionLineArray1 = list.ToArray<StockRevisionLine>();
    StockRevisionLine[] items = stockRevisionLineArray1;
    using (IBucket bucket = revisionsRepository.Cluster.OpenDefaultBucket())
    {
      IDocumentResult<StockRevision> revision = await bucket.GetDocumentAsync<StockRevision>(revisionId);
      List<CouchPatch> patches = new List<CouchPatch>();
      List<IDocument<StockRevisionLine>> docs = new List<IDocument<StockRevisionLine>>();
      StockRevisionLine[] stockRevisionLineArray = items;
      for (int index = 0; index < stockRevisionLineArray.Length; ++index)
      {
        StockRevisionLine x = stockRevisionLineArray[index];
        revisionsRepository._lineValidator.Validate(x);
        revisionsRepository.AuthorizeUpdate(x, revision.Content);
        IDocumentResult<StockRevisionLine> documentAsync = await bucket.GetDocumentAsync<StockRevisionLine>(x.Id);
        Patch patch = revisionsRepository.Patcher.CreatePatch<StockRevisionLine>(x, documentAsync.Content);
        if (patch != null)
        {
          List<CouchPatch> couchPatchList = patches;
          CouchPatch couchPatch = new CouchPatch();
          couchPatch.Id = patch.Id;
          couchPatch.Action = patch.Action;
          couchPatch.PropertyPatches = patch.PropertyPatches;
          couchPatch.SubListPatches = patch.SubListPatches;
          couchPatch.DocType = typeof (StockRevisionLine).Name;
          couchPatch.Author = revisionsRepository.LoginService.Session.Username;
          couchPatchList.Add(couchPatch);
          docs.Add((IDocument<StockRevisionLine>) new Document<StockRevisionLine>()
          {
            Id = x.Id,
            Content = x
          });
          x = (StockRevisionLine) null;
        }
      }
      stockRevisionLineArray = (StockRevisionLine[]) null;
      await revisionsRepository.LocalChangesRepositoryService.StorePatchesAsync((IEnumerable<CouchPatch>) patches, bucket);
      IDocumentResult<StockRevisionLine>[] documentResultArray = await bucket.UpsertAsync<StockRevisionLine>(docs);
      revision = (IDocumentResult<StockRevision>) null;
      patches = (List<CouchPatch>) null;
      docs = (List<IDocument<StockRevisionLine>>) null;
    }
    revisionsRepository.ChangeListener.Touch();
    items = (StockRevisionLine[]) null;
  }

  public async Task DeleteLineAsync(string stockRevisionLineId)
  {
    StockRevisionsRepository revisionsRepository = this;
    using (IBucket bucket1 = revisionsRepository.Cluster.OpenDefaultBucket())
    {
      IDocumentResult<StockRevisionLine> line = await bucket1.GetDocumentAsync<StockRevisionLine>(stockRevisionLineId);
      IDocumentResult<StockRevision> documentAsync = await bucket1.GetDocumentAsync<StockRevision>(line.Content.StockRevisionId);
      revisionsRepository.AuthorizeUpdate(line.Content, documentAsync.Content);
      Patch patch = revisionsRepository.Patcher.CreatePatch<StockRevisionLine>((StockRevisionLine) null, line.Content);
      if (patch == null)
        return;
      ICouchLocalChangesRepositoryService<CouchPatch> repositoryService = revisionsRepository.LocalChangesRepositoryService;
      CouchPatch[] patches = new CouchPatch[1];
      CouchPatch couchPatch = new CouchPatch();
      couchPatch.Id = patch.Id;
      couchPatch.Action = patch.Action;
      couchPatch.PropertyPatches = patch.PropertyPatches;
      couchPatch.SubListPatches = patch.SubListPatches;
      couchPatch.DocType = typeof (StockRevisionLine).Name;
      couchPatch.Author = revisionsRepository.LoginService.Session.Username;
      patches[0] = couchPatch;
      IBucket bucket2 = bucket1;
      await repositoryService.StorePatchesAsync((IEnumerable<CouchPatch>) patches, bucket2);
      IOperationResult operationResult = await bucket1.RemoveAsync(stockRevisionLineId);
      line = (IDocumentResult<StockRevisionLine>) null;
    }
    revisionsRepository.ChangeListener.Touch();
  }

  private void AuthorizeUpdate(StockRevisionLine line, StockRevision revision)
  {
    if (revision.IsCompleted)
      throw new Exception("Revision is already COMPLETED!");
    if (revision.IsDisabled)
      throw new Exception("Revision is DELETED!");
    this.Authorizer.Authorize((Enum) (TransactionAccessLevel) (revision.UserId == line.UserId ? 6 : 102));
  }

  public async Task<StockRevisionCountInfo> GetCountInfoAsync(
    string revisionId,
    string stockId,
    Func<string, DateTime?> countDateGetter = null)
  {
    StockRevisionsRepository revisionsRepository = this;
    StockRevisionCountInfo countInfoAsync;
    using (IBucket bucket = revisionsRepository.Cluster.OpenDefaultBucket())
    {
      BucketContext context = new BucketContext(bucket);
      StockRevision revision = (await bucket.GetDocumentAsync<StockRevision>(revisionId)).Content;
      revisionsRepository.Authorizer.AuthorizeRead(revision);
      Stock stock = (await bucket.GetDocumentAsync<Stock>(stockId)).Content;
      Decimal totalCounted = (await context.Query<StockRevisionLine>().Where<StockRevisionLine>((Expression<Func<StockRevisionLine, bool>>) (x => x.DocType == "StockRevisionLine" && x.StockRevisionId == revisionId && x.Id == N1QlFunctions.Key(x) && x.StockId == stockId)).ExecuteAsync<StockRevisionLine>()).Sum<StockRevisionLine>((Func<StockRevisionLine, Decimal>) (x =>
      {
        StockUnit stockUnit = stock.Units.Single<StockUnit>((Func<StockUnit, bool>) (i => i.Id == x.UnitId));
        return Math.Round(x.Quantity * stockUnit.Multiplier / stockUnit.Divider, 2);
      }));
      DateTime? date = revision.FinishDate;
      if (revisionsRepository._settings.FreezeStockBlanaceOnRevision)
      {
        DateTime? nullable = countDateGetter(stockId);
        if (nullable.HasValue)
          date = nullable;
      }
      Decimal num = (await revisionsRepository._balancesRepository.GetAsync(revision.WarehouseId, new string[1]
      {
        stockId
      }, date)).Sum<StockBalance>((Func<StockBalance, Decimal>) (x => x.Balance));
      countInfoAsync = new StockRevisionCountInfo()
      {
        StockId = stockId,
        TotalCounted = totalCounted,
        TotalComputed = num
      };
    }
    return countInfoAsync;
  }

  public async Task<IEnumerable<StockRevisionCountInfoWithData>> GetCountInfosAsync(
    string revisionId,
    string priceDisplayCurrencyId = null)
  {
    StockRevisionsRepository revisionsRepository = this;
    using (IBucket bucket = revisionsRepository.Cluster.OpenDefaultBucket())
    {
      BucketContext context = new BucketContext(bucket);
      StockRevision revision = (await bucket.GetDocumentAsync<StockRevision>(revisionId)).Content;
      revisionsRepository.Authorizer.AuthorizeRead(revision);
      List<StockRevisionLine> lines = (await context.Query<StockRevisionLine>().Where<StockRevisionLine>((Expression<Func<StockRevisionLine, bool>>) (x => x.DocType == "StockRevisionLine" && x.StockRevisionId == revisionId && x.Id == N1QlFunctions.Key(x))).ScanConsistency<StockRevisionLine>(ScanConsistency.RequestPlus).ExecuteAsync<StockRevisionLine>()).ToList<StockRevisionLine>();
      if (!lines.Any<StockRevisionLine>())
        return (IEnumerable<StockRevisionCountInfoWithData>) Array.Empty<StockRevisionCountInfoWithData>();
      Dictionary<string, IEnumerable<\u003C\u003Ef__AnonymousType16<string, string, Decimal>>> countsByStocks = lines.GroupBy<StockRevisionLine, string>((Func<StockRevisionLine, string>) (x => x.StockId)).ToDictionary<IGrouping<string, StockRevisionLine>, string, IEnumerable<\u003C\u003Ef__AnonymousType16<string, string, Decimal>>>((Func<IGrouping<string, StockRevisionLine>, string>) (g => g.Key), g => g.Select(x => new
      {
        Id = x.Id,
        UnitId = x.UnitId,
        Quantity = x.Quantity
      }));
      string[] stockIds = countsByStocks.Keys.ToArray<string>();
      Dictionary<string, Stock> stocks = (await context.Query<Stock>().UseKeys<Stock>((IEnumerable<string>) stockIds).ExecuteAsync<Stock>()).ToDictionary<Stock, string, Stock>((Func<Stock, string>) (x => x.Id), (Func<Stock, Stock>) (x => x));
      IEnumerable<StockBalance> async;
      if (!revisionsRepository._settings.FreezeStockBlanaceOnRevision)
      {
        async = await revisionsRepository._balancesRepository.GetAsync(revision.WarehouseId, stockIds, revision.FinishDate);
      }
      else
      {
        (string, DateTime?)[] array = lines.GroupBy<StockRevisionLine, string>((Func<StockRevisionLine, string>) (x => x.StockId)).Select<IGrouping<string, StockRevisionLine>, (string, DateTime?)>((Func<IGrouping<string, StockRevisionLine>, (string, DateTime?)>) (g => (g.Key, new DateTime?(g.Min<StockRevisionLine, DateTime>((Func<StockRevisionLine, DateTime>) (x => x.Date)))))).ToArray<(string, DateTime?)>();
        async = await revisionsRepository._balancesRepository.GetAsync(revision.WarehouseId, array);
      }
      Dictionary<string, Decimal> balancesSummed = async.GroupBy<StockBalance, string>((Func<StockBalance, string>) (x => x.StockId)).ToDictionary<IGrouping<string, StockBalance>, string, Decimal>((Func<IGrouping<string, StockBalance>, string>) (g => g.Key), (Func<IGrouping<string, StockBalance>, Decimal>) (g => Math.Round(g.Sum<StockBalance>((Func<StockBalance, Decimal>) (i => i.Balance)), 2)));
      Dictionary<string, Currency> currencies = (await revisionsRepository._currenciesRepository.GetAsync()).ToDictionary<Currency, string, Currency>((Func<Currency, string>) (x => x.Id), (Func<Currency, Currency>) (x => x));
      return lines.GroupBy(x => new
      {
        StockId = x.StockId,
        Price = this._settings.AllowStockPriceChangeOnRevision ? x.Price : new Decimal?(),
        CurrencyId = this._settings.AllowStockPriceChangeOnRevision ? x.CurrencyId : (string) null
      }).Select<IGrouping<\u003C\u003Ef__AnonymousType18<string, Decimal?, string>, StockRevisionLine>, StockRevisionCountInfoWithData>(g =>
      {
        try
        {
          Stock stock = stocks[g.Key.StockId];
          StockPrice price1 = stock.GetPrice(revision.FinishDate);
          Decimal d;
          string key;
          if (this._settings.AllowStockPriceChangeOnRevision)
          {
            Decimal? price2 = g.Key.Price;
            if (price2.HasValue && !string.IsNullOrEmpty(g.Key.CurrencyId))
            {
              price2 = g.Key.Price;
              d = price2.Value;
              key = g.Key.CurrencyId;
              goto label_4;
            }
          }
          d = price1.Price;
          key = price1.CurrencyId;
label_4:
          if (!string.IsNullOrEmpty(priceDisplayCurrencyId) && key != priceDisplayCurrencyId)
          {
            CurrencyRate rate1 = currencies[key].GetRate(revision.FinishDate);
            CurrencyRate rate2 = currencies[priceDisplayCurrencyId].GetRate(revision.FinishDate);
            d = d * rate1.Multiplier / rate1.Divider / rate2.Multiplier * rate2.Divider;
            key = priceDisplayCurrencyId;
          }
          Decimal num1 = Math.Round(d, currencies[key].Decimals);
          Decimal num2 = countsByStocks[g.Key.StockId].Sum(x =>
          {
            StockUnit stockUnit = stock.Units.Single<StockUnit>((Func<StockUnit, bool>) (i => i.Id == x.UnitId));
            return Math.Round(x.Quantity * stockUnit.Multiplier / stockUnit.Divider, 2);
          });
          return new StockRevisionCountInfoWithData()
          {
            StockId = g.Key.StockId,
            StockCode = stock.Code,
            StockName = stock.Name,
            StockUnit = stock.Unit,
            StockPrice = num1,
            StockPriceCurrencyId = key,
            TotalCounted = num2,
            TotalComputed = balancesSummed.ContainsKey(g.Key.StockId) ? balancesSummed[g.Key.StockId] : 0M
          };
        }
        catch (Exception ex)
        {
          Console.WriteLine((object) ex);
          throw;
        }
      });
    }
  }

  public async Task<IEnumerable<StockRevisionUncountedInfo>> GetUncountedAsync(string revisionId)
  {
    StockRevisionsRepository revisionsRepository = this;
    using (IBucket bucket = revisionsRepository.Cluster.OpenDefaultBucket())
    {
      BucketContext context = new BucketContext(bucket);
      StockRevision revision = (await bucket.GetDocumentAsync<StockRevision>(revisionId)).Content;
      revisionsRepository.Authorizer.AuthorizeRead(revision);
      IEnumerable<string> stocksCounted = (await context.Query<StockRevisionLine>().Where<StockRevisionLine>((Expression<Func<StockRevisionLine, bool>>) (x => x.DocType == "StockRevisionLine" && x.StockRevisionId == revisionId && x.Id == N1QlFunctions.Key(x))).Select<StockRevisionLine, string>((Expression<Func<StockRevisionLine, string>>) (x => x.StockId)).ExecuteAsync<string>()).Distinct<string>();
      List<StockBalance> stocksUncountedWithBalance = (await revisionsRepository._balancesRepository.GetAsync((string) null, revision.FinishDate ?? DateTime.Now, revision.WarehouseId)).Where<StockBalance>((Func<StockBalance, bool>) (x => x.Balance != 0M)).Where<StockBalance>((Func<StockBalance, bool>) (x => !stocksCounted.Contains<string>(x.StockId))).ToList<StockBalance>();
      if (!stocksUncountedWithBalance.Any<StockBalance>())
        return (IEnumerable<StockRevisionUncountedInfo>) Array.Empty<StockRevisionUncountedInfo>();
      IEnumerable<string> keys = stocksUncountedWithBalance.Select<StockBalance, string>((Func<StockBalance, string>) (x => x.StockId)).Distinct<string>();
      Dictionary<string, Stock> stocks = (await context.Query<Stock>().Where<Stock>((Expression<Func<Stock, bool>>) (x => x.DocType == "Stock")).UseKeys<Stock>(keys).ExecuteAsync<Stock>()).ToDictionary<Stock, string, Stock>((Func<Stock, string>) (x => x.Id), (Func<Stock, Stock>) (x => x));
      return stocksUncountedWithBalance.Select<StockBalance, StockRevisionUncountedInfo>((Func<StockBalance, StockRevisionUncountedInfo>) (x =>
      {
        Stock stock = stocks[x.StockId];
        return new StockRevisionUncountedInfo()
        {
          StockRevisionId = revisionId,
          StockId = x.StockId,
          StockCode = stock.Code,
          StockName = stock.Name,
          StockUnit = stock.Unit,
          StockUnitId = stock.UnitId,
          Computed = x.Balance
        };
      }));
    }
  }
}
