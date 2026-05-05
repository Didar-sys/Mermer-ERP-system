// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.CoreUiModule
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using Autofac;
using Autofac.Builder;
using Autofac.Features.Scanning;
using AutoMapper;
using Mermer.Common.Services;
using Mermer.StockManagement.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.Services;
using Mermer.Ui.Core.ViewModels.Common;
using Mermer.Mvvm.ViewModels;
using System;
using System.Reflection;

#nullable disable
namespace Mermer.Ui.Core;

public class CoreUiModule : Autofac.Module
{
  protected override void Load(ContainerBuilder builder)
  {
    base.Load(builder);
    Assembly assembly1 = typeof (CoreUiModule).GetTypeInfo().Assembly;
    builder.RegisterAssemblyTypes(assembly1).Where<object, ScanningActivatorData, DynamicRegistrationStyle>((Func<Type, bool>) (x => x.Name.EndsWith("Mapper"))).As<Profile>().SingleInstance();
    builder.RegisterAssemblyTypes(assembly1).Where<object, ScanningActivatorData, DynamicRegistrationStyle>((Func<Type, bool>) (t => t.Name.EndsWith("Service"))).AsImplementedInterfaces<object>();
    builder.RegisterType<InMemoryStockSearchService>().As<IStockSearchService>().SingleInstance();
    builder.RegisterType<CopyCreate>().AsSelf<CopyCreate, ConcreteReflectionActivatorData>().InstancePerDependency();
    builder.RegisterGeneric(typeof (ListViewModel<>)).As(typeof (ListViewModel<>)).InstancePerDependency();
    builder.RegisterGeneric(typeof (DetailsViewModel<>)).As(typeof (DetailsViewModel<>)).InstancePerDependency();
    Assembly assembly2 = typeof (App).GetTypeInfo().Assembly;
    builder.RegisterAssemblyTypes(assembly2).AsClosedTypesOf<object, ScanningActivatorData, DynamicRegistrationStyle>(typeof (ListViewModel<>)).InstancePerDependency();
    builder.RegisterAssemblyTypes(assembly2).AsClosedTypesOf<object, ScanningActivatorData, DynamicRegistrationStyle>(typeof (DetailsViewModel<>)).InstancePerDependency();
    builder.RegisterGeneric(typeof (Reference<>)).As(typeof (Reference<>)).InstancePerDependency();
    builder.RegisterType<StockSearcher>().AsSelf<StockSearcher, ConcreteReflectionActivatorData>();
    builder.RegisterType<MvxDocumentChangedNotifier>().As<IDocumentChangedNotifier>();
  }
}
