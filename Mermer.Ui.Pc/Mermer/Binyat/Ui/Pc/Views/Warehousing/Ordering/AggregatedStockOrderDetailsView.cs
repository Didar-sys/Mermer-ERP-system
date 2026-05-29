using DevExpress.Xpf.Grid;
using MvvmCross.Wpf.Views;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.ViewModels.Warehousing.Ordering;
using Mermer.Warehousing.Ordering.Models;
using System;
using System.Linq;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Mermer.Ui.Pc.Views.Warehousing.Ordering;

public partial class AggregatedStockOrderDetailsView : MvxWpfView
{
    public AggregatedStockOrderDetailsView() => InitializeComponent();

    private void GridControl_CustomUnboundColumnData(object sender, GridColumnDataEventArgs e)
    {
        if (!(DataContext is AggregatedStockOrderDetailsViewModel dataContext))
            return;

        AggregatedStockOrderLine line = dataContext.Details.Lines[e.ListSourceRowIndex];

        if (e.Column.FieldName == "Difference")
        {
            if (!e.IsGetData) return;

            ListHelper<string, decimal> listHelper = dataContext.StocksBalances
                .SingleOrDefault(x => x.Key == line.StockId);

            e.Value = (listHelper != null ? listHelper.Value : 0M) - line.OrdersTotal;
        }
        else
        {
            if (e.IsGetData)
                e.Value = line.Orders[e.Column.FieldName];

            if (!e.IsSetData) return;

            line.Orders[e.Column.FieldName] = Convert.ToDecimal(e.Value);
        }
    }

    private void DetectShortCut(object sender, KeyEventArgs e)
    {
        if (!(DataContext is AggregatedStockOrderDetailsViewModel) || e.Key != Key.F3)
            return;

        FirstFocus.Focus();
        e.Handled = true;
    }
}