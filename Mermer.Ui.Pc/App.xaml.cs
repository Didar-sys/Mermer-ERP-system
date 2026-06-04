using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Wpf.Views.Presenters;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace Mermer.Ui.Pc
{
    public partial class App : Application
    {
        // Створюємо конструктор, який запуститься найпершим
        public App()
        {
            // Встановлюємо українську локаль (дасть формат ДД.ММ.РРРР та правильні роздільники сум)
            var culture = new CultureInfo("uk-UA");

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // Змушуємо WPF інтерфейс використовувати цю локаль для всіх елементів (календарі, таблиці)
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));
        }

        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    base.OnStartup(e);

        //    // Пастка для будь-яких помилок під час старту
        //    AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        //    {
        //        MessageBox.Show(args.ExceptionObject.ToString(), "КРИТИЧНА ПОМИЛКА СТАРТУ");
        //    };

        //    // 1. Створюємо головне вікно програми (оболонку)
        //    var mainWindow = new MainWindow();
        //    this.MainWindow = mainWindow;

        //    // 2. Ініціалізуємо MvvmCross та передаємо йому вікно
        //    var presenter = new MvxWpfViewPresenter(mainWindow);
        //    var setup = new Setup(Dispatcher, presenter);
        //    setup.Initialize();

        //    // 3. Запускаємо логіку (це відкриє твій MainView або LoginView)
        //    var startup = Mvx.Resolve<IMvxAppStart>();
        //    startup.Start();

        //    // 4. Показуємо вікно на екрані
        //    mainWindow.Show();
        //}
    }
}