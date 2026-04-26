// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Common.Authorizers.Authorizer
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Authorization.Enums;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Common.Exceptions;
using Payhas.Data.Authorizers;
using System;

#nullable disable
namespace Payhas.Binyat.Common.Authorizers;

public abstract class Authorizer : IAuthorizer
{
  protected readonly IAuthorizationService AuthService;
  protected readonly Enum Action;

  protected Authorizer(IAuthorizationService authService, Enum action)
  {
    this.AuthService = authService;
    this.Action = action;
  }

  public virtual void Authorize(string errorMessage = null)
  {
    this.Authorize((Enum) AccessLevel.Grant, errorMessage);
  }

  public virtual void Authorize(Enum accessLevel, string errorMessage = null)
  {
    try
    {
      this.AuthService.AuthorizeAction(this.Action, accessLevel);
    }
    catch (Exception ex)
    {
      throw new AuthorizationFailedException(errorMessage, ex);
    }
  }
}
