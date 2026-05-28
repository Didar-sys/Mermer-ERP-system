// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Warehousing.Revisioning.StockRevisionsListViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Mermer.Authorization.Services;
using Mermer.Enterprise.Models;
using Mermer.Transactions.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.ViewModels.Common;
using Mermer.Warehousing.Revisioning.Models;
using Mermer.Warehousing.Revisioning.Services;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Warehousing.Revisioning;

public class StockRevisionsListViewModel : TransactionsListViewModel<StockRevision>
{
  private const string FilterActiveString = "Active";
  private const string FilterCompletedString = "Completed";
  private const string FilterDeletedString = "Deleted";
  private const string FilterAllString = "All Records";
  private readonly ILoginService _loginService;
  private readonly ITransactionCodeGenerationService _codeGentor;

    public StockRevisionsListViewModel(
      IMvxMessenger messenger,
      ILoginService loginService,
      Reference<Warehouse> warehouses,
      IStockRevisionsRepository repository,
      IListAuthorizer<StockRevision> authorizer,
      IMvxNavigationService navigationService,
      ITransactionCodeGenerationService codeGentor,
      IUserInteractionService userInteractionService)
      : base(repository, authorizer, messenger, navigationService, userInteractionService)
    {
        _loginService = loginService;
        _codeGentor = codeGentor;
        Warehouses = warehouses;

        // Чистий масив фільтрів без зайвих кастів
        Filters = new[]
        {
        new ListFilter
        {
            Title = this["Active"],
            Tag = "Active",
            CanLoad = x => !IsBusy,
            Loader = x => LoadByFilterAsync(x),
            Counter = CountByFilterAsync
        },
        new ListFilter
        {
            Title = this["Completed"],
            Tag = "Completed",
            CanLoad = x => !IsBusy,
            Loader = x => LoadByFilterAsync(x),
            Counter = CountByFilterAsync
        },
        new ListFilter
        {
            Title = this["Deleted"],
            Tag = "Deleted",
            CanLoad = x => !IsBusy,
            Loader = x => LoadByFilterAsync(x),
            Counter = CountByFilterAsync
        },
        new ListFilter
        {
            Title = this["All Records"],
            Tag = "All Records",
            CanLoad = x => !IsBusy,
            Loader = x => LoadByFilterAsync(x),
            Counter = CountByFilterAsync
        }
    };
  }

  public Reference<Warehouse> Warehouses { get; }

  protected override Task PreLoad() => Task.WhenAll(base.PreLoad(), this.Warehouses.Initialize());

  protected override async Task OnCreateNewAsync()
  {
    StockRevisionsListViewModel revisionsListViewModel = this;
    try
    {
      StockRevisionsListNewRevisionViewModel.Result result = await revisionsListViewModel.NavigationService.Navigate<StockRevisionsListNewRevisionViewModel, IEnumerable<Warehouse>, StockRevisionsListNewRevisionViewModel.Result>(revisionsListViewModel.Warehouses.List);
      if (result == null)
        return;
      StockRevision stockRevision1 = new StockRevision();
      stockRevision1.Id = Guid.NewGuid().ToString();
      StockRevision stockRevision2 = stockRevision1;
      stockRevision2.Code = await revisionsListViewModel._codeGentor.GetNextCode();
      stockRevision1.Date = result.StartDate;
      stockRevision1.WarehouseId = result.WarehouseId;
      stockRevision1.UserId = revisionsListViewModel._loginService.Session.UserId;
      stockRevision1.UserName = revisionsListViewModel._loginService.Session.Username;
      StockRevision revision = stockRevision1;
      stockRevision2 = (StockRevision) null;
      stockRevision1 = (StockRevision) null;
      await revisionsListViewModel.Repository.CreateAsync(revision);
      await revisionsListViewModel.NavigationService.Navigate<StockRevisionDetailsViewModel, string>(revision.Id);
      result = (StockRevisionsListNewRevisionViewModel.Result) null;
      revision = (StockRevision) null;
    }
    catch (Exception ex)
    {
      revisionsListViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
  }

  protected override Task<int> CountFilteredListAsync(ListFilter filter)
  {
    switch (filter.Tag.ToString())
    {
      case "Active":
        return this.Repository.CountAsync((Expression<Func<StockRevision, bool>>) (x => !x.IsCompleted && !x.IsDisabled));
      case "Completed":
        return this.Repository.CountAsync((Expression<Func<StockRevision, bool>>) (x => x.IsCompleted && !x.IsDisabled));
      case "Deleted":
        return this.Repository.CountAsync((Expression<Func<StockRevision, bool>>) (x => x.IsDisabled));
      default:
        return base.CountFilteredListAsync(filter);
    }
  }

  protected override Task<IEnumerable<StockRevision>> GetFilteredListAsync(ListFilter filter)
  {
    switch (filter.Tag.ToString())
    {
      case "Active":
        return this.Repository.GetAsync((Expression<Func<StockRevision, bool>>) (x => !x.IsCompleted && !x.IsDisabled));
      case "Completed":
        return this.Repository.GetAsync((Expression<Func<StockRevision, bool>>) (x => x.IsCompleted && !x.IsDisabled));
      case "Deleted":
        return this.Repository.GetAsync((Expression<Func<StockRevision, bool>>) (x => x.IsDisabled));
      default:
        return base.GetFilteredListAsync(filter);
    }
  }
}
