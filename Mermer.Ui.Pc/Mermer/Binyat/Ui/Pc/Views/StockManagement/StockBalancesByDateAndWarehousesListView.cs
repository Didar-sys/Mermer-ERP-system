using DevExpress.Xpf.Grid;
using MvvmCross.Wpf.Views;
using Mermer.StockManagement.Models;
using Mermer.Ui.Core.ViewModels.StockManagement;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Mermer.Ui.Pc.Views.StockManagement;

public partial class StockBalancesByDateAndWarehousesListView : MvxWpfView
{
    public StockBalancesByDateAndWarehousesListView() => InitializeComponent();

    private void GridControl_CustomUnboundColumnData(object sender, GridColumnDataEventArgs e)
    {
        if (!(DataContext is StockBalancesByDateAndWarehousesListViewModel dataContext))
            return;

        StockBalanceByWarehouses balanceByWarehouses = dataContext.List[e.ListSourceRowIndex];

        if (!e.IsGetData)
            return;

        e.Value = balanceByWarehouses.Balances.ContainsKey(e.Column.FieldName)
            ? balanceByWarehouses.Balances[e.Column.FieldName]
            : 0M;
    }

    private void DetectShortCut(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.F3)
            return;

        FirstFocus.Focus();
        e.Handled = true;
    }
}