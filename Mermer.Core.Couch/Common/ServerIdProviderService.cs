// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Common.ServerIdProviderService
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Newtonsoft.Json;
using Mermer.Core.Couch.StockManagement;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Common;

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
            // --- БЕЗПЕЧНЕ ФОРМУВАННЯ АДРЕСИ COUCHBASE ---
            string safeUrl = string.IsNullOrWhiteSpace(this._cluster.Url) ? "127.0.0.1" : this._cluster.Url;

            // Якщо адреса не має http://, додаємо його і стандартний порт Couchbase (8091)
            if (!safeUrl.StartsWith("http"))
            {
                safeUrl = safeUrl.Contains(":") ? $"http://{safeUrl}" : $"http://{safeUrl}:8091";
            }

            string bucketName = string.IsNullOrWhiteSpace(this._cluster.DefaultBucket) ? "default" : this._cluster.DefaultBucket;
            string requestUri = $"{safeUrl}/pools/default/buckets/{bucketName}";

            // Виконуємо запит за гарантовано правильною адресою
            var response = await client.GetAsync(requestUri);
            var contentString = await response.Content.ReadAsStringAsync();

            var bucketInfo = JsonConvert.DeserializeObject<BucketInfo>(contentString);
            uuid = bucketInfo?.Uuid;
        }
    return uuid;
  }
}
