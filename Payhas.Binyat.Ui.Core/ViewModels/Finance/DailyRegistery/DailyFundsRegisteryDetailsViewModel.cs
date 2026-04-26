// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Finance.DailyRegistery.DailyFundsRegisteryDetailsViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.Finance.DailyRegistery.Models;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.Transactions.Services;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.ViewModels.Transactions;
using Payhas.Data.Authorizers;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;
using Payhas.Services;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Finance.DailyRegistery;

public class DailyFundsRegisteryDetailsViewModel(
  IRepository<DailyFundsRegistery> repository,
  IListAuthorizer<DailyFundsRegistery> authorizer,
  IConfigurator configurator,
  ILoginService loginService,
  Reference<Currency> currencies,
  Reference<Depository> depositories,
  IMvxNavigationService navigationService,
  ITransactionCodeGenerationService codegentor,
  IUserInteractionService userInteractionService) : 
  FundsTransactionDetailsViewModel<DailyFundsRegistery, DailyFundsRegisteryLine>(repository, authorizer, configurator, loginService, currencies, depositories, navigationService, codegentor, userInteractionService)
{
}
