using MvvmCross.Wpf.Views;
using Mermer.Ui.Core.ViewModels.Warehousing;
using System.Windows.Input;

namespace Mermer.Ui.Pc.Views.Warehousing;

public partial class StockSlipDetailsView : MvxWpfView
{
    public StockSlipDetailsView() => InitializeComponent();

    private void DetectShortCut(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (!(DataContext is StockSlipDetailsViewModel dataContext))
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