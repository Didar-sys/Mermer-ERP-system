// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Controls.StockLookupEdit
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using Mermer.StockManagement.Services;
using Mermer.Ui.Core.Helpers;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

#nullable disable
namespace Mermer.Ui.Pc.Controls;

public class StockLookupEdit : PopupBaseEdit, IComponentConnector, IStyleConnector
{
  private bool _contentLoaded;

  public StockLookupEdit()
  {
    this.InitializeComponent();
    if (!(this.DataContext is StockSearcher dataContext))
      return;
    dataContext.PropertyChanged += new PropertyChangedEventHandler(this.VmOnPropertyChanged);
  }

  private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs args)
  {
    if (!(args.PropertyName == "HasResult") || !(sender is StockSearcher stockSearcher))
      return;
    this.IsPopupOpen = stockSearcher.HasResult;
  }

  public GridControl PopupGrid { get; set; }

  private void GridControl_Loaded(object sender, RoutedEventArgs e)
  {
    this.PopupGrid = sender as GridControl;
  }

  private void GridControl_Unloaded(object sender, RoutedEventArgs e)
  {
    this.PopupGrid = (GridControl) null;
  }

  private void PopupBaseEdit_PopupOpened(object sender, RoutedEventArgs e)
  {
    this.PopupGrid?.View.MoveFirstRow();
  }

  private void PopupBaseEdit_PreviewKeyDown(object sender, KeyEventArgs e)
  {
    if (e.Key == Key.Return)
      this.SelectResult();
    else if (e.Key == Key.Escape)
      this.Text = string.Empty;
    else if (e.Key == Key.Down)
    {
      if (this.PopupGrid == null)
        return;
      this.PopupGrid.View.MoveNextRow();
      e.Handled = true;
    }
    else if (e.Key == Key.Up)
    {
      if (this.PopupGrid == null)
        return;
      this.PopupGrid.View.MovePrevRow();
      e.Handled = true;
    }
    else
    {
      if (e.Key != Key.Tab || this.PopupGrid == null || !(this.DataContext is StockSearcher dataContext))
        return;
      if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
        dataContext.HideDisabled = !dataContext.HideDisabled;
      else
        dataContext.HideZeroBalance = !dataContext.HideZeroBalance;
      e.Handled = true;
    }
  }

  private void TableView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
  {
    this.SelectResult();
  }

  public async void SelectResult()
  {
    StockLookupEdit stockLookupEdit = this;
    StockSearcher vm = stockLookupEdit.DataContext as StockSearcher;
    if (vm != null && (vm.WillSearch || vm.IsSearching))
    {
      while (vm.WillSearch || vm.IsSearching)
        await Task.Delay(TimeSpan.FromSeconds(0.3));
    }
    if (stockLookupEdit.PopupGrid?.SelectedItem == null)
    {
      vm = (StockSearcher) null;
    }
    else
    {
      stockLookupEdit.SelectResult((StockSearchResult) stockLookupEdit.PopupGrid.SelectedItem);
      stockLookupEdit.Text = string.Empty;
      vm = (StockSearcher) null;
    }
  }

  private void SelectResult(StockSearchResult result)
  {
    if (!(this.DataContext is StockSearcher dataContext))
      return;
    dataContext.Select(result);
    dataContext.HasResult = false;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/Mermer.Ui.Pc;component/controls/stocklookupedit.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    if (connectionId == 1)
    {
      ((UIElement) target).PreviewKeyDown += new KeyEventHandler(this.PopupBaseEdit_PreviewKeyDown);
      ((PopupBaseEdit) target).PopupOpened += new RoutedEventHandler(this.PopupBaseEdit_PopupOpened);
    }
    else
      this._contentLoaded = true;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IStyleConnector.Connect(int connectionId, object target)
  {
    if (connectionId != 2)
    {
      if (connectionId != 3)
        return;
      ((Control) target).MouseDoubleClick += new MouseButtonEventHandler(this.TableView_MouseDoubleClick);
    }
    else
    {
      ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.GridControl_Loaded);
      ((FrameworkElement) target).Unloaded += new RoutedEventHandler(this.GridControl_Unloaded);
      ((UIElement) target).PreviewKeyDown += new KeyEventHandler(this.PopupBaseEdit_PreviewKeyDown);
    }
  }
}
