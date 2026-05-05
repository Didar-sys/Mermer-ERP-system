// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Views.FundsManagement.FundsActionsListView
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using MvvmCross.Wpf.Views;
using Mermer.Ui.Pc.Controls;
using Mermer.Ui.Pc.Controls.MenuItems;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

#nullable disable
namespace Mermer.Ui.Pc.Views.FundsManagement;

public class FundsActionsListView : MvxWpfView, IComponentConnector
{
  internal StyledGridSearchControl SearchControl;
  internal ListPrint PrintButton;
  internal ListFilter FilterButton;
  internal ToggleButton ExpandButton;
  internal StyledGridControl GridControl;
  private bool _contentLoaded;

  public FundsActionsListView() => this.InitializeComponent();

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/Mermer.Ui.Pc;component/views/fundsmanagement/fundsactionslistview.xaml", UriKind.Relative));
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
        this.GridControl = (StyledGridControl) target;
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
