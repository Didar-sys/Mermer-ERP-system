// Decompiled with JetBrains decompiler
// Type: Payhas.Data.Mapping.AutoMapperModule
// Assembly: Payhas.Data.Mapping, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 889153DE-AB84-4851-920D-41C1F1DE9C45
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Data.Mapping.dll

using Autofac;
using Autofac.Builder;
using AutoMapper;
using System;
using System.Collections.Generic;

#nullable disable
namespace Payhas.Data.Mapping;

public class AutoMapperModule : Module
{
  protected override void Load(ContainerBuilder builder)
  {
    builder.Register<MapperConfiguration>((Func<IComponentContext, MapperConfiguration>) (c => new MapperConfiguration((Action<IMapperConfigurationExpression>) (cfg =>
    {
      foreach (Profile profile in c.Resolve<IEnumerable<Profile>>())
        cfg.AddProfile(profile);
    })))).AsSelf<MapperConfiguration, SimpleActivatorData>().SingleInstance();
    builder.Register<IMapper>((Func<IComponentContext, IMapper>) (c => c.Resolve<MapperConfiguration>().CreateMapper(new Func<Type, object>(((ResolutionExtensions) c).Resolve)))).As<IMapper>().InstancePerLifetimeScope();
  }
}
