// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Warehousing.Revisioning.StockRevisionsListNewRevisionViewModel
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
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Warehousing.Revisioning;

public class StockRevisionsListNewRevisionViewModel(
  IMvxMessenger messenger,
  IMvxNavigationService navigationService,
  IUserInteractionService userInteractionService) : 
  DialogViewModel(messenger, navigationService, userInteractionService),
  IMvxViewModel<IEnumerable<Warehouse>, StockRevisionsListNewRevisionViewModel.Result>,
  IMvxViewModel<IEnumerable<Warehouse>>,
  IMvxViewModel,
  IMvxViewModelResult<StockRevisionsListNewRevisionViewModel.Result>
{
  private StockRevisionsListNewRevisionViewModel.Result _details;
  private IEnumerable<Warehouse> _warehouses;

  public StockRevisionsListNewRevisionViewModel.Result Details
  {
    get => this._details;
    set
    {
      this.SetProperty<StockRevisionsListNewRevisionViewModel.Result>(ref this._details, value, nameof (Details));
    }
  }

  public IEnumerable<Warehouse> Warehouses
  {
    get => this._warehouses;
    set
    {
      this.SetProperty<IEnumerable<Warehouse>>(ref this._warehouses, value, nameof (Warehouses));
    }
  }

  public void Prepare(IEnumerable<Warehouse> parameter)
  {
    this.Warehouses = parameter.Where<Warehouse>((Func<Warehouse, bool>) (x => !x.IsDisabled));
  }

  protected override async Task OnLoad()
  {
    StockRevisionsListNewRevisionViewModel revisionViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await revisionViewModel.\u003C\u003En__0();
    revisionViewModel.Details = new StockRevisionsListNewRevisionViewModel.Result()
    {
      StartDate = DateTime.Now
    };
  }

  public override Task<bool> OnCloseAsync()
  {
    return this.NavigationService.Close<StockRevisionsListNewRevisionViewModel.Result>((IMvxViewModelResult<StockRevisionsListNewRevisionViewModel.Result>) this, (StockRevisionsListNewRevisionViewModel.Result) null);
  }

  public ICommand StartRevisionCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnStartRevisionCommandAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  protected virtual Task OnStartRevisionCommandAsync()
  {
    return (Task) this.NavigationService.Close<StockRevisionsListNewRevisionViewModel.Result>((IMvxViewModelResult<StockRevisionsListNewRevisionViewModel.Result>) this, this.Details);
  }

  public class Result
  {
    public string WarehouseId { get; set; }

    public DateTime StartDate { get; set; }
  }
}
