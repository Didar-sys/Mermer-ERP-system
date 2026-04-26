// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.StockManagement.StockMergerDialogViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using AutoMapper;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.StockManagement.Services;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Data.Models;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.StockManagement;

public class StockMergerDialogViewModel : DialogViewModel
{
  private readonly IMapper _mapper;
  private readonly IStocksRepository _stocksRepository;
  private ObservableCollection<StockMerge> _list;
  private ObservableCollection<StockMerge> _selectedItems;
  private bool _disableMergedItems = true;

  public StockMergerDialogViewModel(
    IMapper mapper,
    IMvxMessenger messenger,
    StockSearcher stockSearcher,
    IStocksRepository stocksRepository,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._mapper = mapper;
    this._stocksRepository = stocksRepository;
    this.StockSearcher = stockSearcher;
    this.StockSearcher.ResultSelected += new SearchResultSelected(this.StockSelected);
  }

  public StockSearcher StockSearcher { get; }

  public virtual ObservableCollection<StockMerge> List
  {
    get => this._list;
    set
    {
      if (this._list != null)
        this._list.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.List_CollectionChanged);
      this.SetProperty<ObservableCollection<StockMerge>>(ref this._list, value, nameof (List));
      if (this._list != null)
        this._list.CollectionChanged += new NotifyCollectionChangedEventHandler(this.List_CollectionChanged);
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.HasAnyItems));
    }
  }

  public bool HasAnyItems
  {
    get
    {
      ObservableCollection<StockMerge> list = this.List;
      return list != null && list.Any<StockMerge>();
    }
  }

  public ObservableCollection<StockMerge> SelectedItems
  {
    get => this._selectedItems;
    set
    {
      if (this._selectedItems != null)
        this._selectedItems.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.SelectedItems_CollectionChanged);
      this.SetProperty<ObservableCollection<StockMerge>>(ref this._selectedItems, value, nameof (SelectedItems));
      if (this._selectedItems != null)
        this._selectedItems.CollectionChanged += new NotifyCollectionChangedEventHandler(this.SelectedItems_CollectionChanged);
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.HasAnyItemsSelected));
    }
  }

  public bool HasAnyItemsSelected
  {
    get
    {
      ObservableCollection<StockMerge> selectedItems = this.SelectedItems;
      return selectedItems != null && selectedItems.Any<StockMerge>();
    }
  }

  public virtual bool DisableMergedItems
  {
    get => this._disableMergedItems;
    set => this.SetProperty<bool>(ref this._disableMergedItems, value, nameof (DisableMergedItems));
  }

  private void List_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
  {
    if (e.OldItems != null)
    {
      foreach (BindableObject bindableObject in e.OldItems.Cast<StockMerge>())
        bindableObject.PropertyChanged -= new PropertyChangedEventHandler(this.Item_PropertyChanged);
    }
    if (e.NewItems != null)
    {
      foreach (BindableObject bindableObject in e.NewItems.Cast<StockMerge>())
        bindableObject.PropertyChanged += new PropertyChangedEventHandler(this.Item_PropertyChanged);
    }
    this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.HasAnyItems));
  }

  private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    if (!(e.PropertyName == "IsMain"))
      return;
    StockMerge item = sender as StockMerge;
    if (item == null || !item.IsMain)
      return;
    foreach (StockMerge stockMerge in this.List.Where<StockMerge>((Func<StockMerge, bool>) (x => x.Id != item.Id)))
      stockMerge.IsMain = false;
  }

  private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
  {
    this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.HasAnyItemsSelected));
  }

  protected override Task PreLoad()
  {
    this.List = new ObservableCollection<StockMerge>();
    this.SelectedItems = new ObservableCollection<StockMerge>();
    return Task.WhenAll(base.PreLoad(), this.StockSearcher.Initialize());
  }

  private void StockSelected(StockSearcher searcher, StockSearchResult result)
  {
    if (this.List.Any<StockMerge>((Func<StockMerge, bool>) (x => x.Id == result.Id)))
      return;
    this.List.Add(this._mapper.Map<StockMerge>((object) result));
  }

  public ICommand MergeCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnMergeAsync), (Func<bool>) (() => !this.IsBusy && this.HasAnyItems));
    }
  }

  public virtual async Task OnMergeAsync()
  {
    StockMergerDialogViewModel mergerDialogViewModel = this;
    mergerDialogViewModel.IsBusy = true;
    try
    {
      if (mergerDialogViewModel.List.Count <= 1)
        throw new Exception(mergerDialogViewModel["Invalid Operation", Array.Empty<object>()], new Exception(mergerDialogViewModel["At least two items must be selected", Array.Empty<object>()]));
      string id = mergerDialogViewModel.List.Single<StockMerge>((Func<StockMerge, bool>) (x => x.IsMain)).Id;
      string[] array = mergerDialogViewModel.List.Where<StockMerge>((Func<StockMerge, bool>) (x => !x.IsMain)).Select<StockMerge, string>((Func<StockMerge, string>) (x => x.Id)).ToArray<string>();
      await mergerDialogViewModel._stocksRepository.MergeAsync(id, array, mergerDialogViewModel.DisableMergedItems);
    }
    catch (InvalidOperationException ex)
    {
      mergerDialogViewModel.UserInteractionService.ShowMessage(mergerDialogViewModel["Invalid Operation", Array.Empty<object>()], mergerDialogViewModel["One item (only) must be selected as Main", Array.Empty<object>()]);
    }
    catch (Exception ex)
    {
      mergerDialogViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    mergerDialogViewModel.IsBusy = false;
  }

  public ICommand SelectedItemsDeleteCommand
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.OnSelectedItemsDeleteCommand), (Func<bool>) (() => !this.IsBusy && this.HasAnyItemsSelected));
    }
  }

  protected virtual void OnSelectedItemsDeleteCommand()
  {
    this.IsBusy = true;
    try
    {
      StockMerge[] array = this.SelectedItems.ToArray<StockMerge>();
      this.SelectedItems = new ObservableCollection<StockMerge>();
      foreach (StockMerge stockMerge in array)
        this.List.Remove(stockMerge);
    }
    catch (Exception ex)
    {
      this.UserInteractionService.ShowExceptionMessage(ex);
    }
    this.IsBusy = false;
  }
}
