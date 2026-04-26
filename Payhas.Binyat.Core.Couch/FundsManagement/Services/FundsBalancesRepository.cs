// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Couch.FundsManagement.Services.FundsBalancesRepository
// Assembly: Payhas.Binyat.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.Couch.dll

using Payhas.Binyat.Authorization.Enums;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Core.Couch.Common;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.FundsManagement.Services;
using Payhas.Data.Authorizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Core.Couch.FundsManagement.Services;

public class FundsBalancesRepository : CouchView, IFundsBalancesRepository
{
  private readonly ILoginService _loginService;
  private readonly IAuthorizationService _authorizationService;
  private readonly IReadOnlyListAuthorizer<FundsBalance> _authorizer;

  public FundsBalancesRepository(
    ICouchCluster cluster,
    ILoginService loginService,
    IAuthorizationService authorizationService,
    IReadOnlyListAuthorizer<FundsBalance> authorizer)
    : base(cluster)
  {
    this._loginService = loginService;
    this._authorizationService = authorizationService;
    this._authorizer = authorizer;
  }

  public async Task<FundsBalance> GetBalanceToDateAsync(string depositoryId, DateTime date)
  {
    FundsBalancesRepository balancesRepository = this;
    balancesRepository._authorizer.Authorize();
    List<string> source = new List<string>()
    {
      depositoryId
    };
    if (!balancesRepository._loginService.Session.IsAdmin)
    {
      List<string> accounts = balancesRepository._authorizationService.GetAccessableAccounts(AccountAccessLevel.Read).ToList<string>();
      if (!(!source.Any<string>() || source.Any<string>(new Func<string, bool>(string.IsNullOrEmpty)) ? (IEnumerable<string>) accounts : (IEnumerable<string>) source.Where<string>((Func<string, bool>) (x => accounts.Contains(x))).ToList<string>()).Any<string>())
        return new FundsBalance()
        {
          DepositoryId = depositoryId
        };
    }
    Tuple<object, object>[] startEndKeys = new Tuple<object, object>[1]
    {
      new Tuple<object, object>((object) new string[3]
      {
        depositoryId ?? "all",
        "all",
        "0"
      }, (object) new string[3]
      {
        depositoryId ?? "all",
        "all",
        date.AddDays(1.0).ToString("yyyy-MM-dd")
      })
    };
    List<FundsBalance> list = (await balancesRepository.GetRecordsAsync<FundsBalance[]>("funds-management", "funds-balances", startEndKeys, true)).SelectMany<FundsBalance[], FundsBalance>((Func<FundsBalance[], IEnumerable<FundsBalance>>) (x => (IEnumerable<FundsBalance>) x)).ToList<FundsBalance>();
    return new FundsBalance()
    {
      DepositoryId = depositoryId,
      Income = list.Sum<FundsBalance>((Func<FundsBalance, Decimal>) (x => x.Income)),
      Expense = list.Sum<FundsBalance>((Func<FundsBalance, Decimal>) (x => x.Expense))
    };
  }

  public async Task<IEnumerable<FundsBalanceByTypeWithBalance>> GetByTypeAsync(
    string depositoryId,
    DateTime? dateFrom,
    DateTime? dateTill)
  {
    FundsBalancesRepository balancesRepository = this;
    balancesRepository._authorizer.Authorize();
    DateTime? nullable1 = dateFrom;
    DateTime? nullable2 = dateTill;
    if ((nullable1.HasValue & nullable2.HasValue ? (nullable1.GetValueOrDefault() >= nullable2.GetValueOrDefault() ? 1 : 0) : 0) != 0)
      throw new ArgumentException("From date should be lower than or equal to till date");
    List<string> depositories = new List<string>()
    {
      depositoryId
    };
    if (!balancesRepository._loginService.Session.IsAdmin)
    {
      List<string> accounts = balancesRepository._authorizationService.GetAccessableAccounts(AccountAccessLevel.Read).ToList<string>();
      depositories = !depositories.Any<string>() || depositories.Any<string>(new Func<string, bool>(string.IsNullOrEmpty)) ? accounts : depositories.Where<string>((Func<string, bool>) (x => accounts.Contains(x))).ToList<string>();
      if (!depositories.Any<string>())
        return (IEnumerable<FundsBalanceByTypeWithBalance>) new FundsBalanceByTypeWithBalance[0];
    }
    Tuple<object, object>[] array1 = depositories.Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (accountId => new Tuple<object, object>((object) new string[3]
    {
      accountId ?? "all",
      "all",
      "0"
    }, (object) new string[3]
    {
      accountId ?? "all",
      "all",
      dateFrom.HasValue ? dateFrom.Value.ToString("yyyy-MM-dd") : "0"
    }))).ToArray<Tuple<object, object>>();
    List<FundsBalance> startingBalances = (await balancesRepository.GetRecordsAsync<FundsBalance[]>("funds-management", "funds-balances", array1, true)).SelectMany<FundsBalance[], FundsBalance>((Func<FundsBalance[], IEnumerable<FundsBalance>>) (x => (IEnumerable<FundsBalance>) x)).ToList<FundsBalance>();
    Tuple<object, object>[] array2 = depositories.Select<string, Tuple<object, object>>((Func<string, Tuple<object, object>>) (accountId =>
    {
      string[] strArray1 = new string[3]
      {
        accountId ?? "all",
        "all",
        null
      };
      DateTime dateTime;
      string str1;
      if (!dateFrom.HasValue)
      {
        str1 = "0";
      }
      else
      {
        dateTime = dateFrom.Value;
        str1 = dateTime.ToString("yyyy-MM-dd");
      }
      strArray1[2] = str1;
      string[] strArray2 = new string[3]
      {
        accountId ?? "all",
        "all",
        null
      };
      string str2;
      if (!dateTill.HasValue)
      {
        str2 = "zzz";
      }
      else
      {
        dateTime = dateTill.Value;
        str2 = dateTime.ToString("yyyy-MM-dd");
      }
      strArray2[2] = str2;
      return new Tuple<object, object>((object) strArray1, (object) strArray2);
    })).ToArray<Tuple<object, object>>();
    List<FundsBalanceByType> changingBalances = (await balancesRepository.GetRecordsAsync<FundsBalanceByType[]>("funds-management", "funds-balances", array2, true)).SelectMany<FundsBalanceByType[], FundsBalanceByType>((Func<FundsBalanceByType[], IEnumerable<FundsBalanceByType>>) (x => (IEnumerable<FundsBalanceByType>) x)).ToList<FundsBalanceByType>();
    return startingBalances.Select(x => new
    {
      DepositoryId = x.DepositoryId
    }).Union(changingBalances.Select(x => new
    {
      DepositoryId = x.DepositoryId
    })).Distinct().Select(x => new
    {
      item = x,
      startingBalances = startingBalances.Where<FundsBalance>((Func<FundsBalance, bool>) (z => z.DepositoryId == x.DepositoryId)),
      changingBalances = changingBalances.Where<FundsBalanceByType>((Func<FundsBalanceByType, bool>) (z => z.DepositoryId == x.DepositoryId))
    }).Select(x =>
    {
      return new FundsBalanceByTypeWithBalance()
      {
        DepositoryId = x.item.DepositoryId,
        StartingBalance = x.startingBalances.Sum<FundsBalance>((Func<FundsBalance, Decimal>) (z => z.Balance)),
        Income = x.changingBalances.Sum<FundsBalanceByType>((Func<FundsBalanceByType, Decimal>) (z => z.Income)),
        Expense = x.changingBalances.Sum<FundsBalanceByType>((Func<FundsBalanceByType, Decimal>) (z => z.Expense)),
        FundsOpening = x.changingBalances.Sum<FundsBalanceByType>((Func<FundsBalanceByType, Decimal>) (z => z.FundsOpening)),
        FundsRevisionExceed = x.changingBalances.Sum<FundsBalanceByType>((Func<FundsBalanceByType, Decimal>) (z => z.FundsRevisionExceed)),
        FundsRevisionDeficit = x.changingBalances.Sum<FundsBalanceByType>((Func<FundsBalanceByType, Decimal>) (z => z.FundsRevisionDeficit)),
        FundsTransferSource = x.changingBalances.Sum<FundsBalanceByType>((Func<FundsBalanceByType, Decimal>) (z => z.FundsTransferSource)),
        FundsTransferDestination = x.changingBalances.Sum<FundsBalanceByType>((Func<FundsBalanceByType, Decimal>) (z => z.FundsTransferDestination)),
        ExpenseSlip = x.changingBalances.Sum<FundsBalanceByType>((Func<FundsBalanceByType, Decimal>) (z => z.ExpenseSlip)),
        Sales = x.changingBalances.Sum<FundsBalanceByType>((Func<FundsBalanceByType, Decimal>) (z => z.Sales)),
        SalesReturn = x.changingBalances.Sum<FundsBalanceByType>((Func<FundsBalanceByType, Decimal>) (z => z.SalesReturn)),
        Purchase = x.changingBalances.Sum<FundsBalanceByType>((Func<FundsBalanceByType, Decimal>) (z => z.Purchase)),
        PurchaseReturn = x.changingBalances.Sum<FundsBalanceByType>((Func<FundsBalanceByType, Decimal>) (z => z.PurchaseReturn)),
        Payment = x.changingBalances.Sum<FundsBalanceByType>((Func<FundsBalanceByType, Decimal>) (z => z.Payment)),
        Collection = x.changingBalances.Sum<FundsBalanceByType>((Func<FundsBalanceByType, Decimal>) (z => z.Collection))
      };
    });
  }

  public async Task<FundsBalanceAggregated> GetByTypeAggregatedAsync(
    string[] depositoryIds,
    DateTime? dateFrom = null,
    DateTime? dateTill = null)
  {
    List<FundsBalanceByTypeWithBalance> list = new List<FundsBalanceByTypeWithBalance>();
    string[] strArray = depositoryIds;
    for (int index = 0; index < strArray.Length; ++index)
    {
      string depositoryId = strArray[index];
      List<FundsBalanceByTypeWithBalance> byTypeWithBalanceList = list;
      byTypeWithBalanceList.AddRange(await this.GetByTypeAsync(depositoryId, dateFrom, dateTill));
      byTypeWithBalanceList = (List<FundsBalanceByTypeWithBalance>) null;
    }
    strArray = (string[]) null;
    FundsBalanceAggregated typeAggregatedAsync = new FundsBalanceAggregated()
    {
      Income = list.Sum<FundsBalanceByTypeWithBalance>((Func<FundsBalanceByTypeWithBalance, Decimal>) (x => x.Income)),
      Expense = list.Sum<FundsBalanceByTypeWithBalance>((Func<FundsBalanceByTypeWithBalance, Decimal>) (x => x.Expense)),
      StartingBalance = list.Sum<FundsBalanceByTypeWithBalance>((Func<FundsBalanceByTypeWithBalance, Decimal>) (x => x.StartingBalance)),
      Lines = (IEnumerable<FundsBalanceAggregatedLine>) new FundsBalanceAggregatedLine[12]
      {
        new FundsBalanceAggregatedLine("FundsOpening", list.Sum<FundsBalanceByTypeWithBalance>((Func<FundsBalanceByTypeWithBalance, Decimal>) (x => x.FundsOpening))),
        new FundsBalanceAggregatedLine("FundsRevisionExceed", list.Sum<FundsBalanceByTypeWithBalance>((Func<FundsBalanceByTypeWithBalance, Decimal>) (x => x.FundsRevisionExceed))),
        new FundsBalanceAggregatedLine("FundsRevisionDeficit", list.Sum<FundsBalanceByTypeWithBalance>((Func<FundsBalanceByTypeWithBalance, Decimal>) (x => x.FundsRevisionDeficit))),
        new FundsBalanceAggregatedLine("FundsTransferSource", list.Sum<FundsBalanceByTypeWithBalance>((Func<FundsBalanceByTypeWithBalance, Decimal>) (x => x.FundsTransferSource))),
        new FundsBalanceAggregatedLine("FundsTransferDestination", list.Sum<FundsBalanceByTypeWithBalance>((Func<FundsBalanceByTypeWithBalance, Decimal>) (x => x.FundsTransferDestination))),
        new FundsBalanceAggregatedLine("ExpenseSlip", list.Sum<FundsBalanceByTypeWithBalance>((Func<FundsBalanceByTypeWithBalance, Decimal>) (x => x.ExpenseSlip))),
        new FundsBalanceAggregatedLine("Sales", list.Sum<FundsBalanceByTypeWithBalance>((Func<FundsBalanceByTypeWithBalance, Decimal>) (x => x.Sales))),
        new FundsBalanceAggregatedLine("SalesReturn", list.Sum<FundsBalanceByTypeWithBalance>((Func<FundsBalanceByTypeWithBalance, Decimal>) (x => x.SalesReturn))),
        new FundsBalanceAggregatedLine("Purchase", list.Sum<FundsBalanceByTypeWithBalance>((Func<FundsBalanceByTypeWithBalance, Decimal>) (x => x.Purchase))),
        new FundsBalanceAggregatedLine("PurchaseReturn", list.Sum<FundsBalanceByTypeWithBalance>((Func<FundsBalanceByTypeWithBalance, Decimal>) (x => x.PurchaseReturn))),
        new FundsBalanceAggregatedLine("Payment", list.Sum<FundsBalanceByTypeWithBalance>((Func<FundsBalanceByTypeWithBalance, Decimal>) (x => x.Payment))),
        new FundsBalanceAggregatedLine("Collection", list.Sum<FundsBalanceByTypeWithBalance>((Func<FundsBalanceByTypeWithBalance, Decimal>) (x => x.Collection)))
      }
    };
    list = (List<FundsBalanceByTypeWithBalance>) null;
    return typeAggregatedAsync;
  }
}
