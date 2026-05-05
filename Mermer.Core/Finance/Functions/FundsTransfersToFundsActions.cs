// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Finance.Functions.FundsTransfersToFundsActions
// Assembly: Mermer.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2D3AEFA2-C249-4F1B-A81D-5B4AA93CB026
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.dll

using Mermer.Finance.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#nullable disable
namespace Mermer.Core.Finance.Functions;

public class FundsTransfersToFundsActions
{
  protected const string SourceTypeName = "FundsTransferSource";
  protected const string DestinationTypeName = "FundsTransferDestination";

  public FundsTransfersToFundsActions()
  {
    ParameterExpression parameterExpression1;
    ParameterExpression parameterExpression2;
    // ISSUE: method reference
    // ISSUE: method reference
    // ISSUE: method reference
    // ISSUE: method reference
    // ISSUE: method reference
    // ISSUE: method reference
    // ISSUE: method reference
    // ISSUE: method reference
    // ISSUE: method reference
    // ISSUE: method reference
    // ISSUE: method reference
    // ISSUE: method reference
    // ISSUE: method reference
    // ISSUE: method reference
    // ISSUE: method reference
    // ISSUE: method reference
    this.Map = Expression.Lambda<Func<IEnumerable<FundsTransfer>, IEnumerable>>((Expression) Expression.Call((Expression) null, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (Enumerable.Select)), )); // Unable to render the statement
  }

  public Expression<Func<IEnumerable<FundsTransfer>, IEnumerable>> Map { get; set; }
}
