// Decompiled with JetBrains decompiler
// Type: Payhas.Data.Tools.Expressions.SafePredicateBuilder`1
// Assembly: Payhas, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.dll

using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Payhas.Data.Tools.Expressions;

public class SafePredicateBuilder<T>
{
  private readonly List<System.Linq.Expressions.Expression<Func<T, bool>>> _list = new List<System.Linq.Expressions.Expression<Func<T, bool>>>();

  public System.Linq.Expressions.Expression<Func<T, bool>>[] Expressions => this._list.ToArray();

  public SafePredicateBuilder()
  {
  }

  public SafePredicateBuilder(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
  {
    this.Add(predicate);
  }

  public SafePredicateBuilder<T> Add(params System.Linq.Expressions.Expression<Func<T, bool>>[] predicates)
  {
    if (predicates != null && ((IEnumerable<System.Linq.Expressions.Expression<Func<T, bool>>>) predicates).Any<System.Linq.Expressions.Expression<Func<T, bool>>>())
    {
      foreach (System.Linq.Expressions.Expression<Func<T, bool>> predicate in predicates)
        this._list.Add(predicate.Safe<T>());
    }
    return this;
  }

  public static System.Linq.Expressions.Expression<Func<T, bool>> Expression(
    System.Linq.Expressions.Expression<Func<T, bool>> predicate)
  {
    return predicate.Safe<T>();
  }
}
