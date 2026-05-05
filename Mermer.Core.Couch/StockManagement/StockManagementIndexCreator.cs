// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.StockManagement.StockManagementIndexCreator
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Mermer.Core.Couch.Common;
using Mermer.Data.Storage;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.StockManagement;

public class StockManagementIndexCreator : IInitialSchemaCreator
{
  private readonly ICouchCluster _cluster;

  public StockManagementIndexCreator(ICouchCluster cluster) => this._cluster = cluster;

  public async Task CreateAsync(bool includeReporting)
  {
    using (this._cluster.OpenDefaultBucket())
      ;
  }
}
