// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Finance.FundsSlipDetailsViewModel
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
using Mermer.Services;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Finance;

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
        await base.PostLoad();

        if (!string.IsNullOrEmpty(ItemId))
            return;

        Details.SlipType = _newSlipType;
    }

    protected override Task<bool> OnSaveAsync()
  {
    this._newSlipType = this.Details.SlipType;
    return base.OnSaveAsync();
  }
}
