// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Authorization.UserDetailsViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

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

#nullable disable
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
    this.Roleses = roles;
    this.Offices = offices;
    this.Warehouses = warehouses;
    this.Depositories = depositories;
  }

  protected override Task PreLoad()
  {
    return Task.WhenAll(base.PreLoad(), this.Roleses.Initialize(), this.Offices.Initialize(), this.Warehouses.Initialize(), this.Depositories.Initialize());
  }

  protected override async Task PostLoad()
  {
    UserDetailsViewModel detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__0();
    if (detailsViewModel.Details.Roles == null)
      detailsViewModel.Details.Roles = (IEnumerable<string>) new List<string>();
    detailsViewModel.RoleAssignments = new ObservableCollection<RoleAssignment>(detailsViewModel.Details.Roles.Select<string, RoleAssignment>((Func<string, RoleAssignment>) (x => new RoleAssignment()
    {
      RoleId = x
    })));
    // ISSUE: reference to a compiler-generated method
    DirtynessController.ControlSubList<RoleAssignment, User>(detailsViewModel.RoleAssignments, detailsViewModel.Details, new Action<User>(detailsViewModel.\u003CPostLoad\u003Eb__2_1));
    if (detailsViewModel.Details.AccountPrivileges == null)
      detailsViewModel.Details.AccountPrivileges = new Dictionary<string, AccountAccessLevel>();
    detailsViewModel.OfficeAssignments = new ObservableCollection<AccountAssignment>(detailsViewModel.Details.AccountPrivileges.Join<KeyValuePair<string, AccountAccessLevel>, Office, string, AccountAssignment>(detailsViewModel.Offices.List, (Func<KeyValuePair<string, AccountAccessLevel>, string>) (x => x.Key), (Func<Office, string>) (i => i.Id), (Func<KeyValuePair<string, AccountAccessLevel>, Office, AccountAssignment>) ((x, i) => new AccountAssignment()
    {
      AccountId = x.Key,
      OfficeId = i.Id,
      AccessLevel = x.Value
    })));
    // ISSUE: reference to a compiler-generated method
    DirtynessController.ControlSubList<AccountAssignment, User>(detailsViewModel.OfficeAssignments, detailsViewModel.Details, new Action<User>(detailsViewModel.\u003CPostLoad\u003Eb__2_5));
    detailsViewModel.WarehouseAssignments = new ObservableCollection<AccountAssignment>(detailsViewModel.Details.AccountPrivileges.Join<KeyValuePair<string, AccountAccessLevel>, Warehouse, string, AccountAssignment>(detailsViewModel.Warehouses.List, (Func<KeyValuePair<string, AccountAccessLevel>, string>) (x => x.Key), (Func<Warehouse, string>) (i => i.Id), (Func<KeyValuePair<string, AccountAccessLevel>, Warehouse, AccountAssignment>) ((x, i) => new AccountAssignment()
    {
      AccountId = x.Key,
      OfficeId = i.OfficeId,
      AccessLevel = x.Value
    })));
    // ISSUE: reference to a compiler-generated method
    DirtynessController.ControlSubList<AccountAssignment, User>(detailsViewModel.WarehouseAssignments, detailsViewModel.Details, new Action<User>(detailsViewModel.\u003CPostLoad\u003Eb__2_9));
    detailsViewModel.DepositoryAssignments = new ObservableCollection<AccountAssignment>(detailsViewModel.Details.AccountPrivileges.Join<KeyValuePair<string, AccountAccessLevel>, Depository, string, AccountAssignment>(detailsViewModel.Depositories.List, (Func<KeyValuePair<string, AccountAccessLevel>, string>) (x => x.Key), (Func<Depository, string>) (i => i.Id), (Func<KeyValuePair<string, AccountAccessLevel>, Depository, AccountAssignment>) ((x, i) => new AccountAssignment()
    {
      AccountId = x.Key,
      OfficeId = i.OfficeId,
      AccessLevel = x.Value
    })));
    // ISSUE: reference to a compiler-generated method
    DirtynessController.ControlSubList<AccountAssignment, User>(detailsViewModel.DepositoryAssignments, detailsViewModel.Details, new Action<User>(detailsViewModel.\u003CPostLoad\u003Eb__2_13));
    // ISSUE: reference to a compiler-generated method
    detailsViewModel.Offices.Filter = new Func<Office, bool>(detailsViewModel.\u003CPostLoad\u003Eb__2_14);
    // ISSUE: reference to a compiler-generated method
    detailsViewModel.Warehouses.Filter = new Func<Warehouse, bool>(detailsViewModel.\u003CPostLoad\u003Eb__2_15);
    // ISSUE: reference to a compiler-generated method
    detailsViewModel.Depositories.Filter = new Func<Depository, bool>(detailsViewModel.\u003CPostLoad\u003Eb__2_16);
    // ISSUE: reference to a compiler-generated method
    detailsViewModel.Roleses.Filter = new Func<Role, bool>(detailsViewModel.\u003CPostLoad\u003Eb__2_17);
  }

  protected override Task<bool> OnSaveAsync()
  {
    this.Details.Roles = this.RoleAssignments.Select<RoleAssignment, string>((Func<RoleAssignment, string>) (x => x.RoleId)).Distinct<string>();
    if (!string.IsNullOrEmpty(this.Details.Password))
      this.Details.Password = this.Details.Password.Hash();
    this.Details.AccountPrivileges = this.OfficeAssignments.Union<AccountAssignment>((IEnumerable<AccountAssignment>) this.WarehouseAssignments).Union<AccountAssignment>((IEnumerable<AccountAssignment>) this.DepositoryAssignments).Where<AccountAssignment>((Func<AccountAssignment, bool>) (x => !string.IsNullOrEmpty(x.AccountId))).GroupBy<AccountAssignment, string>((Func<AccountAssignment, string>) (x => x.AccountId)).ToDictionary<IGrouping<string, AccountAssignment>, string, AccountAccessLevel>((Func<IGrouping<string, AccountAssignment>, string>) (x => x.Key), (Func<IGrouping<string, AccountAssignment>, AccountAccessLevel>) (x => x.Select<AccountAssignment, AccountAccessLevel>((Func<AccountAssignment, AccountAccessLevel>) (i => i.AccessLevel)).Aggregate<AccountAccessLevel>((Func<AccountAccessLevel, AccountAccessLevel, AccountAccessLevel>) ((i, j) => i | j))));
    return base.OnSaveAsync();
  }

  public Reference<Role> Roleses { get; set; }

  public Reference<Office> Offices { get; }

  public Reference<Warehouse> Warehouses { get; set; }

  public Reference<Depository> Depositories { get; set; }

  public virtual RoleAssignment SelectedRoleAssignment
  {
    get => this._selectedRoleAssignment;
    set
    {
      this.SetProperty<RoleAssignment>(ref this._selectedRoleAssignment, value, nameof (SelectedRoleAssignment));
    }
  }

  public virtual ObservableCollection<RoleAssignment> RoleAssignments
  {
    get => this._roleAssignments;
    set
    {
      this.SetProperty<ObservableCollection<RoleAssignment>>(ref this._roleAssignments, value, nameof (RoleAssignments));
    }
  }

  public virtual AccountAssignment SelectedOfficeAssignment
  {
    get => this._selectedOfficeAssignment;
    set
    {
      this.SetProperty<AccountAssignment>(ref this._selectedOfficeAssignment, value, nameof (SelectedOfficeAssignment));
    }
  }

  public virtual ObservableCollection<AccountAssignment> OfficeAssignments
  {
    get => this._officeAssignments;
    set
    {
      this.SetProperty<ObservableCollection<AccountAssignment>>(ref this._officeAssignments, value, nameof (OfficeAssignments));
    }
  }

  public virtual AccountAssignment SelectedWarehouseAssignment
  {
    get => this._selectedWarehouseAssignment;
    set
    {
      this.SetProperty<AccountAssignment>(ref this._selectedWarehouseAssignment, value, nameof (SelectedWarehouseAssignment));
    }
  }

  public virtual ObservableCollection<AccountAssignment> WarehouseAssignments
  {
    get => this._warehouseAssignments;
    set
    {
      this.SetProperty<ObservableCollection<AccountAssignment>>(ref this._warehouseAssignments, value, nameof (WarehouseAssignments));
    }
  }

  public virtual AccountAssignment SelectedDepositoryAssignment
  {
    get => this._selectedDepositoryAssignment;
    set
    {
      this.SetProperty<AccountAssignment>(ref this._selectedDepositoryAssignment, value, nameof (SelectedDepositoryAssignment));
    }
  }

  public virtual ObservableCollection<AccountAssignment> DepositoryAssignments
  {
    get => this._depositoryAssignments;
    set
    {
      this.SetProperty<ObservableCollection<AccountAssignment>>(ref this._depositoryAssignments, value, nameof (DepositoryAssignments));
    }
  }

  public ICommand RemoveRoleCommand
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.RemoveRole), (Func<bool>) (() => !this.IsBusy && this.SelectedRoleAssignment != null));
    }
  }

  private void RemoveRole()
  {
    this.RoleAssignments.Remove(this.SelectedRoleAssignment);
    this.SelectedRoleAssignment = (RoleAssignment) null;
  }

  public ICommand RemoveOfficeCommand
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.RemoveOffice), (Func<bool>) (() => !this.IsBusy && this.SelectedOfficeAssignment != null));
    }
  }

  private void RemoveOffice()
  {
    this.OfficeAssignments.Remove(this.SelectedOfficeAssignment);
    this.SelectedOfficeAssignment = (AccountAssignment) null;
  }

  public ICommand RemoveWarehouseCommand
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.RemoveWarehouse), (Func<bool>) (() => !this.IsBusy && this.SelectedWarehouseAssignment != null));
    }
  }

  private void RemoveWarehouse()
  {
    this.WarehouseAssignments.Remove(this.SelectedWarehouseAssignment);
    this.SelectedWarehouseAssignment = (AccountAssignment) null;
  }

  public ICommand RemoveDepositoryCommand
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.RemoveDepository), (Func<bool>) (() => !this.IsBusy && this.SelectedDepositoryAssignment != null));
    }
  }

  private void RemoveDepository()
  {
    this.DepositoryAssignments.Remove(this.SelectedDepositoryAssignment);
    this.SelectedDepositoryAssignment = (AccountAssignment) null;
  }

  public ICommand SelectRoleCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.SelectRoleAsync), (Func<bool>) (() => !this.IsBusy && this.SelectedRoleAssignment != null));
    }
  }

  private async Task SelectRoleAsync()
  {
    UserDetailsViewModel detailsViewModel = this;
    RoleAssignment roleAssignment = detailsViewModel.SelectedRoleAssignment;
    roleAssignment.RoleId = await detailsViewModel.NavigationService.Navigate<ListViewModel<Role>, string, string>(detailsViewModel.SelectedRoleAssignment.RoleId);
    roleAssignment = (RoleAssignment) null;
  }

  public ICommand SelectOfficeCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.SelectOfficeAsync), (Func<bool>) (() => !this.IsBusy && this.SelectedOfficeAssignment != null));
    }
  }

  private async Task SelectOfficeAsync()
  {
    UserDetailsViewModel detailsViewModel = this;
    AccountAssignment accountAssignment = detailsViewModel.SelectedOfficeAssignment;
    accountAssignment.AccountId = await detailsViewModel.NavigationService.Navigate<ListViewModel<Office>, string, string>(detailsViewModel.SelectedOfficeAssignment.AccountId ?? Guid.Empty.ToString());
    accountAssignment = (AccountAssignment) null;
  }

  public ICommand SelectWarehouseCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.SelectWarehouseAsync), (Func<bool>) (() => !this.IsBusy && this.SelectedWarehouseAssignment != null));
    }
  }

  private async Task SelectWarehouseAsync()
  {
    UserDetailsViewModel detailsViewModel = this;
    AccountAssignment accountAssignment = detailsViewModel.SelectedWarehouseAssignment;
    accountAssignment.AccountId = await detailsViewModel.NavigationService.Navigate<ListViewModel<Warehouse>, string, string>(detailsViewModel.SelectedWarehouseAssignment.AccountId ?? Guid.Empty.ToString());
    accountAssignment = (AccountAssignment) null;
  }

  public ICommand SelectDepositoryCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.SelectDepositoryAsync), (Func<bool>) (() => !this.IsBusy && this.SelectedDepositoryAssignment != null));
    }
  }

  private async Task SelectDepositoryAsync()
  {
    UserDetailsViewModel detailsViewModel = this;
    AccountAssignment accountAssignment = detailsViewModel.SelectedDepositoryAssignment;
    accountAssignment.AccountId = await detailsViewModel.NavigationService.Navigate<ListViewModel<Depository>, string, string>(detailsViewModel.SelectedDepositoryAssignment.AccountId ?? Guid.Empty.ToString());
    accountAssignment = (AccountAssignment) null;
  }
}
