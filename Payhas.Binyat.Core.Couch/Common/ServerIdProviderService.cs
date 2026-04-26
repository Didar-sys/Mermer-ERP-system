// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Couch.Common.ServerIdProviderService
// Assembly: Payhas.Binyat.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.Couch.dll

using Newtonsoft.Json;
using Payhas.Binyat.Core.Couch.StockManagement;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Core.Couch.Common;

public class ServerIdProviderService : IServerIdProviderService
{
  private readonly ICouchCluster _cluster;

  public ServerIdProviderService(ICouchCluster cluster) => this._cluster = cluster;

  public async Task<string> GetUniqueIdAsync()
  {
    string uuid;
    using (HttpClient client = new HttpClient())
    {
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{this._cluster.Username}:{this._cluster.Password}")));
      uuid = JsonConvert.DeserializeObject<BucketInfo>(await (await client.GetAsync($"{this._cluster.Url}/pools/default/buckets/{this._cluster.DefaultBucket}")).Content.ReadAsStringAsync()).Uuid;
    }
    return uuid;
  }
}
