// Decompiled with JetBrains decompiler
// Type: Mermer.BinyatModule
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Autofac;
using Autofac.Builder;
using Autofac.Features.Scanning;
using FluentValidation.Resources;
using Mermer.Authorization.Services;
using Mermer.Common.Models.Validators;
using Mermer.Common.Services;
using Mermer.Data.Authorizers;
using System;
using System.Reflection;

#nullable disable
namespace Mermer;

public class BinyatModule : Autofac.Module
{
  protected override void Load(ContainerBuilder builder)
  {
    base.Load(builder);
    Assembly assembly = typeof (BinyatModule).GetTypeInfo().Assembly;
    builder.RegisterType<ValidationLanguageManager>().As<ILanguageManager>();
    builder.RegisterType<ValidatorOptionsSetter>().As<IStartable>();
    builder.RegisterAssemblyTypes(assembly).Where<object, ScanningActivatorData, DynamicRegistrationStyle>((Func<Type, bool>) (t => t.Name.EndsWith("Validator"))).AsSelf<object>().AsImplementedInterfaces<object>().SingleInstance();
    builder.RegisterType<DefaultAuthorizer>().WithParameter<DefaultAuthorizer, ConcreteReflectionActivatorData, SingleRegistrationStyle>("defaultAction", (object) false).As<IAuthorizer>().InstancePerLifetimeScope();
    builder.RegisterGeneric(typeof (DefaultListAuthorizer<>)).WithParameter<object, ReflectionActivatorData, DynamicRegistrationStyle>("defaultAction", (object) false).As(typeof (IListAuthorizer<>)).As(typeof (IReadOnlyListAuthorizer<>)).InstancePerLifetimeScope();
    builder.RegisterAssemblyTypes(assembly).Where<object, ScanningActivatorData, DynamicRegistrationStyle>((Func<Type, bool>) (t => t.Name.EndsWith("Authorizer"))).AsSelf<object>().AsImplementedInterfaces<object>().SingleInstance();
    builder.RegisterType<AuthorizationService>().As<IAuthorizationService>().InstancePerLifetimeScope();
    builder.RegisterType<TransliterationService>().As<ITransliterationService>();
  }
}
