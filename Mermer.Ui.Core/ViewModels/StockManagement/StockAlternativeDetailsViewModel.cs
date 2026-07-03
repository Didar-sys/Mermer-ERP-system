// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.StockManagement.StockAlternativeDetailsViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Data.Authorizers;
using Mermer.Data.Extenders;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.StockManagement;

public class StockAlternativeDetailsViewModel : DetailsViewModel<StockAlternative>
{
  private readonly IRepository<Stock> _stocksRepository;
  private ObservableCollection<Stock> _stocksCache;
  private StockAlternativeLine _selectedLine;

  public StockAlternativeDetailsViewModel(
    StockSearcher stockSearcher,
    IRepository<Stock> stocksRepository,
    IRepository<StockAlternative> repository,
    IListAuthorizer<StockAlternative> authorizer,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(repository, authorizer, navigationService, userInteractionService)
  {
    this._stocksRepository = stocksRepository;
    this.StockSearcher = stockSearcher;
    this.StockSearcher.ResultSelected += new SearchResultSelected(this.StockSearcher_ResultSelected);
  }

  public StockSearcher StockSearcher { get; }

  public ObservableCollection<Stock> StocksCache
  {
    get => this._stocksCache;
    set
    {
      this.SetProperty<ObservableCollection<Stock>>(ref this._stocksCache, value, nameof (StocksCache));
    }
  }

  public StockAlternativeLine SelectedLine
  {
    get => this._selectedLine;
    set
    {
      this.SetProperty<StockAlternativeLine>(ref this._selectedLine, value, nameof (SelectedLine));
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsLineSelected));
    }
  }

  public bool IsLineSelected => this.SelectedLine != null;

  public ICommand SelectedLineDeleteCommand
    {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.OnSelectedLineDelete), (Func<bool>) (() => !this.IsBusy && this.IsLineSelected));
    }
  }

  protected virtual void OnSelectedLineDelete()
  {
    this.SelectedLine = this.Details.Lines.RemoveWithSelection<StockAlternativeLine>(this.SelectedLine);
  }

  protected override Task PreLoad()
  {
    this.StocksCache = new ObservableCollection<Stock>();
    return Task.WhenAll(base.PreLoad(), this.StockSearcher.Initialize());
  }

    protected override async Task PostLoad()
    {
        await base.PostLoad();

        if (Details.Lines == null)
            Details.Lines = new ObservableCollection<StockAlternativeLine>();

        await LoadStocksCache();
    }

    protected override async Task<bool> OnSaveAsync()
    {
        try
        {
            // 1. Обов'язкова перевірка поля "Назва" (Name)
            if (string.IsNullOrEmpty(Details.Name))
            {
                throw new Exception(this["Field '{0}' is required", this["Name"]]);
            }

            // 2. Заборона збереження документа без жодного товару-аналога
            if (Details.Lines == null || !Details.Lines.Any())
            {
                throw new Exception(this["Document cannot be empty"]);
            }
        }
        catch (Exception ex)
        {
            // Перехоплюємо помилку, показуємо локалізоване повідомлення і блокуємо збереження
            UserInteractionService.ShowExceptionMessage(ex);
            return false;
        }

        // Якщо всі перевірки пройдено — виконуємо стандартне збереження
        return await base.OnSaveAsync();
    }

    private async Task LoadStocksCache()
  {
    StockAlternativeDetailsViewModel detailsViewModel = this;
    ObservableCollection<Stock> cache = new ObservableCollection<Stock>();
    foreach (StockAlternativeLine line1 in (Collection<StockAlternativeLine>) detailsViewModel.Details.Lines)
    {
      StockAlternativeLine line = line1;
      if (!cache.Any<Stock>((Func<Stock, bool>) (x => x.Id == line.StockId)))
        cache.Add(await detailsViewModel._stocksRepository.GetAsync(line.StockId));
    }
    detailsViewModel.StocksCache = cache;
    cache = (ObservableCollection<Stock>) null;
  }

  private async void StockSearcher_ResultSelected(StockSearcher searcher, StockSearchResult result)
  {
    StockAlternativeDetailsViewModel detailsViewModel = this;
    if (detailsViewModel.StocksCache.All<Stock>((Func<Stock, bool>) (x => x.Id != result.Id)))
    {
      Stock async = await detailsViewModel._stocksRepository.GetAsync(result.Id);
      detailsViewModel.StocksCache.Add(async);
    }
    detailsViewModel.Details.Lines.Add(new StockAlternativeLine()
    {
      StockId = result.Id
    });
  }
}
