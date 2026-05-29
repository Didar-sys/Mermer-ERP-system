using Mermer.StockManagement.Services;
using Mermer.Ui.Core.ViewModels.Warehousing.Revisioning;
using Mermer.Warehousing.Revisioning.Models;
using MvvmCross.Wpf.Views;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Mermer.Ui.Pc.Views.Warehousing.Revisioning;

public partial class StockRevisionDetailsView : MvxWpfView
{
    public StockRevisionDetailsView() => InitializeComponent();

    private void DetectShortCut(object sender, System.Windows.Input.KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Tab:
                MoveToNextStockLine();
                e.Handled = true;
                break;
            case Key.F3:
                FirstFocus.Focus();
                e.Handled = true;
                break;
        }
    }

    private void MoveToNextStockLine()
    {
        if (!(DataContext is StockRevisionDetailsViewModel dataContext))
            return;

        string stockId = dataContext.SelectedLine?.StockId;

        if (FirstFocus.IsPopupOpen)
        {
            if (FirstFocus.PopupGrid?.SelectedItem is StockSearchResult selectedItem)
            {
                stockId = selectedItem.Id;
            }
            FirstFocus.IsPopupOpen = false;
        }

        if (string.IsNullOrEmpty(stockId)) return;

        List<StockRevisionLineInfo> list = dataContext.Lines
            .Where(x => x.StockId == stockId)
            .OrderBy(x => GridControl.FindRowByValue("StockRevisionLineId", x.StockRevisionLineId))
            .ToList();

        if (list.Count < 1) return;

        int index = list.IndexOf(dataContext.SelectedLine) + 1;
        dataContext.SelectedLine = list.Count > index ? list.ElementAt(index) : list.First();
    }
}