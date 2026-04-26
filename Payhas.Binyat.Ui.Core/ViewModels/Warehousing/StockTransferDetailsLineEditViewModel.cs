// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Warehousing.StockTransferDetailsLineEditViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.StockManagement.Models;
using Payhas.Binyat.Ui.Core.ViewModels.Transactions;
using Payhas.Mvvm.Services;
using System;
using System.Collections.Generic;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Warehousing;

public class StockTransferDetailsLineEditViewModel(
  IMvxMessenger messenger,
  IMvxNavigationService navigationService,
  IUserInteractionService userInteractionService) : 
  TransactionLineEditViewModel<StockTransferDetailsLineEditViewModel.Params, StockTransferDetailsLineEditViewModel.Result>(messenger, navigationService, userInteractionService)
{
  public class Params : StockTransferDetailsLineEditViewModel.Result
  {
    public string StockCode { get; set; }

    public string StockName { get; set; }

    public IEnumerable<StockUnit> Units { get; set; }
  }

  public class Result
  {
    public Decimal Quantity { get; set; }

    public string UnitId { get; set; }

    public Decimal ReceivedQuantity { get; set; }

    public string ReceivedUnitId { get; set; }
  }
}
