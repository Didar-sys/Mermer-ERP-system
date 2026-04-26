// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Views.Warehousing.Revisioning.StockRevisionDetailsView
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using DevExpress.Xpf.Grid;
using MvvmCross.Wpf.Views;
using Payhas.Binyat.StockManagement.Services;
using Payhas.Binyat.Ui.Core.ViewModels.Warehousing.Revisioning;
using Payhas.Binyat.Ui.Pc.Controls;
using Payhas.Binyat.Ui.Pc.Controls.MenuItems;
using Payhas.Binyat.Warehousing.Revisioning.Models;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Views.Warehousing.Revisioning;

public class StockRevisionDetailsView : MvxWpfView, IComponentConnector
{
  internal ListPrint PrintButton;
  internal StockLookupEdit FirstFocus;
  internal GridControl GridControl;
  internal ToggleButton ExpandButton;
  private bool _contentLoaded;

  public StockRevisionDetailsView() => this.InitializeComponent();

  private void DetectShortCut(object sender, KeyEventArgs e)
  {
    switch (e.Key)
    {
      case Key.Tab:
        this.MoveToNextStockLine();
        e.Handled = true;
        break;
      case Key.F3:
        this.FirstFocus.Focus();
        e.Handled = true;
        break;
    }
  }

  private void MoveToNextStockLine()
  {
    if (!(this.DataContext is StockRevisionDetailsViewModel dataContext))
      return;
    string stockId = dataContext.SelectedLine?.StockId;
    if (this.FirstFocus.IsPopupOpen)
    {
      if (this.FirstFocus.PopupGrid.SelectedItem != null && this.FirstFocus.PopupGrid.SelectedItem is StockSearchResult selectedItem)
        stockId = selectedItem.Id;
      this.FirstFocus.IsPopupOpen = false;
    }
    if (string.IsNullOrEmpty(stockId))
      return;
    List<StockRevisionLineInfo> list = dataContext.Lines.Where<StockRevisionLineInfo>((Func<StockRevisionLineInfo, bool>) (x => x.StockId == stockId)).OrderBy<StockRevisionLineInfo, int>((Func<StockRevisionLineInfo, int>) (x => this.GridControl.FindRowByValue("StockRevisionLineId", (object) x.StockRevisionLineId))).ToList<StockRevisionLineInfo>();
    if (list.Count < 1)
      return;
    int index = list.IndexOf(dataContext.SelectedLine) + 1;
    dataContext.SelectedLine = list.Count > index ? list.ElementAt<StockRevisionLineInfo>(index) : list.First<StockRevisionLineInfo>();
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/Payhas.Binyat.Ui.Pc;component/views/warehousing.revisioning/stockrevisiondetailsview.xaml", UriKind.Relative));
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
        this.GridControl = (GridControl) target;
        break;
      case 5:
        this.ExpandButton = (ToggleButton) target;
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
