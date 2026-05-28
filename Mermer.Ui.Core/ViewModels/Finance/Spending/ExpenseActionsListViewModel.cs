// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Finance.Spending.ExpenseActionsListViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Mermer.Common.Settings;
using Mermer.Enterprise.Models;
using Mermer.Finance.Spending.Models;
using Mermer.Finance.Spending.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.ViewModels.Common;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using Mermer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Finance.Spending;

public class ExpenseActionsListViewModel : ListViewModelBaseWithFilterDate<ExpenseAction>
{
  private readonly IConfigurator _configurator;
  private readonly IExpenseActionsRepository _repository;
  private System.Collections.Generic.List<object> _selectedDepositoryIds;
  private string _expenseId;
  private bool _loaded;

  public ExpenseActionsListViewModel(
    IMvxMessenger messenger,
    IConfigurator configurator,
    Reference<Expense> expenses,
    Reference<Depository> depositories,
    IExpenseActionsRepository repository,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._configurator = configurator;
    this._repository = repository;
    this.Expenses = expenses;
    this.Depositories = depositories;
    this.Types = new LocalizedTransactionTypes(this.TextSource, Array.Empty<string>());
  }

  public System.Collections.Generic.List<object> SelectedDepositoryIds
  {
    get => this._selectedDepositoryIds;
    set
    {
      if (this._selectedDepositoryIds != null && value != null && this._selectedDepositoryIds.SequenceEqual<object>((IEnumerable<object>) value) || !this.SetProperty<System.Collections.Generic.List<object>>(ref this._selectedDepositoryIds, value, nameof (SelectedDepositoryIds)) || this.IsBusy)
        return;
      this.Initialize();
    }
  }

  public string[] DepositoryIds
  {
    get
    {
      System.Collections.Generic.List<object> selectedDepositoryIds = this.SelectedDepositoryIds;
      return (selectedDepositoryIds != null ? selectedDepositoryIds.Cast<string>().ToArray<string>() : (string[]) null) ?? Array.Empty<string>();
    }
  }

  public virtual string ExpenseId
  {
    get => this._expenseId;
    set
    {
      if (!this.SetProperty<string>(ref this._expenseId, value, nameof (ExpenseId)) || this.IsBusy)
        return;
      this.Initialize();
    }
  }

  public Reference<Expense> Expenses { get; }

  public Reference<Depository> Depositories { get; }

  public LocalizedTransactionTypes Types { get; }

    protected override async Task PreLoad()
    {
        if (!_loaded && !DepositoryIds.Any())
        {
            AppSettings configAsync = await _configurator.GetConfigAsync<AppSettings>();
            SelectedDepositoryIds = new List<object> { configAsync.DefaultDepositoryId };
        }

        _loaded = true;

        await Task.WhenAll(
            base.PreLoad(),
            Expenses.Initialize(),
            Depositories.Initialize()
        );
    }

    protected override Task<int> CountFilteredListByDateAsync(DateTime from, DateTime till)
  {
    return this._repository.CountAsync(new DateTime?(from), new DateTime?(till), this.DepositoryIds, this.ExpenseId);
  }

  protected override Task<int> CountFilteredListAsync(ListFilter filter)
  {
    return this._repository.CountAsync(new DateTime?(), new DateTime?(), this.DepositoryIds, this.ExpenseId);
  }

  protected override Task<IEnumerable<ExpenseAction>> GetFilteredListByDateAsync(
    DateTime from,
    DateTime till)
  {
    return this._repository.GetAsync(new DateTime?(from), new DateTime?(till), this.DepositoryIds, this.ExpenseId);
  }

  protected override Task<IEnumerable<ExpenseAction>> GetFilteredListAsync(ListFilter filter)
  {
    return this._repository.GetAsync(new DateTime?(), new DateTime?(), this.DepositoryIds, this.ExpenseId);
  }

  protected override Expression<Func<ExpenseAction, bool>> GetDateFilter(
    DateTime from,
    DateTime till)
  {
    throw new NotImplementedException();
  }

  protected override Task<int> CountListAsync(
    params Expression<Func<ExpenseAction, bool>>[] predicates)
  {
    throw new NotImplementedException();
  }

  protected override Task<IEnumerable<ExpenseAction>> GetListAsync(
    params Expression<Func<ExpenseAction, bool>>[] predicates)
  {
    throw new NotImplementedException();
  }

  public ICommand SelectOrViewDetailsCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectOrViewDetailsAsync), (Func<bool>) (() => !this.IsBusy && this.SelectedItem != null));
    }
  }

  private Task OnSelectOrViewDetailsAsync()
  {
    return this.NavigationService.Navigate<DetailsViewModel<ExpenseSlip>, string>(this.SelectedItem.TransactionId);
  }
}
