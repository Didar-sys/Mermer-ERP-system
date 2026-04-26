// Decompiled with JetBrains decompiler
// Type: Payhas.Data.Synchronizer.Core.Couch.Services.UsersRepositoryService
// Assembly: Payhas.Data.Synchronizer.Core.Couch, Version=0.0.9.0, Culture=neutral, PublicKeyToken=null
// MVID: 7309E162-8E25-4800-97C2-B3CD230F4B8B
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Data.Synchronizer.Core.Couch.dll

using Payhas.Data.Synchronizer.Core.Models;
using Payhas.Data.Synchronizer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Data.Synchronizer.Core.Couch.Services;

public class UsersRepositoryService : IUsersRepositoryService
{
  private readonly IAuthenticationService _authService;
  internal static readonly List<User> Users = new List<User>();

  public UsersRepositoryService(IAuthenticationService authService)
  {
    this._authService = authService;
  }

  private async Task ValidateAdministratorAsync()
  {
    if (!(await this._authService.GetAuthenticatedUserAsync()).IsAdministrator)
      throw new UnauthorizedAccessException();
  }

  public async Task<User> GetAsync(string id)
  {
    await this.ValidateAdministratorAsync();
    return UsersRepositoryService.Users.Single<User>((Func<User, bool>) (x => x.Id == id));
  }

  public async Task<IEnumerable<User>> GetAsync()
  {
    await this.ValidateAdministratorAsync();
    return (IEnumerable<User>) UsersRepositoryService.Users;
  }

  public async Task CreateAsync(User user)
  {
    await this.ValidateAdministratorAsync();
    UsersRepositoryService.Users.Add(user);
  }

  public async Task UpdateAsync(User user)
  {
    await this.ValidateAdministratorAsync();
    UsersRepositoryService.Users.Remove(UsersRepositoryService.Users.Single<User>((Func<User, bool>) (x => x.Id == user.Id)) ?? throw new KeyNotFoundException());
    UsersRepositoryService.Users.Add(user);
  }
}
