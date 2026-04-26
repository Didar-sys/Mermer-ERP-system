// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Warehousing.Ordering.NewStockTransferDialogViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Warehousing.Ordering;

public class NewStockTransferDialogViewModel(
  IMvxMessenger messenger,
  IMvxNavigationService navigationService,
  IUserInteractionService userInteractionService) : 
  DialogViewModel(messenger, navigationService, userInteractionService),
  IMvxViewModel<NewStockTransferDialogViewModel.Params, string>,
  IMvxViewModel<NewStockTransferDialogViewModel.Params>,
  IMvxViewModel,
  IMvxViewModelResult<string>
{
  private NewStockTransferDialogViewModel.Params _details;
  private string _destinationWarehouseId;

  public NewStockTransferDialogViewModel.Params Details
  {
    get => this._details;
    set
    {
      this.SetProperty<NewStockTransferDialogViewModel.Params>(ref this._details, value, nameof (Details));
    }
  }

  public string DestinationWarehouseId
  {
    get => this._destinationWarehouseId;
    set
    {
      this.SetProperty<string>(ref this._destinationWarehouseId, value, nameof (DestinationWarehouseId));
    }
  }

  public void Prepare(NewStockTransferDialogViewModel.Params parameter) => this.Details = parameter;

  public ICommand SaveCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSaveAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  protected virtual Task OnSaveAsync()
  {
    return (Task) this.NavigationService.Close<string>((IMvxViewModelResult<string>) this, this.DestinationWarehouseId);
  }

  public override Task<bool> OnCloseAsync()
  {
    return this.NavigationService.Close<string>((IMvxViewModelResult<string>) this, (string) null);
  }

  public class Params
  {
    public Warehouse SourceWarehouse { get; set; }

    public IEnumerable<Warehouse> Warehouses { get; set; }
  }
}
