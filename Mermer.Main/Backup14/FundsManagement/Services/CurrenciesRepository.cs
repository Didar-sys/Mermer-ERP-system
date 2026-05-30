// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.FundsManagement.Services.CurrenciesRepository
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase.Core;
using Couchbase.Linq;
using Couchbase.Linq.Extensions;
using FluentValidation;
using Mermer.Authorization.Services;
using Mermer.Common.Services;
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Core.Couch.Common;
using Mermer.FundsManagement.Models;
using Mermer.Data.Authorizers;
using Mermer.Data.Patcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.FundsManagement.Services;

public class CurrenciesRepository(
  IPatcher patcher,
  ICouchCluster cluster,
  ILoginService loginService,
  IValidator<Currency> validator,
  IListAuthorizer<Currency> authorizer,
  IDocumentChangeListener changeListener,
  ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService) : 
  CouchRepository<Currency>(patcher, cluster, validator, loginService, authorizer, changeListener, localChangesRepositoryService)
{
  public override async Task CreateAsync(Currency model)
  {
    if (model.IsDefault)
      await this.AuthorizeDefaultCurrency(model.Id);
    await base.CreateAsync(model);
  }

  public override async Task UpdateAsync(Currency model)
  {
    if (model.IsDefault)
      await this.AuthorizeDefaultCurrency(model.Id);
    await base.UpdateAsync(model);
  }

  private async Task AuthorizeDefaultCurrency(string id)
  {
    using (IBucket bucket = this.Cluster.OpenDefaultBucket())
    {
      string[] array = (await new BucketContext(bucket).Query<Currency>().Where<Currency>((Expression<Func<Currency, bool>>) (x => x.DocType == "Currency" && x.IsDefault)).ExecuteAsync<Currency>()).Select<Currency, string>((Func<Currency, string>) (x => x.Id)).ToArray<string>();
      if (((IEnumerable<string>) array).Any<string>())
      {
        if (!((IEnumerable<string>) array).Contains<string>(id))
          throw new Exception("Only one currency is allowed to be default");
      }
    }
  }

  public override async Task<IEnumerable<Currency>> GetAsync(
    params Expression<Func<Currency, bool>>[] predicates)
  {
    return (IEnumerable<Currency>) (await base.GetAsync(predicates)).OrderByDescending<Currency, bool>((Func<Currency, bool>) (x => x.IsDefault));
  }
}
