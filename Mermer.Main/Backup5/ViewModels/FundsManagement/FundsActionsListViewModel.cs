// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.FundsManagement.FundsActionsListViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Mermer.Commerce.Models;
using Mermer.Common.Settings;
using Mermer.CRM.Models;
using Mermer.Enterprise.Models;
using Mermer.Finance.Models;
using Mermer.Finance.Spending.Models;
using Mermer.FundsManagement.Models;
using Mermer.FundsManagement.Services;
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
namespace Mermer.Ui.Core.ViewModels.FundsManagement;

public class FundsActionsListViewModel : 
  ListViewModelBaseWithFilterDate<FundsAction>,
  IMvxViewModel<FundsActionsFilter>,
  IMvxViewModel
{
  private readonly IConfigurator _configurator;
  private readonly IFundsActionsRepository _repository;
  private System.Collections.Generic.List<object> _selectedDepositoryIds;
  private FundsActionsFilter _parameter;
  private bool _loaded;

  public FundsActionsListViewModel(
    IMvxMessenger messenger,
    IConfigurator configurator,
    Reference<Partner> partners,
    Reference<Depository> depositories,
    IFundsActionsRepository repository,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._configurator = configurator;
    this._repository = repository;
    this.Partners = partners;
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

  public Reference<Partner> Partners { get; }

  public Reference<Depository> Depositories { get; }

  public LocalizedTransactionTypes Types { get; }

  public void Prepare(FundsActionsFilter parameter) => this._parameter = parameter;

  protected override async Task PreLoad()
  {
    FundsActionsListViewModel actionsListViewModel = this;
    if (!actionsListViewModel._loaded)
    {
      if (actionsListViewModel._parameter != null)
      {
        actionsListViewModel.SelectedDepositoryIds = actionsListViewModel._parameter.DepositoryIds.Cast<object>().ToList<object>();
        actionsListViewModel.DateFilterFrom = actionsListViewModel._parameter.DateFrom;
        actionsListViewModel.DateFilterTill = actionsListViewModel._parameter.DateTill;
      }
      else
      {
        AppSettings configAsync = await actionsListViewModel._configurator.GetConfigAsync<AppSettings>();
        actionsListViewModel.SelectedDepositoryIds = new System.Collections.Generic.List<object>((IEnumerable<object>) new object[1]
        {
          (object) configAsync.DefaultDepositoryId
        });
      }
    }
    actionsListViewModel._loaded = true;
    // ISSUE: reference to a compiler-generated method
    await Task.WhenAll(actionsListViewModel.\u003C\u003En__0(), actionsListViewModel.Depositories.Initialize(), actionsListViewModel.Partners.Initialize());
  }

  protected override Task OnLoad()
  {
    if (this._parameter == null)
      return base.OnLoad();
    this._parameter = (FundsActionsFilter) null;
    return this.LoadByDateAsync(false);
  }

  protected override Task<int> CountFilteredListByDateAsync(DateTime from, DateTime till)
  {
    return this._repository.CountAsync(new DateTime?(from), new DateTime?(till), (string) null, this.DepositoryIds);
  }

  protected override Task<int> CountFilteredListAsync(ListFilter filter)
  {
    return this._repository.CountAsync(new DateTime?(), new DateTime?(), (string) null, this.DepositoryIds);
  }

  protected override Task<IEnumerable<FundsAction>> GetFilteredListByDateAsync(
    DateTime from,
    DateTime till)
  {
    return this._repository.GetAsync(new DateTime?(from), new DateTime?(till), (string) null, this.DepositoryIds);
  }

  protected override Task<IEnumerable<FundsAction>> GetFilteredListAsync(ListFilter filter)
  {
    return this._repository.GetAsync(new DateTime?(), new DateTime?(), (string) null, this.DepositoryIds);
  }

  protected override Task<int> CountListAsync(
    params Expression<Func<FundsAction, bool>>[] predicates)
  {
    throw new NotImplementedException();
  }

  protected override Task<IEnumerable<FundsAction>> GetListAsync(
    params Expression<Func<FundsAction, bool>>[] predicates)
  {
    throw new NotImplementedException();
  }

  protected override Expression<Func<FundsAction, bool>> GetDateFilter(DateTime from, DateTime till)
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
    switch (this.SelectedItem.TransactionType)
    {
      case "Collection":
      case "Payment":
        return this.NavigationService.Navigate<DetailsViewModel<Bill>, string>(this.SelectedItem.TransactionId);
      case "ExpenseSlip":
        return this.NavigationService.Navigate<DetailsViewModel<ExpenseSlip>, string>(this.SelectedItem.TransactionId);
      case "FundsOpening":
      case "FundsRevisionDeficit":
      case "FundsRevisionExceed":
        return this.NavigationService.Navigate<DetailsViewModel<FundsSlip>, string>(this.SelectedItem.TransactionId);
      case "FundsTransferDestination":
      case "FundsTransferSource":
        return this.NavigationService.Navigate<DetailsViewModel<FundsTransfer>, string>(this.SelectedItem.TransactionId);
      case "Purchase":
      case "PurchaseReturn":
      case "Sales":
      case "SalesReturn":
        return this.NavigationService.Navigate<DetailsViewModel<Invoice>, string>(this.SelectedItem.TransactionId);
      default:
        return Task.CompletedTask;
    }
  }
}
