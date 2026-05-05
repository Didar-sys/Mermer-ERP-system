// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Commerce.Services.InvoicesPaymentsView
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Commerce.Models;
using Mermer.Core.Couch.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Commerce.Services;

public class InvoicesPaymentsView : CouchView
{
  private readonly ILoginService _loginService;
  private readonly IAuthorizationService _authService;

  public InvoicesPaymentsView(
    ICouchCluster cluster,
    ILoginService loginService,
    IAuthorizationService authService)
    : base(cluster)
  {
    this._loginService = loginService;
    this._authService = authService;
  }

  public async Task<int> CountAsync(
    DateTime from,
    DateTime till,
    string officeId,
    string partnerId)
  {
    return (await this.GetRecordsAsync<int>(from, till, officeId, partnerId, true)).Sum();
  }

  public Task<IEnumerable<InvoicePaymentInfo>> GetAsync(
    DateTime from,
    DateTime till,
    string officeId,
    string partnerId)
  {
    return this.GetRecordsAsync<InvoicePaymentInfo>(from, till, officeId, partnerId);
  }

  private Task<IEnumerable<T>> GetRecordsAsync<T>(
    DateTime startDate,
    DateTime endDate,
    string officeId,
    string partnerId,
    bool reduce = false)
  {
    string userFilter = (string) null;
    List<string> source = new List<string>();
    if (this._loginService.Session.IsAdmin)
    {
      userFilter = "all";
      source.Add(officeId ?? "all");
    }
    else
    {
      if (this._authService.TryAuthorizeAction((Enum) InvoiceType.Sales, (Enum) TransactionAccessLevel.ReadAll))
        userFilter = "all";
      else if (this._authService.TryAuthorizeAction((Enum) InvoiceType.Sales, (Enum) TransactionAccessLevel.ReadOwn))
        userFilter = this._loginService.Session.UserId;
      List<string> list = this._authService.GetAccessableAccounts(AccountAccessLevel.Read).ToList<string>();
      if (string.IsNullOrEmpty(officeId))
        source = list;
      else if (list.Contains(officeId))
        source.Add(officeId);
    }
    return string.IsNullOrEmpty(userFilter) || !source.Any<string>() ? Task.FromResult<IEnumerable<T>>((IEnumerable<T>) Array.Empty<T>()) : this.GetRecordsAsync<T>("commerce-reporting", "invoice-payments", source.Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (accountId => new Tuple<object, object>((object) new string[4]
    {
      userFilter,
      accountId,
      partnerId ?? "all",
      startDate.ToString("o")
    }, (object) new string[4]
    {
      userFilter,
      accountId,
      partnerId ?? "all",
      endDate.ToString("o")
    }))).ToArray<Tuple<object, object>>(), reduce);
  }
}
