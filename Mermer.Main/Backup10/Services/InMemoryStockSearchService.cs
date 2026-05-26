// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.Services.InMemoryStockSearchService
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

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

#nullable disable
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
    this._stocksRepository = stocksRepository;
    this._currenciesRepository = currenciesRepository;
    this._balancesRepository = balancesRepository;
    this._transliterationService = transliterationService;
    this._lastPurchasePricesRepository = lastPurchasePricesRepository;
    this._messageToken = messenger.Subscribe<DocumentModified<Stock>>((Action<DocumentModified<Stock>>) (async m => await this.Initialize(true)), MvxReference.Strong);
  }

  private List<Stock> Stocks { get; set; }

  private Dictionary<string, Currency> Currencies { get; set; }

  public async Task Initialize(bool forceReload)
  {
    if (this._isInitializing || this._isInitialized && !forceReload)
      return;
    this._isInitializing = true;
    this.Stocks = (await this._stocksRepository.GetAsync()).ToList<Stock>();
    this.Currencies = (await this._currenciesRepository.GetAsync()).ToDictionary<Currency, string, Currency>((Func<Currency, string>) (x => x.Id), (Func<Currency, Currency>) (x => x));
    this._isInitialized = true;
    this._isInitializing = false;
  }

  public async Task<IEnumerable<StockSearchResult>> Search(
    string text,
    string warehouseId,
    string priceGroup = null,
    string currencyId = null,
    CancellationToken cancellationToken = default (CancellationToken))
  {
    text = text.ToLower();
    string[][] array = Enum.GetValues(typeof (TransliterationType)).Cast<TransliterationType>().Select<TransliterationType, string[]>((Func<TransliterationType, string[]>) (x => this._transliterationService.Parse(text, x).ToArray<string>())).ToArray<string[]>();
    string[] allTerms = ((IEnumerable<string[]>) array).Aggregate<string[], IEnumerable<string>>((IEnumerable<string>) Array.Empty<string>(), (Func<IEnumerable<string>, string[], IEnumerable<string>>) ((current, term) => current.Concat<string>((IEnumerable<string>) term))).Distinct<string>().ToArray<string>();
    string[] sequantialTerms = ((IEnumerable<string[]>) array).Select<string[], string>(new Func<string[], string>(InMemoryStockSearchService.ArrayToRegular)).Distinct<string>().ToArray<string>();
    this.CheckIfCanceled(cancellationToken);
    List<\u003C\u003Ef__AnonymousType0<Stock, int>> searchResults = this.Stocks.Select(x => new
    {
      Stock = x,
      Boost = ((IEnumerable<string>) allTerms).Count<string>((Func<string, bool>) (t =>
      {
        string code = x.Code;
        if ((code != null ? (code.ToLower().Contains(t) ? 1 : 0) : 0) == 0)
        {
          string name = x.Name;
          if ((name != null ? (name.ToLower().Contains(t) ? 1 : 0) : 0) == 0)
          {
            string shortName = x.ShortName;
            if ((shortName != null ? (shortName.ToLower().Contains(t) ? 1 : 0) : 0) == 0)
            {
              IEnumerable<string> barcodes = x.Barcodes;
              if ((barcodes != null ? (barcodes.Any<string>((Func<string, bool>) (i => i.ToLower().Contains(t))) ? 1 : 0) : 0) == 0)
              {
                IEnumerable<string> tags = x.Tags;
                return tags != null && tags.Any<string>((Func<string, bool>) (i => i.ToLower().Contains(t)));
              }
            }
          }
        }
        return true;
      }))
    }).Where(x => x.Boost > 0).Select(x =>
    {
      Stock stock = x.Stock;
      int num1 = x.Boost + (x.Stock.Code.ToLower() == text ? 10 : 0);
      IEnumerable<string> barcodes = x.Stock.Barcodes;
      int num2 = (barcodes != null ? (barcodes.Any<string>((Func<string, bool>) (b => b.ToLower() == text)) ? 1 : 0) : 0) != 0 ? 10 : 0;
      int num3 = num1 + num2 + ((IEnumerable<string>) allTerms).Count<string>((Func<string, bool>) (t =>
      {
        IEnumerable<string> tags = x.Stock.Tags;
        return tags != null && tags.Contains<string>(t);
      })) * 2 + (Regex.IsMatch(x.Stock.Code, InMemoryStockSearchService.WildCardToRegular("*" + text)) ? 5 : 0) + ((IEnumerable<string>) sequantialTerms).Count<string>((Func<string, bool>) (t => Regex.IsMatch(x.Stock.Name.ToLower(), t))) * 5;
      return new{ Stock = stock, Boost = num3 };
    }).OrderByDescending(x => x.Boost).Take(64 /*0x40*/).ToList();
    this.CheckIfCanceled(cancellationToken);
    string[] stockIds = searchResults.Select(x => x.Stock.Id).ToArray<string>();
    StockBalance[] stockBalances;
    try
    {
      if (string.IsNullOrEmpty(warehouseId))
        throw new Exception();
      stockBalances = (await this._balancesRepository.GetAsync(warehouseId, stockIds)).ToArray<StockBalance>();
    }
    catch (Exception ex)
    {
      stockBalances = Array.Empty<StockBalance>();
    }
    this.CheckIfCanceled(cancellationToken);
    LastPurchasePrice[] lastPurchasePrices;
    try
    {
      if (this.lastPriceCheckFailsCount < 3)
      {
        if (string.IsNullOrEmpty(warehouseId))
          throw new Exception();
        lastPurchasePrices = (await this._lastPurchasePricesRepository.GetAsync(warehouseId, stockIds)).ToArray<LastPurchasePrice>();
      }
      else
        lastPurchasePrices = Array.Empty<LastPurchasePrice>();
    }
    catch (Exception ex)
    {
      ++this.lastPriceCheckFailsCount;
      lastPurchasePrices = Array.Empty<LastPurchasePrice>();
    }
    this.CheckIfCanceled(cancellationToken);
    CurrencyRate currencyRate;
    if (!string.IsNullOrEmpty(currencyId))
    {
      currencyRate = this.Currencies[currencyId].GetRate();
    }
    else
    {
      currencyRate = new CurrencyRate();
      currencyRate.Multiplier = 1M;
      currencyRate.Divider = 1M;
    }
    CurrencyRate displayCurrencyConverter = currencyRate;
    IEnumerable<StockSearchResult> stockSearchResults = searchResults.Select(x =>
    {
      Stock stock = x.Stock;
      string str = priceGroup;
      DateTime? date = new DateTime?();
      string priceGroup1 = str;
      StockPrice price = stock.GetPrice(date, priceGroup1);
      Decimal d1 = price.Price;
      string key1 = price.CurrencyId;
      if (currencyId != null && currencyId != key1)
      {
        CurrencyRate rate = this.Currencies[key1].GetRate();
        d1 = d1 * rate.Multiplier / rate.Divider * displayCurrencyConverter.Divider / displayCurrencyConverter.Multiplier;
        key1 = currencyId;
      }
      Decimal num4 = Math.Round(d1, this.Currencies[key1].Decimals);
      StockSearchResult stockSearchResult = new StockSearchResult()
      {
        Id = x.Stock.Id,
        Code = x.Stock.Code,
        Name = x.Stock.Name,
        CodeHtml = this.FormatText(x.Stock.Code, allTerms),
        NameHtml = this.FormatText(x.Stock.Name, allTerms),
        Price = num4,
        Currency = this.Currencies[key1].Name,
        CurrencyId = key1,
        Balance = ((IEnumerable<StockBalance>) stockBalances).Where<StockBalance>((Func<StockBalance, bool>) (b => b.StockId == x.Stock.Id)).Sum<StockBalance>((Func<StockBalance, Decimal>) (b => b.Balance)),
        Unit = x.Stock.Unit,
        UnitId = x.Stock.UnitId,
        IsDisabled = x.Stock.IsDisabled
      };
      LastPurchasePrice lastPurchasePrice = ((IEnumerable<LastPurchasePrice>) lastPurchasePrices).SingleOrDefault<LastPurchasePrice>((Func<LastPurchasePrice, bool>) (p => p.StockId == x.Stock.Id));
      if (lastPurchasePrice != null)
      {
        Decimal d2 = lastPurchasePrice.Price;
        string key2 = lastPurchasePrice.CurrencyId;
        if (currencyId != null && currencyId != key2)
        {
          CurrencyRate rate = this.Currencies[key2].GetRate();
          d2 = d2 * rate.Multiplier / rate.Divider * displayCurrencyConverter.Divider / displayCurrencyConverter.Multiplier;
          key2 = currencyId;
        }
        Decimal num5 = Math.Round(d2, this.Currencies[key2].Decimals);
        stockSearchResult.LastPurchasePrice = new Decimal?(num5);
        stockSearchResult.LastPurchaseCurrency = this.Currencies[key2].Name;
        stockSearchResult.LastPurchaseCurrencyId = key2;
      }
      return stockSearchResult;
    });
    searchResults = null;
    stockIds = (string[]) null;
    return stockSearchResults;
  }

  private string FormatText(string text, string[] searchWords)
  {
    if (((IEnumerable<string>) searchWords).Contains<string>("b") || ((IEnumerable<string>) searchWords).Contains<string>("B"))
      text = text.ToUpper().Replace("B", "<B>B</B>");
    foreach (string str in ((IEnumerable<string>) searchWords).Where<string>((Func<string, bool>) (w => w != "b" && w != "B")))
    {
      if (text.ToUpper().Contains(str.ToUpper()))
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

  private static string ArrayToRegular(string[] values)
  {
    return InMemoryStockSearchService.WildCardToRegular(string.Join("* ", values) + "*");
  }

  private static string WildCardToRegular(string value)
  {
    return $"^{Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*")}$";
  }

  public void Dispose() => this._messageToken?.Dispose();
}
