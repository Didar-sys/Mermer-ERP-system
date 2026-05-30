// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Commerce.Services.InvoicesInfoView
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

public class InvoicesInfoView : CouchView
{
  private readonly ILoginService _loginService;
  private readonly IAuthorizationService _authService;

  public InvoicesInfoView(
    ICouchCluster cluster,
    ILoginService loginService,
    IAuthorizationService authService)
    : base(cluster)
  {
    this._loginService = loginService;
    this._authService = authService;
  }

  public async Task<int> CountAsync(DateTime from, DateTime till)
  {
    return (await this.GetRecordsAsync<int>(from, till, true)).Sum();
  }

  public Task<IEnumerable<InvoiceInfo>> GetAsync(DateTime from, DateTime till)
  {
    return this.GetRecordsAsync<InvoiceInfo>(from, till);
  }

  private Task<IEnumerable<T>> GetRecordsAsync<T>(
    DateTime startDate,
    DateTime endDate,
    bool reduce = false)
  {
    List<Tuple<string, string>> source;
    List<string> accountIds;
    if (this._loginService.Session.IsAdmin)
    {
      accountIds = new List<string>() { "all" };
      source = new List<Tuple<string, string>>()
      {
        new Tuple<string, string>("all", "all")
      };
    }
    else
    {
      accountIds = this._authService.GetAccessableAccounts(AccountAccessLevel.Read).ToList<string>();
      Enum[] array = Enum.GetValues(typeof (InvoiceType)).Cast<Enum>().ToArray<Enum>();
      List<string> allActions = this._authService.FilterAvailableActions((Enum) TransactionAccessLevel.ReadAll, array).ToList<string>();
      List<string> list = this._authService.FilterAvailableActions((Enum) TransactionAccessLevel.ReadOwn, array).Where<string>((Func<string, bool>) (x => !allActions.Contains(x))).ToList<string>();
      string userId = this._loginService.Session.UserId;
      source = allActions.Select<string, Tuple<string, string>>((Func<string, Tuple<string, string>>) (x => new Tuple<string, string>(x, "all"))).Union<Tuple<string, string>>(list.Select<string, Tuple<string, string>>((Func<string, Tuple<string, string>>) (x => new Tuple<string, string>(x, userId)))).ToList<Tuple<string, string>>();
    }
    return this.GetRecordsAsync<T>("commerce", "invoice-infos", source.SelectMany<Tuple<string, string>, Tuple<object, object>>((Func<Tuple<string, string>, IEnumerable<Tuple<object, object>>>) (x => accountIds.Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (accountId => new Tuple<object, object>((object) new string[4]
    {
      x.Item1,
      x.Item2,
      accountId ?? "all",
      startDate.ToString("o")
    }, (object) new string[4]
    {
      x.Item1,
      x.Item2,
      accountId ?? "all",
      endDate.ToString("o")
    }))))).ToArray<Tuple<object, object>>(), reduce);
  }
}
