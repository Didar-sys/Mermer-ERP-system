// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Authorization.Models.Authorizers.RolesAuthorizer
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Authorization.Enums;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Common.Authorizers;
using Payhas.Data.Authorizers;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Authorization.Models.Authorizers;

public class RolesAuthorizer(IAuthorizationService authService) : 
  Authorizer(authService, (Enum) Actions.UserManagement),
  IListAuthorizer<Role>,
  IReadOnlyListAuthorizer<Role>,
  IAuthorizer
{
  public void AuthorizeRead(Role item, string errorMessage = null) => this.Authorize(errorMessage);

  public Expression<Func<Role, bool>> AuthorizedListFilter() => (Expression<Func<Role, bool>>) null;

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

  public void AuthorizeCreate(Role item, string errorMessage = null)
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

  public void AuthorizeUpdate(Role oldItem, Role newItem, string errorMessage = null)
  {
    this.Authorize(errorMessage);
  }
}
