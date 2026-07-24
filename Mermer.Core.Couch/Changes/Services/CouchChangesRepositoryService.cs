// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Changes.Services.CouchChangesRepositoryService
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase;
using Couchbase.Core;
using Couchbase.Views;
using Mermer.Core.Couch.Common;
using Mermer.Data.Patcher;
using Mermer.Data.Synchronizer.Core.Couch.Services;
using Mermer.Data.Synchronizer.Core.Models;
using Mermer.Data.Synchronizer.Core.Services;
using Mermer.Data.Synchronizer.Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Changes.Services;

public sealed class CouchChangesRepositoryService : 
  ChangesRepositoryService<CouchPatch>,
  ICouchLocalChangesRepositoryService<CouchPatch>,
  ILocalChangesRepositoryService<CouchPatch>,
  IChangesRepositoryService<CouchPatch>
{
  private readonly IPatcher _patcher;
  private readonly IServerIdProviderService _serverIdProviderService;
  private static string _serverId;

  public CouchChangesRepositoryService(
    IPatcher patcher,
    IChangeHelper changeHelper,
    Mermer.Data.Synchronizer.Core.Couch.Common.ICouchCluster couchCluster,
    IAuthenticationService authService,
    IServerIdProviderService serverIdProviderService)
    : base(changeHelper, couchCluster, authService)
  {
    this._patcher = patcher;
    this._serverIdProviderService = serverIdProviderService;
  }

  protected override async Task<string> GetServerId()
  {
    if (string.IsNullOrEmpty(CouchChangesRepositoryService._serverId))
      CouchChangesRepositoryService._serverId = await this._serverIdProviderService.GetUniqueIdAsync();
    return CouchChangesRepositoryService._serverId;
  }

  public override async Task StorePatchesAsync(IEnumerable<CouchPatch> patches)
  {
    CouchChangesRepositoryService repositoryService = this;
    using (IBucket bucket = repositoryService.CouchCluster.OpenDefaultBucket())
      await repositoryService.StorePatchesAsync(patches, bucket);
  }

  public async Task StorePatchesAsync(IEnumerable<CouchPatch> patches, IBucket bucket)
  {
    CouchChangesRepositoryService repositoryService = this;
    string serverId = await repositoryService.GetServerId();
    AuthenticatedUser authenticatedUserAsync = await repositoryService.AuthService.GetAuthenticatedUserAsync();
    foreach (CouchPatch patch in patches)
    {
      if ((await repositoryService.GetPatches(bucket, patch.Id, DateTime.Now)).Any<CouchPatch>())
        throw new Exception("Sagat nädogry, şuwagtdan hem soň üýtgeşme edilen!");
    }
    await bucket.StorePatchesAsync<CouchPatch>(repositoryService.ChangeHelper, authenticatedUserAsync.Id, serverId, patches);
    serverId = (string) null;
    authenticatedUserAsync = (AuthenticatedUser) null;
  }

  public async Task<IEnumerable<CouchPatch>> GetPatches(
    IBucket bucket,
    string documentId,
    DateTime patchDate)
  {
    IViewQuery viewQuery = new ViewQuery().From("changes", "changes-by-doc").StartKey((object) new object[2]
    {
      (object) documentId,
      (object) patchDate.ToString("O")
    });
    object[] endKey = new object[2]
    {
      (object) documentId,
      (object) "zzz"
    };
    return (await bucket.QueryAsync<Change<CouchPatch>>((IViewQueryable) viewQuery.EndKey((object) endKey).Reduce(false).Stale(StaleState.False))).Values.OrderBy<Change<CouchPatch>, DateTime>((Func<Change<CouchPatch>, DateTime>) (x => x.PatchDate)).Select<Change<CouchPatch>, CouchPatch>((Func<Change<CouchPatch>, CouchPatch>) (x => x.Patch));
  }

  protected override async Task ApplyPatch(IBucket bucket, CouchPatch patch, DateTime patchDate)
  {
    CouchChangesRepositoryService repositoryService = this;
    await (Task) ((IEnumerable<MethodInfo>) repositoryService.GetType().GetMethods()).Single<MethodInfo>((Func<MethodInfo, bool>) (x => x.IsGenericMethod && x.Name == nameof (ApplyPatch))).MakeGenericMethod(((IEnumerable<Type>) typeof (BinyatModule).Assembly.GetTypes()).Single<Type>((Func<Type, bool>) (x => x.Name == patch.DocType))).Invoke((object) repositoryService, new object[3]
    {
      (object) bucket,
      (object) patch,
      (object) patchDate
    });
  }

    // --- Исправлены заглушки для реализации интерфейса ---
    public Task<Dictionary<string, IEnumerable<ChangeIdsRange>>> GetChangesIndexAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Change<CouchPatch>>> QueryChangesByIndexAsync(Dictionary<string, IEnumerable<ChangeIdsRange>> index, int skip, int take)
    {
        throw new NotImplementedException();
    }

    public Task StoreChangesAsync(IEnumerable<Change<CouchPatch>> changes, bool saveIndex)
    {
        throw new NotImplementedException();
    }

    public async Task ApplyPatch<T>(IBucket bucket, CouchPatch patch, DateTime patchDate) where T : class, new()
  {
    IDocumentResult<T> existingDocument;
    switch (patch.Action)
    {
      case PatchAction.Create:
        T obj1 = this._patcher.ApplyPatch<T>((Patch) patch, default (T));
        IDocumentResult<T> documentResult1 = await bucket.InsertAsync<T>((IDocument<T>) new Document<T>()
        {
          Id = patch.Id,
          Content = obj1
        });
        break;
      case PatchAction.Update:
        existingDocument = await bucket.GetDocumentAsync<T>(patch.Id);
        List<Patch> list = (await this.GetPatches(bucket, patch.Id, patchDate)).Cast<Patch>().ToList<Patch>();
        T obj2 = this._patcher.ApplyPatch<T>(this._patcher.CreatePatchForLeftPatch((Patch) patch, list), existingDocument.Content);
        IDocumentResult<T> documentResult2 = await bucket.ReplaceAsync<T>((IDocument<T>) new Document<T>()
        {
          Id = patch.Id,
          Content = obj2
        });
        break;
      case PatchAction.Delete:
        IOperationResult operationResult = await bucket.RemoveAsync(patch.Id);
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
    existingDocument = (IDocumentResult<T>) null;
  }
}
