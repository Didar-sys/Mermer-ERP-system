// Decompiled with JetBrains decompiler
// Type: Mermer.Commerce.Services.IInvoicesRepository
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Commerce.Models;
using Mermer.Data.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Commerce.Services;

public interface IInvoicesRepository : IRepository<Invoice>, IReadOnlyRepository<Invoice>
{
  Task<int> CountInfoAsync(DateTime from, DateTime till);

  Task<IEnumerable<InvoiceInfo>> GetInfoAsync(DateTime from, DateTime till);

  Task<int> CountPaymentInfoAsync(DateTime from, DateTime till, string officeId, string partnerId);

  Task<IEnumerable<InvoicePaymentInfo>> GetPaymentInfoAsync(
    DateTime from,
    DateTime till,
    string officeId,
    string partnerId);
}
