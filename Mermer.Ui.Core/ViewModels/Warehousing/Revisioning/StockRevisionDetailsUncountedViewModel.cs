using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Mermer.Authorization.Services;
using Mermer.Warehousing.Revisioning.Models;
using Mermer.Warehousing.Revisioning.Services;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mermer.Ui.Core.ViewModels.Warehousing.Revisioning;

public class StockRevisionDetailsUncountedViewModel :
    DialogViewModel,
    IMvxViewModel<string>,
    IMvxViewModel
{
    private string _revisionId;
    private readonly ILoginService _loginService;
    private readonly IStockRevisionsRepository _revisionsRepository;
    private ObservableCollection<StockRevisionUncountedInfo> _list;
    private ObservableCollection<StockRevisionUncountedInfo> _selectedItems;

    public StockRevisionDetailsUncountedViewModel(
        IMvxMessenger messenger,
        ILoginService loginService,
        IMvxNavigationService navigationService,
        IStockRevisionsRepository revisionsRepository,
        IUserInteractionService userInteractionService)
        : base(messenger, navigationService, userInteractionService)
    {
        _loginService = loginService;
        _revisionsRepository = revisionsRepository;
    }

    public ObservableCollection<StockRevisionUncountedInfo> List
    {
        get => _list;
        set
        {
            if (_list != null)
                _list.CollectionChanged -= List_CollectionChanged;

            SetProperty(ref _list, value);

            if (_list != null)
                _list.CollectionChanged += List_CollectionChanged;

            RaisePropertyChanged(() => HasAnyItems);
        }
    }

    public bool HasAnyItems => List != null && List.Any();

    public ObservableCollection<StockRevisionUncountedInfo> SelectedItems
    {
        get => _selectedItems;
        set
        {
            if (_selectedItems != null)
                _selectedItems.CollectionChanged -= SelectedItems_CollectionChanged;

            SetProperty(ref _selectedItems, value);

            if (_selectedItems != null)
                _selectedItems.CollectionChanged += SelectedItems_CollectionChanged;

            RaisePropertyChanged(() => HasAnyItemsSelected);
        }
    }

    public bool HasAnyItemsSelected => SelectedItems != null && SelectedItems.Any();

    public void Prepare(string parameter) => _revisionId = parameter;

    protected override async Task OnLoad()
    {
        SelectedItems = new ObservableCollection<StockRevisionUncountedInfo>();
        List = new ObservableCollection<StockRevisionUncountedInfo>(await _revisionsRepository.GetUncountedAsync(_revisionId));
    }

    private void List_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        RaisePropertyChanged(() => HasAnyItems);
    }

    private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        RaisePropertyChanged(() => HasAnyItemsSelected);
    }

    public ICommand SelectedItemsDeleteCommand => new MvxCommand(OnSelectedItemsDeleteCommand, () => !IsBusy && HasAnyItemsSelected);

    protected virtual void OnSelectedItemsDeleteCommand()
    {
        IsBusy = true;
        try
        {
            var array = SelectedItems.ToArray();
            SelectedItems = new ObservableCollection<StockRevisionUncountedInfo>();

            foreach (var item in array)
                List.Remove(item);
        }
        catch (Exception ex)
        {
            UserInteractionService.ShowExceptionMessage(ex);
        }
        IsBusy = false;
    }

    public ICommand AddToRevisionCommand => new MvxAsyncCommand(OnAddToRevisionCommandAsync, () => !IsBusy && HasAnyItems);

    protected virtual async Task OnAddToRevisionCommandAsync()
    {
        IsBusy = true;
        try
        {
            // Відновлена логіка замість загубленого b__25_0
            var list = await Task.Run(() =>
        {
            return List.Select(info => new StockRevisionLine // Прибрали довгий префікс
            {
                Id = Guid.NewGuid().ToString(),
                StockId = info.StockId,
                UnitId = info.StockUnitId, // Твоя правильна правка!
                Quantity = 0
            }).ToList();
        });

            await _revisionsRepository.StoreLinesAsync(_revisionId, list);
            await OnCloseAsync();
        }
        catch (Exception ex)
        {
            UserInteractionService.ShowExceptionMessage(ex);
        }
        IsBusy = false;
    }
}