// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Couch.Common.ICouchCluster
// Assembly: Payhas.Binyat.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.Couch.dll

using Couchbase.Core;

#nullable disable
namespace Payhas.Binyat.Core.Couch.Common;

public interface ICouchCluster
{
  ICluster Cluster { get; }

  string Url { get; }

  string DefaultBucket { get; }

  string Username { get; }

  string Password { get; }

  void Initialize(string url, string defaultBucket, string username, string password);

  IBucket OpenDefaultBucket();
}
