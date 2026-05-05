// Decompiled with JetBrains decompiler
// Type: Mermer.Data.Synchronizer.Core.Couch.Common.CouchCluster
// Assembly: Mermer.Data.Synchronizer.Core.Couch, Version=0.0.9.0, Culture=neutral, PublicKeyToken=null
// MVID: 7309E162-8E25-4800-97C2-B3CD230F4B8B
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Data.Synchronizer.Core.Couch.dll

using Couchbase.Authentication;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using System;
using System.Collections.Generic;

#nullable disable
namespace Mermer.Data.Synchronizer.Core.Couch.Common;

public class CouchCluster : ICouchCluster, IDisposable
{
  public string Url { get; protected set; }

  public string Bucket { get; protected set; }

  public string Username { get; protected set; }

  public string Password { get; protected set; }

  public ICluster Cluster { get; protected set; }

  public virtual void Initialize(string url, string bucket, string username, string password)
  {
    this.Url = url;
    this.Bucket = bucket;
    this.Username = username;
    this.Password = password;
    this.Cluster?.Dispose();
    try
    {
      this.Cluster = (ICluster) new Couchbase.Cluster(new ClientConfiguration()
      {
        Servers = new List<Uri>() { new Uri(this.Url) },
        QueryRequestTimeout = 600000U
      });
      this.Cluster.Authenticate((IAuthenticator) new PasswordAuthenticator(this.Username, this.Password));
    }
    catch (Exception ex)
    {
    }
  }

  public virtual IBucket OpenDefaultBucket() => this.Cluster.OpenBucket(this.Bucket);

  public virtual void Dispose() => this.Cluster?.Dispose();
}
