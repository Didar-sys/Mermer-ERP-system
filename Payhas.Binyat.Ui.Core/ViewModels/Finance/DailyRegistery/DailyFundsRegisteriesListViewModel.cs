// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Finance.DailyRegistery.DailyFundsRegisteriesListViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.Finance.DailyRegistery.Models;
using Payhas.Binyat.Finance.DailyRegistery.Services;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.ViewModels.Transactions;
using Payhas.Data.Authorizers;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Finance.DailyRegistery;

public class DailyFundsRegisteriesListViewModel : 
  FundsTransactionsListViewModel<DailyFundsRegistery, DailyFundsRegisteryLine>
{
  private readonly IReadOnlyListAuthorizer<FundsBalance> _balanceAuthorizer;
  private bool _canViewBalance;

  public DailyFundsRegisteriesListViewModel(
    IMvxMessenger messenger,
    Reference<Depository> depositories,
    IDailyFundsRegisteriesRepository repository,
    IListAuthorizer<DailyFundsRegistery> authorizer,
    IReadOnlyListAuthorizer<FundsBalance> balanceAuthorizer,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, (IRepository<DailyFundsRegistery>) repository, authorizer, depositories, navigationService, userInteractionService)
  {
    this._balanceAuthorizer = balanceAuthorizer;
  }

  public bool CanViewBalance
  {
    get => this._canViewBalance;
    set => this.SetProperty<bool>(ref this._canViewBalance, value, nameof (CanViewBalance));
  }

  protected override Task PreLoad()
  {
    try
    {
      this.CanViewBalance = false;
      this._balanceAuthorizer.Authorize();
      this.CanViewBalance = true;
    }
    catch (Exception ex)
    {
    }
    return base.PreLoad();
  }

  protected override async Task<IEnumerable<DailyFundsRegistery>> GetFilteredListAsync(
    ListFilter filter)
  {
    return (IEnumerable<DailyFundsRegistery>) await ((IDailyFundsRegisteriesRepository) this.Repository).GetAsync(Array.Empty<Expression<Func<DailyFundsRegistery, bool>>>());
  }

  protected override async Task<IEnumerable<DailyFundsRegistery>> GetFilteredListByDateAsync(
    DateTime from,
    DateTime till)
  {
    return (IEnumerable<DailyFundsRegistery>) await ((IDailyFundsRegisteriesRepository) this.Repository).GetAsync((Expression<Func<DailyFundsRegistery, bool>>) (x => x.Date >= from && x.Date < till));
  }
}
