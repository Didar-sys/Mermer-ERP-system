// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Warehousing.Ordering.StockOrderTemplatesListViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Mermer.Ui.Core.Helpers;
using Mermer.Warehousing.Ordering.Models;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Warehousing.Ordering;

public class StockOrderTemplatesListViewModel : ListViewModel<StockOrderTemplate>
{
  private const string FilterActiveString = "Active";
  private const string FilterDisabledString = "Disabled";
  private IEnumerable<ListFilter> _filters;
  private ListFilter _selectedFilter;

  public StockOrderTemplatesListViewModel(
    IRepository<StockOrderTemplate> repository,
    IListAuthorizer<StockOrderTemplate> authorizer,
    IMvxMessenger messenger,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(repository, authorizer, messenger, navigationService, userInteractionService)
  {
    this.Filters = (IEnumerable<ListFilter>) new ListFilter[3]
    {
      new ListFilter()
      {
        Title = this["Active", Array.Empty<object>()],
        Tag = (object) "Active",
        CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy),
        Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x)),
        Counter = new Func<ListFilter, Task<int>>(this.CountByFilterAsync)
      },
      new ListFilter()
      {
        Title = this["Disabled", Array.Empty<object>()],
        Tag = (object) "Disabled",
        CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy),
        Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x)),
        Counter = new Func<ListFilter, Task<int>>(this.CountByFilterAsync)
      },
      new ListFilter()
      {
        Title = this["All Records", Array.Empty<object>()],
        Tag = (object) string.Empty,
        CanLoad = (Func<ListFilter, bool>) (x => !this.IsBusy),
        Loader = (Func<ListFilter, Task>) (x => this.LoadByFilterAsync(x)),
        Counter = new Func<ListFilter, Task<int>>(this.CountByFilterAsync)
      }
    };
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

  protected override Task PreLoad()
  {
    System.Collections.Generic.List<Task> list = this.Filters.Select<ListFilter, Task>((Func<ListFilter, Task>) (x => x.Initialize())).ToList<Task>();
    list.Add(base.PreLoad());
    return Task.WhenAll((IEnumerable<Task>) list);
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
        StockOrderTemplatesListViewModel templatesListViewModel = this;
        if (setBusiness)
            templatesListViewModel.IsBusy = true;
        try
        {
            string tag = filter?.Tag?.ToString() ?? string.Empty;

            switch (tag)
            {
                case "Active":
                    IEnumerable<StockOrderTemplate> async1 = await templatesListViewModel.Repository.GetAsync((Expression<Func<StockOrderTemplate, bool>>)(x => !x.IsDisabled));
                    templatesListViewModel.List = async1;
                    break;
                case "Disabled":
                    IEnumerable<StockOrderTemplate> async2 = await templatesListViewModel.Repository.GetAsync((Expression<Func<StockOrderTemplate, bool>>)(x => x.IsDisabled));
                    templatesListViewModel.List = async2;
                    break;
                default:
                    IEnumerable<StockOrderTemplate> async3 = await templatesListViewModel.Repository.GetAsync();
                    templatesListViewModel.List = async3;
                    break;
            }
            templatesListViewModel.SubCaption = filter?.Title;
        }
        catch (Exception ex)
        {
            templatesListViewModel.UserInteractionService.ShowExceptionMessage(ex);
        }
        if (!setBusiness)
            return;
        templatesListViewModel.IsBusy = false;
    }

    protected virtual Task<int> CountByFilterAsync(ListFilter filter)
    {
        string tag = filter?.Tag?.ToString() ?? string.Empty;

        switch (tag)
        {
            case "Active":
                return this.Repository.CountAsync((Expression<Func<StockOrderTemplate, bool>>)(x => !x.IsDisabled));
            case "Disabled":
                return this.Repository.CountAsync((Expression<Func<StockOrderTemplate, bool>>)(x => x.IsDisabled));
            default:
                return this.Repository.CountAsync();
        }
    }
}
