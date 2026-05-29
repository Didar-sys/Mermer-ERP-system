using MvvmCross.Wpf.Views;
using Mermer.Ui.Core.ViewModels.Warehousing.Ordering;
using System.Windows.Input;

namespace Mermer.Ui.Pc.Views.Warehousing.Ordering;

public partial class StockOrderTemplateDetailsView : MvxWpfView
{
    public StockOrderTemplateDetailsView() => InitializeComponent();

    private void DetectShortCut(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (!(DataContext is StockOrderTemplateDetailsViewModel) || e.Key != Key.F3)
            return;

        FirstFocus.Focus();
        e.Handled = true;
    }
}