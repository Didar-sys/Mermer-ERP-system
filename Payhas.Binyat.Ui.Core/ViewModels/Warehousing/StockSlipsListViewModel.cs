// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Warehousing.StockSlipsListViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.ViewModels.Transactions;
using Payhas.Binyat.Warehousing.Models;
using Payhas.Data.Authorizers;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Warehousing;

public class StockSlipsListViewModel(
  IMvxMessenger messenger,
  Reference<Warehouse> warehouses,
  IRepository<StockSlip> repository,
  IListAuthorizer<StockSlip> authorizer,
  IMvxNavigationService navigationService,
  IUserInteractionService userInteractionService) : 
  StockTransactionsListViewModel<StockSlip, StockSlipLine>(messenger, repository, authorizer, warehouses, navigationService, userInteractionService)
{
}
