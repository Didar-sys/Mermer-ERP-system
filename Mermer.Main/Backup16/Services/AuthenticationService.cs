// Decompiled with JetBrains decompiler
// Type: Mermer.Data.Synchronizer.Core.Couch.Services.AuthenticationService
// Assembly: Mermer.Data.Synchronizer.Core.Couch, Version=0.0.9.0, Culture=neutral, PublicKeyToken=null
// MVID: 7309E162-8E25-4800-97C2-B3CD230F4B8B
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Data.Synchronizer.Core.Couch.dll

using Mermer.Data.Synchronizer.Core.Models;
using Mermer.Data.Synchronizer.Core.Services;
using System;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Data.Synchronizer.Core.Couch.Services;

public class AuthenticationService : IAuthenticationService
{
  private AuthenticatedUser _authenticatedUser;

  public virtual Task ValidateUserAsync(string id, bool isAdmin)
  {
    return Task.Run((Action) (() =>
    {
      if (UsersRepositoryService.Users.Single<User>((Func<User, bool>) (x => x.Id == id && !x.IsDisabled && x.IsAdministrator == isAdmin)) == null)
        throw new AuthenticationException();
    }));
  }

  public virtual Task<AuthenticatedUser> GetAuthenticatedUserAsync()
  {
    return Task.FromResult<AuthenticatedUser>(this._authenticatedUser);
  }

  public virtual Task<AuthenticatedUser> AuthorizeUserAsync(AuthenticationRequest request)
  {
    return Task.Run<AuthenticatedUser>((Func<AuthenticatedUser>) (() =>
    {
      if (!UsersRepositoryService.Users.Any<User>())
        UsersRepositoryService.Users.Add(new User()
        {
          Id = request.Id,
          Password = request.Password,
          IsAdministrator = true
        });
      User user = UsersRepositoryService.Users.Single<User>((Func<User, bool>) (x => !x.IsDisabled && x.Id == request.Id && x.Password == request.Password));
      this._authenticatedUser = new AuthenticatedUser()
      {
        Id = user.Id,
        IsAdministrator = user.IsAdministrator
      };
      return this._authenticatedUser;
    }));
  }
}
