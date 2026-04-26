// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Couch.Authentication.AuthInitialDataCreator
// Assembly: Payhas.Binyat.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.Couch.dll

using Couchbase;
using Couchbase.Core;
using Couchbase.Linq;
using Couchbase.Linq.Extensions;
using Payhas.Binyat.Authorization.Models;
using Payhas.Binyat.Core.Couch.Changes;
using Payhas.Binyat.Core.Couch.Changes.Services;
using Payhas.Binyat.Core.Couch.Common;
using Payhas.Data.Patcher;
using Payhas.Data.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Core.Couch.Authentication;

public class AuthInitialDataCreator : IInitialDataCreator
{
  private readonly IPatcher _patcher;
  private readonly ICouchCluster _cluster;
  private readonly ICouchLocalChangesRepositoryService<CouchPatch> _localChangesRepositoryService;

  public AuthInitialDataCreator(
    IPatcher patcher,
    ICouchCluster cluster,
    ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService)
  {
    this._patcher = patcher;
    this._cluster = cluster;
    this._localChangesRepositoryService = localChangesRepositoryService;
  }

  public async Task CreateAsync()
  {
    using (IBucket bucket1 = this._cluster.OpenDefaultBucket())
    {
      if (!(await new BucketContext(bucket1).Query<User>().Where<User>((Expression<Func<User, bool>>) (x => x.DocType == typeof (User).Name && x.IsAdmin)).ExecuteAsync<User>()).Any<User>())
      {
        User user = new User();
        user.Id = Guid.NewGuid().ToString();
        user.Username = "admin";
        user.Password = "admin".Hash();
        user.IsAdmin = true;
        User item = user;
        Patch patch = this._patcher.CreatePatch<User>(item, (User) null);
        if (patch == null)
          return;
        ICouchLocalChangesRepositoryService<CouchPatch> repositoryService = this._localChangesRepositoryService;
        CouchPatch[] patches = new CouchPatch[1];
        CouchPatch couchPatch = new CouchPatch();
        couchPatch.Id = patch.Id;
        couchPatch.Action = patch.Action;
        couchPatch.PropertyPatches = patch.PropertyPatches;
        couchPatch.SubListPatches = patch.SubListPatches;
        couchPatch.DocType = typeof (User).Name;
        couchPatch.Author = item.Username;
        patches[0] = couchPatch;
        IBucket bucket2 = bucket1;
        await repositoryService.StorePatchesAsync((IEnumerable<CouchPatch>) patches, bucket2);
        IDocumentResult<User> documentResult = await bucket1.InsertAsync<User>((IDocument<User>) new Document<User>()
        {
          Id = item.Id,
          Content = item
        });
        item = (User) null;
      }
    }
  }
}
