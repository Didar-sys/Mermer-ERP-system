// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Views.Reporting.RevenueReportView
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using DevExpress.Xpf.Core.ServerMode;
using MvvmCross.Wpf.Views;
using Payhas.Binyat.Ui.Pc.Controls;
using Payhas.Binyat.Ui.Pc.Controls.MenuItems;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Views.Reporting;

public class RevenueReportView : MvxWpfView, IComponentConnector
{
  internal StyledGridSearchControl SearchControl;
  internal ListPrint PrintButton;
  internal ListFilter FilterButton;
  internal ToggleButton ExpandButton;
  internal PLinqInstantFeedbackDataSource ParalelLinqInstantSource;
  internal StyledGridControl GridControl;
  private bool _contentLoaded;

  public RevenueReportView() => this.InitializeComponent();

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/Payhas.Binyat.Ui.Pc;component/views/reporting/revenuereportview.xaml", UriKind.Relative));
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
    switch (connectionId)
    {
      case 1:
        this.SearchControl = (StyledGridSearchControl) target;
        break;
      case 2:
        this.PrintButton = (ListPrint) target;
        break;
      case 3:
        this.FilterButton = (ListFilter) target;
        break;
      case 4:
        this.ExpandButton = (ToggleButton) target;
        break;
      case 5:
        this.ParalelLinqInstantSource = (PLinqInstantFeedbackDataSource) target;
        break;
      case 6:
        this.GridControl = (StyledGridControl) target;
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
