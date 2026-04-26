// Decompiled with JetBrains decompiler
// Type: Payhas.Data.Synchronizer.Core.Couch.Services.ChangesRepositoryService`1
// Assembly: Payhas.Data.Synchronizer.Core.Couch, Version=0.0.9.0, Culture=neutral, PublicKeyToken=null
// MVID: 7309E162-8E25-4800-97C2-B3CD230F4B8B
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Data.Synchronizer.Core.Couch.dll

using Couchbase;
using Couchbase.Core;
using Couchbase.N1QL;
using Couchbase.Views;
using Payhas.Data.Synchronizer.Core.Couch.Common;
using Payhas.Data.Synchronizer.Core.Models;
using Payhas.Data.Synchronizer.Core.Services;
using Payhas.Data.Synchronizer.Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Data.Synchronizer.Core.Couch.Services;

public abstract class ChangesRepositoryService<T> : 
  ILocalChangesRepositoryService<T>,
  IChangesRepositoryService<T>
  where T : class
{
  protected readonly ICouchCluster CouchCluster;
  protected readonly IChangeHelper ChangeHelper;
  protected readonly IAuthenticationService AuthService;

  public ChangesRepositoryService(
    IChangeHelper changeHelper,
    ICouchCluster couchCluster,
    IAuthenticationService authService)
  {
    this.CouchCluster = couchCluster;
    this.ChangeHelper = changeHelper;
    this.AuthService = authService;
  }

  public virtual async Task<Dictionary<string, IEnumerable<ChangeIdsRange>>> GetChangesIndexAsync()
  {
    AuthenticatedUser authenticatedUserAsync = await this.AuthService.GetAuthenticatedUserAsync();
    Dictionary<string, IEnumerable<ChangeIdsRange>> changesIndexAsync;
    using (IBucket bucket = this.CouchCluster.OpenDefaultBucket())
      changesIndexAsync = (await bucket.QueryAsync<Dictionary<string, IEnumerable<ChangeIdsRange>>>((IViewQueryable) new ViewQuery().From("synchronizer", "indexes").Key((object) authenticatedUserAsync.Id).Stale(StaleState.False))).Values.SingleOrDefault<Dictionary<string, IEnumerable<ChangeIdsRange>>>();
    return changesIndexAsync;
  }

  public virtual async Task<IEnumerable<Change<T>>> QueryChangesByIndexAsync(
    Dictionary<string, IEnumerable<ChangeIdsRange>> changesIndex,
    int skip,
    int take)
  {
    IEnumerable<Change<T>> list;
    using (IBucket bucket = this.CouchCluster.OpenDefaultBucket())
    {
      string[] strArray = new string[7]
      {
        "SELECT `",
        this.CouchCluster.Bucket,
        "`.* FROM `",
        this.CouchCluster.Bucket,
        "` WHERE ",
        string.Join(" OR ", this.ChangeHelper.GenerateChangeIdRanges((await this.AuthService.GetAuthenticatedUserAsync()).Id, changesIndex).Select<(string, string), string>((Func<(string, string), string>) (x => $"(META().id BETWEEN '{x.start}' AND '{x.end}')"))),
        " ORDER BY patchDate ASC OFFSET $offset LIMIT $limit"
      };
      list = (IEnumerable<Change<T>>) (await bucket.QueryAsync<ChangeDocument<T>>(new QueryRequest(string.Concat(strArray)).AddNamedParameter("$offset", (object) skip).AddNamedParameter("$limit", (object) take).AdHoc(false))).ToList<ChangeDocument<T>>();
    }
    return list;
  }

  public virtual async Task StorePatchesAsync(IEnumerable<T> patches)
  {
    AuthenticatedUser authenticatedUser = await this.AuthService.GetAuthenticatedUserAsync();
    using (IBucket bucket = this.CouchCluster.OpenDefaultBucket())
    {
      string serverId = await this.GetServerId();
      await bucket.StorePatchesAsync<T>(this.ChangeHelper, authenticatedUser.Id, serverId, patches);
    }
  }

  public virtual async Task StoreChangesAsync(IEnumerable<Change<T>> changes, bool apply)
  {
    AuthenticatedUser authenticatedUser = await this.AuthService.GetAuthenticatedUserAsync();
    IBucket bucket = this.CouchCluster.OpenDefaultBucket();
    try
    {
      foreach (Change<T> change in changes)
      {
        try
        {
          ChangeDocument<T> changeDocument = new ChangeDocument<T>(authenticatedUser.Id, change);
          changeDocument.Id = this.ChangeHelper.GenerateChangeId((IChangeDocument) changeDocument);
          if (await bucket.ExistsAsync(changeDocument.Id))
            throw new Exception("Document already exists!");
          if (apply)
            await this.ApplyPatch(bucket, change.Patch, change.PatchDate);
          IDocumentResult<ChangeDocument<T>> documentResult = await bucket.InsertAsync<ChangeDocument<T>>((IDocument<ChangeDocument<T>>) new Document<ChangeDocument<T>>()
          {
            Id = changeDocument.Id,
            Content = changeDocument
          });
          changeDocument = (ChangeDocument<T>) null;
        }
        catch (Exception ex)
        {
          bucket?.Dispose();
          bucket = this.CouchCluster.OpenDefaultBucket();
        }
      }
    }
    finally
    {
      bucket?.Dispose();
    }
  }

  protected abstract Task<string> GetServerId();

  protected abstract Task ApplyPatch(IBucket bucket, T patch, DateTime patchDate);
}
