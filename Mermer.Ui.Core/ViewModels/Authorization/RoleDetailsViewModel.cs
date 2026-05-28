using MvvmCross.Core.Navigation;
using Mermer.Authorization.Enums;
using Mermer.Authorization.Models;
using Mermer.Commerce.Models;
using Mermer.Finance.Models;
using Mermer.Warehousing.Models;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Data.Tools;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mermer.Ui.Core.ViewModels.Authorization;

public class RoleDetailsViewModel : DetailsViewModel<Role>
{
    private IEnumerable<RoleAction> _actions;
    private IEnumerable<RoleAction> _listActions;
    private IEnumerable<RoleAction> _transactionActions;

    public RoleDetailsViewModel(
        IRepository<Role> repository,
        IListAuthorizer<Role> authorizer,
        IMvxNavigationService navigationService,
        IUserInteractionService userInteractionService)
        : base(repository, authorizer, navigationService, userInteractionService)
    {
    }

    protected override async Task PostLoad()
    {
        await base.PostLoad(); // Виправлений артефакт декомпілятора

        if (Details.Authorizations == null)
            Details.Authorizations = new Dictionary<string, int>();

        Actions = CreateActions<Mermer.Authorization.Enums.Actions, AccessLevel>();
        ListActions = CreateActions<Mermer.Authorization.Enums.ListActions, ListAccessLevel>();

        var roleActionList = new List<RoleAction>();
        roleActionList.AddRange(CreateActions<InvoiceType, TransactionAccessLevel>());
        roleActionList.AddRange(CreateActions<BillType, TransactionAccessLevel>());
        roleActionList.AddRange(CreateActions<StockSlipType, TransactionAccessLevel>());
        roleActionList.AddRange(CreateActions<FundsSlipType, TransactionAccessLevel>());
        roleActionList.AddRange(CreateActions<Mermer.Authorization.Enums.TransactionActions, TransactionAccessLevel>());

        TransactionActions = roleActionList;
    }

    public virtual IEnumerable<RoleAction> Actions
    {
        get => _actions;
        set => SetProperty(ref _actions, value);
    }

    public virtual IEnumerable<RoleAction> ListActions
    {
        get => _listActions;
        set => SetProperty(ref _listActions, value);
    }

    public virtual IEnumerable<RoleAction> TransactionActions
    {
        get => _transactionActions;
        set => SetProperty(ref _transactionActions, value);
    }

    private IEnumerable<RoleAction> CreateActions<TAction, TAccess>()
    {
        var array = Enum.GetValues(typeof(TAction)).Cast<TAction>().Select(x => new RoleAction(Details)
        {
            Id = x.ToString(),
            Name = this[x.ToString()]
        }).ToArray();

        foreach (var action in array)
        {
            action.Options = Enum.GetValues(typeof(TAccess)).Cast<TAccess>().Select(x => new RoleOption(action)
            {
                Name = this[x.ToString()],
                Value = Convert.ToInt32(x)
            }).ToArray();
        }

        DirtynessController.ControlList(array, x => IsDirty = true);
        return array;
    }
}