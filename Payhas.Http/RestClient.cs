// Decompiled with JetBrains decompiler
// Type: Payhas.Http.RestClient
// Assembly: Payhas.Http, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 7DF49D0A-4DE2-4BBD-B7D0-7E5326D360BD
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Http.dll

using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Http;

public class RestClient
{
  protected readonly HttpClient HttpClient;
  private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
  {
    TypeNameHandling = TypeNameHandling.Auto
  };

  public RestClient(HttpClient httpClient) => this.HttpClient = httpClient;

  public async Task<T> GetAsync<T>(string address)
  {
    return await RestClient.ExportResult<T>(await this.HttpClient.GetAsync(address));
  }

  public async Task PostAsync(string address, object model)
  {
    StringContent content = RestClient.PrepareContent(model);
    await RestClient.ExportResult(await this.HttpClient.PostAsync(address, (HttpContent) content));
  }

  public async Task<T> PostAsync<T>(string address, object model)
  {
    StringContent content = RestClient.PrepareContent(model);
    return await RestClient.ExportResult<T>(await this.HttpClient.PostAsync(address, (HttpContent) content));
  }

  public async Task PutAsync(string address, object model)
  {
    StringContent content = RestClient.PrepareContent(model);
    await RestClient.ExportResult(await this.HttpClient.PutAsync(address, (HttpContent) content));
  }

  public async Task<T> PutAsync<T>(string address, object model)
  {
    StringContent content = RestClient.PrepareContent(model);
    return await RestClient.ExportResult<T>(await this.HttpClient.PutAsync(address, (HttpContent) content));
  }

  private static async Task ExportResult(HttpResponseMessage response)
  {
    if (!response.IsSuccessStatusCode)
      throw await RestClient.ExportException(response);
  }

  private static async Task<T> ExportResult<T>(HttpResponseMessage response)
  {
    if (!response.IsSuccessStatusCode)
      throw await RestClient.ExportException(response);
    string str = await response.Content.ReadAsStringAsync();
    return !string.IsNullOrEmpty(str) ? JsonConvert.DeserializeObject<T>(str) : default (T);
  }

  private static async Task<Exception> ExportException(HttpResponseMessage response)
  {
    string str = await response.Content.ReadAsStringAsync();
    return !string.IsNullOrEmpty(str) ? JsonConvert.DeserializeObject<RestException>(str, RestClient.JsonSerializerSettings)?.ToExecption() ?? new Exception() : new Exception();
  }

  private static StringContent PrepareContent(object model)
  {
    StringContent stringContent = new StringContent(JsonConvert.SerializeObject(model, Formatting.Indented, RestClient.JsonSerializerSettings));
    stringContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
    return stringContent;
  }
}
