// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Views.StockManagement.StockBalancesByDateAndWarehousesListView
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using DevExpress.Xpf.Grid;
using MvvmCross.Wpf.Views;
using Mermer.StockManagement.Models;
using Mermer.Ui.Core.ViewModels.StockManagement;
using Mermer.Ui.Pc.Controls;
using Mermer.Ui.Pc.Controls.MenuItems;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

#nullable disable
namespace Mermer.Ui.Pc.Views.StockManagement;

public class StockBalancesByDateAndWarehousesListView : MvxWpfView, IComponentConnector
{
  internal StyledGridSearchControl SearchControl;
  internal ListPrint PrintButton;
  internal ListFilter FilterButton;
  internal StockLookupEdit FirstFocus;
  internal StyledGridControl GridControl;
  private bool _contentLoaded;

  public StockBalancesByDateAndWarehousesListView() => this.InitializeComponent();

  private void GridControl_CustomUnboundColumnData(object sender, GridColumnDataEventArgs e)
  {
    if (!(this.DataContext is StockBalancesByDateAndWarehousesListViewModel dataContext))
      return;
    StockBalanceByWarehouses balanceByWarehouses = dataContext.List[e.ListSourceRowIndex];
    if (!e.IsGetData)
      return;
    e.Value = (object) (balanceByWarehouses.Balances.ContainsKey(e.Column.FieldName) ? balanceByWarehouses.Balances[e.Column.FieldName] : 0M);
  }

  private void DetectShortCut(object sender, KeyEventArgs e)
  {
    if (e.Key != Key.F3)
      return;
    this.FirstFocus.Focus();
    e.Handled = true;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/Mermer.Ui.Pc;component/views/stockmanagement/stockbalancesbydateandwarehouseslistview.xaml", UriKind.Relative));
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
        ((UIElement) target).PreviewKeyDown += new KeyEventHandler(this.DetectShortCut);
        break;
      case 2:
        this.SearchControl = (StyledGridSearchControl) target;
        break;
      case 3:
        this.PrintButton = (ListPrint) target;
        break;
      case 4:
        this.FilterButton = (ListFilter) target;
        break;
      case 5:
        this.FirstFocus = (StockLookupEdit) target;
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
