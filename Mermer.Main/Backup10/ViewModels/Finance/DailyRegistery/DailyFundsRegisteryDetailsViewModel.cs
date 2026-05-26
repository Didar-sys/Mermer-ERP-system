// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Finance.DailyRegistery.DailyFundsRegisteryDetailsViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using Mermer.Authorization.Services;
using Mermer.Enterprise.Models;
using Mermer.Finance.DailyRegistery.Models;
using Mermer.FundsManagement.Models;
using Mermer.Transactions.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.ViewModels.Transactions;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using Mermer.Services;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Finance.DailyRegistery;

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
