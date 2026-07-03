// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Warehousing.Ordering.StockOrderTemplateDetailsViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Mermer.Enterprise.Models;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.ViewModels.Common;
using Mermer.Warehousing.Ordering.Models;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Warehousing.Ordering;

public class StockOrderTemplateDetailsViewModel : 
  DetailsViewModel<StockOrderTemplate>,
  IMvxViewModel<IEnumerable<CopyCreateLine>>,
  IMvxViewModel
{
  private readonly IRepository<Stock> _stocksRepository;
  private ObservableCollection<Stock> _stocksCache;
  private IEnumerable<CopyCreateLine> _stockLineCopies;
  private StockOrderTemplateLine _selectedLine;

  public StockOrderTemplateDetailsViewModel(
    CopyCreate copyCreate,
    StockSearcher stockSearcher,
    IRepository<Stock> stocksRepository,
    IRepository<StockOrderTemplate> repository,
    IListAuthorizer<StockOrderTemplate> authorizer,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(repository, authorizer, navigationService, userInteractionService)
  {
    this._stocksRepository = stocksRepository;
    this.CopyCreate = copyCreate;
    this.CopyCreate.GetLines = (Func<IEnumerable<CopyCreateLine>>) (() => this.Details.Lines.Select<StockOrderTemplateLine, CopyCreateLine>((Func<StockOrderTemplateLine, CopyCreateLine>) (x => new CopyCreateLine()
    {
      StockId = x.StockId
    })));
    this.StockSearcher = stockSearcher;
    this.StockSearcher.ResultSelected += new SearchResultSelected(this.StockSearcher_ResultSelected);
  }

  public ObservableCollection<Stock> StocksCache
  {
    get => this._stocksCache;
    set
    {
      this.SetProperty<ObservableCollection<Stock>>(ref this._stocksCache, value, nameof (StocksCache));
    }
  }

  public void Prepare(IEnumerable<CopyCreateLine> parameter) => this._stockLineCopies = parameter;

  protected override Task PreLoad()
  {
    return Task.WhenAll(base.PreLoad(), this.StockSearcher.Initialize());
  }

    protected override async Task PostLoad()
    {
        await base.PostLoad();

        if (Details.Lines == null)
        {
            Details.Lines = new ObservableCollection<StockOrderTemplateLine>();

            if (_stockLineCopies != null)
            {
                foreach (CopyCreateLine stockLineCopy in _stockLineCopies)
                {
                    Details.Lines.Add(new StockOrderTemplateLine
                    {
                        StockId = stockLineCopy.StockId
                    });
                }
                _stockLineCopies = null;
            }
        }

        await LoadStocksCache();
    }

    private async Task LoadStocksCache()
  {
    StockOrderTemplateDetailsViewModel detailsViewModel = this;
    ObservableCollection<Stock> cache = new ObservableCollection<Stock>();
    foreach (StockOrderTemplateLine line1 in (Collection<StockOrderTemplateLine>) detailsViewModel.Details.Lines)
    {
      StockOrderTemplateLine line = line1;
      if (!cache.Any<Stock>((Func<Stock, bool>) (x => x.Id == line.StockId)))
        cache.Add(await detailsViewModel._stocksRepository.GetAsync(line.StockId));
    }
    detailsViewModel.StocksCache = cache;
    cache = (ObservableCollection<Stock>) null;
  }

  private async void StockSearcher_ResultSelected(StockSearcher searcher, StockSearchResult result)
  {
    StockOrderTemplateDetailsViewModel detailsViewModel = this;
    if (detailsViewModel.StocksCache.All<Stock>((Func<Stock, bool>) (x => x.Id != result.Id)))
    {
      Stock async = await detailsViewModel._stocksRepository.GetAsync(result.Id);
      detailsViewModel.StocksCache.Add(async);
    }
    StockOrderTemplateLine orderTemplateLine = new StockOrderTemplateLine()
    {
      StockId = result.Id
    };
    detailsViewModel.Details.Lines.Add(orderTemplateLine);
    detailsViewModel.SelectedLine = orderTemplateLine;
  }

  public CopyCreate CopyCreate { get; }

  public StockSearcher StockSearcher { get; set; }

  public Reference<Warehouse> Warehouses { get; set; }

  public bool IsLineSelected => this.SelectedLine != null;

  public virtual StockOrderTemplateLine SelectedLine
  {
    get => this._selectedLine;
    set
    {
      this.SetProperty<StockOrderTemplateLine>(ref this._selectedLine, value, nameof (SelectedLine));
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsLineSelected));
    }
  }

  public ICommand SelectedLineDeleteCommand
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.SelectedLineDelete), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess && this.IsLineSelected));
    }
  }

  private void SelectedLineDelete()
  {
    int num = this.Details.Lines.IndexOf(this.SelectedLine);
    this.Details.Lines.Remove(this.SelectedLine);
    int index = num - 1;
    if (index < 0)
      index = 0;
    if (index >= this.Details.Lines.Count)
      return;
    this.SelectedLine = this.Details.Lines.ElementAt<StockOrderTemplateLine>(index);
  }

  public ICommand ImportCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnImportCommandAsync), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess));
    }
  }

  protected virtual async Task OnImportCommandAsync()
  {
    StockOrderTemplateDetailsViewModel detailsViewModel = this;
    IEnumerable<object> source1 = await detailsViewModel.NavigationService.Navigate<DataImportViewModel, Type, IEnumerable<object>>(typeof (StockOrderTemplateDetailsViewModel.LineImport));
    int i = 0;
    detailsViewModel.IsBusy = true;
    detailsViewModel.SuspendLoading = true;
    try
    {
      IEnumerable<StockOrderTemplateDetailsViewModel.LineImport> source2 = source1 != null ? source1.Cast<StockOrderTemplateDetailsViewModel.LineImport>() : (IEnumerable<StockOrderTemplateDetailsViewModel.LineImport>) null;
      if (source2 != null)
      {
        int itemsCount = source2.Count<StockOrderTemplateDetailsViewModel.LineImport>();
        foreach (StockOrderTemplateDetailsViewModel.LineImport lineImport in source2)
        {
          StockOrderTemplateDetailsViewModel.LineImport item = lineImport;
          ++i;
          detailsViewModel.Status = detailsViewModel["Importing {0} of {1} lines", new object[2]
          {
            (object) i,
            (object) itemsCount
          }];
          Stock stock = detailsViewModel.StocksCache.SingleOrDefault<Stock>((Func<Stock, bool>) (x => x.Code != item.StockCode));
          if (stock == null)
          {
            stock = (await detailsViewModel._stocksRepository.GetAsync((Expression<Func<Stock, bool>>) (x => x.Code == item.StockCode))).Single<Stock>();
            detailsViewModel.StocksCache.Add(stock);
          }
          StockOrderTemplateLine orderTemplateLine = new StockOrderTemplateLine()
          {
            StockId = stock.Id
          };
          detailsViewModel.Details.Lines.Add(orderTemplateLine);
        }
      }
    }
    catch (Exception ex)
    {
      detailsViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    detailsViewModel.Status = (string) null;
    detailsViewModel.SuspendLoading = false;
    detailsViewModel.IsBusy = false;
  }
    protected override async Task<bool> OnSaveAsync()
    {
        try
        {
            // 1. Обов'язкова перевірка поля Назва (Name)
            if (string.IsNullOrEmpty(Details.Name))
            {
                throw new Exception(this["Field '{0}' is required", this["Name"]]);
            }

            // 2. Перевірка наявності рядків (заборона збереження порожнього шаблону)
            if (Details.Lines == null || !Details.Lines.Any())
            {
                throw new Exception(this["Document cannot be empty"]);
            }
        }
        catch (Exception ex)
        {
            // Показуємо вікно з помилкою і блокуємо збереження в базу
            UserInteractionService.ShowExceptionMessage(ex);
            return false;
        }

        // Якщо все заповнено — зберігаємо шаблон
        return await base.OnSaveAsync();
    }
    public class LineImport
  {
    public string StockCode { get; internal set; }
  }
}
