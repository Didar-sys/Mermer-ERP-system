using System;
using System.Windows.Markup;

// Цей магічний атрибут каже XAML шукати наш клас без жодних префіксів!
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Mermer.Ui.Pc.Markup")]

namespace Mermer.Ui.Pc.Markup
{
    [MarkupExtensionReturnType(typeof(string))]
    public class LocalizeExtension : MarkupExtension
    {
        public string Key { get; set; }
        public string FallBackText { get; set; }

        public LocalizeExtension() { }

        public LocalizeExtension(string key)
        {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // Якщо є FallBackText - повертаємо його, інакше повертаємо ключ
            return FallBackText ?? Key ?? "";
        }
    }
}