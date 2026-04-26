// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Transactions.FundsTransactionDetailsViewModel`3
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Payhas.Binyat.Authorization.Enums;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.Transactions.Models;
using Payhas.Binyat.Transactions.Models.Authorizers;
using Payhas.Binyat.Transactions.Services;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Data.Authorizers;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;
using Payhas.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Transactions;

public class FundsTransactionDetailsViewModel<T, TLine, TType>(
  IRepository<T> repository,
  IListAuthorizer<T> authorizer,
  IConfigurator configurator,
  ILoginService loginService,
  Reference<Currency> currencies,
  Reference<Depository> depositories,
  IMvxNavigationService navigationService,
  ITransactionCodeGenerationService codegentor,
  IUserInteractionService userInteractionService) : FundsTransactionDetailsViewModel<T, TLine>(repository, authorizer, configurator, loginService, currencies, depositories, navigationService, codegentor, userInteractionService)
  where T : FundsTransaction<TLine>
  where TLine : FundsTransactionLine
{
  private ListHelper<TType>[] _types;
  private bool _canChangeType;

  protected override MvxInpcInterceptionResult InterceptRaisePropertyChanged(
    PropertyChangedEventArgs changedArgs)
  {
    if (changedArgs.PropertyName == "HasSaveAccess")
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.CanChangeType));
    return base.InterceptRaisePropertyChanged(changedArgs);
  }

  public virtual ListHelper<TType>[] Types
  {
    get => this._types;
    set => this.SetProperty<ListHelper<TType>[]>(ref this._types, value, nameof (Types));
  }

  public bool CanChangeType
  {
    get => this.HasSaveAccess && this._canChangeType;
    set => this.SetProperty<bool>(ref this._canChangeType, value, nameof (CanChangeType));
  }

  protected override Task PreLoad()
  {
    if (this.Authorizer is ITransactionAuthorizer<T> authorizer)
    {
      this.Types = Enum.GetValues(typeof (TType)).Cast<TType>().Select<TType, ListHelper<TType>>((Func<TType, ListHelper<TType>>) (x => new ListHelper<TType>()
      {
        Text = this[x.ToString(), Array.Empty<object>()],
        Value = x
      })).ToArray<ListHelper<TType>>();
      if (this.LoginService.Session.IsAdmin)
        this.CanChangeType = true;
      else if (!string.IsNullOrEmpty(this.ItemId))
      {
        this.CanChangeType = false;
      }
      else
      {
        IEnumerable<string> createableActions = authorizer.GetAvailableActions(TransactionAccessLevel.Create);
        this.Types = ((IEnumerable<ListHelper<TType>>) this.Types).Where<ListHelper<TType>>((Func<ListHelper<TType>, bool>) (x => createableActions.Contains<string>(x.Value.ToString()))).ToArray<ListHelper<TType>>();
        this.CanChangeType = true;
      }
    }
    return base.PreLoad();
  }
}
