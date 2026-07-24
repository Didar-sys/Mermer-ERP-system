using DevExpress.Xpf.Grid;
using MvvmCross.Wpf.Views;
using System.Linq;
using System.Windows;

namespace Mermer.Ui.Pc.Views.Warehousing;

public partial class StockTransfersListView : MvxWpfView
{
    public StockTransfersListView()
    {
        InitializeComponent();

        // Подписываемся на загрузку окна, чтобы GridControl уже успел создаться
        this.Loaded += StockTransfersListView_Loaded;
    }

    private void StockTransfersListView_Loaded(object sender, RoutedEventArgs e)
    {
        // Безопасно ищем внутреннюю TableView в нашем кастомном GridControl
        if (GridControl != null && GridControl.View is TableView tableView)
        {
            // Очищаем старые кастомные условия (если они вдруг были добавлены ранее)
            var existingCondition = tableView.FormatConditions
                .FirstOrDefault(x => x.Expression == "[ActionReceivedTotal] == 0");

            if (existingCondition == null)
            {
                // Добавляем правило подсветки строк непосредственно через C#
                tableView.FormatConditions.Add(new FormatCondition
                {
                    Expression = "[ActionReceivedTotal] == 0",
                    ApplyToRow = true,
                    PredefinedFormatName = "LightRedFillWithDarkRedText"
                });
            }
        }
    }
}