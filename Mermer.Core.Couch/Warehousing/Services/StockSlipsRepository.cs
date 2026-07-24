using Couchbase.Core;
using Couchbase.Linq;
using FluentValidation;
using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Common.Services;
using Mermer.Common.Settings;
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Core.Couch.Common;
using Mermer.Transactions.Models.Authorizers;
using Mermer.Warehousing.Models;
using Mermer.Data.Authorizers;
using Mermer.Data.Patcher;
using Mermer.Services;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System;

namespace Mermer.Core.Couch.Warehousing.Services;

public class StockSlipsRepository : CouchRepositoryWithFacet<StockSlip>
{
    private readonly IConfigurator _configurator;
    private readonly IAuthorizationService _authService;

    public StockSlipsRepository(
        ICouchCluster cluster,
        IValidator<StockSlip> validator,
        IConfigurator configurator,
        IAuthorizationService authService,
        IListAuthorizer<StockSlip> authorizer,
        IDocumentChangeListener changeListener,
        ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService,
        IPatcher patcher,
        ILoginService loginService)
        : base(patcher, cluster, validator, loginService, authorizer, changeListener, localChangesRepositoryService)
    {
        _configurator = configurator;
        _authService = authService;
    }

    public override async Task ValidateAsync(StockSlip model)
    {
        var config = await _configurator.GetConfigAsync<AppSettings>();
        await Validator.ValidateAndThrowAsync(model); // Используем стандартный валидтор
    }

    public override Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
    {
        return GetFacetsFromView("transaction", "facets", fields);
    }

    protected override IQueryable<StockSlip> StartListQuery(IBucket bucket, params Expression<Func<StockSlip, bool>>[] predicates)
    {
        if (!(Authorizer is ITransactionAuthorizer<StockSlip> authorizer))
            return base.StartListQuery(bucket, predicates);

        var bucketContext = new BucketContext(bucket);
        bucketContext.EndChangeTracking();

        var queryable = bucketContext.Query<StockSlip>().Where(x => x.DocType == typeof(StockSlip).Name && x.Id == N1QlFunctions.Key(x));
        var currentSession = authorizer.GetCurrentSession();

        if (!currentSession.IsAdmin)
        {
            string userId = currentSession.UserId;
            var accounts = authorizer.GetAvailableAccounts(AccountAccessLevel.Read);
            var allActions = authorizer.GetAvailableActions(TransactionAccessLevel.ReadAll);
            var ownActions = authorizer.GetAvailableActions(TransactionAccessLevel.ReadOwn);

            queryable = queryable.Where(x => accounts.Contains(x.WarehouseId))
                                 .Where(x => allActions.Contains(x.Type) || (ownActions.Contains(x.Type) && x.UserId == userId));
        }

        if (predicates != null)
        {
            foreach (var predicate in predicates.Where(p => p != null))
            {
                queryable = queryable.Where(predicate);
            }
        }

        return queryable;
    }
}