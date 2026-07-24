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

    // ДОБАВЬ ЭТОТ МЕТОД
    private void Quantity_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            // 1. Перекидываем фокус на поле "Цена"
            PriceEdit.Focus();

            // 2. Выделяем весь текст, чтобы кассир мог сразу вводить новые цифры вместо старых
            PriceEdit.SelectAll();

            // 3. ВАЖНО: Останавливаем событие, чтобы не сработала кнопка Update
            e.Handled = true;
        }
    }
}