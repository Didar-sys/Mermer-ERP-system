// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Commerce.Functions.BillsToFundsActions
// Assembly: Mermer.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2D3AEFA2-C249-4F1B-A81D-5B4AA93CB026
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.dll

using Mermer.Commerce.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#nullable disable
namespace Mermer.Core.Commerce.Functions;

public class BillsToFundsActions
{
  public BillsToFundsActions()
  {
        //Заглушки
        this.Map = x => null;
    }

  public Expression<Func<IEnumerable<Bill>, IEnumerable>> Map { get; set; }
}
