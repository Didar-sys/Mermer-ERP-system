// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Commerce.Services.BillsRepository
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase.Core;
using Couchbase.Linq;
using FluentValidation;
using Mermer.Authorization.Enums;
using Mermer.Authorization.Models;
using Mermer.Authorization.Services;
using Mermer.Commerce.Models;
using Mermer.Common.Services;
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Core.Couch.Common;
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
namespace Mermer.Core.Couch.Commerce.Services;

public class BillsRepository(
  IPatcher patcher,
  ICouchCluster cluster,
  IValidator<Bill> validator,
  ILoginService loginService,
  IListAuthorizer<Bill> authorizer,
  IDocumentChangeListener changeListener,
  ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService) : 
  CouchRepositoryWithFacet<Bill>(patcher, cluster, validator, loginService, authorizer, changeListener, localChangesRepositoryService)
{
  public override Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
  {
    return this.GetFacetsFromView("transaction", "facets", fields);
  }

  protected override IQueryable<Bill> StartListQuery(
    IBucket bucket,
    params Expression<Func<Bill, bool>>[] predicates)
  {
    if (!(this.Authorizer is ITransactionAuthorizer<Bill> authorizer))
      return base.StartListQuery(bucket, predicates);
    BucketContext bucketContext = new BucketContext(bucket);
    bucketContext.EndChangeTracking();
    IQueryable<Bill> queryable = bucketContext.Query<Bill>().Where<Bill>((Expression<Func<Bill, bool>>) (x => x.DocType == typeof (Bill).Name));
    UserSession currentSession = authorizer.GetCurrentSession();
    if (!currentSession.IsAdmin)
    {
      string userId = currentSession.UserId;
      IEnumerable<string> accounts = authorizer.GetAvailableAccounts(AccountAccessLevel.Read);
      IEnumerable<string> allActions = authorizer.GetAvailableActions(TransactionAccessLevel.ReadAll);
      IEnumerable<string> ownActions = authorizer.GetAvailableActions(TransactionAccessLevel.ReadOwn);
      queryable = queryable.Where<Bill>((Expression<Func<Bill, bool>>) (x => accounts.Contains<string>(x.DepositoryId))).Where<Bill>((Expression<Func<Bill, bool>>) (x => allActions.Contains<string>(x.Type) || ownActions.Contains<string>(x.Type) && x.UserId == userId));
    }
    return ((predicates != null ? ((IEnumerable<Expression<Func<Bill, bool>>>) predicates).ToList<Expression<Func<Bill, bool>>>() : (List<Expression<Func<Bill, bool>>>) null) ?? new List<Expression<Func<Bill, bool>>>()).Where<Expression<Func<Bill, bool>>>((Func<Expression<Func<Bill, bool>>, bool>) (predicateQuery => predicateQuery != null)).Select<Expression<Func<Bill, bool>>, Expression<Func<Bill, bool>>>((Func<Expression<Func<Bill, bool>>, Expression<Func<Bill, bool>>>) (predicateQuery => predicateQuery.Safe<Bill>())).Aggregate<Expression<Func<Bill, bool>>, IQueryable<Bill>>(queryable, (Func<IQueryable<Bill>, Expression<Func<Bill, bool>>, IQueryable<Bill>>) ((current, predicateQuery) => current.Where<Bill>(predicateQuery)));
  }
}
