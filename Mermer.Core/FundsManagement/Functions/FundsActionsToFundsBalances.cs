// Decompiled with JetBrains decompiler
// Type: Mermer.Core.FundsManagement.Functions.FundsActionsToFundsBalances
// Assembly: Mermer.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2D3AEFA2-C249-4F1B-A81D-5B4AA93CB026
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.dll

using Mermer.FundsManagement.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#nullable disable
namespace Mermer.Core.FundsManagement.Functions;

public class FundsActionsToFundsBalances
{
    public FundsActionsToFundsBalances()
    {
        //Заглушки
        this.Map = x => null;
        this.Reduce = y => null;
    }

  public Expression<Func<IEnumerable<FundsAction>, IEnumerable>> Map { get; set; }

  public Expression<Func<IEnumerable<FundsBalance>, IEnumerable>> Reduce { get; set; }
}
