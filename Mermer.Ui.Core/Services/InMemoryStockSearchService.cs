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
        Stocks = (await _stocksRepository.GetAsync()).ToList();
        Currencies = (await _currenciesRepository.GetAsync()).ToDictionary(x => x.Id, x => x);
        _isInitialized = true;
        _isInitializing = false;
    }

    public async Task<IEnumerable<StockSearchResult>> Search(
        string text,
        string warehouseId,
        string priceGroup = null,
        string currencyId = null,
        CancellationToken cancellationToken = default)
    {
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

        CurrencyRate displayCurrencyConverter;
        if (!string.IsNullOrEmpty(currencyId) && Currencies.ContainsKey(currencyId))
        {
            displayCurrencyConverter = Currencies[currencyId].GetRate();
        }
        else
        {
            displayCurrencyConverter = new CurrencyRate { Multiplier = 1M, Divider = 1M };
        }

        var stockSearchResults = rawResults.Select(x =>
        {
            Stock stock = x.Stock;
            var price = stock.GetPrice(null, priceGroup);
            decimal d1 = price.Price;
            string key1 = price.CurrencyId;

            if (currencyId != null && currencyId != key1 && Currencies.ContainsKey(key1))
            {
                CurrencyRate rate = Currencies[key1].GetRate();
                d1 = d1 * rate.Multiplier / rate.Divider * displayCurrencyConverter.Divider / displayCurrencyConverter.Multiplier;
                key1 = currencyId;
            }

            decimal num4 = Math.Round(d1, Currencies.ContainsKey(key1) ? Currencies[key1].Decimals : 2);

            var result = new StockSearchResult
            {
                Id = x.Stock.Id,
                Code = x.Stock.Code,
                Name = x.Stock.Name,
                CodeHtml = FormatText(x.Stock.Code, allTerms),
                NameHtml = FormatText(x.Stock.Name, allTerms),
                Price = num4,
                Currency = Currencies.ContainsKey(key1) ? Currencies[key1].Name : key1,
                CurrencyId = key1,
                Balance = stockBalances.Where(b => b.StockId == x.Stock.Id).Sum(b => b.Balance),
                Unit = x.Stock.Unit,
                UnitId = x.Stock.UnitId,
                IsDisabled = x.Stock.IsDisabled
            };

            var lastPurchasePrice = lastPurchasePrices.SingleOrDefault(p => p.StockId == x.Stock.Id);
            if (lastPurchasePrice != null)
            {
                decimal d2 = lastPurchasePrice.Price;
                string key2 = lastPurchasePrice.CurrencyId;
                if (currencyId != null && currencyId != key2 && Currencies.ContainsKey(key2))
                {
                    CurrencyRate rate = Currencies[key2].GetRate();
                    d2 = d2 * rate.Multiplier / rate.Divider * displayCurrencyConverter.Divider / displayCurrencyConverter.Multiplier;
                    key2 = currencyId;
                }
                decimal num5 = Math.Round(d2, Currencies.ContainsKey(key2) ? Currencies[key2].Decimals : 2);
                result.LastPurchasePrice = num5;
                result.LastPurchaseCurrency = Currencies.ContainsKey(key2) ? Currencies[key2].Name : key2;
                result.LastPurchaseCurrencyId = key2;
            }
            return result;
        });

        return stockSearchResults;
    }

    private string FormatText(string text, string[] searchWords)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        if (searchWords.Contains("b", StringComparer.OrdinalIgnoreCase))
            text = text.ToUpper().Replace("B", "<B>B</B>");

        foreach (string str in searchWords.Where(w => !w.Equals("b", StringComparison.OrdinalIgnoreCase)))
        {
            if (text.Contains(str, StringComparison.OrdinalIgnoreCase))
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