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

    // Добавлено для работы групп и тегов
    private string[] _groupNames;
    private string[] _tagNames;

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
        this.CopyCreate.GetLines = () => this.Details.Lines.Select(x => new CopyCreateLine { StockId = x.StockId });
        this.StockSearcher = stockSearcher;
        this.StockSearcher.ResultSelected += StockSearcher_ResultSelected;
    }

    public virtual string[] GroupNames
    {
        get => this._groupNames;
        set => this.SetProperty(ref this._groupNames, value, nameof(GroupNames));
    }

    public virtual string[] TagNames
    {
        get => this._tagNames;
        set => this.SetProperty(ref this._tagNames, value, nameof(TagNames));
    }

    protected virtual async Task LoadFacetsAsync()
    {
        var repositoryWithFacets = this.Repository as IRepositoryWithFacets<StockOrderTemplate>;
        if (repositoryWithFacets != null)
        {
            var facets = await repositoryWithFacets.GetFacets("GroupNames", "TagNames");
            if (facets != null)
            {
                if (facets.ContainsKey("GroupNames"))
                    this.GroupNames = facets["GroupNames"].Select(x => x.Key).ToArray();

                if (facets.ContainsKey("TagNames"))
                    this.TagNames = facets["TagNames"].Select(x => x.Key).ToArray();
            }
        }
    }

    public ObservableCollection<Stock> StocksCache
    {
        get => this._stocksCache;
        set => this.SetProperty(ref this._stocksCache, value, nameof(StocksCache));
    }

    public void Prepare(IEnumerable<CopyCreateLine> parameter) => this._stockLineCopies = parameter;

    protected override Task PreLoad()
    {
        // Загружаем фасеты (теги/группы) вместе с поиском
        return Task.WhenAll(base.PreLoad(), LoadFacetsAsync(), this.StockSearcher.Initialize());
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

        // ИСПРАВЛЕНИЕ: Инициализируем теги пустой коллекцией, если они null
        if (Details.Tags == null)
            Details.Tags = new List<string>();

        await LoadStocksCache();
    }

    private async Task LoadStocksCache()
    {
        ObservableCollection<Stock> cache = new ObservableCollection<Stock>();
        foreach (StockOrderTemplateLine line in this.Details.Lines)
        {
            if (!cache.Any(x => x.Id == line.StockId))
                cache.Add(await this._stocksRepository.GetAsync(line.StockId));
        }
        this.StocksCache = cache;
    }

    private async void StockSearcher_ResultSelected(StockSearcher searcher, StockSearchResult result)
    {
        if (this.StocksCache.All(x => x.Id != result.Id))
        {
            Stock async = await this._stocksRepository.GetAsync(result.Id);
            this.StocksCache.Add(async);
        }
        StockOrderTemplateLine orderTemplateLine = new StockOrderTemplateLine { StockId = result.Id };
        this.Details.Lines.Add(orderTemplateLine);
        this.SelectedLine = orderTemplateLine;
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
            this.SetProperty(ref this._selectedLine, value, nameof(SelectedLine));
            this.RaisePropertyChanged(() => this.IsLineSelected);
        }
    }

    public ICommand SelectedLineDeleteCommand => new MvxCommand(this.SelectedLineDelete, () => !this.IsBusy && this.HasSaveAccess && this.IsLineSelected);

    private void SelectedLineDelete()
    {
        int num = this.Details.Lines.IndexOf(this.SelectedLine);
        this.Details.Lines.Remove(this.SelectedLine);
        int index = num - 1;
        if (index < 0) index = 0;
        if (index >= this.Details.Lines.Count) return;
        this.SelectedLine = this.Details.Lines.ElementAt(index);
    }

    public ICommand ImportCommand => new MvxAsyncCommand(this.OnImportCommandAsync, () => !this.IsBusy && this.HasSaveAccess);

    protected virtual async Task OnImportCommandAsync()
    {
        IEnumerable<object> source1 = await this.NavigationService.Navigate<DataImportViewModel, Type, IEnumerable<object>>(typeof(StockOrderTemplateDetailsViewModel.LineImport));
        int i = 0;
        this.IsBusy = true;
        this.SuspendLoading = true;
        try
        {
            if (source1 != null)
            {
                var source2 = source1.Cast<StockOrderTemplateDetailsViewModel.LineImport>();
                int itemsCount = source2.Count();
                foreach (StockOrderTemplateDetailsViewModel.LineImport item in source2)
                {
                    ++i;
                    this.Status = this["Importing {0} of {1} lines", i, itemsCount];
                    Stock stock = this.StocksCache.FirstOrDefault(x => x.Code == item.StockCode);
                    if (stock == null)
                    {
                        stock = (await this._stocksRepository.GetAsync(x => x.Code == item.StockCode)).FirstOrDefault();
                        if (stock != null) this.StocksCache.Add(stock);
                    }
                    if (stock != null)
                    {
                        this.Details.Lines.Add(new StockOrderTemplateLine { StockId = stock.Id });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            this.UserInteractionService.ShowExceptionMessage(ex);
        }
        this.Status = null;
        this.SuspendLoading = false;
        this.IsBusy = false;
    }

    protected override async Task<bool> OnSaveAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(Details.Name))
                throw new Exception(this["Field '{0}' is required", this["Name"]]);

            if (Details.Lines == null || !Details.Lines.Any())
                throw new Exception(this["Document cannot be empty"]);
        }
        catch (Exception ex)
        {
            UserInteractionService.ShowExceptionMessage(ex);
            return false;
        }

        return await base.OnSaveAsync();
    }

    public class LineImport
    {
        public string StockCode { get; internal set; }
    }
}