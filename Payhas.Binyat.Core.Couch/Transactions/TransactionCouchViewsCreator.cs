// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Couch.Transactions.TransactionCouchViewsCreator
// Assembly: Payhas.Binyat.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.Couch.dll

using Couchbase;
using Couchbase.Core;
using Couchbase.Management;
using Payhas.Binyat.Core.Couch.Common;
using Payhas.Data.Storage;
using System;
using System.Reflection;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Core.Couch.Transactions;

public class TransactionCouchViewsCreator : IInitialSchemaCreator
{
  private readonly ICouchCluster _cluster;

  public TransactionCouchViewsCreator(ICouchCluster cluster) => this._cluster = cluster;

  public async Task CreateAsync(bool includeReporting)
  {
    TransactionCouchViewsCreator couchViewsCreator = this;
    string designDoc = await couchViewsCreator.GetType().Assembly.ReadResourceAsync("transaction.json");
    using (IBucket bucket = couchViewsCreator._cluster.OpenDefaultBucket())
    {
      using (IBucketManager manager = bucket.CreateManager())
      {
        IResult result = await manager.UpdateDesignDocumentAsync("transaction", designDoc);
        if (!result.Success)
          throw result.Exception ?? new Exception(result.Message);
      }
    }
  }
}
