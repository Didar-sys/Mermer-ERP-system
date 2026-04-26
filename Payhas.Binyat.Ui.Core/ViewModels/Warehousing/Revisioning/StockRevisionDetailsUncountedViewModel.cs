// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Warehousing.Revisioning.StockRevisionDetailsUncountedViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Warehousing.Revisioning.Models;
using Payhas.Binyat.Warehousing.Revisioning.Services;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Warehousing.Revisioning;

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
    this._loginService = loginService;
    this._revisionsRepository = revisionsRepository;
  }

  public ObservableCollection<StockRevisionUncountedInfo> List
  {
    get => this._list;
    set
    {
      if (this._list != null)
        this._list.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.List_CollectionChanged);
      this.SetProperty<ObservableCollection<StockRevisionUncountedInfo>>(ref this._list, value, nameof (List));
      if (this._list != null)
        this._list.CollectionChanged += new NotifyCollectionChangedEventHandler(this.List_CollectionChanged);
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.HasAnyItems));
    }
  }

  public bool HasAnyItems
  {
    get
    {
      ObservableCollection<StockRevisionUncountedInfo> list = this.List;
      return list != null && list.Any<StockRevisionUncountedInfo>();
    }
  }

  public ObservableCollection<StockRevisionUncountedInfo> SelectedItems
  {
    get => this._selectedItems;
    set
    {
      if (this._selectedItems != null)
        this._selectedItems.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.SelectedItems_CollectionChanged);
      this.SetProperty<ObservableCollection<StockRevisionUncountedInfo>>(ref this._selectedItems, value, nameof (SelectedItems));
      if (this._selectedItems != null)
        this._selectedItems.CollectionChanged += new NotifyCollectionChangedEventHandler(this.SelectedItems_CollectionChanged);
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.HasAnyItemsSelected));
    }
  }

  public bool HasAnyItemsSelected
  {
    get
    {
      ObservableCollection<StockRevisionUncountedInfo> selectedItems = this.SelectedItems;
      return selectedItems != null && selectedItems.Any<StockRevisionUncountedInfo>();
    }
  }

  public void Prepare(string parameter) => this._revisionId = parameter;

  protected override async Task OnLoad()
  {
    this.SelectedItems = new ObservableCollection<StockRevisionUncountedInfo>();
    this.List = new ObservableCollection<StockRevisionUncountedInfo>(await this._revisionsRepository.GetUncountedAsync(this._revisionId));
  }

  private void List_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
  {
    this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.HasAnyItems));
  }

  private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
  {
    this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.HasAnyItemsSelected));
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
      StockRevisionUncountedInfo[] array = this.SelectedItems.ToArray<StockRevisionUncountedInfo>();
      this.SelectedItems = new ObservableCollection<StockRevisionUncountedInfo>();
      foreach (StockRevisionUncountedInfo revisionUncountedInfo in array)
        this.List.Remove(revisionUncountedInfo);
    }
    catch (Exception ex)
    {
      this.UserInteractionService.ShowExceptionMessage(ex);
    }
    this.IsBusy = false;
  }

  public ICommand AddToRevisionCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnAddToRevisionCommandAsync), (Func<bool>) (() => !this.IsBusy && this.HasAnyItems));
    }
  }

  protected virtual async Task OnAddToRevisionCommandAsync()
  {
    StockRevisionDetailsUncountedViewModel uncountedViewModel = this;
    uncountedViewModel.IsBusy = true;
    try
    {
      // ISSUE: reference to a compiler-generated method
      IEnumerable<StockRevisionLine> list = await Task.Run<IEnumerable<StockRevisionLine>>(new Func<IEnumerable<StockRevisionLine>>(uncountedViewModel.\u003COnAddToRevisionCommandAsync\u003Eb__25_0));
      await uncountedViewModel._revisionsRepository.StoreLinesAsync(uncountedViewModel._revisionId, list);
      int num = await uncountedViewModel.OnCloseAsync() ? 1 : 0;
    }
    catch (Exception ex)
    {
      uncountedViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    uncountedViewModel.IsBusy = false;
  }
}
