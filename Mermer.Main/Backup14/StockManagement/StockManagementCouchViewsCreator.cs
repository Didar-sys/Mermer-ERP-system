// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.StockManagement.StockManagementCouchViewsCreator
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
namespace Mermer.Core.Couch.StockManagement;

public class StockManagementCouchViewsCreator : IInitialSchemaCreator
{
  private readonly ICouchCluster _cluster;

  public StockManagementCouchViewsCreator(ICouchCluster cluster) => this._cluster = cluster;

  public async Task CreateAsync(bool includeReporting)
  {
    StockManagementCouchViewsCreator couchViewsCreator = this;
    Assembly assembly = couchViewsCreator.GetType().Assembly;
    using (IBucket bucket = couchViewsCreator._cluster.OpenDefaultBucket())
    {
      using (IBucketManager manager = bucket.CreateManager())
      {
        IResult result1 = await manager.UpdateDesignDocumentAsync("stock-management", await assembly.ReadResourceAsync("stock-management.json"));
        if (!result1.Success)
          throw result1.Exception ?? new Exception(result1.Message);
        if (includeReporting)
        {
          IResult result2 = await manager.UpdateDesignDocumentAsync("stock-management-reporting", await assembly.ReadResourceAsync("stock-management-reporting.json"));
          if (!result2.Success)
            throw result2.Exception ?? new Exception(result2.Message);
        }
        else
        {
          IResult result3 = await manager.RemoveDesignDocumentAsync("stock-management-reporting");
        }
      }
    }
    assembly = (Assembly) null;
  }
}
