// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Reporting.Services.IAggregatedReportsRepository
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Reporting.Models;
using System;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Reporting.Services;

public interface IAggregatedReportsRepository
{
  Task<AggregatedReport> GetAsync(string[] officeIds, DateTime dateFrom, DateTime dateTill);
}
