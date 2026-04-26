// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Couch.Authentication.AuthCouchIndexCreator
// Assembly: Payhas.Binyat.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.Couch.dll

using Couchbase.Core;
using Couchbase.N1QL;
using Payhas.Binyat.Core.Couch.Common;
using Payhas.Data.Storage;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Core.Couch.Authentication;

public class AuthCouchIndexCreator : IInitialSchemaCreator
{
  private readonly ICouchCluster _cluster;

  public AuthCouchIndexCreator(ICouchCluster cluster) => this._cluster = cluster;

  public async Task CreateAsync(bool includeReporting)
  {
    using (IBucket bucket = this._cluster.OpenDefaultBucket())
    {
      IQueryResult<object> queryResult = await bucket.QueryAsync<object>($"DROP INDEX `{this._cluster.DefaultBucket}`.`ix_user`");
    }
  }
}
