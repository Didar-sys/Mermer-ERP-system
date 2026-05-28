using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Mermer.Authorization.Enums;
using Mermer.Authorization.Models;
using Mermer.Enterprise.Models;
using Mermer.Ui.Core.Helpers;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Data.Tools;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mermer.Ui.Core.ViewModels.Authorization;

public class UserDetailsViewModel : DetailsViewModel<User>
{
    private RoleAssignment _selectedRoleAssignment;
    private ObservableCollection<RoleAssignment> _roleAssignments;
    private AccountAssignment _selectedOfficeAssignment;
    private ObservableCollection<AccountAssignment> _officeAssignments;
    private AccountAssignment _selectedWarehouseAssignment;
    private ObservableCollection<AccountAssignment> _warehouseAssignments;
    private AccountAssignment _selectedDepositoryAssignment;
    private ObservableCollection<AccountAssignment> _depositoryAssignments;

    public UserDetailsViewModel(
        IRepository<User> repository,
        IListAuthorizer<User> authorizer,
        Reference<Role> roles,
        Reference<Office> offices,
        Reference<Warehouse> warehouses,
        Reference<Depository> depositories,
        IMvxNavigationService navigationService,
        IUserInteractionService userInteractionService)
        : base(repository, authorizer, navigationService, userInteractionService)
    {
        Roleses = roles;
        Offices = offices;
        Warehouses = warehouses;
        Depositories = depositories;
    }

    public Reference<Role> Roleses { get; set; }
    public Reference<Office> Offices { get; }
    public Reference<Warehouse> Warehouses { get; set; }
    public Reference<Depository> Depositories { get; set; }

    protected override Task PreLoad()
    {
        return Task.WhenAll(base.PreLoad(), Roleses.Initialize(), Offices.Initialize(), Warehouses.Initialize(), Depositories.Initialize());
    }

    protected override async Task PostLoad()
    {
        await base.PostLoad();

        if (Details.Roles == null)
            Details.Roles = new List<string>();

        RoleAssignments = new ObservableCollection<RoleAssignment>(
            Details.Roles.Select(x => new RoleAssignment { RoleId = x })
        );

        // Відновлена втрачена лямбда b__2_1
        DirtynessController.ControlSubList(RoleAssignments, Details, x => IsDirty = true);

        if (Details.AccountPrivileges == null)
            Details.AccountPrivileges = new Dictionary<string, AccountAccessLevel>();

        // Очищені та відновлені LINQ Join
        OfficeAssignments = new ObservableCollection<AccountAssignment>(
            Details.AccountPrivileges.Join(Offices.List,
                x => x.Key,
                i => i.Id,
                (x, i) => new AccountAssignment { AccountId = x.Key, OfficeId = i.Id, AccessLevel = x.Value })
        );
        DirtynessController.ControlSubList(OfficeAssignments, Details, x => IsDirty = true);

        WarehouseAssignments = new ObservableCollection<AccountAssignment>(
            Details.AccountPrivileges.Join(Warehouses.List,
                x => x.Key,
                i => i.Id,
                (x, i) => new AccountAssignment { AccountId = x.Key, OfficeId = i.OfficeId, AccessLevel = x.Value })
        );
        DirtynessController.ControlSubList(WarehouseAssignments, Details, x => IsDirty = true);

        DepositoryAssignments = new ObservableCollection<AccountAssignment>(
            Details.AccountPrivileges.Join(Depositories.List,
                x => x.Key,
                i => i.Id,
                (x, i) => new AccountAssignment { AccountId = x.Key, OfficeId = i.OfficeId, AccessLevel = x.Value })
        );
        DirtynessController.ControlSubList(DepositoryAssignments, Details, x => IsDirty = true);

        // Відновлені втрачені лямбди фільтрів
        Offices.Filter = x => !x.IsDisabled;
        Warehouses.Filter = x => !x.IsDisabled;
        Depositories.Filter = x => !x.IsDisabled;
        Roleses.Filter = x => !x.IsDisabled;
    }

    protected override Task<bool> OnSaveAsync()
    {
        Details.Roles = RoleAssignments.Select(x => x.RoleId).Distinct();

        if (!string.IsNullOrEmpty(Details.Password))
            Details.Password = Details.Password.Hash(); // Якщо Hash() не знайдено, можливо потрібен using

        Details.AccountPrivileges = OfficeAssignments
            .Union(WarehouseAssignments)
            .Union(DepositoryAssignments)
            .Where(x => !string.IsNullOrEmpty(x.AccountId))
            .GroupBy(x => x.AccountId)
            .ToDictionary(
                x => x.Key,
                x => x.Select(i => i.AccessLevel).Aggregate((i, j) => i | j)
            );

        return base.OnSaveAsync();
    }

    public virtual RoleAssignment SelectedRoleAssignment
    {
        get => _selectedRoleAssignment;
        set => SetProperty(ref _selectedRoleAssignment, value);
    }

    public virtual ObservableCollection<RoleAssignment> RoleAssignments
    {
        get => _roleAssignments;
        set => SetProperty(ref _roleAssignments, value);
    }

    public virtual AccountAssignment SelectedOfficeAssignment
    {
        get => _selectedOfficeAssignment;
        set => SetProperty(ref _selectedOfficeAssignment, value);
    }

    public virtual ObservableCollection<AccountAssignment> OfficeAssignments
    {
        get => _officeAssignments;
        set => SetProperty(ref _officeAssignments, value);
    }

    public virtual AccountAssignment SelectedWarehouseAssignment
    {
        get => _selectedWarehouseAssignment;
        set => SetProperty(ref _selectedWarehouseAssignment, value);
    }

    public virtual ObservableCollection<AccountAssignment> WarehouseAssignments
    {
        get => _warehouseAssignments;
        set => SetProperty(ref _warehouseAssignments, value);
    }

    public virtual AccountAssignment SelectedDepositoryAssignment
    {
        get => _selectedDepositoryAssignment;
        set => SetProperty(ref _selectedDepositoryAssignment, value);
    }

    public virtual ObservableCollection<AccountAssignment> DepositoryAssignments
    {
        get => _depositoryAssignments;
        set => SetProperty(ref _depositoryAssignments, value);
    }

    public ICommand RemoveRoleCommand => new MvxCommand(RemoveRole, () => !IsBusy && SelectedRoleAssignment != null);

    private void RemoveRole()
    {
        RoleAssignments.Remove(SelectedRoleAssignment);
        SelectedRoleAssignment = null;
    }

    public ICommand RemoveOfficeCommand => new MvxCommand(RemoveOffice, () => !IsBusy && SelectedOfficeAssignment != null);

    private void RemoveOffice()
    {
        OfficeAssignments.Remove(SelectedOfficeAssignment);
        SelectedOfficeAssignment = null;
    }

    public ICommand RemoveWarehouseCommand => new MvxCommand(RemoveWarehouse, () => !IsBusy && SelectedWarehouseAssignment != null);

    private void RemoveWarehouse()
    {
        WarehouseAssignments.Remove(SelectedWarehouseAssignment);
        SelectedWarehouseAssignment = null;
    }

    public ICommand RemoveDepositoryCommand => new MvxCommand(RemoveDepository, () => !IsBusy && SelectedDepositoryAssignment != null);

    private void RemoveDepository()
    {
        DepositoryAssignments.Remove(SelectedDepositoryAssignment);
        SelectedDepositoryAssignment = null;
    }

    public ICommand SelectRoleCommand => new MvxAsyncCommand(SelectRoleAsync, () => !IsBusy && SelectedRoleAssignment != null);

    private async Task SelectRoleAsync()
    {
        SelectedRoleAssignment.RoleId = await NavigationService.Navigate<ListViewModel<Role>, string, string>(SelectedRoleAssignment.RoleId);
    }

    public ICommand SelectOfficeCommand => new MvxAsyncCommand(SelectOfficeAsync, () => !IsBusy && SelectedOfficeAssignment != null);

    private async Task SelectOfficeAsync()
    {
        SelectedOfficeAssignment.AccountId = await NavigationService.Navigate<ListViewModel<Office>, string, string>(SelectedOfficeAssignment.AccountId ?? Guid.Empty.ToString());
    }

    public ICommand SelectWarehouseCommand => new MvxAsyncCommand(SelectWarehouseAsync, () => !IsBusy && SelectedWarehouseAssignment != null);

    private async Task SelectWarehouseAsync()
    {
        SelectedWarehouseAssignment.AccountId = await NavigationService.Navigate<ListViewModel<Warehouse>, string, string>(SelectedWarehouseAssignment.AccountId ?? Guid.Empty.ToString());
    }

    public ICommand SelectDepositoryCommand => new MvxAsyncCommand(SelectDepositoryAsync, () => !IsBusy && SelectedDepositoryAssignment != null);

    private async Task SelectDepositoryAsync()
    {
        SelectedDepositoryAssignment.AccountId = await NavigationService.Navigate<ListViewModel<Depository>, string, string>(SelectedDepositoryAssignment.AccountId ?? Guid.Empty.ToString());
    }
}