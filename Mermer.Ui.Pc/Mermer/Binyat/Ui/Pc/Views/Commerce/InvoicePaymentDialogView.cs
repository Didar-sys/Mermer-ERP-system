using MvvmCross.Wpf.Views;
using Mermer.Ui.Core.ViewModels.Commerce;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Mermer.Ui.Pc.Views.Commerce;

public partial class InvoicePaymentDialogView : MvxWpfView
{
    public InvoicePaymentDialogView() => InitializeComponent();

    private void Payments_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.End || !(DataContext is InvoicePaymentDialogViewModel dataContext))
            return;

        dataContext.FillPaymentCommand.Execute(null);
        e.Handled = true;
    }

    private void Changes_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.End || !(DataContext is InvoicePaymentDialogViewModel dataContext))
            return;

        dataContext.FillChangesCommand.Execute(null);
        e.Handled = true;
    }
}