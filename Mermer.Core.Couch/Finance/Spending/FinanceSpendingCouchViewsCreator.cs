// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Finance.Spending.FinanceSpendingCouchViewsCreator
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase;
using Couchbase.Core;
using Couchbase.Management;
using Mermer.Core.Couch.Common;
using Mermer.Data.Storage;
using System.Reflection;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Finance.Spending;

public class FinanceSpendingCouchViewsCreator : IInitialSchemaCreator
{
  private readonly ICouchCluster _cluster;

  public FinanceSpendingCouchViewsCreator(ICouchCluster cluster) => this._cluster = cluster;

  public async Task CreateAsync(bool includeReporting)
  {
    FinanceSpendingCouchViewsCreator couchViewsCreator = this;
    string designDoc = await couchViewsCreator.GetType().Assembly.ReadResourceAsync("finance-spending.json");
    using (IBucket bucket = couchViewsCreator._cluster.OpenDefaultBucket())
    {
      using (IBucketManager manager = bucket.CreateManager())
      {
        IResult result = await manager.UpdateDesignDocumentAsync("finance-spending", designDoc);
        if (!result.Success)
          throw result.Exception;
      }
    }
  }
}
