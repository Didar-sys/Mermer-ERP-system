// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Common.CouchModule
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Autofac;
using Mermer.Data.Storage;

#nullable disable
namespace Mermer.Core.Couch.Common;

public class CouchModule : Module
{
  private readonly string _url;
  private readonly string _bucket;
  private readonly string _username;
  private readonly string _password;

  public CouchModule(string url, string bucket, string username, string password)
  {
    this._url = url;
    this._bucket = bucket;
    this._username = username;
    this._password = password;
  }

  protected override void Load(ContainerBuilder builder)
  {
    base.Load(builder);
    CouchCluster instance = new CouchCluster();
    instance.Initialize(this._url, this._bucket, this._username, this._password);
    builder.RegisterInstance<CouchCluster>(instance).As<ICouchCluster>().SingleInstance();
    builder.RegisterGeneric(typeof (CouchReadOnlyRepository<>)).As(typeof (IReadOnlyRepository<>)).InstancePerLifetimeScope();
    builder.RegisterGeneric(typeof (CouchRepository<>)).As(typeof (IRepository<>)).InstancePerLifetimeScope();
    builder.RegisterGeneric(typeof (CouchRepositoryWithFacet<>)).As(typeof (IRepositoryWithFacets<>)).InstancePerLifetimeScope();
  }
}
