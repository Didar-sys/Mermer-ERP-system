// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Warehousing.StockTransfersListViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.ViewModels.Common;
using Payhas.Binyat.Ui.Core.ViewModels.Transactions;
using Payhas.Binyat.Warehousing.Models;
using Payhas.Data.Authorizers;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Warehousing;

public class StockTransfersListViewModel : 
  StockTransactionsListViewModel<StockTransfer, StockTransferLine>
{
  private bool _initialized;

  public StockTransfersListViewModel(
    IMvxMessenger messenger,
    Reference<Warehouse> warehouses,
    IRepository<StockTransfer> repository,
    IListAuthorizer<StockTransfer> authorizer,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, repository, authorizer, warehouses, navigationService, userInteractionService)
  {
    ListFilter[] listFilterArray = new ListFilter[6];
    listFilterArray[0] = new ListFilter()
    {
      Title = this["Conflicted", Array.Empty<object>()],
      CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy),
      Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x)),
      Counter = new Func<ListFilter, Task<int>>(((TransactionsListViewModel<StockTransfer>) this).CountByFilterAsync),
      Tag = (object) "Conflicted"
    };
    ListFilterByDate listFilterByDate1 = new ListFilterByDate();
    listFilterByDate1.Title = this["Today", Array.Empty<object>()];
    listFilterByDate1.CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy);
    listFilterByDate1.Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x));
    listFilterByDate1.Counter = new Func<ListFilter, Task<int>>(((TransactionsListViewModel<StockTransfer>) this).CountByFilterAsync);
    listFilterByDate1.From = DateTime.Today;
    listFilterByDate1.Till = DateTime.Today;
    listFilterArray[1] = (ListFilter) listFilterByDate1;
    ListFilterByDate listFilterByDate2 = new ListFilterByDate();
    listFilterByDate2.Title = this["This Week", Array.Empty<object>()];
    listFilterByDate2.CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy);
    listFilterByDate2.Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x));
    listFilterByDate2.Counter = new Func<ListFilter, Task<int>>(((TransactionsListViewModel<StockTransfer>) this).CountByFilterAsync);
    listFilterByDate2.From = DateTime.Today.StartOfWeek();
    listFilterByDate2.Till = DateTime.Today.EndOfWeek();
    listFilterArray[2] = (ListFilter) listFilterByDate2;
    ListFilterByDate listFilterByDate3 = new ListFilterByDate();
    listFilterByDate3.Title = this["This Month", Array.Empty<object>()];
    listFilterByDate3.CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy);
    listFilterByDate3.Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x));
    listFilterByDate3.Counter = new Func<ListFilter, Task<int>>(((TransactionsListViewModel<StockTransfer>) this).CountByFilterAsync);
    listFilterByDate3.From = DateTime.Today.AddDays((double) (1 - DateTime.Today.Day));
    DateTime dateTime1 = DateTime.Today;
    dateTime1 = dateTime1.AddMonths(1);
    listFilterByDate3.Till = dateTime1.AddDays((double) -DateTime.Today.Day);
    listFilterArray[3] = (ListFilter) listFilterByDate3;
    ListFilterByDate listFilterByDate4 = new ListFilterByDate();
    listFilterByDate4.Title = this["This Year", Array.Empty<object>()];
    listFilterByDate4.CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy);
    listFilterByDate4.Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x));
    listFilterByDate4.Counter = new Func<ListFilter, Task<int>>(((TransactionsListViewModel<StockTransfer>) this).CountByFilterAsync);
    listFilterByDate4.From = DateTime.Today.AddDays((double) (1 - DateTime.Today.DayOfYear));
    DateTime dateTime2 = DateTime.Today;
    dateTime2 = dateTime2.AddYears(1);
    listFilterByDate4.Till = dateTime2.AddDays((double) -DateTime.Today.DayOfYear);
    listFilterArray[4] = (ListFilter) listFilterByDate4;
    listFilterArray[5] = new ListFilter()
    {
      Title = this["All Records", Array.Empty<object>()],
      CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy),
      Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x)),
      Counter = new Func<ListFilter, Task<int>>(((TransactionsListViewModel<StockTransfer>) this).CountByFilterAsync),
      Tag = (object) "All"
    };
    this.Filters = (IEnumerable<ListFilter>) listFilterArray;
  }

  protected override Task OnLoad()
  {
    if (!this._initialized)
    {
      this.SelectedFilter = this.Filters.ElementAt<ListFilter>(2);
      this._initialized = true;
    }
    return base.OnLoad();
  }

  protected override Task<int> CountByFilterAsync(ListFilter filter)
  {
    if (!(filter.Tag?.ToString() == "Conflicted"))
      return base.CountByFilterAsync(filter);
    return this.Repository.CountAsync((Expression<Func<StockTransfer, bool>>) (x => x.IsConflicted));
  }

  protected override Task<IEnumerable<StockTransfer>> GetFilteredListAsync(ListFilter filter)
  {
    if (!(filter.Tag?.ToString() == "Conflicted"))
      return base.GetFilteredListAsync(filter);
    return this.Repository.GetAsync((Expression<Func<StockTransfer, bool>>) (x => x.IsConflicted));
  }
}
