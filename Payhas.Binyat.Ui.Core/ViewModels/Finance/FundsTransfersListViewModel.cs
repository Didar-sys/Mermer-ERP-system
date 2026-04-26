// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Finance.FundsTransfersListViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.Finance.Models;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.ViewModels.Transactions;
using Payhas.Data.Authorizers;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Finance;

public class FundsTransfersListViewModel(
  IMvxMessenger messenger,
  Reference<Depository> depositories,
  IRepository<FundsTransfer> repository,
  IListAuthorizer<FundsTransfer> authorizer,
  IMvxNavigationService navigationService,
  IUserInteractionService userInteractionService) : 
  FundsTransactionsListViewModel<FundsTransfer, FundsTransferLine>(messenger, repository, authorizer, depositories, navigationService, userInteractionService)
{
}
