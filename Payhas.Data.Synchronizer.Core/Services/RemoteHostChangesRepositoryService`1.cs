// Decompiled with JetBrains decompiler
// Type: Payhas.Data.Synchronizer.Core.Services.RemoteHostChangesRepositoryService`1
// Assembly: Payhas.Data.Synchronizer.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 51A01EA9-84E6-49FB-B6E1-8048825E2DB0
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Data.Synchronizer.Core.dll

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Payhas.Data.Synchronizer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Data.Synchronizer.Core.Services;

public class RemoteHostChangesRepositoryService<T> : 
  IRemoteChangesRepositoryService<T>,
  IChangesRepositoryService<T>
  where T : class
{
  private readonly HttpClient _httpClient;
  private readonly ILogger<RemoteHostChangesRepositoryService<T>> _logger;
  private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings()
  {
    DateTimeZoneHandling = DateTimeZoneHandling.Utc
  };
  private static readonly JsonSerializerSettings _deserializerSettings = new JsonSerializerSettings()
  {
    DateTimeZoneHandling = DateTimeZoneHandling.Local
  };

  public RemoteHostChangesRepositoryService(
    HttpClient httpClient,
    ILogger<RemoteHostChangesRepositoryService<T>> logger)
  {
    this._httpClient = httpClient;
    this._logger = logger;
  }

  public virtual async Task ConfigureHttpClient(string url, string username = null, string password = null)
  {
    this._httpClient.Timeout = TimeSpan.FromMinutes(10.0);
    this._httpClient.BaseAddress = new Uri(url);
    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
      return;
    this._httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (await RemoteHostChangesRepositoryService<T>.ExtractResultAsync<AuthorizedUserSession>(await this._httpClient.PostAsync("/api/authentication", (HttpContent) RemoteHostChangesRepositoryService<T>.PrepareContent((object) new AuthenticationRequest()
    {
      Id = username,
      Password = password
    })))).Token);
  }

  public virtual async Task<Dictionary<string, IEnumerable<ChangeIdsRange>>> GetChangesIndexAsync()
  {
    Dictionary<string, IEnumerable<ChangeIdsRange>> dictionary;
    try
    {
      dictionary = (await RemoteHostChangesRepositoryService<T>.ExtractResultAsync<Dictionary<string, List<ChangeIdsRange>>>(await this._httpClient.GetAsync("/api/changes/indexes"))).ToDictionary<KeyValuePair<string, List<ChangeIdsRange>>, string, IEnumerable<ChangeIdsRange>>((Func<KeyValuePair<string, List<ChangeIdsRange>>, string>) (x => x.Key), (Func<KeyValuePair<string, List<ChangeIdsRange>>, IEnumerable<ChangeIdsRange>>) (x => x.Value.AsEnumerable<ChangeIdsRange>()));
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, nameof (GetChangesIndexAsync));
      throw;
    }
    return dictionary;
  }

  public virtual async Task<IEnumerable<Change<T>>> QueryChangesByIndexAsync(
    Dictionary<string, IEnumerable<ChangeIdsRange>> changesIndex,
    int skip = 0,
    int take = 0)
  {
    IEnumerable<Change<T>> resultAsync;
    try
    {
      StringContent content = RemoteHostChangesRepositoryService<T>.PrepareContent((object) changesIndex);
      resultAsync = (IEnumerable<Change<T>>) await RemoteHostChangesRepositoryService<T>.ExtractResultAsync<List<Change<T>>>(await this._httpClient.PostAsync($"/api/changes/query?skip={skip}&take={take}", (HttpContent) content));
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, nameof (QueryChangesByIndexAsync));
      throw;
    }
    return resultAsync;
  }

  public virtual async Task StoreChangesAsync(IEnumerable<Change<T>> changes, bool apply)
  {
    try
    {
      await RemoteHostChangesRepositoryService<T>.CheckForExceptions(await this._httpClient.PostAsync("/api/changes/store", (HttpContent) RemoteHostChangesRepositoryService<T>.PrepareContent((object) changes)));
    }
    catch (Exception ex)
    {
      this._logger.LogError(ex, nameof (StoreChangesAsync));
      throw;
    }
  }

  private static StringContent PrepareContent(object model)
  {
    StringContent stringContent = new StringContent(JsonConvert.SerializeObject(model, RemoteHostChangesRepositoryService<T>._serializerSettings));
    stringContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
    return stringContent;
  }

  private static async Task<TExtract> ExtractResultAsync<TExtract>(HttpResponseMessage response)
  {
    await RemoteHostChangesRepositoryService<T>.CheckForExceptions(response);
    string str = await response.Content.ReadAsStringAsync();
    return !string.IsNullOrEmpty(str) ? JsonConvert.DeserializeObject<TExtract>(str, RemoteHostChangesRepositoryService<T>._deserializerSettings) : default (TExtract);
  }

  private static async Task CheckForExceptions(HttpResponseMessage response)
  {
    if (!response.IsSuccessStatusCode)
    {
      string message = await response.Content.ReadAsStringAsync();
      if (!string.IsNullOrEmpty(message))
        throw new Exception(message);
      throw new Exception();
    }
  }
}
