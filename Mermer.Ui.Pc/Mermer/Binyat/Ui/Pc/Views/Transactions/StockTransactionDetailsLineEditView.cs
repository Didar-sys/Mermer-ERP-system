using MvvmCross.Wpf.Views;
using System.Windows.Input;

namespace Mermer.Ui.Pc.Views.Transactions;

public partial class StockTransactionDetailsLineEditView : MvxWpfView
{
    public StockTransactionDetailsLineEditView() => InitializeComponent();

    private void FirstFocus_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != Key.Return)
            return;

        PriceEdit.Focus();
        e.Handled = true;
    }
}