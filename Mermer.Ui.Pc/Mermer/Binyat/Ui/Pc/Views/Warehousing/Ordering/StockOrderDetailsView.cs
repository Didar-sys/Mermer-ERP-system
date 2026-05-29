using MvvmCross.Wpf.Views;
using Mermer.Ui.Core.ViewModels.Warehousing.Ordering;
using System.Windows.Input;

namespace Mermer.Ui.Pc.Views.Warehousing.Ordering;

public partial class StockOrderDetailsView : MvxWpfView
{
    public StockOrderDetailsView() => InitializeComponent();

    private void DetectShortCut(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (!(DataContext is StockOrderDetailsViewModel dataContext))
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