// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.BinyatCouchModule
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Scanning;
using AutoMapper;
using Mermer.Authorization.Services;
using Mermer.Common.Services;
using Mermer.Core.Couch.Authentication.Services;
using Mermer.Core.Couch.Common;
using Mermer.Data.Patcher;
using Mermer.Data.Storage;
using Mermer.Data.Synchronizer.Core.Tools;
using System;
using System.Reflection;

#nullable disable
namespace Mermer.Core.Couch;

public class BinyatCouchModule : BinyatCoreModule
{
  private readonly string _url;
  private readonly string _bucket;
  private readonly string _username;
  private readonly string _password;

  public BinyatCouchModule(string url, string bucket, string username, string password)
  {
    this._url = url;
    this._bucket = bucket;
    this._username = username;
    this._password = password;
  }

  protected override void Load(ContainerBuilder builder)
  {
    base.Load(builder);
    Assembly assembly = typeof (BinyatCouchModule).GetTypeInfo().Assembly;
    builder.RegisterModule((IModule) new CouchModule(this._url, this._bucket, this._username, this._password));
    builder.RegisterAssemblyTypes(assembly).Where<object, ScanningActivatorData, DynamicRegistrationStyle>((Func<Type, bool>) (t => t.Name.EndsWith("Service") && t.Name != "CouchLoginService")).AsImplementedInterfaces<object>();
    builder.RegisterType<CouchLoginService>().As<ILoginService>().SingleInstance();
    builder.RegisterAssemblyTypes(assembly).Where<object, ScanningActivatorData, DynamicRegistrationStyle>((Func<Type, bool>) (t => t.Name.EndsWith("Mapper"))).As<Profile>();
    builder.RegisterAssemblyTypes(assembly).Where<object, ScanningActivatorData, DynamicRegistrationStyle>((Func<Type, bool>) (t => t.Name.EndsWith("Repository"))).AsImplementedInterfaces<object>();
    builder.RegisterAssemblyTypes(assembly).Where<object, ScanningActivatorData, DynamicRegistrationStyle>((Func<Type, bool>) (t => t.Name.EndsWith("IndexCreator") || t.Name.EndsWith("ViewsCreator"))).As<IInitialSchemaCreator>().SingleInstance();
    builder.RegisterAssemblyTypes(assembly).Where<object, ScanningActivatorData, DynamicRegistrationStyle>((Func<Type, bool>) (t => t.Name.EndsWith("InitialDataCreator"))).As<IInitialDataCreator>().SingleInstance();
    builder.RegisterType<CouchDocumentChangeListener>().As<IDocumentChangeListener>().SingleInstance();
    builder.RegisterType<Mermer.Data.Patcher.Patcher>().As<IPatcher>();
    builder.RegisterType<ChangeHelper>().As<IChangeHelper>();
    Mermer.Data.Synchronizer.Core.Couch.Common.CouchCluster instance = new Mermer.Data.Synchronizer.Core.Couch.Common.CouchCluster();
    instance.Initialize(this._url, this._bucket, this._username, this._password);
    builder.RegisterInstance<Mermer.Data.Synchronizer.Core.Couch.Common.CouchCluster>(instance).As<Mermer.Data.Synchronizer.Core.Couch.Common.ICouchCluster>().SingleInstance();
  }
}
