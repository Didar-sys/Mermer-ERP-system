// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Finance.Spending.ExpenseSlipDetailsViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.Finance.Spending.Models;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.Transactions.Services;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.Services;
using Payhas.Binyat.Ui.Core.ViewModels.Transactions;
using Payhas.Data.Authorizers;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;
using Payhas.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Finance.Spending;

public class ExpenseSlipDetailsViewModel : 
  FundsTransactionDetailsViewModel<ExpenseSlip, ExpenseSlipLine>
{
  private readonly IPrintingService _printingService;

  public ExpenseSlipDetailsViewModel(
    IConfigurator configurator,
    ILoginService loginService,
    Reference<Expense> expenses,
    Reference<Currency> currencies,
    IPrintingService printingService,
    Reference<Depository> depositories,
    IRepository<ExpenseSlip> repository,
    IListAuthorizer<ExpenseSlip> authorizer,
    IMvxNavigationService navigationService,
    ITransactionCodeGenerationService codegentor,
    IUserInteractionService userInteractionService)
    : base(repository, authorizer, configurator, loginService, currencies, depositories, navigationService, codegentor, userInteractionService)
  {
    this._printingService = printingService;
    this.Expenses = expenses;
  }

  public Reference<Expense> Expenses { get; }

  protected override Task PreLoad() => Task.WhenAll(base.PreLoad(), this.Expenses.Initialize());

  protected override async Task PostLoad()
  {
    ExpenseSlipDetailsViewModel detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__0();
    IEnumerable<string> usedExpenseIds = detailsViewModel.Details.Lines.Select<ExpenseSlipLine, string>((Func<ExpenseSlipLine, string>) (x => x.ExpenseId)).Distinct<string>();
    detailsViewModel.Expenses.Filter = (Func<Expense, bool>) (x => !x.IsDisabled || usedExpenseIds.Contains<string>(x.Id));
    detailsViewModel.Details.RaisePropertyChanged("DisplayTotal");
  }

  protected override async Task<bool> OnSaveAsync()
  {
    ExpenseSlipDetailsViewModel detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    if (!await detailsViewModel.\u003C\u003En__1())
      return false;
    await detailsViewModel._printingService.PrintExpenseSlip(detailsViewModel.Details);
    return true;
  }

  public ICommand PrintCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnPrintCommandAsync), (Func<bool>) (() => !this.IsBusy && !this.IsDirty));
    }
  }

  protected virtual async Task OnPrintCommandAsync()
  {
    ExpenseSlipDetailsViewModel detailsViewModel = this;
    await detailsViewModel._printingService.PrintExpenseSlip(detailsViewModel.Details, true);
  }
}
