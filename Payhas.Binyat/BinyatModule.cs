// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.BinyatModule
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Autofac;
using Autofac.Builder;
using Autofac.Features.Scanning;
using FluentValidation.Resources;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Common.Models.Validators;
using Payhas.Binyat.Common.Services;
using Payhas.Data.Authorizers;
using System;
using System.Reflection;

#nullable disable
namespace Payhas.Binyat;

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
