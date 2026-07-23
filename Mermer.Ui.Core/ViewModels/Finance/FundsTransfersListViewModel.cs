using MvvmCross.Core.Navigation;
using MvvmCross.Plugins.Messenger;
using Mermer.Enterprise.Models;
using Mermer.Finance.Models;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.ViewModels.Common;
using Mermer.Ui.Core.ViewModels.Transactions;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mermer.Ui.Core.ViewModels.Finance;

public class FundsTransfersListViewModel :
    FundsTransactionsListViewModel<FundsTransfer, FundsTransferLine>
{
    private bool _initialized;

    public FundsTransfersListViewModel(
        IMvxMessenger messenger,
        Reference<Depository> depositories,
        IRepository<FundsTransfer> repository,
        IListAuthorizer<FundsTransfer> authorizer,
        IMvxNavigationService navigationService,
        IUserInteractionService userInteractionService) :
        base(messenger, repository, authorizer, depositories, navigationService, userInteractionService)
    {
        var today = DateTime.Today;

        Filters = new ListFilter[]
        {
            new ListFilter
            {
                Title = this["Conflicted"],
                CanLoad = x => !IsBusy,
                Loader = x => LoadByFilterAsync(x),
                Counter = CountByFilterAsync,
                Tag = "Conflicted"
            },
            new ListFilterByDate
            {
                Title = this["Today"],
                CanLoad = x => !IsBusy,
                Loader = x => LoadByFilterAsync(x),
                Counter = CountByFilterAsync,
                From = today,
                Till = today
            },
            new ListFilterByDate
            {
                Title = this["This Week"],
                CanLoad = x => !IsBusy,
                Loader = x => LoadByFilterAsync(x),
                Counter = CountByFilterAsync,
                From = today.StartOfWeek(),
                Till = today.EndOfWeek()
            },
            new ListFilterByDate
            {
                Title = this["This Month"],
                CanLoad = x => !IsBusy,
                Loader = x => LoadByFilterAsync(x),
                Counter = CountByFilterAsync,
                From = today.AddDays(1 - today.Day),
                Till = today.AddMonths(1).AddDays(-today.Day)
            },
            new ListFilterByDate
            {
                Title = this["This Year"],
                CanLoad = x => !IsBusy,
                Loader = x => LoadByFilterAsync(x),
                Counter = CountByFilterAsync,
                From = today.AddDays(1 - today.DayOfYear),
                Till = today.AddYears(1).AddDays(-today.DayOfYear)
            },
            new ListFilter
            {
                Title = this["All Records"],
                CanLoad = x => !IsBusy,
                Loader = x => LoadByFilterAsync(x),
                Counter = CountByFilterAsync,
                Tag = "All"
            }
        };
    }

    protected override Task OnLoad()
    {
        if (!_initialized)
        {
            SelectedFilter = Filters.ElementAt(2); // По умолчанию выбираем "This Week"
            _initialized = true;
        }
        return base.OnLoad();
    }

    protected override Task<int> CountByFilterAsync(ListFilter filter)
    {
        if (filter.Tag?.ToString() == "Conflicted")
            return Repository.CountAsync(x => x.IsConflicted);

        return base.CountByFilterAsync(filter);
    }

    protected override Task<IEnumerable<FundsTransfer>> GetFilteredListAsync(ListFilter filter)
    {
        if (filter.Tag?.ToString() == "Conflicted")
            return Repository.GetAsync(x => x.IsConflicted);

        return base.GetFilteredListAsync(filter);
    }
}