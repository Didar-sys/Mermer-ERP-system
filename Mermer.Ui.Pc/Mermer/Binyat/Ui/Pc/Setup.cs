using System;
using System.Collections.Generic;
using System.Threading;
using Autofac;
using Autofac.Core;
using Autofac.Builder;
using Autofac.Extras.MvvmCross;
using Castle.DynamicProxy;
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
using Autofac.Core.Registration;
using System.Linq;

// Правильные пространства имен для старых модулей
using Mermer.Core.Couch;
using Mermer.Authorization.Services;

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

        // Получаем настройки, которые пользователь ввел в окне Connection Settings (или из реестра)
        ConnectionSettings config = _configurator.GetConfig<ConnectionSettings>();

        builder.RegisterModule<CoreUiModule>();

        // Добавляем ?? "http://localhost:5000", чтобы программа не падала из-за отсутствия URL
        builder.RegisterModule(new MermerLicensingClientModule(new ActivationConfiguration
        {
            ActivationUrl = Configuration["ActivationUrl"] ?? "http://localhost:5000",
            PublicKey = Configuration.GetSection("PublicKey").AsString() ?? "dummy_key"
        }));

        // --- ДОБАВЛЯЕМ ГЛОБАЛЬНЫЙ HTTP КЛИЕНТ ---
        builder.Register(c =>
        {
            var client = new System.Net.Http.HttpClient();
            var apiUrl = Configuration["ApiUrl"] ?? "http://localhost:5000";
            client.BaseAddress = new Uri(apiUrl);
            return client;
        }).AsSelf().SingleInstance();

        // --- СУПЕР-УНИВЕРСАЛЬНЫЙ СКАНЕР ВСЕХ МОДУЛЕЙ MERMER ---
        var mermerAssemblies = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "Mermer*.dll")
            .Select(Assembly.LoadFrom)
            .ToArray();

        builder.RegisterAssemblyTypes(mermerAssemblies)
            .Where(t => t.IsClass && !t.IsAbstract
                        && t.Name != "PcJsonLocalizationResourceProvider"
                        && t.Name != "App"
                        && !typeof(ILoginService).IsAssignableFrom(t)
                        && !t.Name.EndsWith("ViewModel")
                        && !t.Name.EndsWith("View")
                        && !t.Name.EndsWith("Setup")
                        && !IsMvxSingletonDeep(t))
            .AsImplementedInterfaces()
            .InstancePerDependency();

        // Регистрируем главный модуль бизнес-логики
        builder.RegisterModule<Mermer.BinyatModule>();

        // Регистрируем менеджер языков для FluentValidation
        builder.RegisterType<FluentValidation.Resources.LanguageManager>()
               .As<FluentValidation.Resources.ILanguageManager>()
               .SingleInstance();

        builder.RegisterAssemblyTypes(GetType().Assembly).Where(t => t.Name.EndsWith("Service")).AsImplementedInterfaces();
        builder.RegisterType<NameHelper>().AsSelf().InstancePerDependency();
        builder.RegisterAssemblyTypes(assembly).Where(x => x.Name.EndsWith("Mapper")).AsSelf().InstancePerDependency();

        builder.RegisterModule<AutoMapperModule>();

        // !!! ХАК ДЛЯ "ПРИЗРАКОВ" ИЗ СТАРОЙ БАЗЫ ДАННЫХ !!!
        builder.RegisterSource(new OldLocalizationSource());

        // --- ДИНАМИЧЕСКАЯ ПРИВЯЗКА БАЗЫ ДАННЫХ ---
        // Теперь Autofac берет реальные данные, которые пользователь ввел в UI
        builder.RegisterModule(new BinyatCouchModule(
             config.DatabaseAddress ?? "http://localhost:8091",
             config.DatabaseName ?? "mermer",
             config.DatabaseUser ?? "admin",
             config.DatabasePassword ?? ""
        ));

        // --- АБСОЛЮТНЫЙ ФИКС ДЛЯ СЕССИИ (LOGIN SERVICE) ---
        var loginServiceType = mermerAssemblies
            .SelectMany(a => {
                try { return a.GetTypes(); }
                catch { return new Type[0]; }
            })
            .LastOrDefault(t => t.IsClass && !t.IsAbstract &&
                                typeof(ILoginService).IsAssignableFrom(t) &&
                                !t.Name.Contains("Mock"));

        if (loginServiceType != null)
        {
            builder.RegisterType(loginServiceType)
                   .As<ILoginService>()
                   .SingleInstance();
        }

        var container = builder.Build();

        // --- МЕТОД "КУВАЛДА": УБИВАЕМ СЛУЧАЙНЫЙ КОНТЕЙНЕР ---
        var existingIoC = MvvmCross.Platform.Core.MvxSingleton<IMvxIoCProvider>.Instance;
        if (existingIoC != null)
        {
            if (existingIoC is IDisposable disposableIoc)
            {
                disposableIoc.Dispose();
            }

            var field = typeof(MvvmCross.Platform.Core.MvxSingleton<IMvxIoCProvider>)
                .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            if (field != null) field.SetValue(null, null);
        }

        return (IMvxIoCProvider)new AutofacMvxIocProvider(container);
    }

    // Глубокая проверка всего дерева наследования
    private static bool IsMvxSingletonDeep(Type type)
    {
        Type current = type;
        while (current != null)
        {
            if (current.Name.Contains("MvxSingleton") || current.Name.Contains("MvxApplication"))
                return true;
            current = current.BaseType;
        }
        return false;
    }

    protected override void InitializeDebugServices()
    {
        if (MvvmCross.Platform.Platform.MvxTrace.Instance != null)
        {
            return;
        }
        base.InitializeDebugServices();
    }

    public override void Initialize()
    {
        base.Initialize(); // Базовая инициализация MvvmCross

        // 1. Задаем язык по умолчанию
        string cultureName = "ru-RU";
        string shortLocale = "ru";

        // 2. Пытаемся достать сохраненный язык из настроек
        try
        {
            var configurator = MvvmCross.Platform.Mvx.Resolve<Mermer.Services.IConfigurator>();
            var config = configurator.GetConfig<Mermer.Common.Settings.AppSettings>();

            if (config != null && !string.IsNullOrEmpty(config.Culture))
            {
                cultureName = config.Culture;
                shortLocale = cultureName.Length >= 2 ? cultureName.Substring(0, 2).ToLowerInvariant() : "en";
            }
        }
        catch
        {
            // Если файла конфигурации еще нет, остается дефолтный язык
        }

        // 3. БЕЗОПАСНАЯ установка системной культуры (для дат и чисел)
        System.Globalization.CultureInfo culture;
        try
        {
            culture = new System.Globalization.CultureInfo(cultureName);
        }
        catch
        {
            culture = new System.Globalization.CultureInfo("ru-RU");
        }

        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
        System.Threading.Thread.CurrentThread.CurrentCulture = culture;
        System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

        // 4. Инициализируем кастомный LocalizationManager
        string locPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Localization");
        if (System.IO.Directory.Exists(locPath))
        {
            Mermer.Mvvm.Tools.LocalizationManager.Instance.Initialize(locPath, "en", "en");
            Mermer.Mvvm.Tools.LocalizationManager.Instance.CurrentLocale = shortLocale;
        }
    }

    protected override void InitializeSingletonCache()
    {
        try
        {
            base.InitializeSingletonCache();
        }
        catch (MvvmCross.Platform.Exceptions.MvxException)
        {
        }
    }

    protected override IMvxWpfViewsContainer CreateWpfViewsContainer() => new ViewsContainer();

    protected override IMvxApplication CreateApp() => new Mermer.Ui.Core.App();

    protected override IMvxTrace CreateDebugTrace() => new DebugTrace();
}

// --- КЛАССЫ ДЛЯ ФЕЙКОВОГО СЕРВИСА ---

public class OldLocalizationSource : IRegistrationSource
{
    public bool IsAdapterForIndividualComponents => false;

    public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
    {
        if (service is TypedService typedService && typedService.ServiceType.FullName == "Payhas.Binyat.Common.Services.ILocalizationService")
        {
            yield return RegistrationBuilder.ForDelegate((c, p) =>
            {
                var proxyGen = new ProxyGenerator();
                return proxyGen.CreateInterfaceProxyWithoutTarget(typedService.ServiceType, new DummyInterceptor());
            }).As(typedService.ServiceType).CreateRegistration();
        }
    }
}

public class DummyInterceptor : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        if (invocation.Method.ReturnType == typeof(string))
            invocation.ReturnValue = "";
        else if (invocation.Method.ReturnType.IsValueType)
            invocation.ReturnValue = Activator.CreateInstance(invocation.Method.ReturnType);
        else
            invocation.ReturnValue = null;
    }
}