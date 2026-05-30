// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Finance.Services.FundsSlipsRepository
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
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Core.Couch.Common;
using Mermer.Finance.Models;
using Mermer.Transactions.Models.Authorizers;
using Mermer.Data.Authorizers;
using Mermer.Data.Patcher;
using Mermer.Data.Tools.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Finance.Services;

public class FundsSlipsRepository(
  ICouchCluster cluster,
  IValidator<FundsSlip> validator,
  IListAuthorizer<FundsSlip> authorizer,
  IDocumentChangeListener changeListener,
  ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService,
  IPatcher patcher,
  ILoginService loginService) : CouchRepositoryWithFacet<FundsSlip>(patcher, cluster, validator, loginService, authorizer, changeListener, localChangesRepositoryService)
{
  public override Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
  {
    return this.GetFacetsFromView("transaction", "facets", fields);
  }

  protected override IQueryable<FundsSlip> StartListQuery(
    IBucket bucket,
    params Expression<Func<FundsSlip, bool>>[] predicates)
  {
    if (!(this.Authorizer is ITransactionAuthorizer<FundsSlip> authorizer))
      return base.StartListQuery(bucket, predicates);
    BucketContext bucketContext = new BucketContext(bucket);
    bucketContext.EndChangeTracking();
    IQueryable<FundsSlip> queryable = bucketContext.Query<FundsSlip>().Where<FundsSlip>((Expression<Func<FundsSlip, bool>>) (x => x.DocType == typeof (FundsSlip).Name));
    UserSession currentSession = authorizer.GetCurrentSession();
    if (!currentSession.IsAdmin)
    {
      string userId = currentSession.UserId;
      IEnumerable<string> accounts = authorizer.GetAvailableAccounts(AccountAccessLevel.Read);
      IEnumerable<string> allActions = authorizer.GetAvailableActions(TransactionAccessLevel.ReadAll);
      IEnumerable<string> ownActions = authorizer.GetAvailableActions(TransactionAccessLevel.ReadOwn);
      queryable = queryable.Where<FundsSlip>((Expression<Func<FundsSlip, bool>>) (x => accounts.Contains<string>(x.DepositoryId))).Where<FundsSlip>((Expression<Func<FundsSlip, bool>>) (x => allActions.Contains<string>(x.Type) || ownActions.Contains<string>(x.Type) && x.UserId == userId));
    }
    return ((predicates != null ? ((IEnumerable<Expression<Func<FundsSlip, bool>>>) predicates).ToList<Expression<Func<FundsSlip, bool>>>() : (List<Expression<Func<FundsSlip, bool>>>) null) ?? new List<Expression<Func<FundsSlip, bool>>>()).Where<Expression<Func<FundsSlip, bool>>>((Func<Expression<Func<FundsSlip, bool>>, bool>) (predicateQuery => predicateQuery != null)).Select<Expression<Func<FundsSlip, bool>>, Expression<Func<FundsSlip, bool>>>((Func<Expression<Func<FundsSlip, bool>>, Expression<Func<FundsSlip, bool>>>) (predicateQuery => predicateQuery.Safe<FundsSlip>())).Aggregate<Expression<Func<FundsSlip, bool>>, IQueryable<FundsSlip>>(queryable, (Func<IQueryable<FundsSlip>, Expression<Func<FundsSlip, bool>>, IQueryable<FundsSlip>>) ((current, predicateQuery) => current.Where<FundsSlip>(predicateQuery)));
  }
}
