// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Common.Authorizers.ReadOnlyListAuthorizer`1
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Authorization.Enums;
using Payhas.Binyat.Authorization.Services;
using Payhas.Data.Authorizers;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Common.Authorizers;

public abstract class ReadOnlyListAuthorizer<T>(IAuthorizationService authService, Enum action) : 
  Authorizer(authService, action),
  IReadOnlyListAuthorizer<T>,
  IAuthorizer
{
  public virtual Enum GetReadAccessLevel() => (Enum) ListAccessLevel.Read;

  public override void Authorize(string errorMessage = null)
  {
    this.Authorize(this.GetReadAccessLevel(), errorMessage);
  }

  public virtual void AuthorizeRead(T item, string errorMessage = null)
  {
    this.Authorize(this.GetReadAccessLevel(), errorMessage);
  }

  public virtual Expression<Func<T, bool>> AuthorizedListFilter()
  {
    return (Expression<Func<T, bool>>) null;
  }
}
