using DevExpress.Xpf.Core;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Wpf.Views.Presenters;
using Mermer.Ui.Pc.Helpers;
using Mermer.Mvvm.Messages;
using System;
using System.Windows;

namespace Mermer.Ui.Pc;

public partial class App : System.Windows.Application
{
    private bool _setupComplete;

    private void DoSetup()
    {
        if (_setupComplete) return; // Страховка: ніколи не запускати двічі

        LoadMvxAssemblyResources();

        // Гарантуємо, що вікно створене до початку ініціалізації
        if (this.MainWindow == null)
        {
            this.MainWindow = new MainWindow();
        }

        MainViewPresenter presenter = new MainViewPresenter(((MainWindow)this.MainWindow).Root);
        presenter.AddPresentationHintHandler<MvxCloseAllPresentationHint>(hint => presenter.CloseAll(hint));

        new Setup(this.Dispatcher, presenter).Initialize();

        Mvx.RegisterType<IMvxCommandHelper, MvxWpfCommandHelper>();
        Mvx.Resolve<IMvxAppStart>().Start();

        _setupComplete = true;

        ApplicationThemeHelper.ApplicationThemeName = "HybridApp";
        DXGridDataController.DisableThreadingProblemsDetection = true;

        this.MainWindow.Show(); // Відкриваємо вікно тільки коли все готово
    }

    // МИ ПЕРЕНЕСЛИ OnStartup СЮДИ! Це єдине правильне місце для старту.
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DoSetup();
    }

    // МЕТОД OnActivated ВИДАЛЕНО ПОВНІСТЮ, щоб вікно не рекурсувало при фокусі!

    private void LoadMvxAssemblyResources()
    {
        int num = 0;
        while (this.TryFindResource("MvxAssemblyImport" + num) != null)
            ++num;
    }
}