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

        // Підписуємося на завантаження вікна, щоб GridControl вже встиг створитися
        this.Loaded += StockTransfersListView_Loaded;
    }

    private void StockTransfersListView_Loaded(object sender, RoutedEventArgs e)
    {
        // Безпечно шукаємо внутрішню TableView у нашому кастомному GridControl
        if (GridControl != null && GridControl.View is TableView tableView)
        {
            // Очищаємо старі кастомні умови (якщо вони раптом були додані раніше)
            var existingCondition = tableView.FormatConditions
                .FirstOrDefault(x => x.Expression == "[ActionReceivedTotal] == 0");

            if (existingCondition == null)
            {
                // Додаємо правило підсвічування рядків безпосередньо через C#
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