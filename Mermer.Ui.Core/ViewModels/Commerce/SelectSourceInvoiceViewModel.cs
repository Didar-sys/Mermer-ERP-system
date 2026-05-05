// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Commerce.SelectSourceInvoiceViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Mermer.Enterprise.Models;
using Mermer.StockManagement.Models;
using Mermer.StockManagement.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Commerce;

public class SelectSourceInvoiceViewModel : 
  DialogViewModel,
  IMvxViewModelResult<IEnumerable<StockAction>>,
  IMvxViewModel
{
  private readonly IStockActionsRepository _repository;
  private string _invoiceCode;
  private IEnumerable<StockAction> _list;
  private ObservableCollection<StockAction> _selectedLines;

  public SelectSourceInvoiceViewModel(
    IMvxMessenger messenger,
    Reference<Warehouse> warehouses,
    IStockActionsRepository repository,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._repository = repository;
    this.Warehouses = warehouses;
  }

  public Reference<Warehouse> Warehouses { get; }

  public virtual string InvoiceCode
  {
    get => this._invoiceCode;
    set => this.SetProperty<string>(ref this._invoiceCode, value, nameof (InvoiceCode));
  }

  public virtual IEnumerable<StockAction> List
  {
    get => this._list;
    set => this.SetProperty<IEnumerable<StockAction>>(ref this._list, value, nameof (List));
  }

  public virtual ObservableCollection<StockAction> SelectedLines
  {
    get => this._selectedLines;
    set
    {
      this.SetProperty<ObservableCollection<StockAction>>(ref this._selectedLines, value, nameof (SelectedLines));
    }
  }

  protected override Task PreLoad() => Task.WhenAll(base.PreLoad(), this.Warehouses.Initialize());

  public ICommand SearchInvoices
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSearchInvoicesAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private async Task OnSearchInvoicesAsync()
  {
    SelectSourceInvoiceViewModel viewModel = this;
    viewModel.IsBusy = true;
    try
    {
      viewModel.SelectedLines = new ObservableCollection<StockAction>();
    }
    catch (Exception ex)
    {
      viewModel.UserInteractionService.ShowExceptionMessage(ex);
      viewModel.Close((IMvxViewModel) viewModel);
    }
    viewModel.IsBusy = false;
  }

  public ICommand Select
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private Task OnSelectAsync()
  {
    return (Task) this.NavigationService.Close<IEnumerable<StockAction>>((IMvxViewModelResult<IEnumerable<StockAction>>) this, (IEnumerable<StockAction>) this.SelectedLines);
  }

  public override Task<bool> OnCloseAsync()
  {
    return this.NavigationService.Close<IEnumerable<StockAction>>((IMvxViewModelResult<IEnumerable<StockAction>>) this, (IEnumerable<StockAction>) null);
  }
}
