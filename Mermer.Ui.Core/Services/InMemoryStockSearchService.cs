using MvvmCross.Plugins.Messenger;
using Mermer.Commerce.Models;
using Mermer.Commerce.Services;
using Mermer.Common.Services;
using Mermer.FundsManagement.Models;
using Mermer.FundsManagement.Models.Extenders;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Models.Extenders;
using Mermer.StockManagement.Services;
using Mermer.Data.Storage;
using Mermer.Mvvm.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Mermer.Ui.Core.Services;

public class InMemoryStockSearchService : IStockSearchService, IDisposable
{
    private readonly MvxSubscriptionToken _messageToken;
    private readonly IStocksRepository _stocksRepository;
    private readonly IRepository<Currency> _currenciesRepository;
    private readonly IStockBalancesRepository _balancesRepository;
    private readonly ITransliterationService _transliterationService;
    private readonly ILastPurchasePricesRepository _lastPurchasePricesRepository;
    private bool _isInitialized;
    private bool _isInitializing;
    private int lastPriceCheckFailsCount;

    public InMemoryStockSearchService(
        IMvxMessenger messenger,
        IStocksRepository stocksRepository,
        IRepository<Currency> currenciesRepository,
        IStockBalancesRepository balancesRepository,
        ITransliterationService transliterationService,
        ILastPurchasePricesRepository lastPurchasePricesRepository)
    {
        _stocksRepository = stocksRepository;
        _currenciesRepository = currenciesRepository;
        _balancesRepository = balancesRepository;
        _transliterationService = transliterationService;
        _lastPurchasePricesRepository = lastPurchasePricesRepository;
        _messageToken = messenger.Subscribe<DocumentModified<Stock>>(async m => await Initialize(true), MvxReference.Strong);
    }

    private List<Stock> Stocks { get; set; }
    private Dictionary<string, Currency> Currencies { get; set; }

    public async Task Initialize(bool forceReload)
    {
        if (_isInitializing || (_isInitialized && !forceReload))
            return;

        _isInitializing = true;
        try
        {
            var stocksData = await _stocksRepository.GetAsync();
            Stocks = stocksData != null ? stocksData.ToList() : new List<Stock>();

            var currenciesData = await _currenciesRepository.GetAsync();
            Currencies = currenciesData != null
                ? currenciesData.ToDictionary(x => x.Id, x => x)
                : new Dictionary<string, Currency>();

            _isInitialized = true;
        }
        catch
        {
            Stocks = Stocks ?? new List<Stock>();
            Currencies = Currencies ?? new Dictionary<string, Currency>();
        }
        finally
        {
            _isInitializing = false;
        }
    }

    public async Task<IEnumerable<StockSearchResult>> Search(
        string text,
        string warehouseId,
        string priceGroup = null,
        string currencyId = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Автоматическая загрузка, если память пуста
        if (Stocks == null || !Stocks.Any() || !_isInitialized)
        {
            await Initialize(false);
        }

        if (string.IsNullOrWhiteSpace(text) || Stocks == null || !Stocks.Any())
            return Enumerable.Empty<StockSearchResult>();

        text = text.ToLower();

        var array = Enum.GetValues(typeof(TransliterationType))
            .Cast<TransliterationType>()
            .Select(x => _transliterationService.Parse(text, x).ToArray())
            .ToArray();

        var allTerms = array.Aggregate(Enumerable.Empty<string>(), (current, term) => current.Concat(term)).Distinct().ToArray();
        var sequantialTerms = array.Select(ArrayToRegular).Distinct().ToArray();

        CheckIfCanceled(cancellationToken);

        var rawResults = Stocks.Select(x => new
        {
            Stock = x,
            Boost = allTerms.Count(t =>
            {
                if (x.Code?.ToLower().Contains(t) == true) return true;
                if (x.Name?.ToLower().Contains(t) == true) return true;
                if (x.ShortName?.ToLower().Contains(t) == true) return true;
                if (x.Barcodes?.Any(i => i.ToLower().Contains(t)) == true) return true;
                if (x.Tags?.Any(i => i.ToLower().Contains(t)) == true) return true;
                return false;
            })
        })
        .Where(x => x.Boost > 0)
        .Select(x =>
        {
            var stock = x.Stock;
            int num1 = x.Boost + (x.Stock.Code?.ToLower() == text ? 10 : 0);
            int num2 = (x.Stock.Barcodes?.Any(b => b.ToLower() == text) == true) ? 10 : 0;
            int num3 = num1 + num2 + allTerms.Count(t => x.Stock.Tags?.Contains(t) == true) * 2 +
                       (Regex.IsMatch(x.Stock.Code ?? "", WildCardToRegular("*" + text)) ? 5 : 0) +
                       sequantialTerms.Count(t => Regex.IsMatch(x.Stock.Name?.ToLower() ?? "", t)) * 5;
            return new { Stock = stock, Boost = num3 };
        })
        .OrderByDescending(x => x.Boost)
        .Take(64)
        .ToList();

        CheckIfCanceled(cancellationToken);
        string[] stockIds = rawResults.Select(x => x.Stock.Id).ToArray();

        StockBalance[] stockBalances;
        try
        {
            if (string.IsNullOrEmpty(warehouseId)) throw new Exception();
            stockBalances = (await _balancesRepository.GetAsync(warehouseId, stockIds)).ToArray();
        }
        catch
        {
            stockBalances = Array.Empty<StockBalance>();
        }

        CheckIfCanceled(cancellationToken);

        LastPurchasePrice[] lastPurchasePrices;
        try
        {
            if (lastPriceCheckFailsCount < 3)
            {
                if (string.IsNullOrEmpty(warehouseId)) throw new Exception();
                lastPurchasePrices = (await _lastPurchasePricesRepository.GetAsync(warehouseId, stockIds)).ToArray();
            }
            else
            {
                lastPurchasePrices = Array.Empty<LastPurchasePrice>();
            }
        }
        catch
        {
            lastPriceCheckFailsCount++;
            lastPurchasePrices = Array.Empty<LastPurchasePrice>();
        }

        CheckIfCanceled(cancellationToken);

        CurrencyRate displayCurrencyConverter = null;
        if (!string.IsNullOrEmpty(currencyId) && Currencies != null && Currencies.ContainsKey(currencyId))
        {
            displayCurrencyConverter = Currencies[currencyId]?.GetRate();
        }
        if (displayCurrencyConverter == null || displayCurrencyConverter.Multiplier == 0 || displayCurrencyConverter.Divider == 0)
        {
            displayCurrencyConverter = new CurrencyRate { Multiplier = 1M, Divider = 1M };
        }

        var stockSearchResults = rawResults.Select(x =>
        {
            Stock stock = x.Stock;
            var price = stock?.GetPrice(null, priceGroup);

            // Безопасное получение цены (как из группы, так и напрямую)
            decimal d1 = 0m;
            string key1 = currencyId ?? "";

            if (price != null && price.Price > 0)
            {
                d1 = price.Price;
                key1 = price.CurrencyId ?? currencyId ?? "";
            }
            else if (stock != null && stock.Price > 0)
            {
                d1 = stock.Price;
                key1 = !string.IsNullOrEmpty(stock.CurrencyId) ? stock.CurrencyId : (currencyId ?? "");
            }

            if (!string.IsNullOrEmpty(currencyId) && !string.IsNullOrEmpty(key1) && !string.Equals(currencyId, key1, StringComparison.OrdinalIgnoreCase) && Currencies != null && Currencies.ContainsKey(key1))
            {
                CurrencyRate rate = Currencies[key1]?.GetRate();
                if (rate != null && rate.Multiplier != 0 && rate.Divider != 0)
                {
                    d1 = d1 * rate.Multiplier / rate.Divider * displayCurrencyConverter.Divider / displayCurrencyConverter.Multiplier;
                    key1 = currencyId;
                }
            }

            int decimals = (Currencies != null && !string.IsNullOrEmpty(key1) && Currencies.ContainsKey(key1))
                ? Currencies[key1].Decimals
                : 2;

            decimal num4 = Math.Round(d1, decimals);

            var result = new StockSearchResult
            {
                Id = x.Stock.Id,
                Code = x.Stock.Code,
                Name = x.Stock.Name,
                CodeHtml = FormatText(x.Stock.Code, allTerms),
                NameHtml = FormatText(x.Stock.Name, allTerms),
                Price = num4,
                Currency = (Currencies != null && !string.IsNullOrEmpty(key1) && Currencies.ContainsKey(key1)) ? Currencies[key1].Name : (key1 ?? "USD"),
                CurrencyId = key1,
                Balance = stockBalances != null && stockBalances.Any() ? stockBalances.Where(b => b.StockId == x.Stock.Id).Sum(b => b.Balance) : 0m,
                Unit = x.Stock.Unit,
                UnitId = x.Stock.UnitId,
                IsDisabled = x.Stock.IsDisabled
            };

            var lastPurchasePrice = lastPurchasePrices?.SingleOrDefault(p => p.StockId == x.Stock.Id);
            if (lastPurchasePrice != null)
            {
                decimal d2 = lastPurchasePrice.Price;
                string key2 = lastPurchasePrice.CurrencyId ?? "";
                if (!string.IsNullOrEmpty(currencyId) && !string.IsNullOrEmpty(key2) && !string.Equals(currencyId, key2, StringComparison.OrdinalIgnoreCase) && Currencies != null && Currencies.ContainsKey(key2))
                {
                    CurrencyRate rate = Currencies[key2]?.GetRate();
                    if (rate != null && rate.Multiplier != 0 && rate.Divider != 0)
                    {
                        d2 = d2 * rate.Multiplier / rate.Divider * displayCurrencyConverter.Divider / displayCurrencyConverter.Multiplier;
                        key2 = currencyId;
                    }
                }

                int lastDecimals = (Currencies != null && !string.IsNullOrEmpty(key2) && Currencies.ContainsKey(key2))
                    ? Currencies[key2].Decimals
                    : 2;

                decimal num5 = Math.Round(d2, lastDecimals);
                result.LastPurchasePrice = num5;
                result.LastPurchaseCurrency = (Currencies != null && !string.IsNullOrEmpty(key2) && Currencies.ContainsKey(key2)) ? Currencies[key2].Name : key2;
                result.LastPurchaseCurrencyId = key2;
            }
            return result;
        }).ToList();

        return stockSearchResults;
    }

    private string FormatText(string text, string[] searchWords)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        if (searchWords.Contains("b", StringComparer.OrdinalIgnoreCase))
            text = text.ToUpper().Replace("B", "<B>B</B>");

        foreach (string str in searchWords.Where(w => !w.Equals("b", StringComparison.OrdinalIgnoreCase)))
        {
            if (text.IndexOf(str, StringComparison.OrdinalIgnoreCase) >= 0)
                text = text.ToUpper().Replace(str.ToUpper(), $"<B>{str.ToUpper()}</B>");
        }
        text = text.ToUpper().Replace("<B>", "<B style=\"background-color:yellow;color:black\">");
        return text;
    }

    private void CheckIfCanceled(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new TaskCanceledException();
    }

    private static string ArrayToRegular(string[] values) => WildCardToRegular(string.Join("* ", values) + "*");

    private static string WildCardToRegular(string value) => $"^{Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*")}$";

    public void Dispose() => _messageToken?.Dispose();
}