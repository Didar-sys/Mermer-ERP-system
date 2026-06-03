using MvvmCross.Wpf.Views;
using Mermer.Ui.Core.ViewModels.Commerce;
using System.Windows.Input;
using System.Windows;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Mermer.Ui.Pc.Helpers; // ДОДАЙ ЦЕЙ РЯДОК, якщо helper в іншій папці

namespace Mermer.Ui.Pc.Views.Commerce;

public partial class InvoiceDetailsView : MvxWpfView
{
    public InvoiceDetailsView() => InitializeComponent();

    private void DetectShortCut(object sender, KeyEventArgs e)
    {
        if (!(DataContext is InvoiceDetailsViewModel dataContext))
            return;

        switch (e.Key)
        {
            case Key.End:
                dataContext.UpdatePaymentCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.Home:
                PartnerEditor.Focus();
                PartnerEditor.OpenPopupCommand.Execute(null);
                e.Handled = true;
                break;
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

    // ========================================================
    // ВСЕ, ЩО ТОБІ ТЕПЕР ПОТРІБНО ДЛЯ ЗАКРИТТЯ
    // ========================================================
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        TabNavigationHelper.ForceCloseTab(this);
    }
}