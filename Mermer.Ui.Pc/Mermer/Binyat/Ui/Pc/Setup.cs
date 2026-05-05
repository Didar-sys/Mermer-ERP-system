// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Setup
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Extras.MvvmCross;
using Autofac.Features.Scanning;
using Microsoft.Extensions.Configuration;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.IoC;
using MvvmCross.Platform.Platform;
using MvvmCross.Wpf.Platform;
using MvvmCross.Wpf.Views;
using MvvmCross.Wpf.Views.Presenters;
using Mermer.Common.Settings;
using Mermer.Core.Couch;
using Mermer.Ui.Core;
using Mermer.Ui.Pc.Reports.Helpers;
using Mermer.Ui.Pc.Services;
using Mermer.Data.Mapping;
using Mermer.Licensing.Client;
using Mermer.Licensing.Client.Models;
using Mermer.Mvvm.Tools;
using Mermer.Services;
using Mermer.Ui.Core.Pc.Tools;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;

#nullable disable
namespace Mermer.Ui.Pc;

public class Setup : MvxWpfSetup
{
  private readonly IConfigurator _configurator;

  public Setup(Dispatcher dispatcher, IMvxWpfViewPresenter presenter)
    : base(dispatcher, presenter)
  {
    this._configurator = (IConfigurator) new RegistryConfigurator("Software\\MermerCS\\Binyat");
    this.Configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", true, true).Build();
  }

  public IConfigurationRoot Configuration { get; set; }

  protected override IMvxIoCProvider CreateIocProvider()
  {
    ContainerBuilder builder = new ContainerBuilder();
    Assembly assembly = typeof (Setup).Assembly;
    builder.Register<IConfigurator>((Func<IComponentContext, IConfigurator>) (x => this._configurator)).As<IConfigurator>().SingleInstance();
    ConnectionSettings config = this._configurator.GetConfig<ConnectionSettings>();
    if (config.IsDirectModeSelected)
      builder.RegisterModule((IModule) new BinyatCouchModule(config.DatabaseAddress, config.DatabaseName, config.DatabaseUser, config.DatabasePassword));
    builder.RegisterModule<CoreUiModule>();
    builder.RegisterModule((IModule) new MermerLicensingClientModule(new ActivationConfiguration()
    {
      ActivationUrl = this.Configuration["ActivationUrl"],
      PublicKey = this.Configuration.GetSection("PublicKey").AsString()
    }));
    builder.RegisterInstance<PcJsonLocalizationResourceProvider>(new PcJsonLocalizationResourceProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Localizations"))).As<IJsonLocalizationResourceProvider>().SingleInstance();
    builder.RegisterAssemblyTypes(this.GetType().Assembly).Where<object, ScanningActivatorData, DynamicRegistrationStyle>((Func<Type, bool>) (t => t.Name.EndsWith("Service"))).AsImplementedInterfaces<object>();
    builder.RegisterType<NameHelper>().AsSelf<NameHelper, ConcreteReflectionActivatorData>().InstancePerDependency();
    builder.RegisterAssemblyTypes(assembly).Where<object, ScanningActivatorData, DynamicRegistrationStyle>((Func<Type, bool>) (x => x.Name.EndsWith("Mapper"))).AsSelf<object>().InstancePerDependency();
    builder.RegisterModule<AutoMapperModule>();
    return (IMvxIoCProvider) new AutofacMvxIocProvider(builder.Build());
  }

  public override void Initialize()
  {
    base.Initialize();
    string name = this._configurator.GetConfig<AppSettings>()?.Culture ?? "tk-TM";
    CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(name);
    CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(name);
    Thread.CurrentThread.CurrentCulture = new CultureInfo(name);
    Thread.CurrentThread.CurrentUICulture = new CultureInfo(name);
  }

  protected override IMvxWpfViewsContainer CreateWpfViewsContainer()
  {
    return (IMvxWpfViewsContainer) new ViewsContainer();
  }

  protected override IMvxApplication CreateApp() => (IMvxApplication) new Mermer.Ui.Core.App();

  protected override IMvxTrace CreateDebugTrace() => (IMvxTrace) new DebugTrace();
}
