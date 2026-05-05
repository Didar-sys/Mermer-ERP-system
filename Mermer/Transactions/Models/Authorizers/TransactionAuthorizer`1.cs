// Decompiled with JetBrains decompiler
// Type: Mermer.Transactions.Models.Authorizers.TransactionAuthorizer`1
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Authorization.Enums;
using Mermer.Authorization.Models;
using Mermer.Authorization.Services;
using Mermer.Common.Authorizers;
using Mermer.Common.Exceptions;
using Mermer.Common.Services;
using Mermer.Data.Authorizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Transactions.Models.Authorizers;

public abstract class TransactionAuthorizer<T> : 
  ListAuthorizer<T>,
  ITransactionAuthorizer<T>,
  IListAuthorizer<T>,
  IReadOnlyListAuthorizer<T>,
  IAuthorizer
  where T : class, ITransactionModel
{
  protected readonly ILoginService LoginService;
  protected readonly ILocalizationService LocalizationService;

  protected TransactionAuthorizer(
    ILoginService loginService,
    ILocalizationService localizationService,
    IAuthorizationService authService,
    Enum action)
    : base(authService, action)
  {
    this.LoginService = loginService;
    this.LocalizationService = localizationService;
  }

  protected virtual void Authorize(
    T item,
    TransactionAccessLevel levelOwn,
    TransactionAccessLevel levelAll,
    string errorMessage = null)
  {
    this.Authorize((Enum) (TransactionAccessLevel) (this.LoginService.Session.UserId == item.UserId ? (int) levelOwn : (int) levelAll), errorMessage);
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

  public override void Authorize(string errorMessage = null)
  {
    this.Authorize((Enum) TransactionAccessLevel.ReadOwn, errorMessage);
  }

  public override void AuthorizeRead(T item, string errorMessage = null)
  {
    this.Authorize(item, TransactionAccessLevel.ReadOwn, TransactionAccessLevel.ReadAll, errorMessage);
    this.AuthorizeAccountAccess(item, AccountAccessLevel.Read, errorMessage);
  }

  public override Expression<Func<T, bool>> AuthorizedListFilter()
  {
    return this.LoginService.Session.IsAdmin ? (Expression<Func<T, bool>>) null : this.GetFilter(this.AuthService.GetAccessableAccounts(AccountAccessLevel.Read));
  }

  protected abstract Expression<Func<T, bool>> GetFilter(IEnumerable<string> accounts);

  public override void AuthorizeCreate(T item, string errorMessage = null)
  {
    if (this.LoginService.Session.IsAdmin)
      return;
    this.Authorize((Enum) TransactionAccessLevel.Create, errorMessage);
    this.AuthorizeAccountAccess(item, AccountAccessLevel.Operate, errorMessage);
    if (DateTime.Now.Subtract(item.Date).Duration() > TimeSpan.FromHours(12.0))
      this.AuthorizeDateChange();
    if (item.IsCompleted)
      this.Authorize(item, TransactionAccessLevel.CompleteOwn, TransactionAccessLevel.CompleteAll, errorMessage ?? this.LocalizationService.GetText("You are not authorized to create completed transaction"));
    if (!item.IsDisabled)
      return;
    this.Authorize(item, TransactionAccessLevel.DeleteOwn, TransactionAccessLevel.DeleteAll, errorMessage ?? this.LocalizationService.GetText("You are not authorized to create deleted transaction"));
  }

  public override void AuthorizeUpdate(T oldItem, T newItem, string errorMessage = null)
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
    return this.AuthService.FilterAvailableActions(Convert.ToInt32((object) accessLevel), typeof (T).Name);
  }

  public virtual bool CanChangeDate()
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
}
