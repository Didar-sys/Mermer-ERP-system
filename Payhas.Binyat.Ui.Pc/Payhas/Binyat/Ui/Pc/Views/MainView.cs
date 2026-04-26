// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Views.MainView
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using DevExpress.Xpf.Core;
using DevExpress.Xpf.WindowsUI;
using MvvmCross.Core.Navigation;
using MvvmCross.Platform;
using MvvmCross.Wpf.Views;
using Payhas.Binyat.Ui.Core.ViewModels;
using Payhas.Binyat.Ui.Pc.Reports;
using Payhas.Binyat.Ui.Pc.ViewModels;
using Payhas.Mvvm.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Views;

public class MainView : MvxWpfView, IComponentConnector, IStyleConnector
{
  private HamburgerMenu _menu;
  internal DXTabControl TabControl;
  private bool _contentLoaded;

  public MainView() => this.InitializeComponent();

  private void VersionTextLoaded(object sender, RoutedEventArgs e)
  {
    if (!(sender is TextBlock textBlock))
      return;
    FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
    textBlock.Text = "|  " + versionInfo.FileVersion;
  }

  private async void MvxWpfView_Loaded(object sender, RoutedEventArgs e)
  {
    MainView mainView = this;
    MainViewPresenter.SetTabControl(mainView.TabControl);
    if (mainView.DataContext is MainViewModel dataContext && dataContext.OpenPosOnLoad)
      dataContext.ShowPosCommand.Execute((object) null);
    await mainView.CreateDumbReport().ConfigureAwait(false);
  }

  private Task CreateDumbReport()
  {
    return Task.Run((Action) (() =>
    {
      ReportStandard reportStandard = new ReportStandard();
      reportStandard.CreateDocument();
      reportStandard.PrintingSystem.Document.AutoFitToPagesWidth = 1;
    }));
  }

  private void TabHiding(object sender, TabControlTabHidingEventArgs e)
  {
    if (!((e.Item is FrameworkElement frameworkElement ? frameworkElement.DataContext : (object) null) is BaseViewModel dataContext))
      return;
    e.Cancel = true;
    dataContext.CloseCommand.Execute((object) null);
  }

  private void Frame_MouseDown(object sender, MouseButtonEventArgs e)
  {
    if (e.ChangedButton != MouseButton.Left)
      return;
    if (MainWindow.Instance.WindowState == WindowState.Maximized)
      MainWindow.Instance.WindowState = WindowState.Normal;
    MainWindow.Instance.DragMove();
  }

  private void Frame_MouseDoubleClick(object sender, MouseButtonEventArgs e)
  {
    if (MainWindow.Instance.WindowState == WindowState.Maximized)
    {
      MainWindow.Instance.WindowState = WindowState.Normal;
    }
    else
    {
      if (MainWindow.Instance.WindowState != WindowState.Normal)
        return;
      MainWindow.Instance.WindowState = WindowState.Maximized;
    }
  }

  private async void UpdateApplication(object sender, RoutedEventArgs e)
  {
    await MainWindow.Instance.CheckForUpdates();
  }

  private void OnMenuLoaded(object sender, RoutedEventArgs e)
  {
    this._menu = (HamburgerMenu) sender;
  }

  private void OnSubMenuClick(object sender, RoutedEventArgs e)
  {
    if (!this._menu.IsInitiallyCompact || this._menu.IsCompact)
      return;
    this._menu.ViewState = this._menu.ViewState == HamburgerMenuViewState.Overlay ? HamburgerMenuViewState.CompactOverlay : HamburgerMenuViewState.Closed;
  }

  private void ShowPrintLayoutConfig(object sender, RoutedEventArgs e)
  {
    Mvx.IocConstruct<IMvxNavigationService>().Navigate<ReportsListViewModel>();
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/Payhas.Binyat.Ui.Pc;component/views/mainview.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    if (connectionId != 1)
    {
      if (connectionId == 3)
      {
        this.TabControl = (DXTabControl) target;
        this.TabControl.TabHiding += new TabControlTabHidingEventHandler(this.TabHiding);
      }
      else
        this._contentLoaded = true;
    }
    else
      ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.MvxWpfView_Loaded);
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IStyleConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 2:
        ((Style) target).Setters.Add((SetterBase) new EventSetter()
        {
          Event = ButtonBase.ClickEvent,
          Handler = (Delegate) new RoutedEventHandler(this.OnSubMenuClick)
        });
        break;
      case 4:
        ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.OnMenuLoaded);
        break;
      case 5:
        ((ButtonBase) target).Click += new RoutedEventHandler(this.ShowPrintLayoutConfig);
        break;
      case 6:
        ((ButtonBase) target).Click += new RoutedEventHandler(this.UpdateApplication);
        break;
      case 7:
        ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.VersionTextLoaded);
        break;
    }
  }
}
