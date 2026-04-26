// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Views.Warehousing.Ordering.AggregatedStockOrderDetailsView
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using DevExpress.Xpf.Grid;
using MvvmCross.Wpf.Views;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.ViewModels.Warehousing.Ordering;
using Payhas.Binyat.Ui.Pc.Controls;
using Payhas.Binyat.Ui.Pc.Controls.MenuItems;
using Payhas.Binyat.Warehousing.Ordering.Models;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Views.Warehousing.Ordering;

public class AggregatedStockOrderDetailsView : MvxWpfView, IComponentConnector
{
  internal ListPrint PrintButton;
  internal StockLookupEdit FirstFocus;
  private bool _contentLoaded;

  public AggregatedStockOrderDetailsView() => this.InitializeComponent();

  private void GridControl_CustomUnboundColumnData(object sender, GridColumnDataEventArgs e)
  {
    if (!(this.DataContext is AggregatedStockOrderDetailsViewModel dataContext))
      return;
    AggregatedStockOrderLine line = dataContext.Details.Lines[e.ListSourceRowIndex];
    if (e.Column.FieldName == "Difference")
    {
      if (!e.IsGetData)
        return;
      ListHelper<string, Decimal> listHelper = dataContext.StocksBalances.SingleOrDefault<ListHelper<string, Decimal>>((Func<ListHelper<string, Decimal>, bool>) (x => x.Key == line.StockId));
      e.Value = (object) ((listHelper != null ? listHelper.Value : 0M) - line.OrdersTotal);
    }
    else
    {
      if (e.IsGetData)
        e.Value = (object) line.Orders[e.Column.FieldName];
      if (!e.IsSetData)
        return;
      line.Orders[e.Column.FieldName] = Convert.ToDecimal(e.Value);
    }
  }

  private void DetectShortCut(object sender, KeyEventArgs e)
  {
    if (!(this.DataContext is AggregatedStockOrderDetailsViewModel) || e.Key != Key.F3)
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
    Application.LoadComponent((object) this, new Uri("/Payhas.Binyat.Ui.Pc;component/views/warehousing.ordering/aggregatedstockorderdetailsview.xaml", UriKind.Relative));
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
        this.PrintButton = (ListPrint) target;
        break;
      case 3:
        this.FirstFocus = (StockLookupEdit) target;
        break;
      case 4:
        ((GridControl) target).CustomUnboundColumnData += new GridColumnDataEventHandler(this.GridControl_CustomUnboundColumnData);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
