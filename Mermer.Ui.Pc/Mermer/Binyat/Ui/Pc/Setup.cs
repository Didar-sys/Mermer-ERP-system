using Autofac;
using Autofac.Extras.MvvmCross;
using Microsoft.Extensions.Configuration;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.IoC;
using MvvmCross.Platform.Platform;
using MvvmCross.Wpf.Platform;
using MvvmCross.Wpf.Views;
using MvvmCross.Wpf.Views.Presenters;
using Mermer.Common.Settings;
using Mermer.Ui.Core;
using Mermer.Ui.Pc.Reports.Helpers;
using Mermer.Ui.Pc.Services;
using Mermer.Licensing.Client;
using Mermer.Licensing.Client.Models;
using Mermer.Mvvm.Tools;
using Mermer.Services;
using Mermer.Ui.Core.Pc.Tools;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Threading;



// Правильні простори імен для старих модулів
using Payhas.Binyat.Core.Couch;

namespace Mermer.Ui.Pc;

public class Setup : MvxWpfSetup
{
    private readonly IConfigurator _configurator;

    public Setup(Dispatcher dispatcher, IMvxWpfViewPresenter presenter)
        : base(dispatcher, presenter)
    {
        _configurator = new RegistryConfigurator("Software\\MermerCS\\Binyat");
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .Build();
    }

    public IConfigurationRoot Configuration { get; set; }

    protected override IMvxIoCProvider CreateIocProvider()
    {
        ContainerBuilder builder = new ContainerBuilder();
        Assembly assembly = typeof(Setup).Assembly;

        builder.Register<IConfigurator>(x => _configurator).As<IConfigurator>().SingleInstance();

        ConnectionSettings config = _configurator.GetConfig<ConnectionSettings>();
        if (config.IsDirectModeSelected)
            builder.RegisterModule(new BinyatCouchModule(config.DatabaseAddress, config.DatabaseName, config.DatabaseUser, config.DatabasePassword));

        builder.RegisterModule<CoreUiModule>();
        builder.RegisterModule(new MermerLicensingClientModule(new ActivationConfiguration
        {
            ActivationUrl = Configuration["ActivationUrl"],
            PublicKey = Configuration.GetSection("PublicKey").AsString()
        }));

        builder.RegisterInstance<PcJsonLocalizationResourceProvider>(new PcJsonLocalizationResourceProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Localizations"))).As<IJsonLocalizationResourceProvider>().SingleInstance();
        builder.RegisterAssemblyTypes(GetType().Assembly).Where(t => t.Name.EndsWith("Service")).AsImplementedInterfaces();
        builder.RegisterType<NameHelper>().AsSelf().InstancePerDependency();
        builder.RegisterAssemblyTypes(assembly).Where(x => x.Name.EndsWith("Mapper")).AsSelf().InstancePerDependency();

        builder.RegisterModule<AutoMapperModule>();

        // Явне приведення типів, яке вирішує помилку CS0266
        return (IMvxIoCProvider)new AutofacMvxIocProvider(builder.Build());
    }

    public override void Initialize()
    {
        base.Initialize();
        string name = _configurator.GetConfig<AppSettings>()?.Culture ?? "tk-TM";
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(name);
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(name);
        Thread.CurrentThread.CurrentCulture = new CultureInfo(name);
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(name);
    }

    protected override IMvxWpfViewsContainer CreateWpfViewsContainer() => new ViewsContainer();

    protected override IMvxApplication CreateApp() => new Mermer.Ui.Core.App();

    protected override IMvxTrace CreateDebugTrace() => new DebugTrace();
}