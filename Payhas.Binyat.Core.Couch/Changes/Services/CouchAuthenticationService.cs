// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Couch.Changes.Services.CouchAuthenticationService
// Assembly: Payhas.Binyat.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.Couch.dll

using Payhas.Data.Synchronizer.Core.Couch.Services;
using Payhas.Data.Synchronizer.Core.Models;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Core.Couch.Changes.Services;

public class CouchAuthenticationService : AuthenticationService
{
  public override Task ValidateUserAsync(string id, bool isAdmin) => Task.CompletedTask;

  public override Task<AuthenticatedUser> GetAuthenticatedUserAsync()
  {
    return Task.FromResult<AuthenticatedUser>(new AuthenticatedUser()
    {
      Id = "default",
      IsAdministrator = true
    });
  }

  public override Task<AuthenticatedUser> AuthorizeUserAsync(AuthenticationRequest request)
  {
    return this.GetAuthenticatedUserAsync();
  }
}
