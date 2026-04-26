// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.StockManagement.StockAlternativeDetailsViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Payhas.Binyat.StockManagement.Models;
using Payhas.Binyat.StockManagement.Services;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Data.Authorizers;
using Payhas.Data.Extenders;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.StockManagement;

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

  public ICommand SelectedLineDelete
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
    StockAlternativeDetailsViewModel detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__0();
    if (detailsViewModel.Details.Lines == null)
      detailsViewModel.Details.Lines = new ObservableCollection<StockAlternativeLine>();
    await detailsViewModel.LoadStocksCache();
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
