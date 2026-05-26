// Decompiled with JetBrains decompiler
// Type: Mermer.Core.CRM.Functions.PartnerActionsToPartnerBalances
// Assembly: Mermer.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2D3AEFA2-C249-4F1B-A81D-5B4AA93CB026
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.dll

using Mermer.CRM.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#nullable disable
namespace Mermer.Core.CRM.Functions;

public class PartnerActionsToPartnerBalances
{
  public PartnerActionsToPartnerBalances()
  {
    ParameterExpression parameterExpression1;
    ParameterExpression parameterExpression2;
    // ISSUE: method reference
    // ISSUE: method reference
    // ISSUE: method reference
    // ISSUE: method reference
    // ISSUE: method reference
    this.Map = Expression.Lambda<Func<IEnumerable<PartnerAction>, IEnumerable>>((Expression) Expression.Call((Expression) null, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (Enumerable.Select)), )); // Unable to render the statement
    ParameterExpression parameterExpression3;
    ParameterExpression parameterExpression4;
    // ISSUE: method reference
    // ISSUE: method reference
    // ISSUE: method reference
    // ISSUE: method reference
    // ISSUE: method reference
    this.Reduce = Expression.Lambda<Func<IEnumerable<PartnerBalance>, IEnumerable>>((Expression) Expression.Call((Expression) null, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (Enumerable.Select)), )); // Unable to render the statement
  }

  public Expression<Func<IEnumerable<PartnerAction>, IEnumerable>> Map { get; set; }

  public Expression<Func<IEnumerable<PartnerBalance>, IEnumerable>> Reduce { get; set; }
}
