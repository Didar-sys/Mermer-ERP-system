// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Common.TransactionsListViewModel`1
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Mermer.Transactions.Models;
using Mermer.Ui.Core.Helpers;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Common;

public abstract class TransactionsListViewModel<T> : ListViewModel<T> where T : class, ITransactionModel, INotifyPropertyChanged
{
    private IEnumerable<ListFilter> _filters;
    private ListFilter _selectedFilter;

    // Жорстко прив'язуємо сьогоднішню дату до полів пам'яті
    private DateTime _dateFilterFrom = DateTime.Today;
    private DateTime _dateFilterTill = DateTime.Today;

    protected TransactionsListViewModel(
      IRepository<T> repository,
      IListAuthorizer<T> authorizer,
      IMvxMessenger messenger,
      IMvxNavigationService navigationService,
      IUserInteractionService userInteractionService)
      : base(repository, authorizer, messenger, navigationService, userInteractionService)
    {
        this.InitFilters();
        this.Types = new LocalizedTransactionTypes("Repricing");
    }

    public IEnumerable<ListFilter> Filters
  {
    get => this._filters;
    set => this.SetProperty<IEnumerable<ListFilter>>(ref this._filters, value, nameof (Filters));
  }

  public virtual ListFilter SelectedFilter
  {
    get => this._selectedFilter;
    set => this.SetProperty<ListFilter>(ref this._selectedFilter, value, nameof (SelectedFilter));
  }

  public DateTime DateFilterFrom
  {
    get => this._dateFilterFrom;
    set => this.SetProperty<DateTime>(ref this._dateFilterFrom, value, nameof (DateFilterFrom));
  }

  public DateTime DateFilterTill
  {
    get => this._dateFilterTill;
    set => this.SetProperty<DateTime>(ref this._dateFilterTill, value, nameof (DateFilterTill));
  }

  public DateTime DateFilterTillInclusive
  {
    get
    {
      DateTime dateTime = this.DateFilterTill;
      dateTime = dateTime.AddDays(1.0);
      return dateTime.Date;
    }
  }

  public LocalizedTransactionTypes Types { get; set; }

  public ICommand FilterByDateCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnFilterByDateCommandAsync), (Func<bool>) (() => !this.IsBusy && this.DateFilterFrom <= this.DateFilterTill));
    }
  }

  protected virtual async Task OnFilterByDateCommandAsync()
  {
    TransactionsListViewModel<T> transactionsListViewModel = this;
    transactionsListViewModel.IsBusy = true;
    try
    {
      transactionsListViewModel.SelectedFilter = (ListFilter) null;
      IEnumerable<T> filteredListByDateAsync = await transactionsListViewModel.GetFilteredListByDateAsync(transactionsListViewModel.DateFilterFrom, transactionsListViewModel.DateFilterTillInclusive);
      transactionsListViewModel.List = filteredListByDateAsync;
      transactionsListViewModel.SubCaption = $"{transactionsListViewModel.DateFilterFrom:MMM d} - {transactionsListViewModel.DateFilterTill:MMM d}";
    }
    catch (Exception ex)
    {
      transactionsListViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    transactionsListViewModel.IsBusy = false;
  }

  protected virtual void InitFilters()
  {
    ListFilter[] listFilterArray = new ListFilter[8];
    ListFilterByDate listFilterByDate1 = new ListFilterByDate();
    listFilterByDate1.Title = this["Today", Array.Empty<object>()];
    listFilterByDate1.CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy);
    listFilterByDate1.Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x));
    listFilterByDate1.Counter = new Func<ListFilter, Task<int>>(this.CountByFilterAsync);
    listFilterByDate1.From = DateTime.Today;
    listFilterByDate1.Till = DateTime.Today;
    listFilterArray[0] = (ListFilter) listFilterByDate1;
    ListFilterByDate listFilterByDate2 = new ListFilterByDate();
    listFilterByDate2.Title = this["Yesturday", Array.Empty<object>()];
    listFilterByDate2.CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy);
    listFilterByDate2.Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x));
    listFilterByDate2.Counter = new Func<ListFilter, Task<int>>(this.CountByFilterAsync);
    listFilterByDate2.From = DateTime.Today.AddDays(-1.0);
    listFilterByDate2.Till = DateTime.Today.AddDays(-1.0);
    listFilterArray[1] = (ListFilter) listFilterByDate2;
    ListFilterByDate listFilterByDate3 = new ListFilterByDate();
    listFilterByDate3.Title = this["This Week", Array.Empty<object>()];
    listFilterByDate3.CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy);
    listFilterByDate3.Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x));
    listFilterByDate3.Counter = new Func<ListFilter, Task<int>>(this.CountByFilterAsync);
    listFilterByDate3.From = DateTime.Today.StartOfWeek();
    listFilterByDate3.Till = DateTime.Today.EndOfWeek();
    listFilterArray[2] = (ListFilter) listFilterByDate3;
    ListFilterByDate listFilterByDate4 = new ListFilterByDate();
    listFilterByDate4.Title = this["Past Week", Array.Empty<object>()];
    listFilterByDate4.CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy);
    listFilterByDate4.Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x));
    listFilterByDate4.Counter = new Func<ListFilter, Task<int>>(this.CountByFilterAsync);
    listFilterByDate4.From = DateTime.Today.AddDays(-7.0).StartOfWeek();
    listFilterByDate4.Till = DateTime.Today.AddDays(-7.0).EndOfWeek();
    listFilterArray[3] = (ListFilter) listFilterByDate4;
    ListFilterByDate listFilterByDate5 = new ListFilterByDate();
    listFilterByDate5.Title = this["This Month", Array.Empty<object>()];
    listFilterByDate5.CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy);
    listFilterByDate5.Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x));
    listFilterByDate5.Counter = new Func<ListFilter, Task<int>>(this.CountByFilterAsync);
    listFilterByDate5.From = DateTime.Today.AddDays((double) (1 - DateTime.Today.Day));
    DateTime dateTime1 = DateTime.Today;
    dateTime1 = dateTime1.AddMonths(1);
    listFilterByDate5.Till = dateTime1.AddDays((double) -DateTime.Today.Day);
    listFilterArray[4] = (ListFilter) listFilterByDate5;
    ListFilterByDate listFilterByDate6 = new ListFilterByDate();
    listFilterByDate6.Title = this["Past Month", Array.Empty<object>()];
    listFilterByDate6.CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy);
    listFilterByDate6.Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x));
    listFilterByDate6.Counter = new Func<ListFilter, Task<int>>(this.CountByFilterAsync);
    DateTime dateTime2 = DateTime.Today;
    dateTime2 = dateTime2.AddDays((double) (1 - DateTime.Today.Day));
    listFilterByDate6.From = dateTime2.AddMonths(-1);
    listFilterByDate6.Till = DateTime.Today.AddDays((double) -DateTime.Today.Day);
    listFilterArray[5] = (ListFilter) listFilterByDate6;
    ListFilterByDate listFilterByDate7 = new ListFilterByDate();
    listFilterByDate7.Title = this["This Year", Array.Empty<object>()];
    listFilterByDate7.CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy);
    listFilterByDate7.Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x));
    listFilterByDate7.Counter = new Func<ListFilter, Task<int>>(this.CountByFilterAsync);
    listFilterByDate7.From = DateTime.Today.AddDays((double) (1 - DateTime.Today.DayOfYear));
    DateTime dateTime3 = DateTime.Today;
    dateTime3 = dateTime3.AddYears(1);
    listFilterByDate7.Till = dateTime3.AddDays((double) -DateTime.Today.DayOfYear);
    listFilterArray[6] = (ListFilter) listFilterByDate7;
    listFilterArray[7] = new ListFilter()
    {
      Title = this["All Records", Array.Empty<object>()],
      CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy),
      Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x)),
      Counter = new Func<ListFilter, Task<int>>(this.CountByFilterAsync)
    };
    this.Filters = (IEnumerable<ListFilter>) listFilterArray;
  }

  protected override Task PreLoad()
  {
    return Task.WhenAll(this.Filters.Select<ListFilter, Task>((Func<ListFilter, Task>) (x => x.Initialize())));
  }

  protected override Task OnLoad()
  {
    ListFilter listFilter = this.SelectedFilter;
    if (listFilter == null)
    {
      IEnumerable<ListFilter> filters = this.Filters;
      listFilter = filters != null ? filters.FirstOrDefault<ListFilter>() : (ListFilter) null;
    }
    this.SelectedFilter = listFilter;
    return this.SelectedFilter == null ? Task.CompletedTask : this.LoadByFilterAsync(this.SelectedFilter, false);
  }

  protected virtual async Task LoadByFilterAsync(ListFilter filter, bool setBusiness = true)
  {
    TransactionsListViewModel<T> transactionsListViewModel = this;
    if (setBusiness)
      transactionsListViewModel.IsBusy = true;
    try
    {
      if (filter is ListFilterByDate listFilterByDate)
      {
        transactionsListViewModel.DateFilterFrom = listFilterByDate.From;
        transactionsListViewModel.DateFilterTill = listFilterByDate.Till;
        IEnumerable<T> filteredListByDateAsync = await transactionsListViewModel.GetFilteredListByDateAsync(transactionsListViewModel.DateFilterFrom, transactionsListViewModel.DateFilterTillInclusive);
        transactionsListViewModel.List = filteredListByDateAsync;
      }
      else
      {
        IEnumerable<T> filteredListAsync = await transactionsListViewModel.GetFilteredListAsync(filter);
        transactionsListViewModel.List = filteredListAsync;
      }
      transactionsListViewModel.SubCaption = filter.Title;
    }
    catch (Exception ex)
    {
      transactionsListViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    if (!setBusiness)
      return;
    transactionsListViewModel.IsBusy = false;
  }

  protected virtual Task<int> CountByFilterAsync(ListFilter filter)
  {
    if (filter == null)
      return Task.FromResult<int>(0);
    return filter is ListFilterByDate listFilterByDate ? this.CountFilteredListByDateAsync(listFilterByDate.From, listFilterByDate.Till.AddDays(1.0)) : this.CountFilteredListAsync(filter);
  }

  protected virtual Task<int> CountFilteredListAsync(ListFilter filter)
  {
    return this.Repository.CountAsync();
  }

  protected virtual Task<int> CountFilteredListByDateAsync(DateTime from, DateTime till)
  {
    return this.Repository.CountAsync((Expression<Func<T, bool>>) (x => x.Date >= from && x.Date < till));
  }

  protected virtual Task<IEnumerable<T>> GetFilteredListAsync(ListFilter filter)
  {
    return this.Repository.GetAsync();
  }

  protected virtual Task<IEnumerable<T>> GetFilteredListByDateAsync(DateTime from, DateTime till)
  {
    return this.Repository.GetAsync((Expression<Func<T, bool>>) (x => x.Date >= from && x.Date < till));
  }
}
