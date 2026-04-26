// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Views.LoginView
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using MvvmCross.Wpf.Views;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Views;

public class LoginView : MvxWpfView, IComponentConnector
{
  internal TextBlock VersionText;
  private bool _contentLoaded;

  public LoginView() => this.InitializeComponent();

  private void LoginView_OnLoaded(object sender, RoutedEventArgs e)
  {
    this.VersionText.Text = ": " + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/Payhas.Binyat.Ui.Pc;component/views/loginview.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    if (connectionId != 1)
    {
      if (connectionId == 2)
        this.VersionText = (TextBlock) target;
      else
        this._contentLoaded = true;
    }
    else
      ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.LoginView_OnLoaded);
  }
}
