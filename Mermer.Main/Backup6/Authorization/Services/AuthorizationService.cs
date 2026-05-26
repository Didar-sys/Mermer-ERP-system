// Decompiled with JetBrains decompiler
// Type: Mermer.Authorization.Services.AuthorizationService
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Authorization.Enums;
using Mermer.Common.Exceptions;
using Mermer.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Mermer.Authorization.Services;

public class AuthorizationService : IAuthorizationService
{
  private readonly ILoginService _loginService;
  private readonly ILocalizationService _localizationService;

  public AuthorizationService(ILoginService loginService, ILocalizationService localizationService)
  {
    this._loginService = loginService;
    this._localizationService = localizationService;
  }

  public void AuthorizeAction(string action, int accessLevel)
  {
    if (!this._loginService.IsLoggedIn)
      throw new AuthorizationFailedException(this._localizationService.GetText("User is not logged in!"));
    if (!this._loginService.Session.IsAdmin && (this._loginService.Session.Roles == null || !this._loginService.Session.Roles.ContainsKey(action) || !this._loginService.Session.Roles[action].HasBit(accessLevel)))
      throw new AuthorizationFailedException(this._localizationService.GetText("Authorization Failed!"));
  }

  public IEnumerable<string> GetAccessableAccounts(AccountAccessLevel level)
  {
    if (!this._loginService.IsLoggedIn)
      throw new AuthorizationFailedException(this._localizationService.GetText("User is not logged in!"));
    if (this._loginService.Session.IsAdmin)
      throw new AuthorizationFailedException(this._localizationService.GetText("Current user is Admin, so all acounts are accessable!"));
    Dictionary<string, AccountAccessLevel> accounts = this._loginService.Session.Accounts;
    return (accounts != null ? accounts.Where<KeyValuePair<string, AccountAccessLevel>>((Func<KeyValuePair<string, AccountAccessLevel>, bool>) (x => x.Value.HasFlag((Enum) level))).Select<KeyValuePair<string, AccountAccessLevel>, string>((Func<KeyValuePair<string, AccountAccessLevel>, string>) (x => x.Key)) : (IEnumerable<string>) null) ?? (IEnumerable<string>) new string[0];
  }

  public IEnumerable<string> FilterAvailableActions(int level, params string[] actions)
  {
    if (!this._loginService.IsLoggedIn)
      throw new AuthorizationFailedException(this._localizationService.GetText("User is not logged in!"));
    if (this._loginService.Session.IsAdmin)
      throw new AuthorizationFailedException(this._localizationService.GetText("Current user is Admin, so all acounts are accessable!"));
    return ((IEnumerable<string>) actions).Where<string>((Func<string, bool>) (x => this._loginService.Session.Roles.ContainsKey(x) && this._loginService.Session.Roles[x].HasBit(level)));
  }

  public void AuthorizeAccountAccess(AccountAccessLevel level, params string[] accountIds)
  {
    string[] accounts = this.GetAccessableAccounts(level).ToArray<string>();
    if (((IEnumerable<string>) accountIds).Any<string>((Func<string, bool>) (accountId => ((IEnumerable<string>) accounts).All<string>((Func<string, bool>) (x => x != accountId)))))
      throw new AuthorizationFailedException(this._localizationService.GetText("Operation is not allowed on selected account(s)"));
  }
}
