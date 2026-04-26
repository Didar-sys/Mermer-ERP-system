// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Transactions.Models.Authorizers.TransactionActionsAuthorizer`2
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Authorization.Enums;
using Payhas.Binyat.Authorization.Models;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Common.Exceptions;
using Payhas.Binyat.Common.Services;
using Payhas.Data.Authorizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Transactions.Models.Authorizers;

public abstract class TransactionActionsAuthorizer<T, TType> : 
  ITransactionAuthorizer<T>,
  IListAuthorizer<T>,
  IReadOnlyListAuthorizer<T>,
  IAuthorizer
  where T : class, ITransactionModel
{
  protected readonly ILoginService LoginService;
  protected readonly ILocalizationService LocalizationService;
  protected readonly IAuthorizationService AuthService;

  protected TransactionActionsAuthorizer(
    ILoginService loginService,
    ILocalizationService localizationService,
    IAuthorizationService authService)
  {
    this.LoginService = loginService;
    this.LocalizationService = localizationService;
    this.AuthService = authService;
  }

  protected virtual void Authorize(
    TType action,
    TransactionAccessLevel accessLevel,
    string errorMessage = null)
  {
    try
    {
      this.AuthService.AuthorizeAction(action.ToString(), Convert.ToInt32((object) accessLevel));
    }
    catch (Exception ex)
    {
      throw new AuthorizationFailedException(errorMessage, ex);
    }
  }

  protected virtual void AuthorizeAny(
    IEnumerable<TType> actions,
    TransactionAccessLevel level,
    string errorMessage = null)
  {
    if (!(actions is TType[] typeArray))
      typeArray = actions.ToArray<TType>();
    foreach (TType type in typeArray)
    {
      try
      {
        this.AuthService.AuthorizeAction(type.ToString(), Convert.ToInt32((object) level));
        return;
      }
      catch (Exception ex)
      {
      }
    }
    throw new AuthorizationFailedException(errorMessage);
  }

  protected virtual void Authorize(
    T item,
    TransactionAccessLevel levelOwn,
    TransactionAccessLevel levelAll,
    string errorMessage = null)
  {
    this.Authorize(this.GetActionFromType(item), this.LoginService.Session.UserId == item.UserId ? levelOwn : levelAll, errorMessage);
  }

  protected virtual TType GetActionFromType(T item)
  {
    return (TType) Enum.Parse(typeof (TType), item.Type);
  }

  protected virtual IEnumerable<TType> GetAllAction()
  {
    return Enum.GetValues(typeof (TType)).Cast<TType>();
  }

  protected virtual void AuthorizeAccountAccess(
    T item,
    AccountAccessLevel level,
    string errorMessage = null)
  {
    try
    {
      if (this.LoginService.Session.IsAdmin)
        return;
      string[] accessedAccounts = this.GetAccessedAccounts(item);
      if (accessedAccounts == null || !((IEnumerable<string>) accessedAccounts).Any<string>())
        return;
      this.AuthService.AuthorizeAccountAccess(level, accessedAccounts);
    }
    catch (Exception ex)
    {
      throw new AuthorizationFailedException(errorMessage, ex);
    }
  }

  protected abstract string[] GetAccessedAccounts(T item);

  protected virtual void AuthorizeDateChange()
  {
    try
    {
      this.AuthService.AuthorizeAction((Enum) Actions.CanEditTransactionDate, (Enum) AccessLevel.Grant);
    }
    catch (Exception ex)
    {
      throw new AuthorizationFailedException(this.LocalizationService.GetText("You are not authorized to change transaction date"), ex);
    }
  }

  public UserSession GetCurrentSession() => this.LoginService.Session;

  public IEnumerable<string> GetAvailableAccounts(AccountAccessLevel accessLevel)
  {
    return this.AuthService.GetAccessableAccounts(accessLevel);
  }

  public IEnumerable<string> GetAvailableActions(TransactionAccessLevel accessLevel)
  {
    IEnumerable<Enum> source = this.GetAllAction().Cast<Enum>();
    if (!(source is Enum[] enumArray1))
      enumArray1 = source.ToArray<Enum>();
    Enum[] enumArray2 = enumArray1;
    return this.AuthService.FilterAvailableActions((Enum) accessLevel, enumArray2);
  }

  public void Authorize(string errorMessage = null)
  {
    this.Authorize((Enum) TransactionAccessLevel.ReadOwn, errorMessage);
  }

  public void Authorize(Enum accessLevel, string errorMessage = null)
  {
    this.AuthorizeAny(this.GetAllAction(), (TransactionAccessLevel) accessLevel);
  }

  public void AuthorizeRead(T item, string errorMessage = null)
  {
    this.Authorize(item, TransactionAccessLevel.ReadOwn, TransactionAccessLevel.ReadAll, errorMessage);
    this.AuthorizeAccountAccess(item, AccountAccessLevel.Read, errorMessage);
  }

  public Expression<Func<T, bool>> AuthorizedListFilter()
  {
    throw new NotImplementedException("Control on repository level!");
  }

  public bool CanCreate()
  {
    try
    {
      this.Authorize((Enum) TransactionAccessLevel.Create, (string) null);
      return true;
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public void AuthorizeCreate(T item, string errorMessage = null)
  {
    if (this.LoginService.Session.IsAdmin)
      return;
    this.Authorize(this.GetActionFromType(item), TransactionAccessLevel.Create, errorMessage);
    this.AuthorizeAccountAccess(item, AccountAccessLevel.Operate, errorMessage);
    if (DateTime.Now.Subtract(item.Date).Duration() > TimeSpan.FromHours(12.0))
      this.AuthorizeDateChange();
    if (item.IsCompleted)
      this.Authorize(item, TransactionAccessLevel.CompleteOwn, TransactionAccessLevel.CompleteAll, errorMessage ?? this.LocalizationService.GetText("You are not authorized to create completed transaction"));
    if (!item.IsDisabled)
      return;
    this.Authorize(item, TransactionAccessLevel.DeleteOwn, TransactionAccessLevel.DeleteAll, errorMessage ?? this.LocalizationService.GetText("You are not authorized to create deleted transaction"));
  }

  public bool CanUpdate()
  {
    try
    {
      this.Authorize((Enum) TransactionAccessLevel.UpdateAll, (string) null);
      return true;
    }
    catch (Exception ex)
    {
    }
    try
    {
      this.Authorize((Enum) TransactionAccessLevel.UpdateOwn, (string) null);
      return true;
    }
    catch (Exception ex)
    {
    }
    return false;
  }

  public bool CanChangeDate()
  {
    try
    {
      this.AuthorizeDateChange();
      return true;
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public void AuthorizeUpdate(T oldItem, T newItem, string errorMessage = null)
  {
    if (this.LoginService.Session.IsAdmin)
      return;
    this.Authorize(newItem, TransactionAccessLevel.UpdateOwn, TransactionAccessLevel.UpdateAll, errorMessage);
    this.AuthorizeAccountAccess(newItem, AccountAccessLevel.Operate, errorMessage);
    if (oldItem.Date != newItem.Date)
      this.AuthorizeDateChange();
    if (newItem.IsCompleted || oldItem.IsCompleted != newItem.IsCompleted)
      this.Authorize(newItem, TransactionAccessLevel.CompleteOwn, TransactionAccessLevel.CompleteAll, errorMessage ?? this.LocalizationService.GetText("You are not authorized to (un)complete this transaction, or modify completed transaction"));
    if (!newItem.IsDisabled && oldItem.IsDisabled == newItem.IsDisabled)
      return;
    this.Authorize(newItem, TransactionAccessLevel.DeleteOwn, TransactionAccessLevel.DeleteAll, errorMessage ?? this.LocalizationService.GetText("You are not authorized to (un)delete this transaction, or modify deleted transaction"));
  }
}
