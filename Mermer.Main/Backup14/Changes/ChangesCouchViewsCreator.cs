// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Changes.ChangesCouchViewsCreator
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase;
using Couchbase.Core;
using Couchbase.Management;
using Mermer.Core.Couch.Common;
using Mermer.Data.Storage;
using System;
using System.Reflection;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Changes;

public class ChangesCouchViewsCreator : IInitialSchemaCreator
{
  private readonly ICouchCluster _cluster;

  public ChangesCouchViewsCreator(ICouchCluster cluster) => this._cluster = cluster;

  public async Task CreateAsync(bool includeReporting)
  {
    ChangesCouchViewsCreator couchViewsCreator = this;
    Assembly assembly = couchViewsCreator.GetType().Assembly;
    string designDocChanges = await assembly.ReadResourceAsync("changes.json");
    string designDocSynchronizer = await assembly.ReadResourceAsync("synchronizer-doc.json");
    using (IBucket bucket = couchViewsCreator._cluster.OpenDefaultBucket())
    {
      using (IBucketManager manager = bucket.CreateManager())
      {
        IResult result1 = await manager.UpdateDesignDocumentAsync("changes", designDocChanges);
        if (!result1.Success)
          throw result1.Exception ?? new Exception(result1.Message);
        IResult result2 = await manager.UpdateDesignDocumentAsync("synchronizer", designDocSynchronizer);
        if (!result2.Success)
          throw result2.Exception ?? new Exception(result2.Message);
      }
    }
    assembly = (Assembly) null;
    designDocChanges = (string) null;
    designDocSynchronizer = (string) null;
  }
}
