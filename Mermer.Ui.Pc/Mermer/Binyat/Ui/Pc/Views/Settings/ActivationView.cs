// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Views.Settings.ActivationView
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using DevExpress.Xpf.Editors;
using MvvmCross.Wpf.Views;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;

#nullable disable
namespace Mermer.Ui.Pc.Views.Settings;

public class ActivationView : MvxWpfView, IComponentConnector
{
  internal TextEdit ClientNote;
  internal TextEdit ServerNote;
  private bool _contentLoaded;

  public ActivationView() => this.InitializeComponent();

  private void ActivationView_OnLoaded(object sender, RoutedEventArgs e)
  {
    this.ClientNote.Text = this.ServerNote.Text = $"{Environment.MachineName} - {Environment.UserName}";
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/Mermer.Ui.Pc;component/views/settings/activationview.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.ActivationView_OnLoaded);
        break;
      case 2:
        this.ClientNote = (TextEdit) target;
        break;
      case 3:
        this.ServerNote = (TextEdit) target;
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
