// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Warehousing.Revisioning.RevisioningCouchIndexCreator
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase.Core;
using Couchbase.N1QL;
using Mermer.Core.Couch.Common;
using Mermer.Data.Storage;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Warehousing.Revisioning;

public class RevisioningCouchIndexCreator : IInitialSchemaCreator
{
  private readonly ICouchCluster _cluster;

  public RevisioningCouchIndexCreator(ICouchCluster cluster) => this._cluster = cluster;

  public async Task CreateAsync(bool includeReporting)
  {
    using (IBucket bucket = this._cluster.OpenDefaultBucket())
    {
      IQueryResult<object> queryResult = await bucket.QueryAsync<object>($"CREATE INDEX `ix_revision_lines` ON `{this._cluster.DefaultBucket}`(`docType`,`stockRevisionId`,`stockId`)");
    }
  }
}
