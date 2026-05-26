// Decompiled with JetBrains decompiler
// Type: Mermer.Finance.Spending.Models.Authorizers.ExpenseSlipAuthorizer
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Common.Services;
using Mermer.Transactions.Models.Authorizers;
using Mermer.Data.Tools.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Finance.Spending.Models.Authorizers;

public class ExpenseSlipAuthorizer(
  ILoginService loginService,
  ILocalizationService localizationService,
  IAuthorizationService authService) : TransactionAuthorizer<ExpenseSlip>(loginService, localizationService, authService, (Enum) TransactionActions.ExpenseSlips)
{
  protected override string[] GetAccessedAccounts(ExpenseSlip item)
  {
    return new string[1]{ item.DepositoryId };
  }

  protected override Expression<Func<ExpenseSlip, bool>> GetFilter(IEnumerable<string> accounts)
  {
    return Predicate.Create<ExpenseSlip>((Expression<Func<ExpenseSlip, bool>>) (x => accounts.Contains<string>(x.DepositoryId)));
  }
}
