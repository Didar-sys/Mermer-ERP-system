using MvvmCross.Wpf.Views;
using System.Windows.Input;

namespace Mermer.Ui.Pc.Views.Warehousing;

public partial class StockTransferDetailsLineEditView : MvxWpfView
{
    public StockTransferDetailsLineEditView() => InitializeComponent();

    private void FirstFocus_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != Key.Return)
            return;

        ReceivedEdit.Focus();
        e.Handled = true;
    }
}