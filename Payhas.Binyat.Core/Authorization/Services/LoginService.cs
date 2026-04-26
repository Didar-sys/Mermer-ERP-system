// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Authorization.Services.LoginService
// Assembly: Payhas.Binyat.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2D3AEFA2-C249-4F1B-A81D-5B4AA93CB026
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.dll

using Payhas.Binyat.Authorization.Enums;
using Payhas.Binyat.Authorization.Models;
using Payhas.Binyat.Authorization.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Core.Authorization.Services;

public abstract class LoginService : ILoginService
{
  public UserSession Session { get; set; }

  public virtual bool IsLoggedIn => this.Session != null;

  public async Task LoginAsync(string username, string password)
  {
    LoginService loginService = this;
    if (string.IsNullOrEmpty(username))
      throw new ArgumentNullException(nameof (username));
    if (string.IsNullOrEmpty(password))
      throw new ArgumentNullException(nameof (password));
    try
    {
      User user = await loginService.GetUser(username, password);
      IEnumerable<string> source1 = !user.IsDisabled ? user.Roles : throw new InvalidOperationException();
      IEnumerable<Role> roles;
      if ((source1 != null ? (!source1.Any<string>() ? 1 : 0) : 1) != 0)
        roles = (IEnumerable<Role>) new Role[0];
      else
        roles = await loginService.GetRoles(user.Roles);
      IEnumerable<Role> source2 = roles;
      // ISSUE: explicit non-virtual call
      __nonvirtual (loginService.Session) = new UserSession()
      {
        UserId = user.Id,
        Username = user.Username,
        IsAdmin = user.IsAdmin,
        Accounts = user.AccountPrivileges ?? new Dictionary<string, AccountAccessLevel>(),
        Roles = source2.SelectMany<Role, KeyValuePair<string, int>>((Func<Role, IEnumerable<KeyValuePair<string, int>>>) (x => (IEnumerable<KeyValuePair<string, int>>) x.Authorizations)).GroupBy<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (x => x.Key)).Select(g => new
        {
          actionId = g.Key,
          accessLevel = g.Select<KeyValuePair<string, int>, int>((Func<KeyValuePair<string, int>, int>) (x => x.Value)).Aggregate<int>((Func<int, int, int>) ((current, next) => current.AddBit(next)))
        }).ToDictionary(x => x.actionId, x => x.accessLevel)
      };
      user = (User) null;
    }
    catch (Exception ex)
    {
      // ISSUE: explicit non-virtual call
      __nonvirtual (loginService.Session) = (UserSession) null;
      throw;
    }
  }

  public Task LogoutAsync() => Task.Run((Action) (() => this.Session = (UserSession) null));

  public abstract Task UpdatePassword(string currentPassword, string newPassword);

  protected abstract Task<User> GetUser(string username, string password);

  protected abstract Task<IEnumerable<Role>> GetRoles(IEnumerable<string> roles);
}
