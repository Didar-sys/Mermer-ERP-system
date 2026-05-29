using MvvmCross.Wpf.Views;
using Mermer.Ui.Core.ViewModels.Warehousing;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Mermer.Ui.Pc.Views.Warehousing;

public partial class StockTransferDetailsView : MvxWpfView
{
    public StockTransferDetailsView() => InitializeComponent();

    private void DetectShortCut(object sender, KeyEventArgs e)
    {
        if (!(DataContext is StockTransferDetailsViewModel dataContext))
            return;

        switch (e.Key)
        {
            case Key.Insert:
                dataContext.SelectedLinePlusOneCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.Delete:
                dataContext.SelectedLineMinusOneCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.F3:
                FirstFocus.Focus();
                e.Handled = true;
                break;
        }
    }
}