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

    // ДОДАЙ ЦЕЙ МЕТОД
    private void Quantity_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            // 1. Перекидаємо фокус на поле "Ціна"
            PriceEdit.Focus();

            // 2. Виділяємо весь текст, щоб касир міг одразу вводити нові цифри замість старих
            PriceEdit.SelectAll();

            // 3. ВАЖЛИВО: Зупиняємо подію, щоб не спрацювала кнопка Update
            e.Handled = true;
        }
    }
}