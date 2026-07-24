using MvvmCross.Wpf.Views;
using Mermer.Ui.Core.ViewModels.Commerce;
using System.Windows.Input;
using System.Windows;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Mermer.Ui.Pc.Helpers;

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
                //PartnerEditor.Focus();
                //PartnerEditor.OpenPopupCommand.Execute(null);
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
    // ПРАВИЛЬНОЕ ЗАКРЫТИЕ ЧЕРЕЗ VIEWMODEL
    // ========================================================
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        // Получаем текущую ViewModel
        if (this.DataContext is Mermer.Mvvm.ViewModels.BaseViewModel viewModel)
        {
            // Вызываем правильную команду закрытия из ViewModel (где лежит наше оригинальное белое окно)
            if (viewModel.CloseCommand != null && viewModel.CloseCommand.CanExecute(null))
            {
                viewModel.CloseCommand.Execute(null);
            }
        }
    }
}