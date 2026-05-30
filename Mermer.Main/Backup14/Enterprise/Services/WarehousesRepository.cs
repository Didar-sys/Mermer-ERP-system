// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Enterprise.Services.WarehousesRepository
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using FluentValidation;
using Mermer.Authorization.Services;
using Mermer.Common.Services;
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Core.Couch.Common;
using Mermer.Enterprise.Models;
using Mermer.Data.Authorizers;
using Mermer.Data.Patcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Enterprise.Services;

public class WarehousesRepository(
  ICouchCluster cluster,
  IValidator<Warehouse> validator,
  IListAuthorizer<Warehouse> authorizer,
  IDocumentChangeListener changeListener,
  ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService,
  IPatcher patcher,
  ILoginService loginService) : CouchRepositoryWithFacet<Warehouse>(patcher, cluster, validator, loginService, authorizer, changeListener, localChangesRepositoryService)
{
  public override Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
  {
    return this.GetFacetsFromView("enterprise", "warehouse-facets", fields);
  }

  public override async Task<IEnumerable<Warehouse>> GetAsync(
    params Expression<Func<Warehouse, bool>>[] predicates)
  {
    return (IEnumerable<Warehouse>) (await base.GetAsync(predicates)).OrderBy<Warehouse, string>((Func<Warehouse, string>) (x => x.Name));
  }
}
