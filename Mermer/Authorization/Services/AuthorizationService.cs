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

    public AuthorizationService(ILoginService loginService)
    {
        this._loginService = loginService;
    }

    public void AuthorizeAction(string action, int accessLevel)
    {
        if (!this._loginService.IsLoggedIn)
            throw new AuthorizationFailedException("User is not logged in!");

        if (!this._loginService.Session.IsAdmin && (this._loginService.Session.Roles == null || !this._loginService.Session.Roles.ContainsKey(action) || !this._loginService.Session.Roles[action].HasBit(accessLevel)))
            throw new AuthorizationFailedException("Authorization Failed!");
    }

    public IEnumerable<string> GetAccessableAccounts(AccountAccessLevel level)
    {
        if (!this._loginService.IsLoggedIn)
            throw new AuthorizationFailedException("User is not logged in!");

        if (this._loginService.Session.IsAdmin)
            throw new AuthorizationFailedException("Current user is Admin, so all acounts are accessable!");

        Dictionary<string, AccountAccessLevel> accounts = this._loginService.Session.Accounts;
        return (accounts != null ? accounts.Where<KeyValuePair<string, AccountAccessLevel>>((Func<KeyValuePair<string, AccountAccessLevel>, bool>)(x => x.Value.HasFlag((Enum)level))).Select<KeyValuePair<string, AccountAccessLevel>, string>((Func<KeyValuePair<string, AccountAccessLevel>, string>)(x => x.Key)) : (IEnumerable<string>)null) ?? (IEnumerable<string>)new string[0];
    }

    public IEnumerable<string> FilterAvailableActions(int level, params string[] actions)
    {
        if (!this._loginService.IsLoggedIn)
            throw new AuthorizationFailedException("User is not logged in!");

        if (this._loginService.Session.IsAdmin)
            throw new AuthorizationFailedException("Current user is Admin, so all acounts are accessable!");

        return ((IEnumerable<string>)actions).Where<string>((Func<string, bool>)(x => this._loginService.Session.Roles.ContainsKey(x) && this._loginService.Session.Roles[x].HasBit(level)));
    }

    public void AuthorizeAccountAccess(AccountAccessLevel level, params string[] accountIds)
    {
        string[] accounts = this.GetAccessableAccounts(level).ToArray<string>();
        if (((IEnumerable<string>)accountIds).Any<string>((Func<string, bool>)(accountId => ((IEnumerable<string>)accounts).All<string>((Func<string, bool>)(x => x != accountId)))))
            throw new AuthorizationFailedException("Operation is not allowed on selected account(s)");
    }
}