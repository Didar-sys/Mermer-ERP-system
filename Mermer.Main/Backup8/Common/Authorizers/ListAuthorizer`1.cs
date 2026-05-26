// Decompiled with JetBrains decompiler
// Type: Mermer.Common.Authorizers.ListAuthorizer`1
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Data.Authorizers;
using System;

#nullable disable
namespace Mermer.Common.Authorizers;

public abstract class ListAuthorizer<T>(IAuthorizationService authService, Enum action) : 
  ReadOnlyListAuthorizer<T>(authService, action),
  IListAuthorizer<T>,
  IReadOnlyListAuthorizer<T>,
  IAuthorizer
{
  public bool CanCreate()
  {
    try
    {
      this.Authorize((Enum) ListAccessLevel.Create, (string) null);
      return true;
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public virtual void AuthorizeCreate(T item, string errorMessage = null)
  {
    this.Authorize((Enum) ListAccessLevel.Create, errorMessage);
  }

  public bool CanUpdate()
  {
    try
    {
      this.Authorize((Enum) ListAccessLevel.Update, (string) null);
      return true;
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public virtual void AuthorizeUpdate(T oldItem, T newItem, string errorMessage = null)
  {
    this.Authorize((Enum) ListAccessLevel.Update, errorMessage);
  }
}
