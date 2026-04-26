// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Couch.Common.CouchCluster
// Assembly: Payhas.Binyat.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.Couch.dll

using Couchbase.Authentication;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using System;
using System.Collections.Generic;

#nullable disable
namespace Payhas.Binyat.Core.Couch.Common;

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
    this.Url = url;
    this.DefaultBucket = defaultBucket;
    this.Username = username;
    this.Password = password;
    try
    {
      this.Cluster = (ICluster) new Couchbase.Cluster(new ClientConfiguration()
      {
        Servers = new List<Uri>() { new Uri(url) },
        QueryRequestTimeout = 600000U
      });
      this.Cluster.Authenticate((IAuthenticator) new PasswordAuthenticator(username, password));
    }
    catch (Exception ex)
    {
    }
  }

  public IBucket OpenDefaultBucket() => this.Cluster.OpenBucket(this.DefaultBucket);

  public void Dispose() => this.Cluster?.Dispose();
}
