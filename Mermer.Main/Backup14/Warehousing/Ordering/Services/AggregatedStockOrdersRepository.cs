// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Warehousing.Ordering.Services.AggregatedStockOrdersRepository
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using FluentValidation;
using Mermer.Authorization.Services;
using Mermer.Common.Services;
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Core.Couch.Common;
using Mermer.Warehousing.Ordering.Models;
using Mermer.Data.Authorizers;
using Mermer.Data.Patcher;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Warehousing.Ordering.Services;

public class AggregatedStockOrdersRepository(
  ICouchCluster cluster,
  IValidator<AggregatedStockOrder> validator,
  IListAuthorizer<AggregatedStockOrder> authorizer,
  IDocumentChangeListener changeListener,
  ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService,
  IPatcher patcher,
  ILoginService loginService) : CouchRepositoryWithFacet<AggregatedStockOrder>(patcher, cluster, validator, loginService, authorizer, changeListener, localChangesRepositoryService)
{
  public override Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
  {
    return this.GetFacetsFromView("transaction", "facets", fields);
  }
}
