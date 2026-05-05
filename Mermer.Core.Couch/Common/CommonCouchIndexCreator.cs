// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Common.CommonCouchIndexCreator
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase;
using Couchbase.Core;
using Couchbase.Management;
using Mermer.Data.Storage;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Common;

public class CommonCouchIndexCreator : IInitialSchemaCreator
{
  private readonly ICouchCluster _cluster;

  public CommonCouchIndexCreator(ICouchCluster cluster) => this._cluster = cluster;

  public async Task CreateAsync(bool includeReporting)
  {
    using (IBucket bucket = this._cluster.OpenDefaultBucket())
    {
      using (IBucketManager manager = bucket.CreateManager())
      {
        IResult[] resultArray = await Task.WhenAll<IResult>(manager.CreateN1qlPrimaryIndexAsync("primary", false), manager.CreateN1qlIndexAsync("ix_code", false, new string[1]
        {
          "code"
        }), manager.CreateN1qlIndexAsync("ix_doctype", false, new string[1]
        {
          "docType"
        }), manager.CreateN1qlIndexAsync("ix_patch_id", false, new string[1]
        {
          "`patch`.`id`"
        }));
      }
    }
  }
}
