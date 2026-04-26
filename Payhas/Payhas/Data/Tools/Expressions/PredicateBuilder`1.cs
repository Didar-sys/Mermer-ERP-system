// Decompiled with JetBrains decompiler
// Type: Payhas.Data.Tools.Expressions.PredicateBuilder`1
// Assembly: Payhas, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Data.Tools.Expressions;

public class PredicateBuilder<T>
{
  private readonly List<Expression<Func<T, bool>>> _list = new List<Expression<Func<T, bool>>>();

  public Expression<Func<T, bool>>[] Expressions => this._list.ToArray();

  public PredicateBuilder()
  {
  }

  public PredicateBuilder(params Expression<Func<T, bool>>[] predicates)
  {
    foreach (Expression<Func<T, bool>> predicate in predicates)
      this.Add(predicate);
  }

  public virtual PredicateBuilder<T> Add(params Expression<Func<T, bool>>[] predicates)
  {
    if (predicates != null && ((IEnumerable<Expression<Func<T, bool>>>) predicates).Any<Expression<Func<T, bool>>>())
    {
      foreach (Expression<Func<T, bool>> predicate in predicates)
        this._list.Add(predicate);
    }
    return this;
  }
}
