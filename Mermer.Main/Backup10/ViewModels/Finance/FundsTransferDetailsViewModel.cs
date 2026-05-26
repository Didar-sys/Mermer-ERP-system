// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Finance.FundsTransferDetailsViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Mermer.Authorization.Services;
using Mermer.Enterprise.Models;
using Mermer.Finance.Models;
using Mermer.FundsManagement.Models;
using Mermer.Transactions.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.ViewModels.Transactions;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using Mermer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Finance;

public class FundsTransferDetailsViewModel(
  IConfigurator configurator,
  ILoginService loginService,
  Reference<Currency> currencies,
  Reference<Depository> depositories,
  IRepository<FundsTransfer> repository,
  IListAuthorizer<FundsTransfer> authorizer,
  IMvxNavigationService navigationService,
  ITransactionCodeGenerationService codegentor,
  IUserInteractionService userInteractionService) : 
  FundsTransactionDetailsViewModel<FundsTransfer, FundsTransferLine>(repository, authorizer, configurator, loginService, currencies, depositories, navigationService, codegentor, userInteractionService)
{
  protected override async Task PostLoad()
  {
    FundsTransferDetailsViewModel detailsViewModel = this;
    IEnumerable<string> usedDepositoryIds = ((IEnumerable<string>) new string[2]
    {
      detailsViewModel.Details.DepositoryId,
      detailsViewModel.Details.DestinationDepositoryId
    }).Distinct<string>();
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__0();
    detailsViewModel.Depositories.Filter = (Func<Depository, bool>) (x => !x.IsDisabled || usedDepositoryIds.Contains<string>(x.Id));
  }

  public ICommand SelectDestinationDepositoryCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectDestinationDepositoryCommandAsync), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess));
    }
  }

  private async Task OnSelectDestinationDepositoryCommandAsync()
  {
    FundsTransferDetailsViewModel detailsViewModel = this;
    FundsTransfer fundsTransfer = detailsViewModel.Details;
    fundsTransfer.DestinationDepositoryId = await detailsViewModel.NavigationService.Navigate<ListViewModel<Depository>, string, string>(detailsViewModel.Details.DestinationDepositoryId ?? Guid.Empty.ToString());
    fundsTransfer = (FundsTransfer) null;
  }
}
