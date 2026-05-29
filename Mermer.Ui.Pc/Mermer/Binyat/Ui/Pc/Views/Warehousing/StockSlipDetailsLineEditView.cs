using MvvmCross.Wpf.Views;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Mermer.Ui.Pc.Views.Warehousing;

public partial class StockSlipDetailsLineEditView : MvxWpfView
{
    public StockSlipDetailsLineEditView() => InitializeComponent();

    private void FirstFocus_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Return || !PriceEdit.IsEnabled)
            return;

        PriceEdit.Focus();
        e.Handled = true;
    }
}