// Decompiled with JetBrains decompiler
// Type: Payhas.Data.Synchronizer.Core.Couch.Common.ICouchCluster
// Assembly: Payhas.Data.Synchronizer.Core.Couch, Version=0.0.9.0, Culture=neutral, PublicKeyToken=null
// MVID: 7309E162-8E25-4800-97C2-B3CD230F4B8B
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Data.Synchronizer.Core.Couch.dll

using Couchbase.Core;

#nullable disable
namespace Payhas.Data.Synchronizer.Core.Couch.Common;

public interface ICouchCluster
{
  ICluster Cluster { get; }

  string Url { get; }

  string Bucket { get; }

  string Username { get; }

  string Password { get; }

  IBucket OpenDefaultBucket();

  void Initialize(string url, string bucket, string username, string password);
}
