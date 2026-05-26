// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Authorization.RoleDetailsViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

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

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Authorization;

public class RoleDetailsViewModel(
  IRepository<Role> repository,
  IListAuthorizer<Role> authorizer,
  IMvxNavigationService navigationService,
  IUserInteractionService userInteractionService) : DetailsViewModel<Role>(repository, authorizer, navigationService, userInteractionService)
{
  private IEnumerable<RoleAction> _actions;
  private IEnumerable<RoleAction> _listActions;
  private IEnumerable<RoleAction> _transactionActions;

  protected override async Task PostLoad()
  {
    RoleDetailsViewModel detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__0();
    if (detailsViewModel.Details.Authorizations == null)
      detailsViewModel.Details.Authorizations = new Dictionary<string, int>();
    detailsViewModel.Actions = detailsViewModel.CreateActions<Mermer.Authorization.Enums.Actions, AccessLevel>();
    detailsViewModel.ListActions = detailsViewModel.CreateActions<Mermer.Authorization.Enums.ListActions, ListAccessLevel>();
    List<RoleAction> roleActionList = new List<RoleAction>();
    roleActionList.AddRange(detailsViewModel.CreateActions<InvoiceType, TransactionAccessLevel>());
    roleActionList.AddRange(detailsViewModel.CreateActions<BillType, TransactionAccessLevel>());
    roleActionList.AddRange(detailsViewModel.CreateActions<StockSlipType, TransactionAccessLevel>());
    roleActionList.AddRange(detailsViewModel.CreateActions<FundsSlipType, TransactionAccessLevel>());
    roleActionList.AddRange(detailsViewModel.CreateActions<Mermer.Authorization.Enums.TransactionActions, TransactionAccessLevel>());
    detailsViewModel.TransactionActions = (IEnumerable<RoleAction>) roleActionList;
  }

  public virtual IEnumerable<RoleAction> Actions
  {
    get => this._actions;
    set => this.SetProperty<IEnumerable<RoleAction>>(ref this._actions, value, nameof (Actions));
  }

  public virtual IEnumerable<RoleAction> ListActions
  {
    get => this._listActions;
    set
    {
      this.SetProperty<IEnumerable<RoleAction>>(ref this._listActions, value, nameof (ListActions));
    }
  }

  public virtual IEnumerable<RoleAction> TransactionActions
  {
    get => this._transactionActions;
    set
    {
      this.SetProperty<IEnumerable<RoleAction>>(ref this._transactionActions, value, nameof (TransactionActions));
    }
  }

  private IEnumerable<RoleAction> CreateActions<TAction, TAccess>()
  {
    RoleAction[] array = Enum.GetValues(typeof (TAction)).Cast<TAction>().Select<TAction, RoleAction>((Func<TAction, RoleAction>) (x => new RoleAction(this.Details)
    {
      Id = x.ToString(),
      Name = this[x.ToString(), Array.Empty<object>()]
    })).ToArray<RoleAction>();
    foreach (RoleAction roleAction in array)
    {
      RoleAction action = roleAction;
      action.Options = (IEnumerable<RoleOption>) Enum.GetValues(typeof (TAccess)).Cast<TAccess>().Select<TAccess, RoleOption>((Func<TAccess, RoleOption>) (x => new RoleOption(action)
      {
        Name = this[x.ToString(), Array.Empty<object>()],
        Value = Convert.ToInt32((object) x)
      })).ToArray<RoleOption>();
    }
    DirtynessController.ControlList<RoleAction>((IEnumerable<RoleAction>) array, (Action<RoleAction>) (x => this.IsDirty = true));
    return (IEnumerable<RoleAction>) array;
  }
}
