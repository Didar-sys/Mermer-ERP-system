// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Warehousing.Services.StockSlipsRepository
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase.Core;
using Couchbase.Linq;
using FluentValidation;
using Mermer.Authorization.Enums;
using Mermer.Authorization.Models;
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
using Mermer.Data.Tools.Expressions;
using Mermer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
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
    this._configurator = configurator;
    this._authService = authService;
  }

  public override async Task ValidateAsync(StockSlip model)
  {
    StockSlipsRepository stockSlipsRepository = this;
    AppSettings configAsync = await stockSlipsRepository._configurator.GetConfigAsync<AppSettings>();
    // ISSUE: reference to a compiler-generated method
    await stockSlipsRepository.Validator.AssertValidAsync<StockSlip>(model, new Action<ValidationContext<StockSlip>>(stockSlipsRepository.\u003CValidateAsync\u003Eb__3_0));
  }

  public override Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
  {
    return this.GetFacetsFromView("transaction", "facets", fields);
  }

  protected override IQueryable<StockSlip> StartListQuery(
    IBucket bucket,
    params Expression<Func<StockSlip, bool>>[] predicates)
  {
    if (!(this.Authorizer is ITransactionAuthorizer<StockSlip> authorizer))
      return base.StartListQuery(bucket, predicates);
    BucketContext bucketContext = new BucketContext(bucket);
    bucketContext.EndChangeTracking();
    IQueryable<StockSlip> queryable = bucketContext.Query<StockSlip>().Where<StockSlip>((Expression<Func<StockSlip, bool>>) (x => x.DocType == typeof (StockSlip).Name && x.Id == N1QlFunctions.Key(x)));
    UserSession currentSession = authorizer.GetCurrentSession();
    if (!currentSession.IsAdmin)
    {
      string userId = currentSession.UserId;
      IEnumerable<string> accounts = authorizer.GetAvailableAccounts(AccountAccessLevel.Read);
      IEnumerable<string> allActions = authorizer.GetAvailableActions(TransactionAccessLevel.ReadAll);
      IEnumerable<string> ownActions = authorizer.GetAvailableActions(TransactionAccessLevel.ReadOwn);
      queryable = queryable.Where<StockSlip>((Expression<Func<StockSlip, bool>>) (x => accounts.Contains<string>(x.WarehouseId))).Where<StockSlip>((Expression<Func<StockSlip, bool>>) (x => allActions.Contains<string>(x.Type) || ownActions.Contains<string>(x.Type) && x.UserId == userId));
    }
    return ((predicates != null ? ((IEnumerable<Expression<Func<StockSlip, bool>>>) predicates).ToList<Expression<Func<StockSlip, bool>>>() : (List<Expression<Func<StockSlip, bool>>>) null) ?? new List<Expression<Func<StockSlip, bool>>>()).Where<Expression<Func<StockSlip, bool>>>((Func<Expression<Func<StockSlip, bool>>, bool>) (predicateQuery => predicateQuery != null)).Select<Expression<Func<StockSlip, bool>>, Expression<Func<StockSlip, bool>>>((Func<Expression<Func<StockSlip, bool>>, Expression<Func<StockSlip, bool>>>) (predicateQuery => predicateQuery.Safe<StockSlip>())).Aggregate<Expression<Func<StockSlip, bool>>, IQueryable<StockSlip>>(queryable, (Func<IQueryable<StockSlip>, Expression<Func<StockSlip, bool>>, IQueryable<StockSlip>>) ((current, predicateQuery) => current.Where<StockSlip>(predicateQuery)));
  }
}
