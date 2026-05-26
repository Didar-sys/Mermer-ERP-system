// Decompiled with JetBrains decompiler
// Type: Mermer.Core.BinyatCoreModule
// Assembly: Mermer.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2D3AEFA2-C249-4F1B-A81D-5B4AA93CB026
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.dll

using Autofac;
using Autofac.Builder;
using Autofac.Features.Scanning;
using System;
using System.Reflection;

#nullable disable
namespace Mermer.Core;

public class BinyatCoreModule : BinyatModule
{
  protected override void Load(ContainerBuilder builder)
  {
    base.Load(builder);
    Assembly assembly = typeof (BinyatCoreModule).GetTypeInfo().Assembly;
    builder.RegisterAssemblyTypes(assembly).Where<object, ScanningActivatorData, DynamicRegistrationStyle>((Func<Type, bool>) (t => t.Name.EndsWith("Repository"))).AsSelf<object>().AsImplementedInterfaces<object>().InstancePerLifetimeScope();
  }
}
