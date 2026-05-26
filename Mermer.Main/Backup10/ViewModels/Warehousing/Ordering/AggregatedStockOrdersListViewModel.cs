// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Warehousing.Ordering.AggregatedStockOrdersListViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Mermer.Enterprise.Models;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.ViewModels.Common;
using Mermer.Warehousing.Ordering.Models;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Warehousing.Ordering;

public class AggregatedStockOrdersListViewModel : TransactionsListViewModel<AggregatedStockOrder>
{
  public AggregatedStockOrdersListViewModel(
    IMvxMessenger messenger,
    Reference<Warehouse> warehouses,
    IMvxNavigationService navigationService,
    IRepository<AggregatedStockOrder> repository,
    IListAuthorizer<AggregatedStockOrder> authorizer,
    IUserInteractionService userInteractionService)
    : base(repository, authorizer, messenger, navigationService, userInteractionService)
  {
    this.Warehouses = warehouses;
  }

  public Reference<Warehouse> Warehouses { get; }

  protected override Task PreLoad() => Task.WhenAll(base.PreLoad(), this.Warehouses.Initialize());
}
