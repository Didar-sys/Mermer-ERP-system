// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Transactions.FundsTransactionDetailsViewModel`2
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Mermer.Authorization.Services;
using Mermer.Enterprise.Models;
using Mermer.FundsManagement.Models;
using Mermer.Transactions.Models;
using Mermer.Transactions.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Data.Authorizers;
using Mermer.Data.Extenders;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using Mermer.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Transactions;

public class FundsTransactionDetailsViewModel<T, TLine> : TransactionDetailsViewModel<T, TLine>
  where T : FundsTransaction<TLine>
  where TLine : FundsTransactionLine
{
  protected FundsTransactionDetailsViewModel(
    IRepository<T> repository,
    IListAuthorizer<T> authorizer,
    IConfigurator configurator,
    ILoginService loginService,
    Reference<Currency> currencies,
    Reference<Depository> depositories,
    IMvxNavigationService navigationService,
    ITransactionCodeGenerationService codegentor,
    IUserInteractionService userInteractionService)
    : base(configurator, repository, authorizer, loginService, currencies, navigationService, codegentor, userInteractionService)
  {
    this.Depositories = depositories;
  }

  public Reference<Depository> Depositories { get; }

  protected override Task PreLoad() => Task.WhenAll(base.PreLoad(), this.Depositories.Initialize());

  protected override async Task OnLoad()
  {
    FundsTransactionDetailsViewModel<T, TLine> detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__0();
    if (!string.IsNullOrEmpty(detailsViewModel.ItemId))
      return;
    detailsViewModel.Details.DepositoryId = detailsViewModel.AppSettings.DefaultDepositoryId;
  }

  protected override async Task PostLoad()
  {
    FundsTransactionDetailsViewModel<T, TLine> detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__1();
    // ISSUE: reference to a compiler-generated method
    detailsViewModel.Depositories.Filter = new Func<Depository, bool>(detailsViewModel.\u003CPostLoad\u003Eb__6_0);
  }

  public ICommand SelectedLineDeleteCommand
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.OnSelectedLineDelete), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess && this.IsLineSelected));
    }
  }

  private void OnSelectedLineDelete()
  {
    this.SelectedLine = this.Details.Lines.RemoveWithSelection<TLine>(this.SelectedLine);
  }

  public ICommand SelectDepositoryCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectDepositoryCommandAsync), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess));
    }
  }

  private async Task OnSelectDepositoryCommandAsync()
  {
    FundsTransactionDetailsViewModel<T, TLine> detailsViewModel = this;
    T obj = detailsViewModel.Details;
    obj.DepositoryId = await detailsViewModel.NavigationService.Navigate<ListViewModel<Depository>, string, string>(detailsViewModel.Details.DepositoryId ?? Guid.Empty.ToString());
    obj = default (T);
  }
}
