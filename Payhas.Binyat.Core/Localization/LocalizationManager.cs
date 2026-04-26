using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Payhas.Binyat.Core.Localization;

/// <summary>
/// Localization manager that loads JSON locale files and provides key-based access
/// to translated strings. Replaces hardcoded UI strings and the broken single-file
/// IJsonLocalizationResourceProvider from the old system.
/// 
/// Supports hierarchical keys: "commerce.invoice", "common.save", etc.
/// Supports format strings with parameters: Get("validation.fieldRequired", "Name") → "Поле 'Name' обязательно"
/// 
/// Usage in ViewModels:
///   var text = LocalizationManager.Instance["common.save"];  // "Сохранить"
///   var msg  = LocalizationManager.Instance.Get("validation.fieldRequired", "Цена");
/// 
/// Thread-safe singleton implementation.
/// </summary>
public sealed class LocalizationManager
{
    private static readonly Lazy<LocalizationManager> _instance =
        new Lazy<LocalizationManager>(() => new LocalizationManager());

    /// <summary>Singleton instance.</summary>
    public static LocalizationManager Instance => _instance.Value;

    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _locales = new();
    private string _currentLocale = "ru";
    private string _fallbackLocale = "ru";
    private string? _localesDirectory;

    /// <summary>
    /// Metadata from the current locale file (date format, number format, etc.)
    /// </summary>
    public LocaleMetadata? CurrentMetadata { get; private set; }

    /// <summary>
    /// Gets or sets the current locale code (e.g., "ru", "tm").
    /// Setting this property automatically loads the locale file if not already loaded.
    /// </summary>
    public string CurrentLocale
    {
        get => _currentLocale;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Locale code cannot be null or empty.", nameof(value));

            _currentLocale = value.ToLowerInvariant();

            if (!_locales.ContainsKey(_currentLocale) && _localesDirectory != null)
                LoadLocaleFromFile(_currentLocale, _localesDirectory);

            UpdateMetadata();
            LocaleChanged?.Invoke(this, _currentLocale);
        }
    }

    /// <summary>
    /// Gets the list of available locale codes.
    /// </summary>
    public IReadOnlyCollection<string> AvailableLocales => _locales.Keys.ToList().AsReadOnly();

    /// <summary>
    /// Event raised when the current locale changes.
    /// UI can subscribe to refresh displayed strings.
    /// </summary>
    public event EventHandler<string>? LocaleChanged;

    /// <summary>
    /// Indexer for quick key access. Returns the key itself if not found.
    /// Supports dot-notation: this["commerce.invoice"] → "Накладная"
    /// </summary>
    public string this[string key] => Get(key);

    private LocalizationManager() { }

    /// <summary>
    /// Initializes the localization manager by loading all .json files from the specified directory.
    /// Should be called once during application startup.
    /// </summary>
    /// <param name="localesDirectory">Path to directory containing locale .json files</param>
    /// <param name="defaultLocale">Default locale code (default: "ru")</param>
    /// <param name="fallbackLocale">Fallback locale when key is not found (default: "ru")</param>
    public void Initialize(string localesDirectory, string defaultLocale = "ru", string fallbackLocale = "ru")
    {
        if (!Directory.Exists(localesDirectory))
            throw new DirectoryNotFoundException($"Locales directory not found: {localesDirectory}");

        _localesDirectory = localesDirectory;
        _fallbackLocale = fallbackLocale.ToLowerInvariant();

        // Load all JSON locale files from directory
        foreach (var file in Directory.GetFiles(localesDirectory, "*.json"))
        {
            var localeCode = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
            LoadLocaleFromFile(localeCode, localesDirectory);
        }

        CurrentLocale = defaultLocale.ToLowerInvariant();
    }

    /// <summary>
    /// Gets a localized string by key with optional format parameters.
    /// Falls back to fallback locale, then returns the key itself if not found.
    /// </summary>
    /// <param name="key">Dot-notation key, e.g., "commerce.grandTotal"</param>
    /// <param name="args">Optional format arguments for string.Format</param>
    /// <returns>Localized string or the key if not found</returns>
    public string Get(string key, params object[] args)
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        // Try current locale
        var value = GetFromLocale(_currentLocale, key);

        // Try fallback locale
        if (value == null && _currentLocale != _fallbackLocale)
            value = GetFromLocale(_fallbackLocale, key);

        // If still not found, return the key itself
        if (value == null)
            return key;

        // Apply format arguments if any
        if (args.Length > 0)
        {
            try
            {
                return string.Format(CultureInfo.CurrentCulture, value, args);
            }
            catch (FormatException)
            {
                return value;
            }
        }

        return value;
    }

    /// <summary>
    /// Checks if a key exists in the current locale (or fallback).
    /// </summary>
    public bool HasKey(string key)
    {
        return GetFromLocale(_currentLocale, key) != null
            || GetFromLocale(_fallbackLocale, key) != null;
    }

    /// <summary>
    /// Gets all keys and values for a section (e.g., "commerce" returns all commerce keys).
    /// </summary>
    public IReadOnlyDictionary<string, string> GetSection(string sectionPrefix)
    {
        if (!_locales.TryGetValue(_currentLocale, out var locale))
            return new Dictionary<string, string>();

        var prefix = sectionPrefix.EndsWith(".") ? sectionPrefix : sectionPrefix + ".";
        return locale
            .Where(kv => kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    /// <summary>
    /// Manually registers a locale dictionary (useful for testing or dynamic locales).
    /// </summary>
    public void RegisterLocale(string localeCode, Dictionary<string, string> flattenedKeys)
    {
        _locales[localeCode.ToLowerInvariant()] = flattenedKeys;
    }

    private string? GetFromLocale(string localeCode, string key)
    {
        if (_locales.TryGetValue(localeCode, out var locale))
        {
            if (locale.TryGetValue(key, out var value))
                return value;
        }
        return null;
    }

    private void LoadLocaleFromFile(string localeCode, string directory)
    {
        var filePath = Path.Combine(directory, $"{localeCode}.json");
        if (!File.Exists(filePath))
            return;

        try
        {
            var json = File.ReadAllText(filePath);
            var document = JsonDocument.Parse(json);
            var flattened = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            FlattenJson(document.RootElement, "", flattened);
            _locales[localeCode] = flattened;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse locale file: {filePath}", ex);
        }
    }

    /// <summary>
    /// Recursively flattens a JSON object into dot-notation keys.
    /// { "commerce": { "invoice": "Накладная" } } → { "commerce.invoice": "Накладная" }
    /// </summary>
    private static void FlattenJson(JsonElement element, string prefix, Dictionary<string, string> result)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                {
                    var key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
                    FlattenJson(prop.Value, key, result);
                }
                break;

            case JsonValueKind.String:
                result[prefix] = element.GetString() ?? string.Empty;
                break;

            case JsonValueKind.Number:
                result[prefix] = element.GetRawText();
                break;

            case JsonValueKind.True:
            case JsonValueKind.False:
                result[prefix] = element.GetBoolean().ToString().ToLowerInvariant();
                break;

            case JsonValueKind.Array:
                // Arrays are stored as comma-separated values
                var items = new List<string>();
                foreach (var item in element.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                        items.Add(item.GetString() ?? "");
                }
                result[prefix] = string.Join(",", items);
                break;
        }
    }

    private void UpdateMetadata()
    {
        if (!_locales.TryGetValue(_currentLocale, out var locale))
        {
            CurrentMetadata = null;
            return;
        }

        CurrentMetadata = new LocaleMetadata
        {
            Locale = locale.GetValueOrDefault("meta.locale", _currentLocale),
            Name = locale.GetValueOrDefault("meta.name", _currentLocale),
            Direction = locale.GetValueOrDefault("meta.direction", "ltr"),
            DateFormat = locale.GetValueOrDefault("meta.dateFormat", "dd.MM.yyyy"),
            DecimalSeparator = locale.GetValueOrDefault("meta.numberFormat.decimal", ","),
            ThousandsSeparator = locale.GetValueOrDefault("meta.numberFormat.thousands", " "),
            CurrencySymbol = locale.GetValueOrDefault("meta.numberFormat.currency", "")
        };
    }
}

/// <summary>
/// Metadata about the current locale: formatting rules, direction, etc.
/// </summary>
public class LocaleMetadata
{
    public string Locale { get; set; } = "ru";
    public string Name { get; set; } = "Русский";
    public string Direction { get; set; } = "ltr";
    public string DateFormat { get; set; } = "dd.MM.yyyy";
    public string DecimalSeparator { get; set; } = ",";
    public string ThousandsSeparator { get; set; } = " ";
    public string CurrencySymbol { get; set; } = "";
}

/// <summary>
/// Extension for dictionary fallback access.
/// </summary>
internal static class DictionaryExtensions
{
    public static TValue GetValueOrDefault<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue defaultValue) where TKey : notnull
    {
        return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
    }
}
