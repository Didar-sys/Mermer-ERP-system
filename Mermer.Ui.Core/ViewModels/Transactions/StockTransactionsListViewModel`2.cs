// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Transactions.StockTransactionsListViewModel`2
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Mermer.Enterprise.Models;
using Mermer.Transactions.Models;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.ViewModels.Common;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Transactions;

public class StockTransactionsListViewModel<T, TLine> : TransactionsListViewModel<T>
  where T : StockTransaction<TLine>
  where TLine : StockTransactionLine
{
  protected StockTransactionsListViewModel(
    IMvxMessenger messenger,
    IRepository<T> repository,
    IListAuthorizer<T> authorizer,
    Reference<Warehouse> warehouses,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(repository, authorizer, messenger, navigationService, userInteractionService)
  {
    this.Warehouses = warehouses;
  }

  public Reference<Warehouse> Warehouses { get; }

  protected override Task PreLoad() => Task.WhenAll(base.PreLoad(), this.Warehouses.Initialize());
}
