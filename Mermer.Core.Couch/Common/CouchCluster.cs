// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Common.CouchCluster
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase.Authentication;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using System;
using System.Collections.Generic;

#nullable disable
namespace Mermer.Core.Couch.Common;

public class CouchCluster : ICouchCluster, IDisposable
{
  public string Url { get; private set; }

  public string DefaultBucket { get; private set; }

  public string Username { get; private set; }

  public string Password { get; private set; }

  public ICluster Cluster { get; private set; }

    public void Initialize(string url, string defaultBucket, string username, string password)
    {
        this.Cluster?.Dispose();

        // 1. Формуємо безпечну адресу (щоб не падало створення Uri)
        string safeUrl = string.IsNullOrWhiteSpace(url) ? "127.0.0.1" : url;
        if (!safeUrl.StartsWith("http"))
        {
            safeUrl = safeUrl.Contains(":") ? $"http://{safeUrl}" : $"http://{safeUrl}:8091";
        }

        // --- ЗАЛІЗОБЕТОННИЙ ХАК ДЛЯ ЛОКАЛЬНОГО ТЕСТУВАННЯ ---
        // Ігноруємо будь-які старі збережені конфіги з диска
        this.Url = "http://localhost:8091";
        this.DefaultBucket = "binyat"; // Ніяких .ymb3!
        this.Username = "binyat";
        this.Password = "Password123!";

        try
        {
            this.Cluster = (ICluster)new Couchbase.Cluster(new ClientConfiguration()
            {
                Servers = new List<Uri>() { new Uri(this.Url) },
                QueryRequestTimeout = 600000U
            });
            this.Cluster.Authenticate((IAuthenticator)new PasswordAuthenticator(this.Username, this.Password));
        }
        catch (Exception ex)
        {
            // Замість того, щоб ковтати помилку і робити Cluster = null, 
            // ми жбурляємо її наверх із детальним описом!
            throw new Exception($"КРИТИЧНА ПОМИЛКА COUCHBASE: {ex.Message} --- Деталі: {ex.InnerException?.Message}", ex);
        }
    }

    public IBucket OpenDefaultBucket()
    {
        if (this.Cluster == null)
        {
            throw new InvalidOperationException("Неможливо відкрити бакет, тому що Cluster дорівнює null (ініціалізація бази провалилася).");
        }
        return this.Cluster.OpenBucket(this.DefaultBucket);
    }

    public void Dispose() => this.Cluster?.Dispose();
}
