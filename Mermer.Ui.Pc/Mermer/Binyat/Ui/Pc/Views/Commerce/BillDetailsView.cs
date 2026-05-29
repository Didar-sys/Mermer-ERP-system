using MvvmCross.Wpf.Views;
using Mermer.Ui.Core.ViewModels.Commerce;
using System.Windows.Input;

namespace Mermer.Ui.Pc.Views.Commerce;

public partial class BillDetailsView : MvxWpfView
{
    public BillDetailsView() => InitializeComponent();

    private void DetectShortCut(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (!(DataContext is BillDetailsViewModel dataContext) || e.Key != Key.Home)
            return;

        dataContext.SelectPartnerCommand.Execute(null);
        e.Handled = true;
    }
}