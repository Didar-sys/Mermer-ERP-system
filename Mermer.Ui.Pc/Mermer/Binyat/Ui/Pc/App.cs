using DevExpress.Xpf.Core;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Wpf.Views.Presenters;
using Mermer.Ui.Pc.Helpers;
using Mermer.Mvvm.Messages;
using System;
using System.Windows;

namespace Mermer.Ui.Pc;

// 1. Додано partial
// 2. Явно вказано System.Windows.Application, щоб уникнути конфлікту
public partial class App : System.Windows.Application
{
    private bool _setupComplete;

    private void DoSetup()
    {
        LoadMvxAssemblyResources();
        MainViewPresenter presenter = new MainViewPresenter(((MainWindow)this.MainWindow).Root);
        presenter.AddPresentationHintHandler<MvxCloseAllPresentationHint>(hint => presenter.CloseAll(hint));
        new Setup(this.Dispatcher, presenter).Initialize();
        Mvx.RegisterType<IMvxCommandHelper, MvxWpfCommandHelper>();
        Mvx.Resolve<IMvxAppStart>().Start();
        _setupComplete = true;
        ApplicationThemeHelper.ApplicationThemeName = "HybridApp";
        DXGridDataController.DisableThreadingProblemsDetection = true;
    }

    protected override void OnActivated(EventArgs e)
    {
        if (!_setupComplete)
            DoSetup();
        base.OnActivated(e);
    }

    private void LoadMvxAssemblyResources()
    {
        int num = 0;
        while (this.TryFindResource("MvxAssemblyImport" + num) != null)
            ++num;
    }

    // МИ ВИДАЛИЛИ ЗВІДСИ InitializeComponent, Main та _contentLoaded!
    // Вони автоматично згенеруються самою Visual Studio у файлі App.g.cs
}