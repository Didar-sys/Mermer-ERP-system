// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Views.Settings.ReportsListView
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using MvvmCross.Wpf.Views;
using Payhas.Binyat.Ui.Pc.Controls;
using Payhas.Binyat.Ui.Pc.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Views.Settings;

public class ReportsListView : MvxWpfView, IComponentConnector
{
  internal StyledGridSearchControl SearchControl;
  internal StyledGridControl GridControl;
  private bool _contentLoaded;

  public ReportsListView() => this.InitializeComponent();

  private ReportsListViewModel Vm => this.ViewModel as ReportsListViewModel;

  private void OnEditReportClick(object sender, RoutedEventArgs e)
  {
    if (this.Vm.SelectedItem == null)
      return;
    new ReportDesigner(this.Vm.SelectedItem.Value).Show();
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/Payhas.Binyat.Ui.Pc;component/views/settings/reportslistview.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  internal Delegate _CreateDelegate(Type delegateType, string handler)
  {
    return Delegate.CreateDelegate(delegateType, (object) this, handler);
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    if (connectionId != 1)
    {
      if (connectionId == 2)
        this.GridControl = (StyledGridControl) target;
      else
        this._contentLoaded = true;
    }
    else
      this.SearchControl = (StyledGridSearchControl) target;
  }
}
