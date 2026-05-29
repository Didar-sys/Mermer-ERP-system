using MvvmCross.Wpf.Views;
using Mermer.Ui.Core.ViewModels.Commerce;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Mermer.Ui.Pc.Views.Commerce;

public partial class SelectSourceInvoiceView : MvxWpfView
{
    public SelectSourceInvoiceView() => InitializeComponent();

    private void ButtonEdit_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Return || !(DataContext is SelectSourceInvoiceViewModel dataContext))
            return;

        dataContext.SearchInvoices.Execute(null);
        e.Handled = true;
    }
}