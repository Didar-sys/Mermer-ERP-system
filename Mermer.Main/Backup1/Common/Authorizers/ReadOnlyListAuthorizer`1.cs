// Decompiled with JetBrains decompiler
// Type: Mermer.Common.Authorizers.ReadOnlyListAuthorizer`1
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Data.Authorizers;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Common.Authorizers;

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
