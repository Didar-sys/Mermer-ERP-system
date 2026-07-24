using System;
using System.Globalization;
using System.Windows.Data;

namespace Mermer.Ui.Pc.Converters
{
    public class IgnoreNotLoadedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Если DevExpress подсовывает заглушку во время загрузки – говорим WPF ничего не делать
            if (value != null && value.GetType().Name == "NotLoadedObject")
            {
                return Binding.DoNothing;
            }

            return value;
        }
    }
}