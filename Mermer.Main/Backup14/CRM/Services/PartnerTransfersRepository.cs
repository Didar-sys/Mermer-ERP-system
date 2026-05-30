// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.CRM.Services.PartnerTransfersRepository
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase.Core;
using Couchbase.Linq;
using FluentValidation;
using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Common.Services;
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Core.Couch.Common;
using Mermer.CRM.Models;
using Mermer.Data.Authorizers;
using Mermer.Data.Patcher;
using Mermer.Data.Tools.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.CRM.Services;

public class PartnerTransfersRepository : CouchRepositoryWithFacet<PartnerTransfer>
{
  private readonly ILoginService _loginService;
  private readonly IAuthorizationService _authService;

  public PartnerTransfersRepository(
    ICouchCluster cluster,
    ILoginService loginService,
    IAuthorizationService authService,
    IValidator<PartnerTransfer> validator,
    IListAuthorizer<PartnerTransfer> authorizer,
    IDocumentChangeListener changeListener,
    ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService,
    IPatcher patcher,
    ILoginService loginService1)
    : base(patcher, cluster, validator, loginService1, authorizer, changeListener, localChangesRepositoryService)
  {
    this._loginService = loginService;
    this._authService = authService;
  }

  public override Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
  {
    return this.GetFacetsFromView("transaction", "facets", fields);
  }

  protected override IQueryable<PartnerTransfer> StartListQuery(
    IBucket bucket,
    params Expression<Func<PartnerTransfer, bool>>[] predicates)
  {
    BucketContext bucketContext = new BucketContext(bucket);
    bucketContext.EndChangeTracking();
    IQueryable<PartnerTransfer> queryable = bucketContext.Query<PartnerTransfer>().Where<PartnerTransfer>((Expression<Func<PartnerTransfer, bool>>) (x => x.DocType == typeof (PartnerTransfer).Name && x.Id == N1QlFunctions.Key(x)));
    if (!this._loginService.Session.IsAdmin)
    {
      IEnumerable<string> accounts = this._authService.GetAccessableAccounts(AccountAccessLevel.Read);
      queryable = queryable.Where<PartnerTransfer>((Expression<Func<PartnerTransfer, bool>>) (x => x.OfficeIds.All<string>((Func<string, bool>) (i => accounts.Contains<string>(i)))));
    }
    return ((predicates != null ? ((IEnumerable<Expression<Func<PartnerTransfer, bool>>>) predicates).ToList<Expression<Func<PartnerTransfer, bool>>>() : (List<Expression<Func<PartnerTransfer, bool>>>) null) ?? new List<Expression<Func<PartnerTransfer, bool>>>()).Where<Expression<Func<PartnerTransfer, bool>>>((Func<Expression<Func<PartnerTransfer, bool>>, bool>) (predicateQuery => predicateQuery != null)).Select<Expression<Func<PartnerTransfer, bool>>, Expression<Func<PartnerTransfer, bool>>>((Func<Expression<Func<PartnerTransfer, bool>>, Expression<Func<PartnerTransfer, bool>>>) (predicateQuery => predicateQuery.Safe<PartnerTransfer>())).Aggregate<Expression<Func<PartnerTransfer, bool>>, IQueryable<PartnerTransfer>>(queryable, (Func<IQueryable<PartnerTransfer>, Expression<Func<PartnerTransfer, bool>>, IQueryable<PartnerTransfer>>) ((current, predicateQuery) => current.Where<PartnerTransfer>(predicateQuery)));
  }
}
