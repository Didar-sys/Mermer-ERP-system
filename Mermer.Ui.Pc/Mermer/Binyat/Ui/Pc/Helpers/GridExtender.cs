using DevExpress.Xpf.Grid;
using System.Windows;

using ButtonBase = System.Windows.Controls.Primitives.ButtonBase;

namespace Mermer.Ui.Pc.Helpers;

public static class GridExtender
{
    public static readonly DependencyProperty PrintButtonProperty = DependencyProperty.RegisterAttached("PrintButton", typeof(FrameworkElement), typeof(GridExtender), new UIPropertyMetadata(new PropertyChangedCallback(OnPrintButtonPropertyChanged)));
    public static readonly DependencyProperty FilterButtonProperty = DependencyProperty.RegisterAttached("FilterButton", typeof(FrameworkElement), typeof(GridExtender), new UIPropertyMetadata(new PropertyChangedCallback(OnFilterButtonPropertyChanged)));
    public static readonly DependencyProperty ForceTotalSummaryUpdateProperty = DependencyProperty.RegisterAttached("ForceTotalSummaryUpdate", typeof(bool), typeof(GridExtender), new UIPropertyMetadata(new PropertyChangedCallback(OnForceTotalSummaryUpdatePropertyChanged)));

    public static Window MainForm { get; set; }

    public static ButtonBase GetPrintButton(DependencyObject obj)
    {
        return (ButtonBase)obj.GetValue(PrintButtonProperty);
    }

    public static void SetPrintButton(DependencyObject obj, ButtonBase value)
    {
        obj.SetValue(PrintButtonProperty, value);
    }

    private static void OnPrintButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is ButtonBase button)
        {
            button.Click += (sender, arg) =>
            {
                if (d is TableView tableView)
                {
                    tableView.ShowPrintPreviewDialog(MainForm);
                }
            };
        }
    }

    public static ButtonBase GetFilterButton(DependencyObject obj)
    {
        return (ButtonBase)obj.GetValue(FilterButtonProperty);
    }

    public static void SetFilterButton(DependencyObject obj, ButtonBase value)
    {
        obj.SetValue(FilterButtonProperty, value);
    }

    private static void OnFilterButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is ButtonBase button)
        {
            button.Click += (sender, arg) =>
            {
                if (d is TableView tableView)
                {
                    tableView.ShowFilterEditor(null);
                }
            };
        }
    }

    public static bool GetForceTotalSummaryUpdate(DependencyObject obj)
    {
        return (bool)obj.GetValue(ForceTotalSummaryUpdateProperty);
    }

    public static void SetForceTotalSummaryUpdate(DependencyObject obj, bool value)
    {
        obj.SetValue(ForceTotalSummaryUpdateProperty, value);
    }

    private static void OnForceTotalSummaryUpdatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue == e.NewValue || !(d is TableView tableView))
            return;

        if ((bool)e.NewValue)
            tableView.CellValueChanged += UpdateTotalSummary;
        else
            tableView.CellValueChanged -= UpdateTotalSummary;
    }

    private static void UpdateTotalSummary(object sender, CellValueChangedEventArgs e)
    {
        if (sender is TableView tableView)
        {
            tableView.Grid.UpdateTotalSummary();
        }
    }
}