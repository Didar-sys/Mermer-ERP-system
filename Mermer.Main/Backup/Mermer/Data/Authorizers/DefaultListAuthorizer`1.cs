// Decompiled with JetBrains decompiler
// Type: Mermer.Data.Authorizers.DefaultListAuthorizer`1
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;

#nullable disable
namespace Mermer.Data.Authorizers;

public class DefaultListAuthorizer<T>(bool defaultAction) : 
  DefaultReadOnlyListAuthorizer<T>(defaultAction),
  IListAuthorizer<T>,
  IReadOnlyListAuthorizer<T>,
  IAuthorizer
{
  public bool CanCreate() => this.DefaultAction;

  public void AuthorizeCreate(T item, string errorMessage = null)
  {
    if (!this.DefaultAction)
      throw new Exception(errorMessage ?? "Access Denied!");
  }

  public bool CanUpdate() => this.DefaultAction;

  public void AuthorizeUpdate(T oldItem, T newItem, string errorMessage = null)
  {
    if (!this.DefaultAction)
      throw new Exception(errorMessage ?? "Access Denied!");
  }
}
