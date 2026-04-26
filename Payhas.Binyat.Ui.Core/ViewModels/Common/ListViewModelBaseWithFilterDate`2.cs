// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Common.ListViewModelBaseWithFilterDate`2
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Data.Tools.Expressions;
using Payhas.Mvvm.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Common;

public abstract class ListViewModelBaseWithFilterDate<TList, TFilter> : 
  ListViewModelBaseWithFilter<TList, TFilter>
{
  private DateTime _dateFilterFrom;
  private DateTime _dateFilterTill;
  private bool _isCustomDateFilter;

  protected ListViewModelBaseWithFilterDate(
    IMvxMessenger messenger,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    ListFilter[] listFilterArray = new ListFilter[8];
    ListFilterByDate listFilterByDate1 = new ListFilterByDate();
    listFilterByDate1.Title = this["Today", Array.Empty<object>()];
    listFilterByDate1.CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy);
    listFilterByDate1.Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x));
    listFilterByDate1.Counter = new Func<ListFilter, Task<int>>(((ListViewModelBaseWithFilter<TList, TFilter>) this).CountByFilterAsync);
    listFilterByDate1.From = DateTime.Today;
    listFilterByDate1.Till = DateTime.Today;
    listFilterArray[0] = (ListFilter) listFilterByDate1;
    ListFilterByDate listFilterByDate2 = new ListFilterByDate();
    listFilterByDate2.Title = this["Yesturday", Array.Empty<object>()];
    listFilterByDate2.CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy);
    listFilterByDate2.Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x));
    listFilterByDate2.Counter = new Func<ListFilter, Task<int>>(((ListViewModelBaseWithFilter<TList, TFilter>) this).CountByFilterAsync);
    listFilterByDate2.From = DateTime.Today.AddDays(-1.0);
    listFilterByDate2.Till = DateTime.Today.AddDays(-1.0);
    listFilterArray[1] = (ListFilter) listFilterByDate2;
    ListFilterByDate listFilterByDate3 = new ListFilterByDate();
    listFilterByDate3.Title = this["This Week", Array.Empty<object>()];
    listFilterByDate3.CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy);
    listFilterByDate3.Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x));
    listFilterByDate3.Counter = new Func<ListFilter, Task<int>>(((ListViewModelBaseWithFilter<TList, TFilter>) this).CountByFilterAsync);
    listFilterByDate3.From = DateTime.Today.StartOfWeek();
    listFilterByDate3.Till = DateTime.Today.EndOfWeek();
    listFilterArray[2] = (ListFilter) listFilterByDate3;
    ListFilterByDate listFilterByDate4 = new ListFilterByDate();
    listFilterByDate4.Title = this["Past Week", Array.Empty<object>()];
    listFilterByDate4.CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy);
    listFilterByDate4.Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x));
    listFilterByDate4.Counter = new Func<ListFilter, Task<int>>(((ListViewModelBaseWithFilter<TList, TFilter>) this).CountByFilterAsync);
    listFilterByDate4.From = DateTime.Today.AddDays(-7.0).StartOfWeek();
    listFilterByDate4.Till = DateTime.Today.AddDays(-7.0).EndOfWeek();
    listFilterArray[3] = (ListFilter) listFilterByDate4;
    ListFilterByDate listFilterByDate5 = new ListFilterByDate();
    listFilterByDate5.Title = this["This Month", Array.Empty<object>()];
    listFilterByDate5.CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy);
    listFilterByDate5.Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x));
    listFilterByDate5.Counter = new Func<ListFilter, Task<int>>(((ListViewModelBaseWithFilter<TList, TFilter>) this).CountByFilterAsync);
    listFilterByDate5.From = DateTime.Today.AddDays((double) (1 - DateTime.Today.Day));
    DateTime dateTime1 = DateTime.Today;
    dateTime1 = dateTime1.AddMonths(1);
    listFilterByDate5.Till = dateTime1.AddDays((double) -DateTime.Today.Day);
    listFilterArray[4] = (ListFilter) listFilterByDate5;
    ListFilterByDate listFilterByDate6 = new ListFilterByDate();
    listFilterByDate6.Title = this["Past Month", Array.Empty<object>()];
    listFilterByDate6.CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy);
    listFilterByDate6.Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x));
    listFilterByDate6.Counter = new Func<ListFilter, Task<int>>(((ListViewModelBaseWithFilter<TList, TFilter>) this).CountByFilterAsync);
    DateTime dateTime2 = DateTime.Today;
    dateTime2 = dateTime2.AddDays((double) (1 - DateTime.Today.Day));
    listFilterByDate6.From = dateTime2.AddMonths(-1);
    listFilterByDate6.Till = DateTime.Today.AddDays((double) -DateTime.Today.Day);
    listFilterArray[5] = (ListFilter) listFilterByDate6;
    ListFilterByDate listFilterByDate7 = new ListFilterByDate();
    listFilterByDate7.Title = this["This Year", Array.Empty<object>()];
    listFilterByDate7.CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy);
    listFilterByDate7.Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x));
    listFilterByDate7.Counter = new Func<ListFilter, Task<int>>(((ListViewModelBaseWithFilter<TList, TFilter>) this).CountByFilterAsync);
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
      Counter = new Func<ListFilter, Task<int>>(((ListViewModelBaseWithFilter<TList, TFilter>) this).CountByFilterAsync)
    };
    this.Filters = (IEnumerable<ListFilter>) listFilterArray;
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

  protected override Task OnLoad()
  {
    return !this._isCustomDateFilter ? base.OnLoad() : this.LoadByDateAsync(false);
  }

  protected override async Task LoadByFilterAsync(ListFilter filter, bool setBusiness = true)
  {
    ListViewModelBaseWithFilterDate<TList, TFilter> baseWithFilterDate = this;
    if (setBusiness)
      baseWithFilterDate.IsBusy = true;
    try
    {
      if (filter is ListFilterByDate listFilterByDate)
      {
        baseWithFilterDate.DateFilterFrom = listFilterByDate.From;
        baseWithFilterDate.DateFilterTill = listFilterByDate.Till;
        IEnumerable<TList> filteredListByDateAsync = await baseWithFilterDate.GetFilteredListByDateAsync(baseWithFilterDate.DateFilterFrom, baseWithFilterDate.DateFilterTillInclusive);
        baseWithFilterDate.List = filteredListByDateAsync;
      }
      else
      {
        IEnumerable<TList> filteredListAsync = await baseWithFilterDate.GetFilteredListAsync(filter);
        baseWithFilterDate.List = filteredListAsync;
      }
      baseWithFilterDate.SubCaption = filter.Title;
      baseWithFilterDate._isCustomDateFilter = false;
    }
    catch (Exception ex)
    {
      baseWithFilterDate.UserInteractionService.ShowExceptionMessage(ex);
    }
    if (!setBusiness)
      return;
    baseWithFilterDate.IsBusy = false;
  }

  protected override Task<int> CountByFilterAsync(ListFilter filter)
  {
    if (filter == null)
      return Task.FromResult<int>(0);
    return filter is ListFilterByDate listFilterByDate ? this.CountFilteredListByDateAsync(listFilterByDate.From, listFilterByDate.Till.AddDays(1.0)) : this.CountFilteredListAsync(filter);
  }

  protected abstract Expression<Func<TFilter, bool>> GetDateFilter(DateTime from, DateTime till);

  protected virtual Task<int> CountFilteredListByDateAsync(DateTime from, DateTime till)
  {
    PredicateBuilder<TFilter> predicateBuilder = this.GetPredicateBuilder((ListFilter) null);
    predicateBuilder.Add(this.GetDateFilter(from, till));
    return this.CountListAsync(predicateBuilder.Expressions);
  }

  protected virtual Task<IEnumerable<TList>> GetFilteredListByDateAsync(
    DateTime from,
    DateTime till)
  {
    PredicateBuilder<TFilter> predicateBuilder = this.GetPredicateBuilder((ListFilter) null);
    predicateBuilder.Add(this.GetDateFilter(from, till));
    return this.GetListAsync(predicateBuilder.Expressions);
  }

  public ICommand FilterByDateCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnFilterByDateCommandAsync), (Func<bool>) (() => !this.IsBusy && this.DateFilterFrom <= this.DateFilterTill));
    }
  }

  protected virtual async Task OnFilterByDateCommandAsync() => await this.LoadByDateAsync();

  protected virtual async Task LoadByDateAsync(bool setBusiness = true)
  {
    ListViewModelBaseWithFilterDate<TList, TFilter> baseWithFilterDate = this;
    int num;
    if (num != 0 && setBusiness)
      baseWithFilterDate.IsBusy = true;
    try
    {
      baseWithFilterDate.SelectedFilter = (ListFilter) null;
      IEnumerable<TList> filteredListByDateAsync = await baseWithFilterDate.GetFilteredListByDateAsync(baseWithFilterDate.DateFilterFrom, baseWithFilterDate.DateFilterTillInclusive);
      baseWithFilterDate.List = filteredListByDateAsync;
      baseWithFilterDate.SubCaption = $"{baseWithFilterDate.DateFilterFrom:MMM d} - {baseWithFilterDate.DateFilterTill:MMM d}";
      baseWithFilterDate._isCustomDateFilter = true;
    }
    catch (Exception ex)
    {
      baseWithFilterDate.UserInteractionService.ShowExceptionMessage(ex);
    }
    if (!setBusiness)
      return;
    baseWithFilterDate.IsBusy = false;
  }
}
