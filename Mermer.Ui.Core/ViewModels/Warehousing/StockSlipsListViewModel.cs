// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Warehousing.StockSlipsListViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Mermer.Enterprise.Models;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.ViewModels.Transactions;
using Mermer.Warehousing.Models;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Warehousing;

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
