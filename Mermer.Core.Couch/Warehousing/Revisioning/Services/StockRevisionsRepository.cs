using Couchbase;
using Couchbase.Core;
using Couchbase.Linq;
using Couchbase.Linq.Extensions;
using Couchbase.N1QL;
using FluentValidation;
using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Common.Services;
using Mermer.Common.Settings;
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Core.Couch.Common;
using Mermer.Data.Authorizers;
using Mermer.Data.Patcher;
using Mermer.Data.Storage;
using Mermer.FundsManagement.Models;
using Mermer.FundsManagement.Models.Extenders;
using Mermer.Services;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Models.Extenders;
using Mermer.StockManagement.Services;
using Mermer.Warehousing.Revisioning.Models;
using Mermer.Warehousing.Revisioning.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mermer.Core.Couch.Warehousing.Revisioning.Services;

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
        _configurator = configurator;
        _lineValidator = lineValidator;
        _balancesRepository = balancesRepository;
        _currenciesRepository = currenciesRepository;
        _settings = _configurator.GetConfig<AppSettings>();
    }

    public override Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
    {
        return GetFacetsFromView("transaction", "facets", fields);
    }

    public async Task<StockRevisionLine> GetLineAsync(string stockRevisionLineId)
    {
        using (IBucket bucket = Cluster.OpenDefaultBucket())
        {
            var line = await bucket.GetDocumentAsync<StockRevisionLine>(stockRevisionLineId);
            var revision = await bucket.GetDocumentAsync<StockRevision>(line.Content.StockRevisionId);
            Authorizer.AuthorizeRead(revision.Content);
            return line.Content;
        }
    }

    public async Task<IEnumerable<StockRevisionLine>> GetLinesAsync(string revisionId, params string[] lineIds)
    {
        using (IBucket bucket = Cluster.OpenDefaultBucket())
        {
            var context = new BucketContext(bucket);
            return await GetLinesAsync(revisionId, lineIds, context);
        }
    }

    private Task<IEnumerable<StockRevisionLine>> GetLinesAsync(string revisionId, string[] lineIds, IBucketContext context)
    {
        var queryable = context.Query<StockRevisionLine>().Where(x => x.DocType == "StockRevisionLine" && x.StockRevisionId == revisionId && x.Id == N1QlFunctions.Key(x));
        if (lineIds.Any())
            queryable = queryable.UseKeys(lineIds);
        return queryable.ExecuteAsync();
    }

    public async Task<IEnumerable<StockRevisionLineInfo>> GetLineInfosAsync(string revisionId, params string[] lineIds)
    {
        using (IBucket bucket = Cluster.OpenDefaultBucket())
        {
            var context = new BucketContext(bucket);
            var revision = (await bucket.GetDocumentAsync<StockRevision>(revisionId)).Content;
            Authorizer.AuthorizeRead(revision);

            var lines = await GetLinesAsync(revisionId, lineIds, context);

            // Очищено від __nonvirtual
            return await CalcLineInfosAsync(
                revision,
                lines,
                stockIds => context.Query<Stock>().Where(x => x.DocType == "Stock").UseKeys(stockIds).ExecuteAsync(),
                stockIds => _balancesRepository.GetAsync(revision.WarehouseId, stockIds, revision.FinishDate),
                stockBalanceDates => _balancesRepository.GetAsync(revision.WarehouseId, stockBalanceDates),
                null
            );
        }
    }

    public async Task<IEnumerable<StockRevisionLineInfo>> CalcLineInfosAsync(
        StockRevision revision,
        IEnumerable<StockRevisionLine> lines,
        Func<string[], Task<IEnumerable<Stock>>> stocksGetter,
        Func<string[], Task<IEnumerable<StockBalance>>> stockBalancesGetter,
        Func<(string stockId, DateTime? balanceDate)[], Task<IEnumerable<StockBalance>>> stockBalancesGetterAlt,
        string priceDisplayCurrencyId = null)
    {
        var revisionLines = lines.ToArray();
        if (!revisionLines.Any()) return Array.Empty<StockRevisionLineInfo>();

        var countsByStocks = revisionLines.GroupBy(x => x.StockId).ToDictionary(
            g => g.Key,
            g => g.Select(x => new { x.Id, x.UnitId, x.Quantity })
        );

        var stockIds = revisionLines.Select(x => x.StockId).Distinct().ToArray();
        var stocks = (await stocksGetter(stockIds)).ToDictionary(x => x.Id, x => x);

        IEnumerable<StockBalance> source;
        if (!_settings.FreezeStockBlanaceOnRevision)
        {
            source = await stockBalancesGetter(stockIds);
        }
        else
        {
            var dates = revisionLines.GroupBy(x => x.StockId)
                .Select(g => (g.Key, (DateTime?)g.Min(x => x.Date)))
                .ToArray();
            source = await stockBalancesGetterAlt(dates);
        }

        var balancesSummed = source.GroupBy(x => x.StockId)
            .ToDictionary(g => g.Key, g => Math.Round(g.Sum(i => i.Balance), 2));

        var currencies = (await _currenciesRepository.GetAsync()).ToDictionary(x => x.Id, x => x);

        return revisionLines.Select(x =>
        {
            var stock = stocks[x.StockId];
            decimal d;
            string key;

            // Очищено від жахливого goto
            if (_settings.AllowStockPriceChangeOnRevision && x.Price.HasValue && !string.IsNullOrEmpty(x.CurrencyId))
            {
                d = x.Price.Value;
                key = x.CurrencyId;
            }
            else
            {
                var price1 = stock.GetPrice(revision.FinishDate);
                d = price1.Price;
                key = price1.CurrencyId;
            }

            if (!string.IsNullOrEmpty(priceDisplayCurrencyId) && key != priceDisplayCurrencyId)
            {
                var rate1 = currencies[key].GetRate(revision.FinishDate);
                var rate2 = currencies[priceDisplayCurrencyId].GetRate(revision.FinishDate);
                d = d * rate1.Multiplier / rate1.Divider / rate2.Multiplier * rate2.Divider;
                key = priceDisplayCurrencyId;
            }

            decimal num1 = Math.Round(d, currencies[key].Decimals);

            var list = countsByStocks[x.StockId].Select(i =>
            {
                var stockUnit = stock.Units?.SingleOrDefault(j => j.Id == i.UnitId) ?? new StockUnit { Multiplier = 0M, Divider = 1M };
                return new
                {
                    i.Id,
                    i.UnitId,
                    UnitName = stockUnit.Name,
                    Total = Math.Round(i.Quantity * stockUnit.Multiplier / stockUnit.Divider, 2)
                };
            }).Distinct().ToList();

            var data = list.Single(i => i.Id == x.Id);
            decimal num2 = list.Sum(i => i.Total);

            return new StockRevisionLineInfo
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
        });
    }

    public async Task StoreLineAsync(StockRevisionLine line)
    {
        using (IBucket bucket = Cluster.OpenDefaultBucket())
        {
            _lineValidator.ValidateAndThrow(line);
            var revision = await bucket.GetDocumentAsync<StockRevision>(line.StockRevisionId);
            AuthorizeUpdate(line, revision.Content);

            var existingLine = await bucket.GetDocumentAsync<StockRevisionLine>(line.Id);
            var patch = Patcher.CreatePatch(line, existingLine.Content);

            if (patch == null) return;

            var couchPatch = new CouchPatch
            {
                Id = patch.Id,
                Action = patch.Action,
                PropertyPatches = patch.PropertyPatches,
                SubListPatches = patch.SubListPatches,
                DocType = typeof(StockRevisionLine).Name,
                Author = LoginService.Session.Username
            };

            await LocalChangesRepositoryService.StorePatchesAsync(new[] { couchPatch }, bucket);

            await bucket.UpsertAsync(new Document<StockRevisionLine>
            {
                Id = line.Id,
                Content = line
            });
        }
        ChangeListener.Touch();
    }

    public async Task StoreLinesAsync(string revisionId, IEnumerable<StockRevisionLine> list)
    {
        var items = list.ToArray();
        using (IBucket bucket = Cluster.OpenDefaultBucket())
        {
            var revision = await bucket.GetDocumentAsync<StockRevision>(revisionId);
            var patches = new List<CouchPatch>();
            var docs = new List<IDocument<StockRevisionLine>>();

            foreach (var x in items)
            {
                _lineValidator.ValidateAndThrow(x);
                AuthorizeUpdate(x, revision.Content);

                var existingLine = await bucket.GetDocumentAsync<StockRevisionLine>(x.Id);
                var patch = Patcher.CreatePatch(x, existingLine.Content);

                if (patch != null)
                {
                    patches.Add(new CouchPatch
                    {
                        Id = patch.Id,
                        Action = patch.Action,
                        PropertyPatches = patch.PropertyPatches,
                        SubListPatches = patch.SubListPatches,
                        DocType = typeof(StockRevisionLine).Name,
                        Author = LoginService.Session.Username
                    });

                    docs.Add(new Document<StockRevisionLine>
                    {
                        Id = x.Id,
                        Content = x
                    });
                }
            }

            await LocalChangesRepositoryService.StorePatchesAsync(patches, bucket);
            await bucket.UpsertAsync(docs);
        }
        ChangeListener.Touch();
    }

    public async Task DeleteLineAsync(string stockRevisionLineId)
    {
        using (IBucket bucket = Cluster.OpenDefaultBucket())
        {
            var line = await bucket.GetDocumentAsync<StockRevisionLine>(stockRevisionLineId);
            var revision = await bucket.GetDocumentAsync<StockRevision>(line.Content.StockRevisionId);

            AuthorizeUpdate(line.Content, revision.Content);

            var patch = Patcher.CreatePatch<StockRevisionLine>(null, line.Content);
            if (patch == null) return;

            var couchPatch = new CouchPatch
            {
                Id = patch.Id,
                Action = patch.Action,
                PropertyPatches = patch.PropertyPatches,
                SubListPatches = patch.SubListPatches,
                DocType = typeof(StockRevisionLine).Name,
                Author = LoginService.Session.Username
            };

            await LocalChangesRepositoryService.StorePatchesAsync(new[] { couchPatch }, bucket);
            await bucket.RemoveAsync(stockRevisionLineId);
        }
        ChangeListener.Touch();
    }

    private void AuthorizeUpdate(StockRevisionLine line, StockRevision revision)
    {
        if (revision.IsCompleted) throw new Exception("Revision is already COMPLETED!");
        if (revision.IsDisabled) throw new Exception("Revision is DELETED!");

        Authorizer.Authorize((TransactionAccessLevel)(revision.UserId == line.UserId ? 6 : 102));
    }

    public async Task<StockRevisionCountInfo> GetCountInfoAsync(string revisionId, string stockId, Func<string, DateTime?> countDateGetter = null)
    {
        using (IBucket bucket = Cluster.OpenDefaultBucket())
        {
            var context = new BucketContext(bucket);
            var revision = (await bucket.GetDocumentAsync<StockRevision>(revisionId)).Content;
            Authorizer.AuthorizeRead(revision);

            var stock = (await bucket.GetDocumentAsync<Stock>(stockId)).Content;

            var lines = await context.Query<StockRevisionLine>()
                .Where(x => x.DocType == "StockRevisionLine" && x.StockRevisionId == revisionId && x.Id == N1QlFunctions.Key(x) && x.StockId == stockId)
                .ExecuteAsync();

            decimal totalCounted = lines.Sum(x =>
            {
                var stockUnit = stock.Units.Single(i => i.Id == x.UnitId);
                return Math.Round(x.Quantity * stockUnit.Multiplier / stockUnit.Divider, 2);
            });

            DateTime? date = revision.FinishDate;
            if (_settings.FreezeStockBlanaceOnRevision && countDateGetter != null)
            {
                var customDate = countDateGetter(stockId);
                if (customDate.HasValue) date = customDate;
            }

            decimal totalComputed = (await _balancesRepository.GetAsync(revision.WarehouseId, new[] { stockId }, date))
                .Sum(x => x.Balance);

            return new StockRevisionCountInfo
            {
                StockId = stockId,
                TotalCounted = totalCounted,
                TotalComputed = totalComputed
            };
        }
    }

    public async Task<IEnumerable<StockRevisionCountInfoWithData>> GetCountInfosAsync(string revisionId, string priceDisplayCurrencyId = null)
    {
        using (IBucket bucket = Cluster.OpenDefaultBucket())
        {
            var context = new BucketContext(bucket);
            var revision = (await bucket.GetDocumentAsync<StockRevision>(revisionId)).Content;
            Authorizer.AuthorizeRead(revision);

            var lines = (await context.Query<StockRevisionLine>()
                .Where(x => x.DocType == "StockRevisionLine" && x.StockRevisionId == revisionId && x.Id == N1QlFunctions.Key(x))
                .ScanConsistency(ScanConsistency.RequestPlus)
                .ExecuteAsync()).ToList();

            if (!lines.Any()) return Array.Empty<StockRevisionCountInfoWithData>();

            var countsByStocks = lines.GroupBy(x => x.StockId).ToDictionary(
                g => g.Key,
                g => g.Select(x => new { x.Id, x.UnitId, x.Quantity })
            );

            var stockIds = countsByStocks.Keys.ToArray();
            var stocks = (await context.Query<Stock>().UseKeys(stockIds).ExecuteAsync()).ToDictionary(x => x.Id, x => x);

            IEnumerable<StockBalance> source;
            if (!_settings.FreezeStockBlanaceOnRevision)
            {
                source = await _balancesRepository.GetAsync(revision.WarehouseId, stockIds, revision.FinishDate);
            }
            else
            {
                var array = lines.GroupBy(x => x.StockId).Select(g => (g.Key, (DateTime?)g.Min(x => x.Date))).ToArray();
                source = await _balancesRepository.GetAsync(revision.WarehouseId, array);
            }

            var balancesSummed = source.GroupBy(x => x.StockId).ToDictionary(g => g.Key, g => Math.Round(g.Sum(i => i.Balance), 2));
            var currencies = (await _currenciesRepository.GetAsync()).ToDictionary(x => x.Id, x => x);

            return lines.GroupBy(x => new
            {
                x.StockId,
                Price = _settings.AllowStockPriceChangeOnRevision ? x.Price : null,
                CurrencyId = _settings.AllowStockPriceChangeOnRevision ? x.CurrencyId : null
            }).Select(g =>
            {
                var stock = stocks[g.Key.StockId];
                decimal d;
                string key;

                if (_settings.AllowStockPriceChangeOnRevision && g.Key.Price.HasValue && !string.IsNullOrEmpty(g.Key.CurrencyId))
                {
                    d = g.Key.Price.Value;
                    key = g.Key.CurrencyId;
                }
                else
                {
                    var price1 = stock.GetPrice(revision.FinishDate);
                    d = price1.Price;
                    key = price1.CurrencyId;
                }

                if (!string.IsNullOrEmpty(priceDisplayCurrencyId) && key != priceDisplayCurrencyId)
                {
                    var rate1 = currencies[key].GetRate(revision.FinishDate);
                    var rate2 = currencies[priceDisplayCurrencyId].GetRate(revision.FinishDate);
                    d = d * rate1.Multiplier / rate1.Divider / rate2.Multiplier * rate2.Divider;
                    key = priceDisplayCurrencyId;
                }

                decimal num1 = Math.Round(d, currencies[key].Decimals);
                decimal num2 = countsByStocks[g.Key.StockId].Sum(x =>
                {
                    var stockUnit = stock.Units.Single(i => i.Id == x.UnitId);
                    return Math.Round(x.Quantity * stockUnit.Multiplier / stockUnit.Divider, 2);
                });

                return new StockRevisionCountInfoWithData
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
            });
        }
    }

    public async Task<IEnumerable<StockRevisionUncountedInfo>> GetUncountedAsync(string revisionId)
    {
        using (IBucket bucket = Cluster.OpenDefaultBucket())
        {
            var context = new BucketContext(bucket);
            var revision = (await bucket.GetDocumentAsync<StockRevision>(revisionId)).Content;
            Authorizer.AuthorizeRead(revision);

            var stocksCounted = (await context.Query<StockRevisionLine>()
                .Where(x => x.DocType == "StockRevisionLine" && x.StockRevisionId == revisionId && x.Id == N1QlFunctions.Key(x))
                .Select(x => x.StockId)
                .ExecuteAsync()).Distinct();

            var stocksUncountedWithBalance = (await _balancesRepository.GetAsync(null, revision.FinishDate ?? DateTime.Now, revision.WarehouseId))
                .Where(x => x.Balance != 0M && !stocksCounted.Contains(x.StockId))
                .ToList();

            if (!stocksUncountedWithBalance.Any()) return Array.Empty<StockRevisionUncountedInfo>();

            var keys = stocksUncountedWithBalance.Select(x => x.StockId).Distinct();
            var stocks = (await context.Query<Stock>().Where(x => x.DocType == "Stock").UseKeys(keys).ExecuteAsync())
                .ToDictionary(x => x.Id, x => x);

            return stocksUncountedWithBalance.Select(x =>
            {
                var stock = stocks[x.StockId];
                return new StockRevisionUncountedInfo
                {
                    StockRevisionId = revisionId,
                    StockId = x.StockId,
                    StockCode = stock.Code,
                    StockName = stock.Name,
                    StockUnit = stock.Unit,
                    StockUnitId = stock.UnitId,
                    Computed = x.Balance
                };
            });
        }
    }
}