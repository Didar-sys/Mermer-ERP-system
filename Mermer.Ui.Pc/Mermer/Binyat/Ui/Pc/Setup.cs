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

// Правильні простори імен для старих модулів
using Mermer.Core.Couch;

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
       

        builder.RegisterModule<CoreUiModule>();
        // Додаємо ?? "http://localhost:5000", щоб програма не падала через відсутність URL
        builder.RegisterModule(new MermerLicensingClientModule(new ActivationConfiguration
        {
            ActivationUrl = Configuration["ActivationUrl"] ?? "http://localhost:5000",
            PublicKey = Configuration.GetSection("PublicKey").AsString() ?? "dummy_key"
        }));

        // --- ДОДАЄМО ГЛОБАЛЬНИЙ HTTP КЛІЄНТ ---
        // Завдяки цьому всі сервіси (зокрема і авторизація) знатимуть, куди відправляти запити
        builder.Register(c =>
        {
            var client = new System.Net.Http.HttpClient();
            var apiUrl = Configuration["ApiUrl"] ?? "http://localhost:5000";
            client.BaseAddress = new Uri(apiUrl);
            return client;
        }).AsSelf().SingleInstance();

        // --- СУПЕР-УНІВЕРСАЛЬНИЙ СКАНЕР ВСІХ МОДУЛІВ MERMER ---
        var mermerAssemblies = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "Mermer*.dll")
            .Select(Assembly.LoadFrom)
            .ToArray();

        builder.RegisterAssemblyTypes(mermerAssemblies)
            .Where(t => t.IsClass && !t.IsAbstract
                        && t.Name != "PcJsonLocalizationResourceProvider"
                        && t.Name != "App"
                        && !t.Name.EndsWith("ViewModel")
                        && !t.Name.EndsWith("View")
                        && !t.Name.EndsWith("Setup")
                        && !IsMvxSingletonDeep(t)) // <-- Використовуємо наш новий глибокий захист!
            .AsImplementedInterfaces()
            .InstancePerDependency();

        // Реєструємо головний модуль бізнес-логіки (ти показував його раніше)
        builder.RegisterModule<Mermer.BinyatModule>();

        // Реєструємо менеджер мов для FluentValidation (ОБОВ'ЯЗКОВО!)
        builder.RegisterType<FluentValidation.Resources.LanguageManager>()
               .As<FluentValidation.Resources.ILanguageManager>()
               .SingleInstance();

        builder.RegisterAssemblyTypes(GetType().Assembly).Where(t => t.Name.EndsWith("Service")).AsImplementedInterfaces();
        builder.RegisterType<NameHelper>().AsSelf().InstancePerDependency();
        builder.RegisterAssemblyTypes(assembly).Where(x => x.Name.EndsWith("Mapper")).AsSelf().InstancePerDependency();

        builder.RegisterModule<AutoMapperModule>();

        // !!! ХАК ДЛЯ "ПРИВИДІВ" ЗІ СТАРОЇ БАЗИ ДАНИХ !!!
        builder.RegisterSource(new OldLocalizationSource());


        // Тепер ми ігноруємо збережений кеш і завжди підключаємося до чистої бази
        builder.RegisterModule(new BinyatCouchModule(
            "http://localhost:8091",
            "binyat",
            "binyat", // Наш новий користувач з правами Full Admin
            "Password123!"
        ));

        var container = builder.Build();

        // --- МЕТОД "КУВАЛДА": ВБИВАЄМО ВИПАДКОВИЙ КОНТЕЙНЕР ---
        // Якщо якийсь модуль під час реєстрації випадково смикнув Mvx.Resolve,
        // MvvmCross створив свій дефолтний контейнер, який зараз заблокує нам Autofac.
        var existingIoC = MvvmCross.Platform.Core.MvxSingleton<IMvxIoCProvider>.Instance;
        if (existingIoC != null)
        {
            // Коректно знищуємо випадковий синглтон
            if (existingIoC is IDisposable disposableIoc)
            {
                disposableIoc.Dispose();
            }

            // Якщо він опирається — знищуємо через рефлексію
            var field = typeof(MvvmCross.Platform.Core.MvxSingleton<IMvxIoCProvider>)
                .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            if (field != null) field.SetValue(null, null);
        }

        // Тепер шлях вільний! Реєструємо Autofac
        return (IMvxIoCProvider)new AutofacMvxIocProvider(container);
    }



    // Глибока перевірка всього дерева успадкування
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
        // Якщо логер вже ініціалізований — просто виходимо, нічого не роблячи.
        // Це врятує нас від помилки "MvxTrace already initialized".
        if (MvvmCross.Platform.Platform.MvxTrace.Instance != null)
        {
            return;
        }

        // Якщо він порожній — дозволяємо MvvmCross створити його штатно.
        base.InitializeDebugServices();
    }

    public override void Initialize()
    {
        base.Initialize(); // Базова ініціалізація MvvmCross

        // 1. Задаємо мову за замовчуванням
        string cultureName = "ru-RU";
        string shortLocale = "ru";

        // 2. Намагаємося дістати збережену мову з налаштувань
        try
        {
            var configurator = MvvmCross.Platform.Mvx.Resolve<Mermer.Services.IConfigurator>();
            var config = configurator.GetConfig<Mermer.Common.Settings.AppSettings>();

            if (config != null && !string.IsNullOrEmpty(config.Culture))
            {
                cultureName = config.Culture;
                // Витягуємо перші 2 літери ("ru", "en", "tm") і переводимо в нижній регістр
                shortLocale = cultureName.Length >= 2 ? cultureName.Substring(0, 2).ToLowerInvariant() : "en";
            }
        }
        catch
        {
            // Якщо файлу конфігурації ще немає, залишається дефолтна мова
        }

        // 3. БЕЗПЕЧНЕ встановлення системної культури (для дат і чисел)
        System.Globalization.CultureInfo culture;
        try
        {
            // В .NET туркменська має код "tk-TM". 
            // Якщо в налаштуваннях зберігся нестандартний "tm-TM" або "tm", створюємо культуру в try-catch
            culture = new System.Globalization.CultureInfo(cultureName);
        }
        catch
        {
            // Якщо система не розпізнала код культури, безпечно відкочуємось на англійську або російську
            culture = new System.Globalization.CultureInfo("ru-RU");
        }

        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
        System.Threading.Thread.CurrentThread.CurrentCulture = culture;
        System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

        // 4. Ініціалізуємо кастомний LocalizationManager
        string locPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Localization");
        if (System.IO.Directory.Exists(locPath))
        {
            // ВАЖЛИВО: Default і Fallback тепер ЗАВЖДИ "en" (англійська)
            Mermer.Mvvm.Tools.LocalizationManager.Instance.Initialize(locPath, "en", "en");

            // Встановлюємо поточну мову динамічно ("ru", "en", "tm")
            Mermer.Mvvm.Tools.LocalizationManager.Instance.CurrentLocale = shortLocale;
        }
    }

    protected override void InitializeSingletonCache()
    {
        try
        {
            // Намагаємося створити кеш офіційно
            base.InitializeSingletonCache();
        }
        catch (MvvmCross.Platform.Exceptions.MvxException)
        {
            // Якщо вікно MainWindow вже встигло створити кеш біндінгів до нас —
            // ми просто проковтуємо цю помилку і дозволяємо завантаженню йти далі!
        }
    }

    protected override IMvxWpfViewsContainer CreateWpfViewsContainer() => new ViewsContainer();

    protected override IMvxApplication CreateApp() => new Mermer.Ui.Core.App();

    protected override IMvxTrace CreateDebugTrace() => new DebugTrace();
}

// --- КЛАСИ ДЛЯ ФЕЙКОВОГО СЕРВІСУ ---

public class OldLocalizationSource : IRegistrationSource
{
    public bool IsAdapterForIndividualComponents => false;

    // Зверни увагу на змінений тип: IEnumerable<IComponentRegistration> замість IEnumerable<ServiceRegistration>
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