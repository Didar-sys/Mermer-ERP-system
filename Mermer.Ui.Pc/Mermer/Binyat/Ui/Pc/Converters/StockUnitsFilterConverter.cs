using Mermer.StockManagement.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace Mermer.Ui.Pc.Converters;

public class StockUnitsFilterConverter : MarkupExtension, IMultiValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // 1. Захист від порожнього масиву значень
        if (values == null || values.Length < 3)
            return null;

        if (!(values[1] is ObservableCollection<Stock> source))
            return null;

        if ((values[2] as bool?).GetValueOrDefault())
        {
            string stockId = values[0] as string;
            if (!string.IsNullOrEmpty(stockId))
            {
                // Використовуємо FirstOrDefault і перевіряємо x на null
                Stock stock = source.FirstOrDefault(x => x != null && x.Id == stockId);
                return stock?.Units;
            }
        }

        // 2. БЕЗПЕЧНИЙ SELECTMANY: 
        // Відфільтровуємо всі товари, які є null або у яких немає одиниць виміру
        return source
            .Where(x => x != null && x.Units != null)
            .SelectMany(x => x.Units);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}