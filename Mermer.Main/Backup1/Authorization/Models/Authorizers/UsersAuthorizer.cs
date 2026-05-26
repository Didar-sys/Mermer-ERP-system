// Decompiled with JetBrains decompiler
// Type: Mermer.Authorization.Models.Authorizers.UsersAuthorizer
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Common.Authorizers;
using Mermer.Data.Authorizers;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Authorization.Models.Authorizers;

public class UsersAuthorizer(IAuthorizationService authService) : 
  Authorizer(authService, (Enum) Actions.UserManagement),
  IListAuthorizer<User>,
  IReadOnlyListAuthorizer<User>,
  IAuthorizer
{
  public void AuthorizeRead(User item, string errorMessage = null) => this.Authorize(errorMessage);

  public Expression<Func<User, bool>> AuthorizedListFilter() => (Expression<Func<User, bool>>) null;

  public bool CanCreate()
  {
    try
    {
      this.Authorize((string) null);
      return true;
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public void AuthorizeCreate(User item, string errorMessage = null)
  {
    this.Authorize(errorMessage);
  }

  public bool CanUpdate()
  {
    try
    {
      this.Authorize((string) null);
      return true;
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public void AuthorizeUpdate(User oldItem, User newItem, string errorMessage = null)
  {
    this.Authorize(errorMessage);
  }
}
