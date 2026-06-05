using Mermer.Common.Services;
using Mermer.Mvvm.Tools;
using System.Linq;

namespace Mermer.Ui.Core.Services;

// Цей сервіс тепер - міст між старими вікнами MvvmCross і твоїм новим LocalizationManager
public class LocalizationService : ILocalizationService
{
    private readonly string _namespacePrefix;

    public LocalizationService()
    {
        _namespacePrefix = string.Empty;
    }

    public LocalizationService(string owningObjectType)
    {
        // MvvmCross зазвичай передає сюди ім'я класу, наприклад "Mermer.Ui.Core.ViewModels.LoginViewModel"
        // Ми витягуємо з нього базовий контекст (наприклад, "auth" або "common")
        // Якщо це надто складно, ми можемо просто шукати по всьому словнику.
        _namespacePrefix = owningObjectType;
    }

    public string GetText(string entryKey)
    {
        // 1. Спочатку шукаємо точний збіг ключа у твоєму менеджері (наприклад, "common.save")
        if (LocalizationManager.Instance.HasKey(entryKey))
            return LocalizationManager.Instance.Get(entryKey);

        // 2. Якщо ключ прийшов без префікса (просто "save"), шукаємо його по всіх секціях
        // Це необхідно, бо старий XAML часто передає просто слова, а не повні ключі.
        var allKeys = LocalizationManager.Instance.GetSection(""); // Беремо всі ключі
        var foundPair = allKeys.FirstOrDefault(k =>
            k.Key.EndsWith("." + entryKey, System.StringComparison.OrdinalIgnoreCase) ||
            k.Key.Equals(entryKey, System.StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(foundPair.Value))
            return foundPair.Value;

        // 3. Fallback: якщо не знайшли, повертаємо сам ключ (щоб на екрані не було пусто)
        return entryKey;
    }

    public string GetText(string entryKey, params object[] args)
    {
        var text = GetText(entryKey);
        try
        {
            return string.Format(text, args);
        }
        catch
        {
            return text; // Захист від битих аргументів форматування
        }
    }
}