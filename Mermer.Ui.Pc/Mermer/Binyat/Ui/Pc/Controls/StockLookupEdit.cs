using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using Mermer.StockManagement.Services;
using Mermer.Ui.Core.Helpers;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mermer.Ui.Pc.Controls;

public partial class StockLookupEdit : PopupBaseEdit
{
    public StockLookupEdit()
    {
        InitializeComponent();
        if (DataContext is StockSearcher dataContext)
            dataContext.PropertyChanged += VmOnPropertyChanged;
    }

    private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == "HasResult" && sender is StockSearcher stockSearcher)
        {
            IsPopupOpen = stockSearcher.HasResult;
        }
    }

    public GridControl PopupGrid { get; set; }

    private void GridControl_Loaded(object sender, RoutedEventArgs e)
    {
        PopupGrid = sender as GridControl;
    }

    private void GridControl_Unloaded(object sender, RoutedEventArgs e)
    {
        PopupGrid = null;
    }

    private void PopupBaseEdit_PopupOpened(object sender, RoutedEventArgs e)
    {
        PopupGrid?.View.MoveFirstRow();
    }

    private void PopupBaseEdit_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Return)
            SelectResult();
        else if (e.Key == Key.Escape)
            Text = string.Empty;
        else if (e.Key == Key.Down)
        {
            if (PopupGrid == null) return;
            PopupGrid.View.MoveNextRow();
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            if (PopupGrid == null) return;
            PopupGrid.View.MovePrevRow();
            e.Handled = true;
        }
        else
        {
            if (e.Key != Key.Tab || PopupGrid == null || !(DataContext is StockSearcher dataContext))
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
        SelectResult();
    }

    public async void SelectResult()
    {
        if (DataContext is StockSearcher vm && (vm.WillSearch || vm.IsSearching))
        {
            while (vm.WillSearch || vm.IsSearching)
                await Task.Delay(TimeSpan.FromSeconds(0.3));
        }

        if (PopupGrid?.SelectedItem != null)
        {
            SelectResult((StockSearchResult)PopupGrid.SelectedItem);
            Text = string.Empty;
        }
    }

    private void SelectResult(StockSearchResult result)
    {
        if (DataContext is StockSearcher dataContext)
        {
            dataContext.Select(result);
            dataContext.HasResult = false;
        }
    }
}