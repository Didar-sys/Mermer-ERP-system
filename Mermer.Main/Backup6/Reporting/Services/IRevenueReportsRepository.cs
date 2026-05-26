// Decompiled with JetBrains decompiler
// Type: Mermer.Reporting.Services.IRevenueReportsRepository
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Reporting.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Reporting.Services;

public interface IRevenueReportsRepository
{
  Task<IEnumerable<RevenueReport>> GetAsync(
    string[] warehouseIds,
    DateTime dateFrom,
    DateTime dateTill);
}
