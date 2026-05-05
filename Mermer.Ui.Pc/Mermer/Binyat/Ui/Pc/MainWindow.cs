// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.MainWindow
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using DevExpress.Xpf.Core;
using DevExpress.Xpf.WindowsUI;
using MvvmCross.Localization;
using MvvmCross.Platform;
using Mermer.Common.Settings;
using Mermer.Ui.Pc.Helpers;
using Mermer.Ui.Pc.Services;
using Mermer.Services;
using Squirrel;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

#nullable disable
namespace Mermer.Ui.Pc;

public class MainWindow : DXTabbedWindow, IComponentConnector
{
  internal ContentControl ViewsRoot;
  private bool _contentLoaded;

  public static MainWindow Instance { get; private set; }

  public MainWindow()
  {
    this.InitializeComponent();
    MainWindow.Instance = this;
    GridExtender.MainForm = (Window) this;
    ShortcutExtender.MainForm = (Window) this;
  }

  public ContentControl Root => this.ViewsRoot;

  private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
  {
    if (!Mvx.Resolve<IConfigurator>().GetConfig<UpdateSettings>().CheckForUpdates)
      return;
    await this.CheckForUpdates(false);
  }

  public async Task CheckForUpdates(bool report = true)
  {
    MainWindow mainWindow = this;
    try
    {
      MvxLanguageBinder localizer = new MvxLanguageBinder(mainWindow.GetType());
      bool? nullable1 = await AppUpdaterService.CheckForUpdatesAsync();
      if (!nullable1.HasValue)
      {
        if (!report)
          return;
        mainWindow.ShowMessage(localizer.GetText("Error Checking For Updates"), localizer.GetText("An error ocured while checking for updates, try again later!"));
      }
      else
      {
        if (nullable1.Value)
        {
          bool? nullable2 = mainWindow.ShowMessage(localizer.GetText("Update Available"), localizer.GetText($"An updated version of this application is available.{Environment.NewLine}Would you like to update now?{Environment.NewLine}{Environment.NewLine}New Version: {{0}}", new object[1]
          {
            (object) AppUpdaterService.UpdateVersion
          }), MessageBoxButton.YesNo);
          if (nullable2.HasValue && nullable2.Value)
          {
            ReleaseEntry releaseEntry = await AppUpdaterService.UpdateAsync();
            mainWindow.ShowMessage(localizer.GetText("Application Updated"), localizer.GetText($"Application has been updated to version: {{0}}{Environment.NewLine}You must restart application for changes to take effect.", new object[1]
            {
              (object) releaseEntry.Version
            }));
          }
        }
        else if (report)
          mainWindow.ShowMessage(localizer.GetText("No Updates Available"), localizer.GetText("No updates found, Application is up to date!"));
        localizer = (MvxLanguageBinder) null;
      }
    }
    catch (Exception ex)
    {
    }
  }

  private bool? ShowMessage(string caption, string message, MessageBoxButton button = MessageBoxButton.OK)
  {
    StackPanel stackPanel = new StackPanel()
    {
      Orientation = Orientation.Horizontal
    };
    stackPanel.Children.Add((UIElement) new TextBlock()
    {
      Text = message
    });
    WinUIDialogWindow winUiDialogWindow = new WinUIDialogWindow(caption, button);
    winUiDialogWindow.Content = (object) stackPanel;
    ThemeManager.SetThemeName((DependencyObject) winUiDialogWindow, "Office2013DarkGray;Touch");
    return winUiDialogWindow.ShowDialog();
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/Mermer.Ui.Pc;component/mainwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    if (connectionId != 1)
    {
      if (connectionId == 2)
        this.ViewsRoot = (ContentControl) target;
      else
        this._contentLoaded = true;
    }
    else
      ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.MainWindow_OnLoaded);
  }
}
