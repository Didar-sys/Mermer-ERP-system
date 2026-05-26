// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Warehousing.Ordering.StockOrderDetailsLineEditViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Mermer.StockManagement.Models;
using Mermer.Ui.Core.ViewModels.Transactions;
using Mermer.Mvvm.Services;
using System;
using System.Collections.Generic;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Warehousing.Ordering;

public class StockOrderDetailsLineEditViewModel(
  IMvxMessenger messenger,
  IMvxNavigationService navigationService,
  IUserInteractionService userInteractionService) : 
  TransactionLineEditViewModel<StockOrderDetailsLineEditViewModel.Params, StockOrderDetailsLineEditViewModel.Result>(messenger, navigationService, userInteractionService)
{
  public class Params : StockOrderDetailsLineEditViewModel.Result
  {
    public string StockCode { get; set; }

    public string StockName { get; set; }

    public IEnumerable<StockUnit> Units { get; set; }
  }

  public class Result
  {
    public Decimal Quantity { get; set; }

    public string UnitId { get; set; }
  }
}
