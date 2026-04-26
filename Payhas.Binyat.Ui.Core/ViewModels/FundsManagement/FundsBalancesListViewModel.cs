// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.FundsManagement.FundsBalancesListViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Common.Settings;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.FundsManagement.Services;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.ViewModels.Common;
using Payhas.Mvvm.Services;
using Payhas.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.FundsManagement;

public class FundsBalancesListViewModel : 
  ListViewModelBaseWithFilterDate<FundsBalanceByTypeWithBalance>
{
  private readonly IConfigurator _configurator;
  private readonly IFundsBalancesRepository _repository;
  private string _depositoryId;
  private bool _loaded;

  public FundsBalancesListViewModel(
    IMvxMessenger messenger,
    IConfigurator configurator,
    Reference<Depository> depositories,
    IFundsBalancesRepository repository,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._configurator = configurator;
    this._repository = repository;
    this.Depositories = depositories;
  }

  public virtual string DepositoryId
  {
    get => this._depositoryId;
    set
    {
      if (!this.SetProperty<string>(ref this._depositoryId, value, nameof (DepositoryId)) || this.IsBusy)
        return;
      this.Initialize();
    }
  }

  public Reference<Depository> Depositories { get; }

  protected override async Task PreLoad()
  {
    if (!this._loaded && string.IsNullOrEmpty(this.DepositoryId))
      this.DepositoryId = (await this._configurator.GetConfigAsync<AppSettings>()).DefaultDepositoryId;
    this._loaded = true;
    await Task.WhenAll(base.PreLoad(), this.Depositories.Initialize());
  }

  protected override Task<IEnumerable<FundsBalanceByTypeWithBalance>> GetFilteredListByDateAsync(
    DateTime from,
    DateTime till)
  {
    return this._repository.GetByTypeAsync(this.DepositoryId, new DateTime?(from), new DateTime?(till));
  }

  protected override Task<IEnumerable<FundsBalanceByTypeWithBalance>> GetFilteredListAsync(
    ListFilter filter)
  {
    return this._repository.GetByTypeAsync(this.DepositoryId, new DateTime?(), new DateTime?());
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
    return this.NavigationService.Navigate<FundsActionsListViewModel, FundsActionsFilter>(new FundsActionsFilter()
    {
      DepositoryIds = new string[1]
      {
        this.SelectedItem.DepositoryId
      },
      DateFrom = this.DateFilterFrom,
      DateTill = this.DateFilterTill
    });
  }

  public ICommand ShowActionsCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnShowActionsAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private Task OnShowActionsAsync()
  {
    return this.NavigationService.Navigate<FundsActionsListViewModel, FundsActionsFilter>(new FundsActionsFilter()
    {
      DepositoryIds = new string[1]{ this.DepositoryId },
      DateFrom = this.DateFilterFrom,
      DateTill = this.DateFilterTill
    });
  }

  protected override Expression<Func<FundsBalanceByTypeWithBalance, bool>> GetDateFilter(
    DateTime from,
    DateTime till)
  {
    throw new NotImplementedException();
  }

  protected override Task<int> CountListAsync(
    params Expression<Func<FundsBalanceByTypeWithBalance, bool>>[] predicates)
  {
    throw new NotImplementedException();
  }

  protected override Task<IEnumerable<FundsBalanceByTypeWithBalance>> GetListAsync(
    params Expression<Func<FundsBalanceByTypeWithBalance, bool>>[] predicates)
  {
    throw new NotImplementedException();
  }
}
