// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.CRM.Services.PartnerBalancesRepository
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase.Views;
using Microsoft.CSharp.RuntimeBinder;
using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Core.Couch.Common;
using Mermer.CRM.Models;
using Mermer.CRM.Services;
using Mermer.Data.Authorizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.CRM.Services;

public class PartnerBalancesRepository : CouchView, IPartnerBalancesRepository
{
  private readonly ILoginService _loginService;
  private readonly IAuthorizationService _authorizationService;
  private readonly IReadOnlyListAuthorizer<PartnerBalance> _authorizer;

  public PartnerBalancesRepository(
    ICouchCluster cluster,
    ILoginService loginService,
    IAuthorizationService authorizationService,
    IReadOnlyListAuthorizer<PartnerBalance> authorizer)
    : base(cluster)
  {
    this._loginService = loginService;
    this._authorizationService = authorizationService;
    this._authorizer = authorizer;
  }

  public async Task<PartnerBalanceResult> GetBalanceToDateAsync(
    string officeId,
    string partnerId,
    DateTime date,
    string excludeTransactionId = null)
  {
    PartnerBalancesRepository balancesRepository = this;
    if (string.IsNullOrEmpty(officeId))
      throw new ArgumentNullException(nameof (officeId));
    if (string.IsNullOrEmpty(partnerId))
      throw new ArgumentException(nameof (partnerId));
    if (!balancesRepository._loginService.Session.IsAdmin && !balancesRepository._authorizationService.GetAccessableAccounts(AccountAccessLevel.Read).ToList<string>().Contains(officeId))
      return new PartnerBalanceResult();
    Tuple<object, object>[] startEndKeys = new Tuple<object, object>[1]
    {
      new Tuple<object, object>((object) new string[5]
      {
        "all",
        "all",
        officeId,
        partnerId,
        "0"
      }, (object) new string[5]
      {
        "all",
        "all",
        officeId,
        partnerId,
        date.ToString("o")
      })
    };
    List<PartnerAction> list = (await balancesRepository.GetRecordsAsync<PartnerAction>("crm", "partner-actions", startEndKeys)).ToList<PartnerAction>();
    return new PartnerBalanceResult()
    {
      Balance = list.Where<PartnerAction>((Func<PartnerAction, bool>) (x => x.TransactionId != excludeTransactionId && x.TransactionDate < date && x.TransactionIsCompleted && !x.TransactionIsDisabled)).Sum<PartnerAction>((Func<PartnerAction, Decimal>) (x => x.ActionEffect))
    };
  }

  public async Task<IEnumerable<PartnerBalanceByTypeWithBalance>> GetByTypeAsync(
    DateTime dateFrom,
    DateTime dateTill,
    string partnerId,
    params string[] officeIds)
  {
    PartnerBalancesRepository balancesRepository = this;
    balancesRepository._authorizer.Authorize();
    if (dateFrom >= dateTill)
      throw new ArgumentException("From date should be lower than or equal to till date");
    string[] source = officeIds;
    officeIds = source != null ? ((IEnumerable<string>) source).Where<string>((Func<string, bool>) (x => !string.IsNullOrEmpty(x))).ToArray<string>() : (string[]) null;
    if (officeIds == null || !((IEnumerable<string>) officeIds).Any<string>())
      throw new ArgumentException("Offices should not be empty");
    if (!balancesRepository._loginService.Session.IsAdmin)
    {
      string[] accounts = balancesRepository._authorizationService.GetAccessableAccounts(AccountAccessLevel.Read).ToArray<string>();
      officeIds = !((IEnumerable<string>) officeIds).Any<string>(new Func<string, bool>(string.IsNullOrEmpty)) ? ((IEnumerable<string>) officeIds).Where<string>((Func<string, bool>) (x => ((IEnumerable<string>) accounts).Contains<string>(x))).ToArray<string>() : accounts;
      if (!((IEnumerable<string>) officeIds).Any<string>())
        return (IEnumerable<PartnerBalanceByTypeWithBalance>) new PartnerBalanceByTypeWithBalance[0];
    }
    List<PartnerBalance> startingBalances;
    List<PartnerBalanceByType> changingBalances;
    if (!string.IsNullOrEmpty(partnerId))
    {
      Tuple<object, object>[] array1 = ((IEnumerable<string>) officeIds).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (officeId => new Tuple<object, object>((object) new string[3]
      {
        officeId,
        partnerId,
        "0"
      }, (object) new string[3]
      {
        officeId,
        partnerId,
        dateFrom.ToString("o")
      }))).ToArray<Tuple<object, object>>();
      startingBalances = (await balancesRepository.GetRecordsAsync<PartnerBalance>("crm", "partner-balances-by-office-and-id", array1, true, 2, (Func<ViewRow<PartnerBalance>, PartnerBalance>) (x =>
      {
        PartnerBalance byTypeAsync = x.Value;
        PartnerBalance partnerBalance1 = byTypeAsync;
        // ISSUE: reference to a compiler-generated field
        if (PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__1 == null)
        {
          // ISSUE: reference to a compiler-generated field
          PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__1 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (PartnerBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target1 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__1.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p1 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__1;
        // ISSUE: reference to a compiler-generated field
        if (PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__0 == null)
        {
          // ISSUE: reference to a compiler-generated field
          PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (PartnerBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj1 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__0.Target((CallSite) PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__0, x.Key, 0);
        string str1 = target1((CallSite) p1, obj1);
        partnerBalance1.OfficeId = str1;
        PartnerBalance partnerBalance2 = byTypeAsync;
        // ISSUE: reference to a compiler-generated field
        if (PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__3 == null)
        {
          // ISSUE: reference to a compiler-generated field
          PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__3 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (PartnerBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target2 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__3.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p3 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__3;
        // ISSUE: reference to a compiler-generated field
        if (PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__2 == null)
        {
          // ISSUE: reference to a compiler-generated field
          PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__2 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (PartnerBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj2 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__2.Target((CallSite) PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__2, x.Key, 1);
        string str2 = target2((CallSite) p3, obj2);
        partnerBalance2.PartnerId = str2;
        return byTypeAsync;
      }))).ToList<PartnerBalance>();
      Tuple<object, object>[] array2 = ((IEnumerable<string>) officeIds).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (officeId => new Tuple<object, object>((object) new string[3]
      {
        officeId,
        partnerId,
        dateFrom.ToString("o")
      }, (object) new string[3]
      {
        officeId,
        partnerId,
        dateTill.ToString("o")
      }))).ToArray<Tuple<object, object>>();
      changingBalances = (await balancesRepository.GetRecordsAsync<PartnerBalanceByType>("crm", "partner-balances-by-office-and-id", array2, true, 2, (Func<ViewRow<PartnerBalanceByType>, PartnerBalanceByType>) (x =>
      {
        PartnerBalanceByType byTypeAsync = x.Value;
        PartnerBalanceByType partnerBalanceByType1 = byTypeAsync;
        // ISSUE: reference to a compiler-generated field
        if (PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__5 == null)
        {
          // ISSUE: reference to a compiler-generated field
          PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__5 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (PartnerBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target3 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__5.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p5 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__5;
        // ISSUE: reference to a compiler-generated field
        if (PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__4 == null)
        {
          // ISSUE: reference to a compiler-generated field
          PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__4 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (PartnerBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj3 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__4.Target((CallSite) PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__4, x.Key, 0);
        string str3 = target3((CallSite) p5, obj3);
        partnerBalanceByType1.OfficeId = str3;
        PartnerBalanceByType partnerBalanceByType2 = byTypeAsync;
        // ISSUE: reference to a compiler-generated field
        if (PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__7 == null)
        {
          // ISSUE: reference to a compiler-generated field
          PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__7 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (PartnerBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target4 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__7.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p7 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__7;
        // ISSUE: reference to a compiler-generated field
        if (PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__6 == null)
        {
          // ISSUE: reference to a compiler-generated field
          PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__6 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (PartnerBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj4 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__6.Target((CallSite) PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__6, x.Key, 1);
        string str4 = target4((CallSite) p7, obj4);
        partnerBalanceByType2.PartnerId = str4;
        return byTypeAsync;
      }))).ToList<PartnerBalanceByType>();
    }
    else
    {
      Tuple<object, object>[] array3 = ((IEnumerable<string>) officeIds).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (officeId => new Tuple<object, object>((object) new string[2]
      {
        officeId,
        "0"
      }, (object) new string[2]
      {
        officeId,
        dateFrom.ToString("o")
      }))).ToArray<Tuple<object, object>>();
      startingBalances = (await balancesRepository.GetRecordsAsync<PartnerBalance>("crm", "partner-balances-by-office", array3, true, 3, (Func<ViewRow<PartnerBalance>, PartnerBalance>) (x =>
      {
        PartnerBalance byTypeAsync = x.Value;
        PartnerBalance partnerBalance3 = byTypeAsync;
        // ISSUE: reference to a compiler-generated field
        if (PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__9 == null)
        {
          // ISSUE: reference to a compiler-generated field
          PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__9 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (PartnerBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target5 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__9.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p9 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__9;
        // ISSUE: reference to a compiler-generated field
        if (PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__8 == null)
        {
          // ISSUE: reference to a compiler-generated field
          PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__8 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (PartnerBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj5 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__8.Target((CallSite) PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__8, x.Key, 0);
        string str5 = target5((CallSite) p9, obj5);
        partnerBalance3.OfficeId = str5;
        PartnerBalance partnerBalance4 = byTypeAsync;
        // ISSUE: reference to a compiler-generated field
        if (PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__11 == null)
        {
          // ISSUE: reference to a compiler-generated field
          PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__11 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (PartnerBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target6 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__11.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p11 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__11;
        // ISSUE: reference to a compiler-generated field
        if (PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__10 == null)
        {
          // ISSUE: reference to a compiler-generated field
          PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__10 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (PartnerBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj6 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__10.Target((CallSite) PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__10, x.Key, 2);
        string str6 = target6((CallSite) p11, obj6);
        partnerBalance4.PartnerId = str6;
        return byTypeAsync;
      }))).GroupBy(x => new
      {
        OfficeId = x.OfficeId,
        PartnerId = x.PartnerId
      }).Select<IGrouping<\u003C\u003Ef__AnonymousType1<string, string>, PartnerBalance>, PartnerBalance>(g => new PartnerBalance()
      {
        OfficeId = g.Key.OfficeId,
        PartnerId = g.Key.PartnerId,
        Debit = g.Sum<PartnerBalance>((Func<PartnerBalance, Decimal>) (x => x.Debit)),
        Credit = g.Sum<PartnerBalance>((Func<PartnerBalance, Decimal>) (x => x.Credit))
      }).ToList<PartnerBalance>();
      Tuple<object, object>[] array4 = ((IEnumerable<string>) officeIds).Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (officeId => new Tuple<object, object>((object) new string[2]
      {
        officeId,
        dateFrom.ToString("o")
      }, (object) new string[2]
      {
        officeId,
        dateTill.ToString("o")
      }))).ToArray<Tuple<object, object>>();
      changingBalances = (await balancesRepository.GetRecordsAsync<PartnerBalanceByType>("crm", "partner-balances-by-office", array4, true, 3, (Func<ViewRow<PartnerBalanceByType>, PartnerBalanceByType>) (x =>
      {
        PartnerBalanceByType byTypeAsync = x.Value;
        PartnerBalanceByType partnerBalanceByType3 = byTypeAsync;
        // ISSUE: reference to a compiler-generated field
        if (PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__13 == null)
        {
          // ISSUE: reference to a compiler-generated field
          PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__13 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (PartnerBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target7 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__13.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p13 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__13;
        // ISSUE: reference to a compiler-generated field
        if (PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__12 == null)
        {
          // ISSUE: reference to a compiler-generated field
          PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__12 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (PartnerBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj7 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__12.Target((CallSite) PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__12, x.Key, 0);
        string str7 = target7((CallSite) p13, obj7);
        partnerBalanceByType3.OfficeId = str7;
        PartnerBalanceByType partnerBalanceByType4 = byTypeAsync;
        // ISSUE: reference to a compiler-generated field
        if (PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__15 == null)
        {
          // ISSUE: reference to a compiler-generated field
          PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__15 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (PartnerBalancesRepository)));
        }
        // ISSUE: reference to a compiler-generated field
        Func<CallSite, object, string> target8 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__15.Target;
        // ISSUE: reference to a compiler-generated field
        CallSite<Func<CallSite, object, string>> p15 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__15;
        // ISSUE: reference to a compiler-generated field
        if (PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__14 == null)
        {
          // ISSUE: reference to a compiler-generated field
          PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__14 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof (PartnerBalancesRepository), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj8 = PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__14.Target((CallSite) PartnerBalancesRepository.\u003C\u003Eo__5.\u003C\u003Ep__14, x.Key, 2);
        string str8 = target8((CallSite) p15, obj8);
        partnerBalanceByType4.PartnerId = str8;
        return byTypeAsync;
      }))).GroupBy(x => new
      {
        OfficeId = x.OfficeId,
        PartnerId = x.PartnerId
      }).Select<IGrouping<\u003C\u003Ef__AnonymousType1<string, string>, PartnerBalanceByType>, PartnerBalanceByType>(g =>
      {
        return new PartnerBalanceByType()
        {
          OfficeId = g.Key.OfficeId,
          PartnerId = g.Key.PartnerId,
          Debit = g.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (x => x.Debit)),
          Credit = g.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (x => x.Credit)),
          PartnerOpeningBalance = g.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (x => x.PartnerOpeningBalance)),
          PartnerBalanceRevision = g.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (x => x.PartnerBalanceRevision)),
          PartnerTransfer = g.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (x => x.PartnerTransfer)),
          Sales = g.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (x => x.Sales)),
          SalesReturn = g.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (x => x.SalesReturn)),
          Purchase = g.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (x => x.Purchase)),
          PurchaseReturn = g.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (x => x.PurchaseReturn)),
          Payment = g.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (x => x.Payment)),
          Collection = g.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (x => x.Collection))
        };
      }).ToList<PartnerBalanceByType>();
    }
    return startingBalances.Select(x => new
    {
      OfficeId = x.OfficeId,
      PartnerId = x.PartnerId
    }).Union(changingBalances.Select(x => new
    {
      OfficeId = x.OfficeId,
      PartnerId = x.PartnerId
    })).Distinct().Select(x => new
    {
      item = x,
      startingBalances = startingBalances.Where<PartnerBalance>((Func<PartnerBalance, bool>) (z => z.OfficeId == x.OfficeId && z.PartnerId == x.PartnerId)),
      changingBalances = changingBalances.Where<PartnerBalanceByType>((Func<PartnerBalanceByType, bool>) (z => z.OfficeId == x.OfficeId && z.PartnerId == x.PartnerId))
    }).Select(x =>
    {
      return new PartnerBalanceByTypeWithBalance()
      {
        OfficeId = x.item.OfficeId,
        PartnerId = x.item.PartnerId,
        StartingBalance = x.startingBalances.Sum<PartnerBalance>((Func<PartnerBalance, Decimal>) (z => z.Balance)),
        Debit = x.changingBalances.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (z => z.Debit)),
        Credit = x.changingBalances.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (z => z.Credit)),
        PartnerOpeningBalance = x.changingBalances.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (z => z.PartnerOpeningBalance)),
        PartnerBalanceRevision = x.changingBalances.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (z => z.PartnerBalanceRevision)),
        PartnerTransfer = x.changingBalances.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (z => z.PartnerTransfer)),
        Sales = x.changingBalances.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (z => z.Sales)),
        SalesReturn = x.changingBalances.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (z => z.SalesReturn)),
        Purchase = x.changingBalances.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (z => z.Purchase)),
        PurchaseReturn = x.changingBalances.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (z => z.PurchaseReturn)),
        Payment = x.changingBalances.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (z => z.Payment)),
        Collection = x.changingBalances.Sum<PartnerBalanceByType>((Func<PartnerBalanceByType, Decimal>) (z => z.Collection))
      };
    });
  }

  public async Task<PartnerBalanceAggregated> GetByTypeAggregatedAsync(
    string[] officeIds,
    DateTime dateFrom,
    DateTime dateTill)
  {
    List<PartnerBalanceByTypeWithBalance> list = (await this.GetByTypeAsync(dateFrom, dateTill, (string) null, officeIds)).ToList<PartnerBalanceByTypeWithBalance>();
    return new PartnerBalanceAggregated()
    {
      Debit = list.Sum<PartnerBalanceByTypeWithBalance>((Func<PartnerBalanceByTypeWithBalance, Decimal>) (x => x.Debit)),
      Credit = list.Sum<PartnerBalanceByTypeWithBalance>((Func<PartnerBalanceByTypeWithBalance, Decimal>) (x => x.Credit)),
      StartingBalance = list.Sum<PartnerBalanceByTypeWithBalance>((Func<PartnerBalanceByTypeWithBalance, Decimal>) (x => x.StartingBalance)),
      Lines = (IEnumerable<PartnerBalanceAggregatedLine>) new PartnerBalanceAggregatedLine[9]
      {
        new PartnerBalanceAggregatedLine("PartnerOpeningBalance", list.Sum<PartnerBalanceByTypeWithBalance>((Func<PartnerBalanceByTypeWithBalance, Decimal>) (z => z.PartnerOpeningBalance))),
        new PartnerBalanceAggregatedLine("PartnerBalanceRevision", list.Sum<PartnerBalanceByTypeWithBalance>((Func<PartnerBalanceByTypeWithBalance, Decimal>) (z => z.PartnerBalanceRevision))),
        new PartnerBalanceAggregatedLine("PartnerTransfer", list.Sum<PartnerBalanceByTypeWithBalance>((Func<PartnerBalanceByTypeWithBalance, Decimal>) (z => z.PartnerTransfer))),
        new PartnerBalanceAggregatedLine("Sales", list.Sum<PartnerBalanceByTypeWithBalance>((Func<PartnerBalanceByTypeWithBalance, Decimal>) (z => z.Sales))),
        new PartnerBalanceAggregatedLine("SalesReturn", list.Sum<PartnerBalanceByTypeWithBalance>((Func<PartnerBalanceByTypeWithBalance, Decimal>) (z => z.SalesReturn))),
        new PartnerBalanceAggregatedLine("Purchase", list.Sum<PartnerBalanceByTypeWithBalance>((Func<PartnerBalanceByTypeWithBalance, Decimal>) (z => z.Purchase))),
        new PartnerBalanceAggregatedLine("PurchaseReturn", list.Sum<PartnerBalanceByTypeWithBalance>((Func<PartnerBalanceByTypeWithBalance, Decimal>) (z => z.PurchaseReturn))),
        new PartnerBalanceAggregatedLine("Payment", list.Sum<PartnerBalanceByTypeWithBalance>((Func<PartnerBalanceByTypeWithBalance, Decimal>) (z => z.Payment))),
        new PartnerBalanceAggregatedLine("Collection", list.Sum<PartnerBalanceByTypeWithBalance>((Func<PartnerBalanceByTypeWithBalance, Decimal>) (z => z.Collection)))
      }
    };
  }
}
