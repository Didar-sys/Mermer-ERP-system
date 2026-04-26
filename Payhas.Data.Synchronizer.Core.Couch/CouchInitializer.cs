// Decompiled with JetBrains decompiler
// Type: Payhas.Data.Synchronizer.Core.Couch.CouchInitializer
// Assembly: Payhas.Data.Synchronizer.Core.Couch, Version=0.0.9.0, Culture=neutral, PublicKeyToken=null
// MVID: 7309E162-8E25-4800-97C2-B3CD230F4B8B
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Data.Synchronizer.Core.Couch.dll

using Couchbase;
using Couchbase.Core;
using Couchbase.Management;
using Payhas.Data.Synchronizer.Core.Couch.Common;
using Payhas.Data.Synchronizer.Core.Couch.Helpers;
using Payhas.Data.Synchronizer.Core.Services;
using System;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Data.Synchronizer.Core.Couch;

public class CouchInitializer : IInitializer
{
  private readonly ICouchCluster _cluster;

  public CouchInitializer(ICouchCluster cluster) => this._cluster = cluster;

  public async Task InitializeAsync()
  {
    CouchInitializer couchInitializer = this;
    string designDoc = await couchInitializer.GetType().Assembly.ReadResourceAsync("synchronizer.json");
    using (IBucket bucket = couchInitializer._cluster.OpenDefaultBucket())
    {
      using (IBucketManager manager = bucket.CreateManager())
      {
        IResult result = await manager.UpdateDesignDocumentAsync("synchronizer", designDoc);
        if (!result.Success)
          throw result.Exception ?? new Exception(result.Message);
      }
    }
  }
}
