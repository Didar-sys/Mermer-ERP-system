// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.App
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using DevExpress.Xpf.Core;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Wpf.Views.Presenters;
using Mermer.Ui.Pc.Helpers;
using Mermer.Mvvm.Messages;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;

#nullable disable
namespace Mermer.Ui.Pc;

public class App : Application
{
  private bool _setupComplete;
  private bool _contentLoaded;

  private void DoSetup()
  {
    this.LoadMvxAssemblyResources();
    MainViewPresenter presenter = new MainViewPresenter(((Mermer.Ui.Pc.MainWindow) this.MainWindow).Root);
    presenter.AddPresentationHintHandler<MvxCloseAllPresentationHint>((Func<MvxCloseAllPresentationHint, bool>) (hint => presenter.CloseAll(hint)));
    new Setup(this.Dispatcher, (IMvxWpfViewPresenter) presenter).Initialize();
    Mvx.RegisterType<IMvxCommandHelper, MvxWpfCommandHelper>();
    Mvx.Resolve<IMvxAppStart>().Start();
    this._setupComplete = true;
    ApplicationThemeHelper.ApplicationThemeName = "HybridApp";
    DXGridDataController.DisableThreadingProblemsDetection = true;
  }

  protected override void OnActivated(EventArgs e)
  {
    if (!this._setupComplete)
      this.DoSetup();
    base.OnActivated(e);
  }

  private void LoadMvxAssemblyResources()
  {
    int num = 0;
    while (this.TryFindResource((object) ("MvxAssemblyImport" + num.ToString())) != null)
      ++num;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    this.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
    Application.LoadComponent((object) this, new Uri("/Mermer.Ui.Pc;component/app.xaml", UriKind.Relative));
  }

  [STAThread]
  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public static void Main()
  {
    App app = new App();
    app.InitializeComponent();
    app.Run();
  }
}
