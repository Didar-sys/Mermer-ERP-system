// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.CRM.Services.IPartnerActionsRepository
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.CRM.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.CRM.Services;

public interface IPartnerActionsRepository
{
  Task<int> CountAsync(
    DateTime? startDate,
    DateTime? endDate,
    string partnerId,
    params string[] officeIds);

  Task<IEnumerable<PartnerAction>> GetAsync(
    DateTime? startDate,
    DateTime? endDate,
    string partnerId,
    params string[] officeIds);

  Task<Dictionary<string, PartnerActionInfo[]>> GetByPartnersAsync(
    string officeId,
    params string[] partners);
}
