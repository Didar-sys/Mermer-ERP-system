// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Warehousing.Ordering.StockOrdersListViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Mermer.Authorization.Services;
using Mermer.Enterprise.Models;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.ViewModels.Common;
using Mermer.Warehousing.Ordering.Models;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Warehousing.Ordering;

public class StockOrdersListViewModel : TransactionsListViewModel<StockOrder>
{
  private readonly ILoginService _loginService;
  private const string FilterOwnString = "My Orders";
  private const string FilterOpenString = "Open";
  private const string FilterCompletedString = "Completed";
  private const string FilterDeletedString = "Deleted";

  public StockOrdersListViewModel(
    ILoginService loginService,
    Reference<Warehouse> warehouses,
    IMvxMessenger messenger,
    IRepository<StockOrder> repository,
    IListAuthorizer<StockOrder> authorizer,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(repository, authorizer, messenger, navigationService, userInteractionService)
  {
    this._loginService = loginService;
    this.Warehouses = warehouses;
    this.Filters = (IEnumerable<ListFilter>) new ListFilter[4]
    {
      new ListFilter()
      {
        Title = this["My Orders", Array.Empty<object>()],
        Tag = (object) "My Orders",
        CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy),
        Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x)),
        Counter = new Func<ListFilter, Task<int>>(((TransactionsListViewModel<StockOrder>) this).CountByFilterAsync)
      },
      new ListFilter()
      {
        Title = this["Open", Array.Empty<object>()],
        Tag = (object) "Open",
        CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy),
        Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x)),
        Counter = new Func<ListFilter, Task<int>>(((TransactionsListViewModel<StockOrder>) this).CountByFilterAsync)
      },
      new ListFilter()
      {
        Title = this["Completed", Array.Empty<object>()],
        Tag = (object) "Completed",
        CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy),
        Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x)),
        Counter = new Func<ListFilter, Task<int>>(((TransactionsListViewModel<StockOrder>) this).CountByFilterAsync)
      },
      new ListFilter()
      {
        Title = this["Deleted", Array.Empty<object>()],
        Tag = (object) "Deleted",
        CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy),
        Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x)),
        Counter = new Func<ListFilter, Task<int>>(((TransactionsListViewModel<StockOrder>) this).CountByFilterAsync)
      }
    };
  }

  public Reference<Warehouse> Warehouses { get; }

  protected override Task PreLoad() => Task.WhenAll(base.PreLoad(), this.Warehouses.Initialize());

  protected override Task<int> CountFilteredListAsync(ListFilter filter)
  {
    switch (filter.Tag.ToString())
    {
      case "My Orders":
        string userId = this._loginService.Session.UserId;
        return this.Repository.CountAsync((Expression<Func<StockOrder, bool>>) (x => x.UserId == userId));
      case "Open":
        return this.Repository.CountAsync((Expression<Func<StockOrder, bool>>) (x => !x.IsCompleted && !x.IsDisabled));
      case "Completed":
        return this.Repository.CountAsync((Expression<Func<StockOrder, bool>>) (x => x.IsCompleted && !x.IsDisabled));
      case "Deleted":
        return this.Repository.CountAsync((Expression<Func<StockOrder, bool>>) (x => x.IsDisabled));
      default:
        return base.CountFilteredListAsync(filter);
    }
  }

  protected override Task<IEnumerable<StockOrder>> GetFilteredListAsync(ListFilter filter)
  {
    switch (filter.Tag.ToString())
    {
      case "My Orders":
        string userId = this._loginService.Session.UserId;
        return this.Repository.GetAsync((Expression<Func<StockOrder, bool>>) (x => x.UserId == userId));
      case "Open":
        return this.Repository.GetAsync((Expression<Func<StockOrder, bool>>) (x => !x.IsCompleted && !x.IsDisabled));
      case "Completed":
        return this.Repository.GetAsync((Expression<Func<StockOrder, bool>>) (x => x.IsCompleted && !x.IsDisabled));
      case "Deleted":
        return this.Repository.GetAsync((Expression<Func<StockOrder, bool>>) (x => x.IsDisabled));
      default:
        return base.GetFilteredListAsync(filter);
    }
  }
}
