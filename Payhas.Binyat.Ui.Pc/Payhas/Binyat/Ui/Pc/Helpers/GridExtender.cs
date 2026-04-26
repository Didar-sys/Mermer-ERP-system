// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Helpers.GridExtender
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using DevExpress.Xpf.Grid;
using System.Windows;
using System.Windows.Controls.Primitives;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Helpers;

public static class GridExtender
{
  public static readonly DependencyProperty PrintButtonProperty = DependencyProperty.RegisterAttached("PrintButton", typeof (FrameworkElement), typeof (GridExtender), (PropertyMetadata) new UIPropertyMetadata(new PropertyChangedCallback(GridExtender.OnPrintButtonPropertyChanged)));
  public static readonly DependencyProperty FilterButtonProperty = DependencyProperty.RegisterAttached("FilterButton", typeof (FrameworkElement), typeof (GridExtender), (PropertyMetadata) new UIPropertyMetadata(new PropertyChangedCallback(GridExtender.OnFilterButtonPropertyChanged)));
  public static readonly DependencyProperty ForceTotalSummaryUpdateProperty = DependencyProperty.RegisterAttached("ForceTotalSummaryUpdate", typeof (bool), typeof (GridExtender), (PropertyMetadata) new UIPropertyMetadata(new PropertyChangedCallback(GridExtender.OnForceTotalSummaryUpdatePropertyChanged)));

  public static Window MainForm { get; set; }

  public static ButtonBase GetPrintButton(DependencyObject obj)
  {
    return (ButtonBase) obj.GetValue(GridExtender.PrintButtonProperty);
  }

  public static void SetPrintButton(DependencyObject obj, ButtonBase value)
  {
    obj.SetValue(GridExtender.PrintButtonProperty, (object) value);
  }

  private static void OnPrintButtonPropertyChanged(
    DependencyObject d,
    DependencyPropertyChangedEventArgs e)
  {
    ((ButtonBase) e.NewValue).Click += (RoutedEventHandler) ((sender, arg) =>
    {
      if (!(d is TableView tableView2))
        return;
      tableView2.ShowPrintPreviewDialog(GridExtender.MainForm);
    });
  }

  public static ButtonBase GetFilterButton(DependencyObject obj)
  {
    return (ButtonBase) obj.GetValue(GridExtender.FilterButtonProperty);
  }

  public static void SetFilterButton(DependencyObject obj, ButtonBase value)
  {
    obj.SetValue(GridExtender.FilterButtonProperty, (object) value);
  }

  private static void OnFilterButtonPropertyChanged(
    DependencyObject d,
    DependencyPropertyChangedEventArgs e)
  {
    ((ButtonBase) e.NewValue).Click += (RoutedEventHandler) ((sender, arg) =>
    {
      if (!(d is TableView tableView2))
        return;
      tableView2.ShowFilterEditor((ColumnBase) null);
    });
  }

  public static bool GetForceTotalSummaryUpdate(DependencyObject obj)
  {
    return (bool) obj.GetValue(GridExtender.ForceTotalSummaryUpdateProperty);
  }

  public static void SetForceTotalSummaryUpdate(DependencyObject obj, bool value)
  {
    obj.SetValue(GridExtender.ForceTotalSummaryUpdateProperty, (object) value);
  }

  private static void OnForceTotalSummaryUpdatePropertyChanged(
    DependencyObject d,
    DependencyPropertyChangedEventArgs e)
  {
    if (e.OldValue == e.NewValue || !(d is TableView tableView))
      return;
    if ((bool) e.NewValue)
      tableView.CellValueChanged += new CellValueChangedEventHandler(GridExtender.UpdateTotalSummary);
    else
      tableView.CellValueChanged -= new CellValueChangedEventHandler(GridExtender.UpdateTotalSummary);
  }

  private static void UpdateTotalSummary(object sender, CellValueChangedEventArgs e)
  {
    (sender as TableView).Grid.UpdateTotalSummary();
  }
}
