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

    [Obsolete]
    private void DoSetup()
    {
        if (_setupComplete) return;

        LoadMvxAssemblyResources();

        if (this.MainWindow == null)
        {
            this.MainWindow = new MainWindow();
        }

        MainViewPresenter presenter = new MainViewPresenter(((MainWindow)this.MainWindow).Root);
        // ИСПРАВЛЕННЫЙ БЛОК:
        presenter.AddPresentationHintHandler<MvxCloseAllPresentationHint>(hint =>
        {
            if (hint != null)
            {
                return presenter.CloseAll(hint);
            }
            return false;
        });

        new Setup(this.Dispatcher, presenter).Initialize();

        Mvx.RegisterType<IMvxCommandHelper, MvxWpfCommandHelper>();
        Mvx.Resolve<IMvxAppStart>().Start();

        _setupComplete = true;

        // ИМЕННО ЗДЕСЬ — ЕДИНСТВЕННО ПРАВИЛЬНОЕ МЕСТО ДЛЯ ТЕМЫ:
        DevExpress.Xpf.Core.ApplicationThemeHelper.ApplicationThemeName = "HybridApp";
        // (Или попробуй "MetropolisLight", если хочешь полностью плоские стрелочки)

        DXGridDataController.DisableThreadingProblemsDetection = true;

        this.MainWindow.Show();
    }

    // МЫ ПЕРЕНЕСЛИ OnStartup СЮДА! Это единственное правильное место для старта.
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DoSetup();
    }

    // МЕТОД OnActivated УДАЛЕН ПОЛНОСТЬЮ, чтобы окно не рекурсировало при фокусе!

    private void LoadMvxAssemblyResources()
    {
        int num = 0;
        while (this.TryFindResource("MvxAssemblyImport" + num) != null)
            ++num;
    }
}