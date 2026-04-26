// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Finance.FundsSlipDetailsViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.Finance.Models;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.Transactions.Services;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.ViewModels.Transactions;
using Payhas.Data.Authorizers;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;
using Payhas.Services;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Finance;

public class FundsSlipDetailsViewModel(
  IConfigurator configurator,
  ILoginService loginService,
  Reference<Currency> currencies,
  IRepository<FundsSlip> repository,
  IListAuthorizer<FundsSlip> authorizer,
  Reference<Depository> depositories,
  IMvxNavigationService navigationService,
  ITransactionCodeGenerationService codegentor,
  IUserInteractionService userInteractionService) : 
  FundsTransactionDetailsViewModel<FundsSlip, FundsSlipLine, FundsSlipType>(repository, authorizer, configurator, loginService, currencies, depositories, navigationService, codegentor, userInteractionService),
  IMvxViewModel<FundsSlipType>,
  IMvxViewModel
{
  private FundsSlipType _newSlipType;

  public void Prepare(FundsSlipType parameter) => this._newSlipType = parameter;

  protected override async Task PostLoad()
  {
    FundsSlipDetailsViewModel detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__0();
    if (!string.IsNullOrEmpty(detailsViewModel.ItemId))
      return;
    detailsViewModel.Details.SlipType = detailsViewModel._newSlipType;
  }

  protected override Task<bool> OnSaveAsync()
  {
    this._newSlipType = this.Details.SlipType;
    return base.OnSaveAsync();
  }
}
