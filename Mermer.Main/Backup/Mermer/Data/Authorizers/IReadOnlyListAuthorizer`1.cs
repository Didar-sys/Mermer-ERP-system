// Decompiled with JetBrains decompiler
// Type: Mermer.Data.Authorizers.IReadOnlyListAuthorizer`1
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Data.Authorizers;

public interface IReadOnlyListAuthorizer<T> : IAuthorizer
{
  void AuthorizeRead(T item, string errorMessage = null);

  Expression<Func<T, bool>> AuthorizedListFilter();
}
