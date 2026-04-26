// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Transactions.FundsTransactionsListViewModel`2
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.Transactions.Models;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.ViewModels.Common;
using Payhas.Data.Authorizers;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Transactions;

public class FundsTransactionsListViewModel<T, TLine> : TransactionsListViewModel<T>
  where T : FundsTransaction<TLine>
  where TLine : FundsTransactionLine
{
  protected FundsTransactionsListViewModel(
    IMvxMessenger messenger,
    IRepository<T> repository,
    IListAuthorizer<T> authorizer,
    Reference<Depository> depositories,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(repository, authorizer, messenger, navigationService, userInteractionService)
  {
    this.Depositories = depositories;
  }

  public Reference<Depository> Depositories { get; }

  protected override Task PreLoad() => Task.WhenAll(base.PreLoad(), this.Depositories.Initialize());
}
