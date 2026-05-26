// Decompiled with JetBrains decompiler
// Type: Mermer.Core.CRM.Functions.PartnerSlipsToPartnerActions
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

public class PartnerSlipsToPartnerActions
{
    public PartnerSlipsToPartnerActions()
    {
        //Заглушки
        this.Map = x => null;
    }

  public Expression<Func<IEnumerable<PartnerSlip>, IEnumerable>> Map { get; set; }
}
