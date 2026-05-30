// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.StockManagement.Services.StockAlternativesRepository
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase.Core;
using Couchbase.Views;
using FluentValidation;
using Mermer.Authorization.Services;
using Mermer.Common.Services;
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Core.Couch.Common;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;
using Mermer.Data.Authorizers;
using Mermer.Data.Patcher;
using Mermer.Data.Storage;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.StockManagement.Services;

public class StockAlternativesRepository(
  IPatcher patcher,
  ICouchCluster cluster,
  ILoginService loginService,
  IValidator<StockAlternative> validator,
  IListAuthorizer<StockAlternative> authorizer,
  IDocumentChangeListener changeListener,
  ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService) : 
  CouchRepository<StockAlternative>(patcher, cluster, validator, loginService, authorizer, changeListener, localChangesRepositoryService),
  IStockAlternativesRepository,
  IRepository<StockAlternative>,
  IReadOnlyRepository<StockAlternative>
{
  public async Task<SingleStockAlternative> GetAlternativesAsync(string stockId)
  {
    SingleStockAlternative alternativesAsync;
    using (IBucket bucket = this.Cluster.OpenDefaultBucket())
    {
      IViewResult<string> viewResult = await bucket.QueryAsync<string>((IViewQueryable) new ViewQuery().From("stock-management", "stock-alternatives").Key((object) stockId));
      if (viewResult.Exception != null)
        throw viewResult.Exception;
      alternativesAsync = new SingleStockAlternative()
      {
        StockId = stockId,
        Alternatives = viewResult.Values.Distinct<string>()
      };
    }
    return alternativesAsync;
  }
}
