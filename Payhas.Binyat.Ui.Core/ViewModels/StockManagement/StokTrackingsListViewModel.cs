// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.StockManagement.StokTrackingsListViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.StockManagement.Models;
using Payhas.Binyat.StockManagement.Services;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.StockManagement;

public class StokTrackingsListViewModel : 
  ListViewModelBase<StockTracking>,
  IMvxViewModel<(string, string)>,
  IMvxViewModel
{
  private string _transactionId;
  private readonly IStockActionsRepository _repository;
  private string _transactionCode;

  public StokTrackingsListViewModel(
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

  public override string SubCaption => this.TransactionCode;

  public string TransactionCode
  {
    get => this._transactionCode;
    private set
    {
      this.SetProperty<string>(ref this._transactionCode, value, nameof (TransactionCode));
    }
  }

  public void Prepare((string, string) parameter)
  {
    this._transactionId = parameter.Item1;
    this._transactionCode = parameter.Item2;
  }

  protected override Task PreLoad() => Task.WhenAll(base.PreLoad(), this.Warehouses.Initialize());

  protected override async Task OnLoad()
  {
    StokTrackingsListViewModel trackingsListViewModel = this;
    IEnumerable<StockTracking> stockTrackings = await trackingsListViewModel._repository.TrackByTransactionIdAsync(trackingsListViewModel._transactionId);
    trackingsListViewModel.List = stockTrackings;
  }
}
